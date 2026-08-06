using Photon.Pun;

public class SwordEnemy : Enemy
{
    protected override float Hp { get; set; } = 100f;

    [PunRPC]
    public override void Attacked(float damage) { base.Attacked(damage); }
}
