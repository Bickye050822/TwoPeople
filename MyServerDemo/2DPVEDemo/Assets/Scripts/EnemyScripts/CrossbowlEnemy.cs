using Photon.Pun;

public class CrossbowEnemy : Enemy
{
    protected override float attckDistance => 15f;

    [PunRPC]
    public override void Attacked(float damage) { base.Attacked(damage); }
}
