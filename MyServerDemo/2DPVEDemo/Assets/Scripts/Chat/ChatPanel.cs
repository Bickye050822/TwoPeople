using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyCommon;
using ExitGames.Client.Photon;

public class ChatPanel : MonoBehaviour
{
    public static ChatPanel instance;

    [Header("左侧 — 好友列表")]
    public Transform friendListContent;
    public GameObject friendBtnPrefab;

    [Header("右侧 — 聊天窗口")]
    public Text currentChatTitle;
    public Transform chatContentParent;
    public GameObject chatTextPrefab;
    public InputField inputField;
    public Button submitBtn;
    public Transform emojiParent;             // 表情按钮容器
    public GameObject emojiBtnPrefab;         // 表情按钮预制体

    private enum ChatMode { Public, Private }
    private ChatMode currentMode = ChatMode.Public;
    private string currentTarget = "";
    private List<GameObject> friendBtns = new List<GameObject>();
    private Image selectedBtnImage;                    // 当前选中的按钮 Image
    private Color normalColor;
    private Color selectedColor;
    private string[] emojis = { ":)", ":D", ":P" };
    [Header("表情图片")]
    public Sprite[] emojiSprites;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        BuildPublicChatButton();
        BuildEmojiButtons();

        if (submitBtn != null)
            submitBtn.onClick.AddListener(SendChatMessage);

