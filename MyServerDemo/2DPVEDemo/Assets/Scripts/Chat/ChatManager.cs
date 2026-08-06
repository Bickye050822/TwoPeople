using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    public static ChatManager instance;

    [Header("UI 引用")]
    public GameObject chatPanel;
    public Button toggleButton;
    public GameObject unreadBadge;       // 总未读红点（挂 toggleButton 下）
    public Text unreadBadgeText;         // 总未读数字

    [Header("公频")]
    public List<string> publicChatHistory = new List<string>();
    public int publicUnreadCount;

    [Header("私聊")]
    public Dictionary<string, List<string>> privateChatHistory = new Dictionary<string, List<string>>();
    public Dictionary<string, int> privateUnreadCounts = new Dictionary<string, int>();

    [Header("在线用户")]
    public List<string> onlineUsers = new List<string>();

    // 事件
    public event Action<string, string> OnPublicMessage;
    public event Action<string, string> OnPrivateMessage;
    public event Action<List<string>> OnOnlineUsersList;
    public event Action<int> OnPublicUnreadChanged;
    public event Action<string, int> OnPrivateUnreadChanged;
    public event Action<int> OnTotalUnreadChanged;

    // ChatPanel 当前状态（由 ChatPanel 设置）
    public bool isViewingPublicChat;
    public string currentPrivateTarget = "";

    private bool IsPanelVisible
    {
        get { return chatPanel != null && chatPanel.activeSelf; }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (chatPanel != null)
            chatPanel.SetActive(false);

        if (toggleButton != null)
            toggleButton.onClick.AddListener(ToggleChat);

        UpdateTotalBadge();
    }

    public void ToggleChat()
    {
        if (chatPanel == null) return;

        bool active = !chatPanel.activeSelf;
        chatPanel.SetActive(active);

        if (active)
        {
            RefreshOnlineUsers();
            // 打开时清除当前查看的未读
            ClearCurrentUnread();
        }
    }

    public void RefreshOnlineUsers()
    {
        if (PhotonManager.instance != null && PhotonManager.instance.Peer != null)
        {
            PhotonManager.instance.Peer.SendOperation(
                (byte)MyCommon.OperationCode.RefreshOnlineUsers,
                new Dictionary<byte, object>(),
                ExitGames.Client.Photon.SendOptions.SendReliable);
        }
    }

    #region 未读消息

    public void ClearCurrentUnread()
    {
        if (isViewingPublicChat)
        {
            publicUnreadCount = 0;
            if (OnPublicUnreadChanged != null) OnPublicUnreadChanged(0);
        }
        else if (!string.IsNullOrEmpty(currentPrivateTarget))
        {
            if (privateUnreadCounts.ContainsKey(currentPrivateTarget))
            {
                privateUnreadCounts[currentPrivateTarget] = 0;
                if (OnPrivateUnreadChanged != null)
                    OnPrivateUnreadChanged(currentPrivateTarget, 0);
            }
        }
        UpdateTotalBadge();
    }

    public int GetPrivateUnread(string friendName)
    {
        return privateUnreadCounts.ContainsKey(friendName) ? privateUnreadCounts[friendName] : 0;
    }

    private void UpdateTotalBadge()
    {
        int total = publicUnreadCount;
        foreach (var kv in privateUnreadCounts)
            total += kv.Value;

        if (unreadBadge != null)
            unreadBadge.SetActive(total > 0);

        if (unreadBadgeText != null)
            unreadBadgeText.text = total > 99 ? "99+" : total.ToString();

        if (OnTotalUnreadChanged != null)
            OnTotalUnreadChanged(total);
    }

    #endregion

    #region 接收消息

    public void OnReceivePublicChat(string payload)
    {
        string[] parts = payload.Split('|');
        if (parts.Length >= 2)
        {
            string sender = parts[0];
            string message = parts[1];
            publicChatHistory.Add(payload);

            string myName = LocalUserData.instance != null ? LocalUserData.instance.currentUserId : "";

            // 不是自己发的 + 没有正在看公屏 → 未读+1
            if (sender != myName && !(IsPanelVisible && isViewingPublicChat))
            {
                publicUnreadCount++;
                if (OnPublicUnreadChanged != null) OnPublicUnreadChanged(publicUnreadCount);
                UpdateTotalBadge();
            }

            if (OnPublicMessage != null)
                OnPublicMessage(sender, message);
        }
    }

    public void OnReceivePrivateChat(string payload)
    {
        string[] parts = payload.Split('|');
        if (parts.Length >= 2)
        {
            string sender = parts[0];
            string message = parts[1];

            if (!privateChatHistory.ContainsKey(sender))
                privateChatHistory[sender] = new List<string>();
            privateChatHistory[sender].Add(message);

            // 没有正在和这个人私聊 → 未读+1
            if (!(IsPanelVisible && currentPrivateTarget == sender))
            {
                if (!privateUnreadCounts.ContainsKey(sender))
                    privateUnreadCounts[sender] = 0;
                privateUnreadCounts[sender]++;

                if (OnPrivateUnreadChanged != null)
                    OnPrivateUnreadChanged(sender, privateUnreadCounts[sender]);
                UpdateTotalBadge();
            }

            if (OnPrivateMessage != null)
                OnPrivateMessage(sender, message);
        }
    }

    public void OnOnlineUsersUpdated(string userList)
    {
        onlineUsers.Clear();
        if (!string.IsNullOrEmpty(userList))
        {
            string[] users = userList.Split('*');
            onlineUsers.AddRange(users);
        }
        if (OnOnlineUsersList != null)
            OnOnlineUsersList(onlineUsers);
    }

    #endregion
}
