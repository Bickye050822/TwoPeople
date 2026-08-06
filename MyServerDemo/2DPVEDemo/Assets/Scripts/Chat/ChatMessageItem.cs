using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 单条聊天消息显示组件（挂载在聊天文本预制体上）
/// </summary>
public class ChatMessageItem : MonoBehaviour
{
    public Text messageText;

    public void SetMessage(string text)
    {
        if (messageText != null)
            messageText.text = text;
    }
}
