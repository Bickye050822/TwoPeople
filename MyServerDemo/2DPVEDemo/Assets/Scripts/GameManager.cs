using System.Collections;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;
    private GameOverPanel overPanel;

    public void Awake() { instance = this; }

    void Start()
    {
        overPanel = FindObjectOfType<GameOverPanel>(true);
        if (overPanel) overPanel.gameObject.SetActive(false);

        if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
            PhotonNetwork.Instantiate("Player", new Vector3(-15, 1, 0), Quaternion.identity);
        else if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
            PhotonNetwork.Instantiate("Player2", new Vector3(-12, 1, 0), Quaternion.identity);
        else
            PhotonNetwork.Instantiate("Player", new Vector3(-15, 1, 0), Quaternion.identity);
    }

    public void GameOver(string result = "通关失败", string time = "00:00", string score = "0")
    {
        string userName = LocalUserData.instance ? LocalUserData.instance.currentUserId : "";
        overPanel?.Show(result, time, score, userName);

        // 设置 CustomProperties 标记完成 + 存结果
        int actor = PhotonNetwork.LocalPlayer.ActorNumber;
        var props = new Hashtable
        {
            { "p" + actor + "Done", true },
            { "p" + actor + "Result", result },
            { "p" + actor + "Time", time },
            { "p" + actor + "Score", score }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        Debug.Log("[GameOver] Saved: actor=" + actor + " result=" + result + " time=" + time + " score=" + score);

        // 检查双方是否都完成
        StartCoroutine(WaitBothDone());
    }

    IEnumerator WaitBothDone()
    {
        yield return new WaitForSeconds(1f);
        if (!PhotonNetwork.IsMasterClient) yield break;
        bool isSingle = PhotonNetwork.PlayerList.Length < 2;
        while (true)
        {
            bool p1d = false, p2d = isSingle; // 单人模式 p2 直接算完成
            foreach (var p in PhotonNetwork.PlayerList)
            {
                object val;
                if (p.ActorNumber == 1 && p.CustomProperties.TryGetValue("p1Done", out val)) p1d = (bool)val;
                if (p.ActorNumber == 2 && p.CustomProperties.TryGetValue("p2Done", out val)) p2d = (bool)val;
            }
            if (p1d && p2d) { PhotonNetwork.LoadLevel("Map"); yield break; }
            yield return new WaitForSeconds(1f);
        }
    }
}
