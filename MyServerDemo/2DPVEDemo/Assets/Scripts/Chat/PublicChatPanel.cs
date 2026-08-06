using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyCommon;
using ExitGames.Client.Photon;

/// <summary>
/// 公频聊天面板
/// </summary>
public class PublicChatPanel : MonoBehaviour
{
    public InputField inputField;
    public Transform chatContentParent;
    public GameObject chatTextPrefab;

    private void OnEnable()
    {
        if (ChatManager.instance != null)
            ChatManager.instance.OnPublicMessage += OnNewMessage;
    }

    private void OnDisable()
    {
        if (ChatManager.instance != null)
            ChatManager.instance.OnPublicMessage -= OnNewMessage;
    }

    private void Update()
    {
        if (inputField != null && inputField.isFocused && Input.GetKeyDown(KeyCode.Return))
        {
            SendPublicMessage();
        }
    }

    public void SendPublicMessage()
    {
        if (inputField == null || string.IsNullOrEmpty(inputField.text))
            return;

        string message = inputField.text;
        inputField.text = "";

        Dictionary<byte, object> data = new Dictionary<byte, object>
        {
            { (byte)ParameterCode.ChatMessage, message }
        };
        if (PhotonManager.instance != null && PhotonManager.instance.Peer != null)
            PhotonManager.instance.Peer.SendOperation((byte)OperationCode.PublicChat, data, SendOptions.SendReliable);

        inputField.ActivateInputField();
    }

    private void OnNewMessage(string sender, string message)
    {
        if (chatContentParent == null || chatTextPrefab == null)
            return;

        GameObject msgObj = Instantiate(chatTextPrefab, chatContentParent);
        Text msgText = msgObj.GetComponentInChildren<Text>();
        if (msgText != null)
            msgText.text = "[" + sender + "]: " + message;

        // 滚动到底部
        Canvas.ForceUpdateCanvases();
        Transform scrollParent = chatContentParent.parent;
        if (scrollParent != null)
        {
            ScrollRect scrollRect = scrollParent.GetComponent<ScrollRect>();
            if (scrollRect != null)
                scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
