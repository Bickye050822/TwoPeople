using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
    [Header("结果显示")]
    public Text resultText;
    public Text timeText;
    public Text scoreText;
    public Text userNameText;

    public void Show(string result, string time, string score, string userName)
    {
        gameObject.SetActive(true);
        if (resultText != null) resultText.text = result;
        if (timeText != null) timeText.text = time;
        if (scoreText != null) scoreText.text = score;
        if (userNameText != null) userNameText.text = userName;
    }

}
