using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum EnemyState { Idle, Chase, Attack, Die, Attcked }

public abstract class Enemy : MonoBehaviourPun, IPunObservable
{
    public Enemy instance;
    protected virtual float Hp { get; set; } = 40;
    public float CurrentHp { get { return Hp; } }

    [SerializeField] protected EnemyState state;
    protected Animator an;
    protected float IdleTimer, x, attckTimer, attackIntervalTimer;
    protected bool isAttcking, isAttacked, isDie, hasCountedKill, isTurn;
    protected virtual float attckDistance => 1.5f;
    [SerializeField] private GameObject AttRange;

    protected virtual void Awake() { an = GetComponent<Animator>(); }

    protected virtual void Start()
    {
        instance = this;
        animator = GetComponent<Animator>();
        currentPos = transform.position;
        currentScale = transform.localScale;
        if (photonView.IsMine)
            InvokeRepeating(nameof(SendEnemyState), 0f, 0.033f);
    }

    void SendEnemyState()
    {
        if (state == EnemyState.Die) return;
        string data = string.Format("{0}|{1:F2}|{2:F2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}",
            photonView.ViewID, transform.position.x, transform.position.y, transform.localScale.x,
            animator.GetBool("IsWalk") ? 1 : 0,
            animator.GetBool("Die") ? 1 : 0,
            animator.GetBool("Attcked") ? 1 : 0,
            animator.GetBool("GoAtt") ? 1 : 0, Hp, (int)state);
        var dict = new Dictionary<byte, object> { { (byte)MyCommon.ParameterCode.ChatMessage, data } };
        PhotonManager.instance?.Peer.SendOperation((byte)MyCommon.OperationCode.EnemySync, dict, ExitGames.Client.Photon.SendOptions.SendUnreliable);
    }

