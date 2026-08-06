using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 绑定到 HP 血条上，自动追随本地玩家并更新血量
/// </summary>
public class HpBar : MonoBehaviour
{
    public Image fillImage;        // HP > Value 上的 Image
    private PlayerManager player;

    private void Start()
    {
        if (fillImage == null)
            fillImage = transform.Find("Value")?.GetComponent<Image>();

        StartCoroutine(FindPlayer());
    }

    private System.Collections.IEnumerator FindPlayer()
    {
        while (player == null)
        {
            if (PlayerManager.instance != null)
            {
                player = PlayerManager.instance;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void Update()
    {
        if (player == null || fillImage == null) return;

        float ratio = player.CurrentHp / player.MaxHp;
        fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, ratio, 10f * Time.deltaTime);

        // 血量低于 30% 变红闪烁提示
        if (fillImage.fillAmount < 0.3f)
            fillImage.color = Color.Lerp(Color.red, Color.white, Mathf.PingPong(Time.time * 3f, 1f));
        else
            fillImage.color = Color.red;
    }
}
