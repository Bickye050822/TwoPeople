using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using MyCommon;
using ExitGames.Client.Photon;

public class PlayerManager : MonoBehaviourPun, IPunObservable
{
    public static PlayerManager instance;
    private static List<PlayerManager> allPlayers = new List<PlayerManager>();
    void OnEnable() { allPlayers.Add(this); }
    void OnDisable() { allPlayers.Remove(this); }
    public static PlayerManager GetRemotePlayer()
    {
        foreach (var p in allPlayers)
            if (p.photonView && !p.photonView.IsMine) return p;
        return null;
    }

    #region 属性
    private float Hp = 100f;
    private float maxHp = 100f;
    private bool isDead;

    // 技能冷却
    private float aoeCd, aoeCdMax = 3f;
    private float projCd, projCdMax = 4f;
    private float healCd, healCdMax = 5f;
    private float ultiCd, ultiCdMax = 60f;
    public float AoeCdPercent => aoeCd / aoeCdMax;
    public float ProjCdPercent => projCd / projCdMax;
    public float HealCdPercent => healCd / healCdMax;
    public float UltiCdPercent => ultiCd / ultiCdMax;
    public float CurrentHp => Hp;
    public float MaxHp => maxHp;
    public bool IsDead => isDead;
    private float gameTime;
    private int score;
    public string GetGameTime() { int m = (int)(gameTime / 60); int s = (int)(gameTime % 60); return m.ToString("D2") + ":" + s.ToString("D2"); }
    public int Score => score;
    public void AddScore(int a) { score += a; }
    #endregion

    #region 组件
    private Animator anim;
    private Rigidbody2D rb;
    private float speed = 5f;
    [SerializeField] private GameObject AttRange;
    private string[] attackAnims = { "Joanna Attack L1", "Joanna Attack L2", "Joanna Attack H1" };
    private int curCombo;
    private float atkBufTimer, comboResetTimer;
    private bool isAttacking;
    private Vector3 remotePos, remoteScale = Vector3.one;
    #endregion

