using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyCommon;
using ExitGames.Client.Photon;

/// <summary>
/// 私聊面板
/// </summary>
public class PrivateChatPanel : MonoBehaviour
{
    [Header("好友列表")]
    public Transform friendListContent;
    public GameObject friendBtnPrefab;
    public Text currentChatTargetText;

    [Header("聊天区域")]
    public InputField inputField;
    public Transform chatContentParent;
    public GameObject chatTextPrefab;

    private string currentTarget = "";
    private List<GameObject> friendBtns = new List<GameObject>();

    private void OnEnable()
    {
        if (ChatManager.instance != null)
        {
            ChatManager.instance.OnPrivateMessage += OnNewMessage;
            ChatManager.instance.OnOnlineUsersList += OnOnlineUsersUpdated;
        }
    }

    private void OnDisable()
    {
        if (ChatManager.instance != null)
        {
            ChatManager.instance.OnPrivateMessage -= OnNewMessage;
            ChatManager.instance.OnOnlineUsersList -= OnOnlineUsersUpdated;
        }
    }

    private void Update()
    {
        if (inputField != null && inputField.isFocused && Input.GetKeyDown(KeyCode.Return))
        {
            SendPrivateMessage();
        }
    }

    private void OnOnlineUsersUpdated(List<string> users)
    {
        foreach (var btn in friendBtns)
            Destroy(btn);
        friendBtns.Clear();

        if (friendListContent == null || friendBtnPrefab == null)
            return;

        string myName = LocalUserData.instance != null ? LocalUserData.instance.currentUserId : "";
        foreach (string userName in users)
        {
            if (userName == myName) continue;

            GameObject btnObj = Instantiate(friendBtnPrefab, friendListContent);
            Text btnText = btnObj.GetComponentInChildren<Text>();
            if (btnText != null)
                btnText.text = userName;

            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                string targetName = userName;
                btn.onClick.AddListener(() => OpenChatWith(targetName));
            }

            friendBtns.Add(btnObj);
        }
    }

    public void OpenChatWith(string targetName)
    {
        currentTarget = targetName;
        if (currentChatTargetText != null)
            currentChatTargetText.text = "私聊: " + targetName;

        if (chatContentParent != null)
        {
            foreach (Transform child in chatContentParent)
                Destroy(child.gameObject);
        }

        if (ChatManager.instance != null &&
            ChatManager.instance.privateChatHistory.ContainsKey(targetName))
        {
            foreach (string msg in ChatManager.instance.privateChatHistory[targetName])
            {
                AddMessageToView(msg);
            }
        }
    }

    public void SendPrivateMessage()
    {
        if (string.IsNullOrEmpty(currentTarget) || inputField == null || string.IsNullOrEmpty(inputField.text))
            return;

        string message = inputField.text;
        inputField.text = "";

        string myName = LocalUserData.instance != null ? LocalUserData.instance.currentUserId : "";
        AddMessageToView("我: " + message);

        Dictionary<byte, object> data = new Dictionary<byte, object>
        {
            { (byte)ParameterCode.TargetName, currentTarget },
            { (byte)ParameterCode.ChatMessage, message }
        };
        if (PhotonManager.instance != null && PhotonManager.instance.Peer != null)
            PhotonManager.instance.Peer.SendOperation((byte)OperationCode.PrivateChat, data, SendOptions.SendReliable);

        inputField.ActivateInputField();
    }

    private void OnNewMessage(string sender, string message)
    {
        if (sender == currentTarget)
        {
            AddMessageToView(sender + ": " + message);
        }
    }

    private void AddMessageToView(string text)
    {
        if (chatContentParent == null || chatTextPrefab == null)
            return;

        GameObject msgObj = Instantiate(chatTextPrefab, chatContentParent);
        Text msgText = msgObj.GetComponentInChildren<Text>();
        if (msgText != null)
            msgText.text = text;
    }
}
