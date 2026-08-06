using System;
using System.Collections;
using System.Collections.Generic;
using MyCommon;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.UI;

public class PhotonManager : MonoBehaviour, IPhotonPeerListener
{
    public static PhotonManager instance;
    private PhotonPeer peer;

    public PhotonPeer Peer
    {
        get => peer;
        set => peer = value;
    }

    private void Awake()
    {
        #region 单例

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        #endregion

        peer = new PhotonPeer(this, ConnectionProtocol.Udp);
        peer.Connect("127.0.0.1:5822", "TwoDDemoApp");

   
    }

    private void Update()
    {
        if (peer != null) peer.Service();
    }

    private void OnDestroy()
    {
        if (peer != null) peer.Disconnect();
    }

    /// <summary>
    /// 收到服务器的debug时执行
    /// </summary>
    public void DebugReturn(DebugLevel level, string message)
    {
        Debug.Log($"[{level}] {message}");
    }

    /// <summary>
    /// 收到服务器的推送事件时执行
    /// </summary>
    public void OnEvent(EventData eventData)
    {
        Debug.Log($"收到服务器事件，事件码: {eventData.Code}");
        switch (eventData.Code)
        {
            case (byte)EventCode.PublicChatEvent:
                if (eventData.Parameters.TryGetValue((byte)ParameterCode.ChatMessage, out object pubMsg))
                {
                    ChatManager.instance?.OnReceivePublicChat((string)pubMsg);
                }
                break;
            case (byte)EventCode.PrivateChatEvent:
                if (eventData.Parameters.TryGetValue((byte)ParameterCode.ChatMessage, out object privMsg))
                {
                    ChatManager.instance?.OnReceivePrivateChat((string)privMsg);
                }
                break;
            case (byte)EventCode.OnlineUsersEvent:
                if (eventData.Parameters.TryGetValue((byte)ParameterCode.ChatMessage, out object userList))
                {
                    ChatManager.instance?.OnOnlineUsersUpdated((string)userList);
                }
                break;

            // ===== 游戏同步 =====
            case (byte)EventCode.PlayerPosEvent:
                PlayerManager.GetRemotePlayer()?.ApplyRemotePosition(eventData.Parameters);
                break;
            case (byte)EventCode.PlayerAttackEvent:
                if (eventData.Parameters.TryGetValue((byte)ParameterCode.ComboIndex, out object comboIdx))
                    PlayerManager.GetRemotePlayer()?.PlayRemoteAttack((int)comboIdx);
                break;
            case (byte)EventCode.PlayerAttackedEvent:
                if (PlayerManager.instance != null)
                    PlayerManager.instance.TakeDamage(10f);
                break;
            case (byte)EventCode.PlayerDieEvent:
                PlayerManager.GetRemotePlayer()?.Die();
                break;
            case (byte)EventCode.EnemySyncEvent:
                Enemy.ApplyEnemySync(eventData.Parameters);
                break;
            case (byte)EventCode.SkillEffectEvent:
                if (eventData.Parameters.TryGetValue((byte)ParameterCode.ComboIndex, out object type))
                    PlayerManager.GetRemotePlayer()?.PlayRemoteSkill((int)type);
                break;

            default:
                Debug.Log($"未处理的事件码: {eventData.Code}");
                break;
        }
    }

    /// <summary>
    /// 收到来自客户端的请求数据
    /// 获得操作码
    /// operationRequest.OperationCode
    /// 获得操作参数
    /// operationRequest.Parameters
    /// </summary>
    /// <param name="operationResponse"></param>
    public void OnOperationResponse(OperationResponse operationResponse)
    {
        Debug.Log(
            $"⚡ OnOperationResponse 被触发！操作码: {operationResponse.OperationCode}, 返回码: {operationResponse.ReturnCode}");
        switch (operationResponse.OperationCode)
        {
            case (byte)OperationCode.Login:
                Debug.Log($"收到登录响应，返回码: {operationResponse.ReturnCode}");
                // 提取服务器返回的用户名
                if (operationResponse.Parameters != null &&
                    operationResponse.Parameters.TryGetValue((byte)ParameterCode.UserName, out object userName))
                {
                    LocalUserData.instance.currentUserName = (string)userName;
                    UiManager.Instance?.UpdateUserName((string)userName);
                }
                LoginCanvas.instance.OnHandleLogin((ReturnCode)operationResponse.ReturnCode);
                break;
            case (byte)OperationCode.Register:
                Debug.Log($"处理注册响应，返回码: {operationResponse.ReturnCode}");
                if (LoginCanvas.instance != null)
                    LoginCanvas.instance.OnHandleRegister((ReturnCode)operationResponse.ReturnCode);
                break;
            case (byte)OperationCode.ChangePassword:
                Debug.Log($"处理修改密码响应，返回码: {operationResponse.ReturnCode}");
                if (UserDataPanel.instance != null)
                    UserDataPanel.instance.OnHandleChangePassword((ReturnCode)operationResponse.ReturnCode);
                else
                    Debug.LogError("UserDataPanel.instance 为 null，无法处理修改密码响应");
                break;
            case (byte)OperationCode.DeleteAccount:
                Debug.Log($"处理注销响应，返回码: {operationResponse.ReturnCode}");
                if (UserDataPanel.instance != null)
                    UserDataPanel.instance.OnHandleDeleteAccount((ReturnCode)operationResponse.ReturnCode);
                else
                    Debug.LogError("UserDataPanel.instance 为 null，无法处理注销响应");
                break;
            case (byte)OperationCode.PublicChat:
                Debug.Log($"处理公频响应，返回码: {operationResponse.ReturnCode}");
                break;
            case (byte)OperationCode.PrivateChat:
                Debug.Log($"处理私聊响应，返回码: {operationResponse.ReturnCode}");
                break;
            case (byte)OperationCode.RefreshOnlineUsers:
                Debug.Log($"处理刷新在线用户响应，返回码: {operationResponse.ReturnCode}");
                break;
            case (byte)OperationCode.UpdateGameResult:
                Debug.Log($"游戏结果已保存，返回码: {operationResponse.ReturnCode}");
                break;
            default:
                Debug.Log($"未处理的操作码: {operationResponse.OperationCode}");
                break;
        }
    }

    /// <summary>
    /// 连接状态改变时执行
    /// </summary>
    public void OnStatusChanged(StatusCode statusCode)
    {
        Debug.Log($"状态改变: {statusCode}");
        if (statusCode == StatusCode.Connect)
        {
            Debug.Log("✅ 服务器连接成功！");
        }
        else if (statusCode == StatusCode.Disconnect)
        {
            Debug.LogError("❌ 与服务器断开连接！");
        }
    }
}