    protected virtual void Update()
    {
        if (photonView.IsMine)
        {
            attackIntervalTimer += Time.deltaTime;
            if (state == EnemyState.Idle) IdleTimer += Time.deltaTime;
            switch (state)
            {
                case EnemyState.Idle: IdleState(); break;
                case EnemyState.Chase: ChaseState(); break;
                case EnemyState.Attack: AttackState(); break;
                case EnemyState.Die: Die(); break;
                case EnemyState.Attcked: AttckedState(); break;
            }
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, currentPos, 25f * Time.deltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, currentScale, 50f * Time.deltaTime);
            animator.SetBool("IsWalk", IsWalk);
            animator.SetBool("Die", IsDie);
            animator.SetBool("Attcked", Attcked);
            animator.SetBool("GoAtt", GoAtt);
        }
    }

    [PunRPC]
    public virtual void Attacked(float damage)
    {
        if (state == EnemyState.Die || isDie) return;
        Hp -= damage;
        if (Hp <= 0)
        {
            Hp = 0; isDie = true; state = EnemyState.Die;
            an.SetBool("Die", true); an.SetTrigger("Die");
            if (!hasCountedKill) { hasCountedKill = true; WaveManager.instance?.UpKillDirect(); }
            Destroy(gameObject, 2f);
        }
        else { an.SetTrigger("Attcked"); state = EnemyState.Attcked; }
    }

    public virtual void IdleState()
    {
        if (x == 0) { x = Random.Range(2f, 5f); return; }
        if (IdleTimer >= x * 0.4f && IdleTimer < x)
        {
            if (!isTurn) { transform.localScale = (Random.Range(0, 2) == 0 ? Vector3.one : new Vector3(-1, 1, 1)); isTurn = true; }
            an.SetBool("IsWalk", true);
            int speed = transform.localScale.x == -1 ? -1 : 1;
            transform.Translate(Vector3.right * speed * Time.deltaTime * 2);
        }
        else if (IdleTimer >= x) { an.SetBool("IsWalk", false); IdleTimer = 0; x = 0; state = EnemyState.Chase; }
    }

    public virtual void ChaseState()
    {
        PlayerManager chasePlayer = FindPlayer();
        if (chasePlayer == null) { state = EnemyState.Idle; return; }
        int direction = chasePlayer.transform.position.x > transform.position.x ? 1 : -1;
        transform.localScale = new Vector3(direction, 1, 1);
        an.SetBool("IsWalk", true);
        transform.Translate(Vector3.right * direction * Time.deltaTime * 2);
        if (Mathf.Abs(chasePlayer.transform.position.x - transform.position.x) <= attckDistance && attackIntervalTimer >= 4)
        { an.SetBool("IsWalk", false); state = EnemyState.Attack; }
    }

    public virtual void AttackState()
    {
        if (!isAttcking) { an.SetTrigger("GoAtt"); isAttcking = true; isAttacked = true; }
        var s = an.GetCurrentAnimatorStateInfo(0);
        if (s.IsName("Attack") && s.normalizedTime >= 0.9f) { state = EnemyState.Idle; isAttcking = false; }
        else if (s.IsName("Attack") && s.normalizedTime >= 0.3f && s.normalizedTime < 0.4f && isAttacked) { AttPlayer(); isAttacked = false; }
    }

    public virtual void AttckedState()
    {
        if (an.GetCurrentAnimatorStateInfo(0).IsName("Hurt") && an.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.8f)
            state = EnemyState.Chase;
    }

    public virtual void Die()
    {
        if (isDie) return;
        isDie = true; an.SetTrigger("Die");
        Destroy(gameObject, 2f);
    }

    public virtual PlayerManager FindPlayer()
    {
        PlayerManager[] players = FindObjectsOfType<PlayerManager>();
        float minDis = float.MaxValue; PlayerManager target = null;
        foreach (var p in players)
        { float d = Vector3.Distance(transform.position, p.transform.position); if (d < minDis) { minDis = d; target = p; } }
        return target;
    }

    protected void AttPlayer()
    {
        AttRange.SetActive(true);
        BoxCollider2D col = AttRange.GetComponent<BoxCollider2D>();
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Player")); filter.useTriggers = true;
        List<Collider2D> results = new List<Collider2D>();
        col.OverlapCollider(filter, results);
        for (int i = 0; i < results.Count; i++)
        {
            PlayerManager p = results[i].GetComponent<PlayerManager>();
            if (p != null && p.photonView != null)
                p.photonView.RPC("Attacked", RpcTarget.All, 10f);
        }
        AttRange.SetActive(false);
    }

    // Photon 序列化（空，位置走自建服务器）
    public Animator animator;
    public Vector3 currentPos, currentScale;
    public bool IsWalk, IsDie, Attcked, GoAtt;

    private static List<Enemy> allEnemies = new List<Enemy>();
    void OnEnable() { allEnemies.Add(this); }
    void OnDisable() { allEnemies.Remove(this); }

    public static void ApplyEnemySync(ExitGames.Client.Photon.ParameterDictionary data)
    {
        object val;
        if (!data.TryGetValue((byte)MyCommon.ParameterCode.ChatMessage, out val)) return;
        string[] p = ((string)val).Split('|');
        if (p.Length < 10) return;
        int viewId = int.Parse(p[0]);
        float px = float.Parse(p[1]), py = float.Parse(p[2]), sx = float.Parse(p[3]);
        bool walk = p[4] == "1", die = p[5] == "1", attcked = p[6] == "1", goatt = p[7] == "1";
        float hp = float.Parse(p[8]); int es = int.Parse(p[9]);
        foreach (var e in allEnemies)
        {
            if (e.photonView.ViewID == viewId)
            {
                e.currentPos = new Vector3(px, py, 0); e.currentScale = new Vector3(sx, 1, 1);
                e.IsWalk = walk; e.IsDie = die; e.Attcked = attcked; e.GoAtt = goatt; e.Hp = hp;
                if (e.state != (EnemyState)es)
                {
                    e.state = (EnemyState)es;
                    if (e.state == EnemyState.Attack) e.an.SetTrigger("GoAtt");
                    else if (e.state == EnemyState.Attcked) e.an.SetTrigger("Attcked");
                    else if (e.state == EnemyState.Die && !e.isDie) { e.isDie = true; e.an.SetBool("Die", true); e.an.SetTrigger("Die"); Destroy(e.gameObject, 2f); }
                }
                break;
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) { }
}
