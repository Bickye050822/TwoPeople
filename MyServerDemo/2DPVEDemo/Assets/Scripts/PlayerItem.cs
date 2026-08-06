using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerItem : MonoBehaviour
{
    public int playerId;
    public bool isReady = false;

    void Start()
    {
        if (playerId == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            transform.Find("ReadyBtn").GetComponent<Toggle>().onValueChanged.AddListener((value) =>
            {
                isReady = value;
                ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
                hashtable.Add("isReady", isReady);
                PhotonNetwork.SetPlayerCustomProperties(hashtable); //在服务器端设置属性 （本地玩家
                ChangeReady(isReady);
            });
        }
        else
        {
            transform.Find("ReadyBtn").GetComponent<Image>().color = Color.black;
        }
        ChangeReady(isReady);
    }

    public void ChangeReady(bool isReady)
    {
        transform.Find("ReadyBtn").GetComponent<Toggle>().isOn = isReady;
    }
}