    void Awake() { }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        if (photonView.IsMine) { instance = this; InvokeRepeating(nameof(SendPos), 0f, 0.05f); }
    }

    void Update()
    {
        if (isDead) return;
        if (photonView.IsMine)
        {
            gameTime += Time.deltaTime;
            Jump(); Move(); Att(); Skills();
            if (atkBufTimer > 0) atkBufTimer -= Time.deltaTime;
            if (isAttacking && comboResetTimer > 0) { comboResetTimer -= Time.deltaTime; if (comboResetTimer <= 0) ResetCombo(); }
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, remotePos, 20f * Time.deltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, remoteScale, 40f * Time.deltaTime);
        }
    }

    // ========== 位置同步 ==========
    void SendPos()
    {
        if (isDead) return;
        string d = string.Format("{0:F2}|{1:F2}|{2}|{3}|{4}|{5}|{6}|{7}",
            transform.position.x, transform.position.y, transform.localScale.x,
            anim.GetBool("IsRun") ? 1 : 0, anim.GetBool("IsJump") ? 1 : 0,
            anim.GetBool("IsJumpTurn") ? 1 : 0, anim.GetBool("IsJumpFall") ? 1 : 0, Hp);
        PhotonManager.instance?.Peer.SendOperation((byte)OperationCode.PlayerPosition,
            new Dictionary<byte, object> { { (byte)ParameterCode.ChatMessage, d } }, SendOptions.SendReliable);
    }

    public void ApplyRemotePosition(ExitGames.Client.Photon.ParameterDictionary data)
    {
        if (!data.TryGetValue((byte)ParameterCode.ChatMessage, out object v)) return;
        string[] p = ((string)v).Split('|');
        if (p.Length < 8) return;
        remotePos.x = float.Parse(p[0]); remotePos.y = float.Parse(p[1]);
        remoteScale.x = float.Parse(p[2]);
        anim.SetBool("IsRun", p[3] == "1"); anim.SetBool("IsJump", p[4] == "1");
        anim.SetBool("IsJumpTurn", p[5] == "1"); anim.SetBool("IsJumpFall", p[6] == "1");
        Hp = float.Parse(p[7]);
    }

    // ========== 移动 ==========
    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        anim.SetBool("IsRun", Mathf.Abs(x) > 0);
        if (x > 0) transform.localScale = Vector3.one;
        else if (x < 0) transform.localScale = new Vector3(-1, 1, 1);
        transform.Translate(new Vector3(x, 0, 0) * Time.deltaTime * speed);
    }

    bool GoJump;
    void Jump()
    {
        var gc = transform.Find("GroundCheck")?.GetComponent<BoxCollider2D>();
        if (gc == null) return;
        bool g = gc.IsTouchingLayers(LayerMask.GetMask("Ground"));
        if (g)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Joanna Jump Turn"))
            { anim.SetBool("IsJumpTurn", false); anim.SetBool("IsJumpFall", true); }
            anim.SetBool("IsJump", false);
            if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.K)) GoJump = true;
        }
        else { anim.SetBool("IsJumpFall", false); }
        if (rb.velocity.y >= 1.5f) anim.SetBool("IsJump", true);
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Joanna Jump Up") && rb.velocity.y < 1.5f) anim.SetBool("IsJumpTurn", true);
        if (GoJump) { rb.velocity = new Vector2(0, 6); GoJump = false; }
    }

    // ========== 攻击 ==========
    void Att()
    {
        var s = anim.GetCurrentAnimatorStateInfo(0);
        bool inAtk = false; foreach (var n in attackAnims) if (s.IsName(n)) { inAtk = true; break; }
        if (Input.GetKeyDown(KeyCode.J))
        { if (!isAttacking) DoAttack(0); else atkBufTimer = 0.3f; }
        if (isAttacking && !inAtk)
        { if (atkBufTimer > 0) { int nx = curCombo + 1; if (nx < attackAnims.Length) DoAttack(nx); else ResetCombo(); } else ResetCombo(); }
    }

    void DoAttack(int idx)
    {
        curCombo = idx; anim.Play(attackAnims[idx]); isAttacking = true; atkBufTimer = 0; HitEnemy(); comboResetTimer = 1.5f;
        PhotonManager.instance?.Peer.SendOperation((byte)OperationCode.PlayerAttack,
            new Dictionary<byte, object> { { (byte)ParameterCode.ComboIndex, idx } }, SendOptions.SendReliable);
    }

    void HitEnemy()
    {
        AttRange.SetActive(true);
        var col = AttRange.GetComponent<CapsuleCollider2D>();
        var f = new ContactFilter2D(); f.SetLayerMask(LayerMask.GetMask("Enemy")); f.useTriggers = true;
        var hits = new List<Collider2D>(); col.OverlapCollider(f, hits);
        foreach (var h in hits)
        {
            var e = h.GetComponent<Enemy>();
            if (e && e.photonView) { e.photonView.RPC("Attacked", RpcTarget.All, 10f); AddScore(100); }
        }
        AttRange.SetActive(false);
    }

    public void PlayRemoteAttack(int idx) { if (idx < attackAnims.Length) anim.Play(attackAnims[idx]); }
    void ResetCombo() { isAttacking = false; curCombo = 0; atkBufTimer = 0; comboResetTimer = 0; }

    // ========== 技能 ==========
    void Skills()
    {
        if (aoeCd > 0) aoeCd -= Time.deltaTime;
        if (projCd > 0) projCd -= Time.deltaTime;
        if (healCd > 0) healCd -= Time.deltaTime;
        if (ultiCd > 0) ultiCd -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.U) && aoeCd <= 0) AoeAttack();
        if (Input.GetKeyDown(KeyCode.I) && projCd <= 0) ProjectileAttack();
        if (Input.GetKeyDown(KeyCode.O) && healCd <= 0) HealSelf();
        if (Input.GetKeyDown(KeyCode.L) && ultiCd <= 0) UltiAttack();
    }

    [SerializeField] private GameObject aoeEffectPrefab;
    [SerializeField] private GameObject healEffectPrefab;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject ultiEffectPrefab;
    [SerializeField] private float projectileScale = 3f;

    void SendSkillEffect(int type)
    {
        PhotonManager.instance?.Peer.SendOperation((byte)OperationCode.SkillEffect,
            new Dictionary<byte, object> { { (byte)ParameterCode.ComboIndex, type } }, SendOptions.SendReliable);
    }

    public void PlayRemoteSkill(int type)
    {
        if (type == 0 && aoeEffectPrefab) { var fx = Instantiate(aoeEffectPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity); fx.transform.localScale = Vector3.one * 3f; Destroy(fx, 0.5f); }
        if (type == 1 && healEffectPrefab) { var fx = Instantiate(healEffectPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity); Destroy(fx, 0.5f); }
        if (type == 2 && projectilePrefab) { Vector3 s = new Vector3(projectileScale, projectileScale, projectileScale); var proj = Instantiate(projectilePrefab, transform.position + (transform.localScale.x > 0 ? Vector3.right : Vector3.left) * 1.5f + Vector3.up, Quaternion.identity); proj.transform.localScale = transform.localScale.x > 0 ? s : new Vector3(-s.x, s.y, s.z); }
        if (type == 3 && ultiEffectPrefab) { var fx = Instantiate(ultiEffectPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity); fx.transform.localScale = Vector3.one * 10f; Destroy(fx, 0.5f); }
    }

    void AoeAttack()
    {
        aoeCd = aoeCdMax;
        if (aoeEffectPrefab) { var fx = Instantiate(aoeEffectPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity); fx.transform.localScale = Vector3.one * 3f; Destroy(fx, 0.5f); }
        SendSkillEffect(0); // 0=AOE
        var hits = Physics2D.OverlapCircleAll(transform.position, 3f, LayerMask.GetMask("Enemy"));
        foreach (var h in hits)
        {
            var e = h.GetComponent<Enemy>();
            if (e && e.photonView) e.photonView.RPC("Attacked", RpcTarget.All, 15f);
        }
        Debug.Log("[Skill] AOE! Hit " + hits.Length + " enemies");
    }

    void HealSelf()
    {
        healCd = healCdMax;
        SendSkillEffect(1); // 1=Heal
        if (healEffectPrefab) Destroy(Instantiate(healEffectPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity), 0.5f);
        Hp = Mathf.Min(Hp + 30f, maxHp);
        LocalUserData.instance.Hp = Hp;
    }

    void UltiAttack()
    {
        ultiCd = ultiCdMax;
        if (ultiEffectPrefab) { var fx = Instantiate(ultiEffectPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity); fx.transform.localScale = Vector3.one * 10f; Destroy(fx, 0.5f); }
        SendSkillEffect(3);
        var hits = Physics2D.OverlapCircleAll(transform.position, 50f, LayerMask.GetMask("Enemy"));
        foreach (var h in hits)
        {
            var e = h.GetComponent<Enemy>();
            if (e && e.photonView) e.photonView.RPC("Attacked", RpcTarget.All, 20f);
        }
    }

    void ProjectileAttack()
    {
        Debug.Log("[Skill] I pressed! projCd=" + projCd + " prefab=" + (projectilePrefab != null));
        projCd = projCdMax;
        Vector3 spawnPos = transform.position + (transform.localScale.x > 0 ? Vector3.right : Vector3.left) * 1.5f + Vector3.up;
        var proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        Vector3 scale = new Vector3(projectileScale, projectileScale, projectileScale);
        proj.transform.localScale = transform.localScale.x > 0 ? scale : new Vector3(-scale.x, scale.y, scale.z);
        SendSkillEffect(2);
    }

    // ========== 受伤 & 死亡 ==========
    [PunRPC]
    public void Attacked(float dmg) { TakeDamage(dmg); }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;
        Hp -= dmg; anim.SetTrigger("IsHit"); LocalUserData.instance.Hp = Hp;
        if (Hp <= 0) Die();
    }

    public void Die()
    {
        if (isDead) return;
        Hp = 0; isDead = true; anim.SetTrigger("IsDead"); GetComponent<Collider2D>().enabled = false; CancelInvoke();
        if (photonView.IsMine)
        {
            PhotonManager.instance?.Peer.SendOperation((byte)OperationCode.PlayerDie, new Dictionary<byte, object>(), SendOptions.SendReliable);
            GameManager.instance?.GameOver("通关失败", GetGameTime(), score.ToString());
        }
    }

    public void OnPhotonSerializeView(PhotonStream s, PhotonMessageInfo i) { }
}
