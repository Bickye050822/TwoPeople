using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyCommon;
using ExitGames.Client.Photon;

public class UserDataPanel : MonoBehaviour
{
    public static UserDataPanel instance;

    // 主面板按钮
    private Button modifyBtn, deleteBtn, enterLobbyBtn, enterChatBtn;

    // 修改密码子面板
    private Transform changePwdPanel;
    private InputField cpOldPwdField, cpNewPwdField, cpConfirmPwdField;
    private Button cpSubmitBtn, cpReturnBtn;

    // 注销子面板
    private Transform deleteAccountPanel;
    private InputField daPhoneField, daPwdField;
    private Button daSubmitBtn, daReturnBtn;

    private Text tipsText;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        tipsText = transform.Find("Tips")?.GetComponent<Text>();

        // --- 主面板按钮 ---
        modifyBtn = transform.Find("ModifyBtn")?.GetComponent<Button>();
        deleteBtn = transform.Find("DeleteBtn")?.GetComponent<Button>();
        enterLobbyBtn = transform.Find("EnterLobbyBtn")?.GetComponent<Button>();

        modifyBtn?.onClick.AddListener(ShowChangePwd);
        deleteBtn?.onClick.AddListener(ShowDeleteAccount);
        
        enterLobbyBtn?.onClick.AddListener(() =>
        {
            this.gameObject.SetActive(false);
            UiManager.Instance.GoLobby();    
        });
        

        enterChatBtn = transform.Find("EnterChatBtn")?.GetComponent<Button>();
        enterChatBtn?.onClick.AddListener(() =>
        {
            if (ChatManager.instance != null)
                ChatManager.instance.ToggleChat();
        });

        // --- 修改密码子面板 ---
        changePwdPanel = transform.Find("ChangePwd");
        if (changePwdPanel != null)
        {
            cpOldPwdField = changePwdPanel.Find("OldPwd/InputField (1)")?.GetComponent<InputField>();
            cpNewPwdField = changePwdPanel.Find("NewPwd/InputField (1)")?.GetComponent<InputField>();
            cpConfirmPwdField = changePwdPanel.Find("ConfirmPwd/InputField (1)")?.GetComponent<InputField>();
            cpSubmitBtn = changePwdPanel.Find("SubmitBtn")?.GetComponent<Button>();
            cpReturnBtn = changePwdPanel.Find("ReturnBtn")?.GetComponent<Button>();
            cpSubmitBtn?.onClick.AddListener(OnChangePassword);
            cpReturnBtn?.onClick.AddListener(HideChangePwd);
        }

        // --- 注销子面板 ---
        deleteAccountPanel = transform.Find("DeleteAccount");
        if (deleteAccountPanel != null)
        {
            daPhoneField = deleteAccountPanel.Find("PhoneNumber/InputField (1)")?.GetComponent<InputField>();
            daPwdField = deleteAccountPanel.Find("PassWord/InputField (1)")?.GetComponent<InputField>();
            daSubmitBtn = deleteAccountPanel.Find("SubmitBtn")?.GetComponent<Button>();
            daReturnBtn = deleteAccountPanel.Find("ReturnBtn")?.GetComponent<Button>();
            daSubmitBtn?.onClick.AddListener(OnDeleteAccount);
            daReturnBtn?.onClick.AddListener(HideDeleteAccount);
        }
    }

    #region 子面板显隐

    public void ShowChangePwd()
    {
        if (changePwdPanel != null) changePwdPanel.gameObject.SetActive(true);
    }

    private void HideChangePwd()
    {
        if (changePwdPanel != null) changePwdPanel.gameObject.SetActive(false);
    }

    public void ShowDeleteAccount()
    {
        if (deleteAccountPanel != null) deleteAccountPanel.gameObject.SetActive(true);
    }

    private void HideDeleteAccount()
    {
        if (deleteAccountPanel != null) deleteAccountPanel.gameObject.SetActive(false);
    }

    #endregion

    #region 修改密码

    private void OnChangePassword()
    {
        string oldPwd = cpOldPwdField?.text;
        string newPwd = cpNewPwdField?.text;
        string confirmPwd = cpConfirmPwdField?.text;

        if (string.IsNullOrEmpty(oldPwd) || string.IsNullOrEmpty(newPwd) || string.IsNullOrEmpty(confirmPwd))
        {
            ShowTips("密码不能为空");
            return;
        }
        if (newPwd != confirmPwd)
        {
            ShowTips("两次输入的新密码不一致");
            return;
        }
        if (newPwd == oldPwd)
        {
            ShowTips("新密码不能与旧密码相同");
            return;
        }

        ShowTips("正在修改密码");
        Dictionary<byte, object> data = new Dictionary<byte, object>();
        data.Add((byte)ParameterCode.PhoneNumber, LocalUserData.instance.currentUserId);
        data.Add((byte)ParameterCode.PassWord, oldPwd);
        data.Add((byte)ParameterCode.NewPassWord, newPwd);
        PhotonManager.instance.Peer.SendOperation((byte)OperationCode.ChangePassword, data, SendOptions.SendReliable);
    }

    public void OnHandleChangePassword(ReturnCode returnCode)
    {
        if (returnCode == ReturnCode.Success)
        {
            ShowTips("密码修改成功");
            HideChangePwd();
        }
        else
        {
            ShowTips("密码修改失败，请检查旧密码是否正确");
        }
    }

    #endregion

    #region 注销账号

    private void OnDeleteAccount()
    {
        string phoneNum = daPhoneField?.text;
        string password = daPwdField?.text;

        if (string.IsNullOrEmpty(phoneNum) || string.IsNullOrEmpty(password))
        {
            ShowTips("账号和密码不能为空");
            return;
        }

        ShowTips("正在注销账号");
        Dictionary<byte, object> data = new Dictionary<byte, object>();
        data.Add((byte)ParameterCode.PhoneNumber, phoneNum);
        data.Add((byte)ParameterCode.PassWord, password);
        PhotonManager.instance.Peer.SendOperation((byte)OperationCode.DeleteAccount, data, SendOptions.SendReliable);
    }

    public void OnHandleDeleteAccount(ReturnCode returnCode)
    {
        if (returnCode == ReturnCode.Success)
        {
            ShowTips("账号已注销");
            HideDeleteAccount();
        }
        else
        {
            ShowTips("注销失败，请检查账号密码是否正确");
        }
    }

    #endregion

    #region 提示

    private void ShowTips(string message)
    {
        if (tipsText != null)
        {
            tipsText.text = message;
            Invoke(nameof(HideTips), 1.5f);
        }
    }

    private void HideTips()
    {
        if (tipsText != null)
            tipsText.text = "";
    }

    #endregion
}
