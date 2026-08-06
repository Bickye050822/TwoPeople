using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Random = UnityEngine.Random;

public class CreatRoom : MonoBehaviourPunCallbacks
{
    private InputField roomName;
    
    private void Start()
    {
        roomName = transform.Find("RoomName/InputField").GetComponent<InputField>();
        
        transform.Find("CloseBtn").GetComponent<Button>().onClick.AddListener(() =>
        {
            UiManager.Instance.CreateRoom.gameObject.SetActive(false);
        });
        transform.Find("CreateRoomBtn").GetComponent<Button>().onClick.AddListener(() =>
        {
            UiManager.Instance.RoomMask.gameObject.SetActive(true);
            RoomOptions room = new RoomOptions();
            room.MaxPlayers = 2;
            PhotonNetwork.CreateRoom(roomName.text,room);
        });  
        roomName.text="房间"+Random.Range(0,1000);
    }
    //创建房间成功
    public override void OnCreatedRoom()
    {
        UiManager.Instance.CreateRoom.gameObject.SetActive(false);
        UiManager.Instance.RoomMask.gameObject.SetActive(false);
        UiManager.Instance.Room.gameObject.SetActive(false);
        UiManager.Instance.IsRoom.gameObject.SetActive(true);
    }
    //创建房间失败
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("创建房间失败");
        UiManager.Instance.RoomMask.gameObject.SetActive(false);
    }

  
}
