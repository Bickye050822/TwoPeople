using Photon.Pun;

public class HalberdEnemy : Enemy
{
    protected override float Hp { get; set; } = 60f;

    [PunRPC]
    public override void Attacked(float damage) { base.Attacked(damage); }
}
