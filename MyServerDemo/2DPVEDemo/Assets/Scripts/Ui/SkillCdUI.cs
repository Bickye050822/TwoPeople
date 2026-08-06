using UnityEngine;
using UnityEngine.UI;

public class SkillCdUI : MonoBehaviour
{
    private Image uMask, iMask, oMask, lMask;

    void Start()
    {
        var canvas = GameObject.Find("Canvas");
        if (!canvas) return;
        var u = canvas.transform.Find("UI/SikllQ/Mask");
        var i = canvas.transform.Find("UI/SikllI/Mask");
        var o = canvas.transform.Find("UI/SikllE/Mask");
        var l = canvas.transform.Find("UI/SikllL/Mask");
        if (u) { uMask = u.GetComponent<Image>(); uMask.type = Image.Type.Filled; }
        if (i) { iMask = i.GetComponent<Image>(); iMask.type = Image.Type.Filled; }
        if (o) { oMask = o.GetComponent<Image>(); oMask.type = Image.Type.Filled; }
        if (l) { lMask = l.GetComponent<Image>(); lMask.type = Image.Type.Filled; }
    }

    void Update()
    {
        if (!PlayerManager.instance || PlayerManager.instance.photonView == null || !PlayerManager.instance.photonView.IsMine) return;
        if (uMask) uMask.fillAmount = Mathf.Clamp01(PlayerManager.instance.AoeCdPercent);
        if (iMask) iMask.fillAmount = Mathf.Clamp01(PlayerManager.instance.ProjCdPercent);
        if (oMask) oMask.fillAmount = Mathf.Clamp01(PlayerManager.instance.HealCdPercent);
        if (lMask) lMask.fillAmount = Mathf.Clamp01(PlayerManager.instance.UltiCdPercent);
    }
}
