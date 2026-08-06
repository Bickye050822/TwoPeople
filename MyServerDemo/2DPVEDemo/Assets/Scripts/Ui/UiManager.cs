using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class UiManager : MonoBehaviour, IConnectionCallbacks
{
    public Transform Login, Register, Load, Room,CreateRoom,RoomMask,IsRoom,UserDataPanel;
    public Transform UserName;
    public static UiManager Instance;
    void Start()
    {
        Instance = this;
        Login = transform.Find("Login");
        Load = transform.Find("Load");
        Room = transform.Find("Room");
        IsRoom = transform.Find("IsRoom");
        CreateRoom = Room.transform.Find("CreateRoom");
        RoomMask = Room.transform.Find("RoomMask");
        UserDataPanel = transform.Find("UserData");
        UserName = transform.Find("CurrentUserName");
        
    }

    public void UpdateUserName(string name)
    {
        if (UserName != null)
        {
            UserName.gameObject.SetActive(true);
            UserName.GetChild(0).GetComponent<Text>().text = name;
        }
    }

    public void GoLobby()
    {
        Load.gameObject.SetActive(true);
        LoginLobby();
    }
    void LoginLobby()
    {
        PhotonNetwork.SendRate = 120;
        PhotonNetwork.SerializationRate = 120;

        // 连接前设昵称
        string nick = LocalUserData.instance != null ? LocalUserData.instance.currentUserName : "";
        if (!string.IsNullOrEmpty(nick))
            PhotonNetwork.LocalPlayer.NickName = nick;

        //连接到主服务器
        PhotonNetwork.ConnectUsingSettings();

    }
    void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);//注册事件
    }

    void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);//注销事件
    }

    public void OnConnected()
    {
    }
    //连接成功时调用
    public void OnConnectedToMaster()
    {
        Load.gameObject.SetActive(false);
        Room.gameObject.SetActive(true);
        UserName.gameObject.SetActive(true);
        UserName.GetChild(0).GetComponent<Text>().text = LocalUserData.instance.currentUserName;
    }

    /// <summary>
    /// 登录成功后由 LoginCanvas 调用，切换到 UserData 面板
    /// </summary>
    public void ShowUserDataPanel()
    {
        Login.gameObject.SetActive(false);
        if (UserDataPanel != null)
            UserDataPanel.gameObject.SetActive(true);
    }

     //断开连接时调用
    public void OnDisconnected(DisconnectCause cause)
    {
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {
        throw new System.NotImplementedException();
    }
}