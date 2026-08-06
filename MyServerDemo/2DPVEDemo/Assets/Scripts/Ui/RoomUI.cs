using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class RoomUI : MonoBehaviourPunCallbacks
{
    TypedLobby lobby; //大厅

    private Transform RoomList;
    private GameObject RoomItem;

    void Start()
    {
        lobby = new TypedLobby("Room", LobbyType.SqlLobby);
        RoomList = transform.Find("RoomList");
        RoomItem = Resources.Load<GameObject>("Ui/RoomItem");
        PhotonNetwork.JoinLobby(lobby);
        transform.Find("CloseBtn").GetComponent<Button>().onClick.AddListener(() =>
        {
            PhotonNetwork.Disconnect();
            UiManager.Instance.Login.gameObject.SetActive(true);
            UiManager.Instance.Room.gameObject.SetActive(false);
        });
        transform.Find("CreateRoomBtn").GetComponent<Button>().onClick.AddListener(() =>
        {
            UiManager.Instance.CreateRoom.gameObject.SetActive(true);
        });
        transform.Find("UpDateBtn").GetComponent<Button>().onClick.AddListener(() =>
        {
            PhotonNetwork.GetCustomRoomList(lobby, "1"); //获取房间列表,参数1为lobby,参数2为查询条件 
        });
    }

    private void ClearRoomList()
    {
        while (RoomList.childCount != 0)
        {
            Destroy(RoomList.GetChild(0).gameObject);
        }
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("进入大厅成功");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("房间列表更新");
        ClearRoomList();
        for (int i = 0; i < roomList.Count; i++)
        {
            GameObject obj = Instantiate(RoomItem, RoomList);
            obj.SetActive(true);
            string roomName = roomList[i].Name; //房间名称
            obj.transform.Find("RoomName").GetComponent<Text>().text = roomName;
            obj.transform.Find("JoinRoomBtn").GetComponent<Button>().onClick.AddListener(() =>
            {
                PhotonNetwork.JoinRoom(roomName);
            });
        }
    }

    public override void OnJoinedRoom()
    {
        UiManager.Instance.Room.gameObject.SetActive(false);
        UiManager.Instance.IsRoom.gameObject.SetActive(true);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("加入房间失败");
    }
}