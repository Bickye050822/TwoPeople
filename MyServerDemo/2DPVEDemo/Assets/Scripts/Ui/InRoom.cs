using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;


public class InRoom : MonoBehaviour, IInRoomCallbacks
{
    private Button StartBtn;
    private Transform PlayerItem;
    private List<PlayerItem> MyPlayerList = new List<PlayerItem>();
    GameObject PlayerPrefab;

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PlayerItem = transform.Find("PlayerList");
        PlayerPrefab = Resources.Load<GameObject>("Ui/PlayerItem");
        StartBtn = transform.Find("StartBtn").GetComponent<Button>();
        StartBtn.onClick.AddListener(() =>
        {
            // 设置手机号到 CustomProperties 供 Map 结算用
            string phone = LocalUserData.instance ? LocalUserData.instance.currentUserId : "";
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "phone", phone } });
            PhotonNetwork.LoadLevel("Map");
        });
        transform.Find("CloseBtn").GetComponent<Button>().onClick.AddListener(() =>
        {
            PhotonNetwork.Disconnect();
            UiManager.Instance.IsRoom.gameObject.SetActive(true);
            UiManager.Instance.Room.gameObject.SetActive(false);
        });


        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Player p = PhotonNetwork.PlayerList[i];
            CreatePlayerItem(p);
        }
    }

    public void CreatePlayerItem(Player player)
    {
        GameObject go = Instantiate(PlayerPrefab, PlayerItem);
        PlayerItem item = go.AddComponent<PlayerItem>();
        item.playerId = player.ActorNumber;

        // 设置玩家名字
        Text userNameText = go.transform.Find("UserName")?.GetComponent<Text>();
        if (userNameText != null)
            userNameText.text = player.NickName;

        MyPlayerList.Add(item);

        object val;
        if (player.CustomProperties.TryGetValue("isReady", out val))
        {
            item.isReady = (bool)val;
        }
    }

    public void DeletePlayerItem(Player player)
    {
        PlayerItem item = MyPlayerList.Find(x => x.playerId == player.ActorNumber);
        if (item != null)
        {
            MyPlayerList.Remove(item);
            Destroy(item.gameObject);
        }
    }

    public void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this); //注册事件
    }

    public void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this); //注销事件
    }

    //新玩家进入房间时调用
    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        CreatePlayerItem(newPlayer);
    }

    //玩家离开房间时调用
    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        DeletePlayerItem(otherPlayer);
    }

    //房间属性更新时调用
    public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
    }

    //玩家属性更新时调用
    public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        PlayerItem item = MyPlayerList.Find((item) => { return item.playerId == targetPlayer.ActorNumber; });
        if (item != null)
        {
            object readyValue;
            if (changedProps.TryGetValue("isReady", out readyValue))
            {
                item.isReady = (bool)readyValue; // 只有存在时才转换
                item.ChangeReady(item.isReady);
            }
        }

        if (PhotonNetwork.IsMasterClient)
        {
            bool isAllReady = true;
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                if (MyPlayerList[i].isReady == false)
                {
                    isAllReady = false;
                    break;
                }
            }

            StartBtn.gameObject.SetActive(isAllReady);
        }
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
        throw new NotImplementedException();
    }
}