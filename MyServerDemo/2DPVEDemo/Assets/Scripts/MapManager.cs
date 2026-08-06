using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using MyCommon;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class MapManager : MonoBehaviourPunCallbacks
{
    [Header("关卡按钮")]
    public Button game1Btn;
    public Button game2Btn;

    [Header("客机等待")]
    public GameObject waitPanel;
    public Text waitText;

    [Header("结算按钮")]
    public Button settleBtn;

    [Header("结果面板(即结算面板)")]
    public GameObject resultPanel;
    public Text myName, myTime, myScore;
    public Text otherName, otherTime, otherScore;
    public Button settleConfirmBtn;
    public Button settleCancelBtn;

    private bool isMaster;
    private bool settled;

    void Start()
    {
        isMaster = PhotonNetwork.IsMasterClient;
        if (game1Btn) game1Btn.onClick.AddListener(() => StartGame("Game1"));
        if (game2Btn) game2Btn.onClick.AddListener(() => StartGame("Game2"));
        if (settleBtn) settleBtn.onClick.AddListener(() => { if (resultPanel) resultPanel.SetActive(true); });
        if (settleConfirmBtn) settleConfirmBtn.onClick.AddListener(DoSettle);
        if (settleCancelBtn) settleCancelBtn.onClick.AddListener(() => { if (resultPanel) resultPanel.SetActive(false); });

        if (game1Btn) game1Btn.interactable = isMaster;
        if (game2Btn) game2Btn.interactable = isMaster;
        if (settleBtn) settleBtn.interactable = false;
        if (waitPanel) waitPanel.SetActive(!isMaster);
        if (resultPanel) resultPanel.SetActive(false);

        // 每次进 Map 都检查双方是否完成（延迟等 CustomProperties 同步）
        Invoke(nameof(TryCheckBothDone), 0.5f);
    }

    void TryCheckBothDone()
    {
        CheckBothDone();
        Invoke(nameof(CheckSettleReady), 0.5f);
    }

    void StartGame(string scene)
    {
        if (!isMaster) return;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.LoadLevel(scene);
    }

    public override void OnPlayerPropertiesUpdate(Player target, Hashtable changed)
    {
        // 双方都完成 → 显示结果
        if (changed.ContainsKey("p1Done") || changed.ContainsKey("p2Done"))
            CheckBothDone();

        // 双方都返回地图准备结算
        if (changed.ContainsKey("p1Ready") || changed.ContainsKey("p2Ready"))
            CheckSettleReady();
    }

    void CheckBothDone()
    {
        bool p1d = false, p2d = PhotonNetwork.PlayerList.Length < 2;
        foreach (var p in PhotonNetwork.PlayerList)
        {
            object v;
            if (p.ActorNumber == 1 && p.CustomProperties.TryGetValue("p1Done", out v)) p1d = (bool)v;
            if (p.ActorNumber == 2 && p.CustomProperties.TryGetValue("p2Done", out v)) p2d = (bool)v;
        }
        if (!p1d || !p2d) return;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "p" + PhotonNetwork.LocalPlayer.ActorNumber + "Ready", true } });
    }

    void CheckSettleReady()
    {
        bool single = PhotonNetwork.PlayerList.Length < 2;
        bool p1r = GetProp<bool>("p1Ready");
        bool p2r = single || GetProp<bool>("p2Ready");
        if (p1r && p2r && !settled)
        {
            if (settleBtn && isMaster) settleBtn.interactable = true;
            // 填充结果面板
            int me = PhotonNetwork.LocalPlayer.ActorNumber;
            int other = (me == 1) ? 2 : 1;
            if (myName) myName.text = GetPlayerName(me);
            if (myTime) myTime.text = GetProp<string>("p" + me + "Time");
            if (myScore) myScore.text = GetProp<string>("p" + me + "Score");
            if (otherName) otherName.text = GetPlayerName(other);
            if (otherTime) otherTime.text = GetProp<string>("p" + other + "Time");
            if (otherScore) otherScore.text = GetProp<string>("p" + other + "Score");
        }
    }

    void DoSettle()
    {
        if (!isMaster || settled) return;
        settled = true;
        SaveToDB(1);
        SaveToDB(2);
        PhotonNetwork.LoadLevel("SampleScene");
    }

    void SaveToDB(int actor)
    {
        if (PhotonManager.instance == null || PhotonManager.instance.Peer == null) return;
        Player[] players = PhotonNetwork.PlayerList;
        Player target = null;
        foreach (var p in players) if (p.ActorNumber == actor) { target = p; break; }
        if (target == null) return;

        object phone, result, time, score;
        target.CustomProperties.TryGetValue("phone", out phone);
        target.CustomProperties.TryGetValue("p" + actor + "Result", out result);
        target.CustomProperties.TryGetValue("p" + actor + "Time", out time);
        target.CustomProperties.TryGetValue("p" + actor + "Score", out score);

        var data = new Dictionary<byte, object>
        {
            { (byte)MyCommon.ParameterCode.PhoneNumber, (string)(phone ?? "") },
            { (byte)MyCommon.ParameterCode.PassResult, (string)(result ?? "通关失败") },
            { (byte)MyCommon.ParameterCode.PassTime, (string)(time ?? "00:00") },
            { (byte)MyCommon.ParameterCode.PassScore, (string)(score ?? "0") }
        };
        PhotonManager.instance.Peer.SendOperation((byte)MyCommon.OperationCode.UpdateGameResult, data, SendOptions.SendReliable);
    }

    T GetProp<T>(string key)
    {
        foreach (var p in PhotonNetwork.PlayerList)
            if (p.CustomProperties.ContainsKey(key)) return (T)p.CustomProperties[key];
        return default;
    }

    string GetPlayerName(int actor)
    {
        foreach (var p in PhotonNetwork.PlayerList)
            if (p.ActorNumber == actor) return p.NickName;
        return "P" + actor;
    }
}