        // 订阅未读事件
        if (ChatManager.instance != null)
        {
            ChatManager.instance.OnPublicUnreadChanged += OnPublicUnreadChanged;
            ChatManager.instance.OnPrivateUnreadChanged += OnPrivateUnreadChanged;
        }
    }

    private void OnEnable()
    {
        if (ChatManager.instance != null)
        {
            ChatManager.instance.OnPublicMessage += OnPublicMessage;
            ChatManager.instance.OnPrivateMessage += OnPrivateMessage;
            ChatManager.instance.OnOnlineUsersList += OnOnlineUsersUpdated;
        }
        SwitchToPublicChat();
        if (ChatManager.instance != null)
        {
            ChatManager.instance.RefreshOnlineUsers();
            ChatManager.instance.ClearCurrentUnread();
        }
    }

    private void OnDisable()
    {
        if (ChatManager.instance != null)
        {
            ChatManager.instance.OnPublicMessage -= OnPublicMessage;
            ChatManager.instance.OnPrivateMessage -= OnPrivateMessage;
            ChatManager.instance.OnOnlineUsersList -= OnOnlineUsersUpdated;
            // 关闭面板时清除当前查看状态
            ChatManager.instance.isViewingPublicChat = false;
            ChatManager.instance.currentPrivateTarget = "";
        }
    }

    private void Update()
    {
        if (inputField != null && inputField.isFocused && Input.GetKeyDown(KeyCode.Return))
        {
            SendChatMessage();
        }
    }

    // ===================== 好友列表 =====================

    private void BuildPublicChatButton()
    {
        if (friendListContent == null || friendBtnPrefab == null) return;

        GameObject pubBtn = Instantiate(friendBtnPrefab, friendListContent);
        Text btnText = pubBtn.GetComponentInChildren<Text>();
        if (btnText != null) btnText.text = "公屏聊天";

        Button btn = pubBtn.GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(SwitchToPublicChat);

        friendBtns.Add(pubBtn);
        UpdatePublicUnreadBadge();
        HighlightButton(0);
    }

    private void OnOnlineUsersUpdated(List<string> users)
    {
        if (friendListContent == null || friendBtnPrefab == null) return;

        for (int i = friendBtns.Count - 1; i >= 1; i--)
            Destroy(friendBtns[i]);
        friendBtns.RemoveRange(1, friendBtns.Count - 1);

        string myName = LocalUserData.instance != null ? LocalUserData.instance.currentUserId : "";
        foreach (string userName in users)
        {
            if (userName == myName) continue;

            GameObject btnObj = Instantiate(friendBtnPrefab, friendListContent);
            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                string target = userName;
                btn.onClick.AddListener(() => SwitchToPrivateChat(target));
            }
            friendBtns.Add(btnObj);
            UpdateFriendButtonText(btnObj, userName);
        }
    }

    /// <summary>
    /// 更新好友按钮文本（含未读数）
    /// </summary>
    private void UpdateFriendButtonText(GameObject btnObj, string userName)
    {
        Text btnText = btnObj.GetComponentInChildren<Text>();
        if (btnText == null) return;

        int unread = ChatManager.instance != null ? ChatManager.instance.GetPrivateUnread(userName) : 0;
        btnText.text = unread > 0 ? userName + " (" + unread + ")" : userName;
    }

    /// <summary>
    /// 更新公屏按钮的未读显示
    /// </summary>
    private void UpdatePublicUnreadBadge()
    {
        if (friendBtns.Count == 0) return;
        Text btnText = friendBtns[0].GetComponentInChildren<Text>();
        if (btnText == null) return;

        int unread = ChatManager.instance != null ? ChatManager.instance.publicUnreadCount : 0;
        btnText.text = unread > 0 ? "公屏聊天 (" + unread + ")" : "公屏聊天";
    }

    // ===================== 未读事件 =====================

    private void OnPublicUnreadChanged(int count)
    {
        UpdatePublicUnreadBadge();
    }

    private void OnPrivateUnreadChanged(string friendName, int count)
    {
        // 找到对应好友按钮并更新
        string myName = LocalUserData.instance != null ? LocalUserData.instance.currentUserId : "";
        for (int i = 1; i < friendBtns.Count; i++)
        {
            Text t = friendBtns[i].GetComponentInChildren<Text>();
            if (t != null)
            {
                // 从文本中提取原始用户名（去掉未读后缀）
                string displayName = t.text.Contains(" (") ? t.text.Substring(0, t.text.IndexOf(" (")) : t.text;
                if (displayName == friendName)
                {
                    UpdateFriendButtonText(friendBtns[i], friendName);
                    break;
                }
            }
        }
    }

    // ===================== 切换聊天 =====================

    private void HighlightButton(int index)
    {
        // 还原上一个
        if (selectedBtnImage != null)
            selectedBtnImage.color = normalColor;

        if (index >= 0 && index < friendBtns.Count)
        {
            Transform imgChild = friendBtns[index].transform.Find("WindowTitleBig");
            if (imgChild != null)
            {
                selectedBtnImage = imgChild.GetComponent<Image>();
                if (selectedBtnImage != null)
                {
                    normalColor = selectedBtnImage.color;           // 记住原色
                    selectedColor = normalColor * 0.8f;            // 原色变暗
                    selectedBtnImage.color = selectedColor;
                }
            }
        }
    }

    public void SwitchToPublicChat()
    {
        currentMode = ChatMode.Public;
        HighlightButton(0);
        currentTarget = "";
        if (currentChatTitle != null) currentChatTitle.text = "公屏聊天";
        if (emojiParent) emojiParent.gameObject.SetActive(true);
        if (ChatManager.instance != null)
        {
            ChatManager.instance.isViewingPublicChat = true;
            ChatManager.instance.currentPrivateTarget = "";
            ChatManager.instance.ClearCurrentUnread();
        }
        RefreshChatView();
    }

    public void SwitchToPrivateChat(string targetName)
    {
        currentMode = ChatMode.Private;
        currentTarget = targetName;
        // 找到对应好友按钮的 index 并高亮
        int idx = friendBtns.FindIndex(b =>
        {
            Text t = b.GetComponentInChildren<Text>();
            if (t == null) return false;
            string displayName = t.text.Contains(" (") ? t.text.Substring(0, t.text.IndexOf(" (")) : t.text;
            return displayName == targetName;
        });
        HighlightButton(idx);
        if (currentChatTitle != null) currentChatTitle.text = "私聊: " + targetName;
        if (emojiParent) emojiParent.gameObject.SetActive(false);
        if (ChatManager.instance != null)
        {
            ChatManager.instance.isViewingPublicChat = false;
            ChatManager.instance.currentPrivateTarget = targetName;
            ChatManager.instance.ClearCurrentUnread();
        }
        RefreshChatView();
    }

    private void RefreshChatView()
    {
        if (chatContentParent == null) return;

        foreach (Transform child in chatContentParent)
            Destroy(child.gameObject);

        if (ChatManager.instance == null) return;

        if (currentMode == ChatMode.Public)
        {
            foreach (string record in ChatManager.instance.publicChatHistory)
            {
                string[] parts = record.Split('|');
                if (parts.Length < 2) continue;
                string msg = parts[1];
                if (msg.StartsWith("#EMO") && msg.EndsWith("#"))
                {
                    string num = msg.Substring(4, msg.Length - 5);
                    int ei; if (int.TryParse(num, out ei) && ei >= 0 && emojiSprites != null && ei < emojiSprites.Length && emojiSprites[ei] != null)
                    { AddEmojiToView(parts[0], emojiSprites[ei]); continue; }
                }
                AddMessageToView("[" + parts[0] + "]: " + msg);
            }
        }
        else
        {
            if (ChatManager.instance.privateChatHistory.ContainsKey(currentTarget))
            {
                foreach (string msg in ChatManager.instance.privateChatHistory[currentTarget])
                {
                    AddMessageToView(msg);
                }
            }
        }
    }

    // ===================== 发送 =====================

    void BuildEmojiButtons()
    {
        if (emojiParent == null || emojiBtnPrefab == null) return;
        for (int i = 0; i < emojis.Length; i++)
        {
            var btn = Instantiate(emojiBtnPrefab, emojiParent);
            int idx = i;
            // 优先显示 Sprite 图片
            var img = btn.GetComponent<Image>();
            if (img && emojiSprites != null && idx < emojiSprites.Length && emojiSprites[idx] != null)
                img.sprite = emojiSprites[idx];
            else
            { var t = btn.GetComponentInChildren<Text>(); if (t) t.text = emojis[idx]; }
            var b = btn.GetComponent<Button>();
            if (b) b.onClick.AddListener(() => SendEmoji(emojis[idx]));
        }
    }

    void SendEmoji(string em)
    {
        if (currentMode != ChatMode.Public) return;
        int idx = System.Array.IndexOf(emojis, em);
        string myName = LocalUserData.instance ? LocalUserData.instance.currentUserId : "";
        // 本地显示
        if (idx >= 0 && emojiSprites != null && idx < emojiSprites.Length && emojiSprites[idx] != null)
            AddEmojiToView(myName, emojiSprites[idx]);
        else
            AddMessageToView("[" + myName + "]: " + em);
        // 发送 emoji 标签给服务器，其他客户端根据标签匹配图片
        string tag = "#EMO" + (idx >= 0 ? idx : -1) + "#";
        var data = new Dictionary<byte, object> { { (byte)ParameterCode.ChatMessage, tag } };
        PhotonManager.instance?.Peer.SendOperation((byte)OperationCode.PublicChat, data, SendOptions.SendReliable);
    }

    void AddEmojiToView(string sender, Sprite sp)
    {
        if (chatContentParent == null || chatTextPrefab == null) return;
        var go = Instantiate(chatTextPrefab, chatContentParent);
        var t = go.GetComponentInChildren<Text>();
        float size = t != null ? t.fontSize : 24;
        if (t) t.text = "[" + sender + "]: ";
        var imgObj = new GameObject("Emoji", typeof(Image));
        imgObj.transform.SetParent(go.transform, false);
        var img = imgObj.GetComponent<Image>();
        img.sprite = sp;
        img.preserveAspect = true;
        img.rectTransform.sizeDelta = new Vector2(size, size);
        Canvas.ForceUpdateCanvases();
        var sparent = chatContentParent.parent;
        if (sparent) { var sr = sparent.GetComponent<ScrollRect>(); if (sr) sr.verticalNormalizedPosition = 0f; }
    }

    public void SendChatMessage()
    {
        if (inputField == null || string.IsNullOrEmpty(inputField.text)) return;

        string message = inputField.text;
        inputField.text = "";
        string myName = LocalUserData.instance != null ? LocalUserData.instance.currentUserId : "";

        if (currentMode == ChatMode.Public)
        {
            AddMessageToView("[" + myName + "]: " + message);

            Dictionary<byte, object> data = new Dictionary<byte, object>
            {
                { (byte)ParameterCode.ChatMessage, message }
            };
            if (PhotonManager.instance != null && PhotonManager.instance.Peer != null)
            {
                PhotonManager.instance.Peer.SendOperation((byte)OperationCode.PublicChat, data, SendOptions.SendReliable);
                Debug.Log("[Chat] 公频发送: " + message);
            }
        }
        else
        {
            AddMessageToView("我: " + message);

            Dictionary<byte, object> data = new Dictionary<byte, object>
            {
                { (byte)ParameterCode.TargetName, currentTarget },
                { (byte)ParameterCode.ChatMessage, message }
            };
            if (PhotonManager.instance != null && PhotonManager.instance.Peer != null)
            {
                PhotonManager.instance.Peer.SendOperation((byte)OperationCode.PrivateChat, data, SendOptions.SendReliable);
                Debug.Log("[Chat] 私聊发送给 " + currentTarget + ": " + message);
            }
        }

        inputField.ActivateInputField();
    }

    // ===================== 接收 =====================

    private void OnPublicMessage(string sender, string message)
    {
        string myName = LocalUserData.instance != null ? LocalUserData.instance.currentUserId : "";
        if (sender == myName) return;

        if (currentMode == ChatMode.Public)
        {
            if (message.StartsWith("#EMO") && message.EndsWith("#"))
            {
                string num = message.Substring(4, message.Length - 5);
                int idx; if (int.TryParse(num, out idx) && idx >= 0 && emojiSprites != null && idx < emojiSprites.Length)
                { AddEmojiToView(sender, emojiSprites[idx]); return; }
            }
            AddMessageToView("[" + sender + "]: " + message);
        }
    }

    private void OnPrivateMessage(string sender, string message)
    {
        if (currentMode == ChatMode.Private && sender == currentTarget)
            AddMessageToView(sender + ": " + message);
    }

    private void AddMessageToView(string text)
    {
        if (chatContentParent == null || chatTextPrefab == null) return;

        GameObject msgObj = Instantiate(chatTextPrefab, chatContentParent);
        Text msgText = msgObj.GetComponentInChildren<Text>();
        if (msgText != null)
            msgText.text = text;

        Canvas.ForceUpdateCanvases();
        Transform scrollParent = chatContentParent.parent;
        if (scrollParent != null)
        {
            ScrollRect sr = scrollParent.GetComponent<ScrollRect>();
            if (sr != null) sr.verticalNormalizedPosition = 0f;
        }
    }

    private string GetPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
