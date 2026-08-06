using Photon.Pun;

public class ShieldEnemy : Enemy
{
    protected override float Hp { get; set; } = 100f;

    [PunRPC]
    public override void Attacked(float damage) { base.Attacked(damage); }

    public override void AttackState()
    {
        if (!isAttcking) { an.SetTrigger("GoAtt"); isAttcking = true; isAttacked = true; }
        var s = an.GetCurrentAnimatorStateInfo(0);
        if (s.IsName("Attack") && s.normalizedTime >= 0.9f) { state = EnemyState.Idle; isAttcking = false; }
        else if (s.IsName("Attack") && s.normalizedTime >= 0.4f && s.normalizedTime < 0.5f && isAttacked) { AttPlayer(); isAttacked = false; }
    }
}
