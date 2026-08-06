using System.Collections.Generic;
using UnityEngine;
using MyCommon;
using ExitGames.Client.Photon;

/// <summary>
/// 远程玩家的表现层：接收服务器事件并更新显示
/// </summary>
public class RemotePlayer : MonoBehaviour
{
    public static RemotePlayer Instance;

    private Animator anim;
    private Vector3 targetPos;
    private Vector3 targetScale = Vector3.one;
    private string[] attackAnims = new string[] { "Joanna Attack L1", "Joanna Attack L2", "Joanna Attack H1" };

    void Awake()
    {
        Instance = this;
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPos, 12f * Time.deltaTime);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, 500f * Time.deltaTime);
    }

    public void ApplyPosition(ExitGames.Client.Photon.ParameterDictionary data)
    {
        object val;
        if (data.TryGetValue((byte)ParameterCode.PosX, out val)) targetPos.x = (float)val;
        if (data.TryGetValue((byte)ParameterCode.PosY, out val)) targetPos.y = (float)val;
        targetPos.z = 0;

        if (data.TryGetValue((byte)ParameterCode.ScaleX, out val)) targetScale.x = (float)val;
        targetScale.y = 1; targetScale.z = 1;

        if (anim != null)
        {
            if (data.TryGetValue((byte)ParameterCode.IsRun, out val)) anim.SetBool("IsRun", (bool)val);
            if (data.TryGetValue((byte)ParameterCode.IsJump, out val)) anim.SetBool("IsJump", (bool)val);
            if (data.TryGetValue((byte)ParameterCode.IsJumpTurn, out val)) anim.SetBool("IsJumpTurn", (bool)val);
            if (data.TryGetValue((byte)ParameterCode.IsJumpFall, out val)) anim.SetBool("IsJumpFall", (bool)val);
        }
    }

    public void PlayAttack(ExitGames.Client.Photon.ParameterDictionary data)
    {
        if (anim == null) return;
        object val;
        int index = 0;
        if (data.TryGetValue((byte)ParameterCode.ComboIndex, out val)) index = (int)val;
        if (index < attackAnims.Length) anim.Play(attackAnims[index]);
    }

    public void PlayDie()
    {
        if (anim != null) anim.SetTrigger("IsDead");
    }
}
