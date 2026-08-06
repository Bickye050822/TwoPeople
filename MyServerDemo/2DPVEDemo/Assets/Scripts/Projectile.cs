using UnityEngine;
using Photon.Pun;

public class Projectile : MonoBehaviour
{
    public float speed = 5f;
    public float lifetime = 3f;
    public float damageInterval = 0.2f;
    public float damage = 3f;
    public float radius = 1f;

    private int dir;
    private float timer;
    private float dmgTimer;

    void Start()
    {
        dir = transform.localScale.x > 0 ? 1 : -1;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector3.right * dir * speed * Time.deltaTime);

        dmgTimer += Time.deltaTime;
        if (dmgTimer >= damageInterval)
        {
            dmgTimer = 0;
            var hits = Physics2D.OverlapCircleAll(transform.position, radius, LayerMask.GetMask("Enemy"));
            foreach (var h in hits)
            {
                var e = h.GetComponent<Enemy>();
                if (e && e.photonView) e.photonView.RPC("Attacked", RpcTarget.All, damage);
            }
        }
    }
}
