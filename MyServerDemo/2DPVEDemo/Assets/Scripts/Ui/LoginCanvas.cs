using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyCommon;
using ExitGames.Client.Photon;

public class LoginCanvas : MonoBehaviour
{
    public static LoginCanvas instance;
    private Transform register;
    private InputField userNameField, passwordField;
    private InputField rUserNameField, rPasswordField, UserNameField;
    private Button loginButton, registerButton, registerButton2, returnBtn, statrBtn;
    private Text tipsText;

    private void Start()
    {
        instance = this;
        register = transform.Find("Register");
        userNameField = transform.Find("PhoneNumber/InputField (1)").GetComponent<InputField>();
        passwordField = transform.Find("PassWord/InputField (1)").GetComponent<InputField>();
        rUserNameField = register.Find("PhoneNumber/InputField (1)").GetComponent<InputField>();
        rPasswordField = register.Find("PassWord/InputField (1)").GetComponent<InputField>();
        UserNameField = register.Find("UserName/InputField (1)").GetComponent<InputField>();
        loginButton = transform.Find("LoginBtn").GetComponent<Button>();
        registerButton = transform.Find("RegisterBtn").GetComponent<Button>();
         registerButton2 = register.Find("RegisterBtn").GetComponent<Button>();
        returnBtn = register.Find("ReturnBtn").GetComponent<Button>();
        tipsText = transform.Find("Tips").GetComponent<Text>();
        loginButton.onClick.AddListener(OnLogin);
        registerButton.onClick.AddListener(() => { register.gameObject.SetActive(true); });
        returnBtn.onClick.AddListener(() => register.gameObject.SetActive(false));
        returnBtn.onClick.AddListener(() => { register.gameObject.SetActive(false); });

        registerButton2.onClick.AddListener(OnRegister2);
    }

    private void OnLogin()
    {
        Debug.Log("OnLogin 被调用");
        string userName = userNameField.text;
        string password = passwordField.text;

        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
        {
            ShowTips("账号或密码不能为空");
            return;
        }

        ShowTips("正在登录");
        Dictionary<byte, object> data = new Dictionary<byte, object>();
        data.Add((byte)ParameterCode.PhoneNumber, userNameField.text);
        data.Add((byte)ParameterCode.PassWord, passwordField.text);

        if (PhotonManager.instance.Peer.PeerState == PeerStateValue.Connected)
        {
            PhotonManager.instance.Peer.SendOperation((byte)OperationCode.Login, data, SendOptions.SendReliable);
            Debug.Log("登录请求已发送");
        }
        else
        {
            ShowTips("未连接到服务器");
        }
    }
    private void OnRegister2()
    {
        #region 空验证

        string userName = rUserNameField.text;
        string password = rPasswordField.text;
        string captcha = UserNameField.text;
        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
        {
            ShowTips("账号或密码不能为空");
            return;
        }

        if (string.IsNullOrEmpty(captcha))
        {
            ShowTips("用户名不能为空");
            return;
        }

        #endregion

        tipsText.text = "正在注册";
        Dictionary<byte, object> data = new Dictionary<byte, object>();
        data.Add((byte)ParameterCode.PhoneNumber, rUserNameField.text);
        data.Add((byte)ParameterCode.PassWord, rPasswordField.text);
        data.Add((byte)ParameterCode.UserName, captcha);
        PhotonManager.instance.Peer.SendOperation((byte)OperationCode.Register, data, SendOptions.SendReliable);
        //新版：peer.SendOperation(byte operationCode, Dictionary<byte, object> parameters, SendOptions sendOptions)
    }
    public void OnHandleLogin(ReturnCode returnCode)
    {
        if (returnCode == ReturnCode.Success)
        {
            Debug.Log("登录成功");
            ShowTips("登录成功");
            // 保存当前登录用户信息
            LocalUserData.instance.currentUserId = userNameField.text;
            // 登录成功 → 隐藏 Login，显示 UserData 面板
            gameObject.SetActive(false);
            if (UiManager.Instance != null)
                UiManager.Instance.ShowUserDataPanel();
        }
        else
        {
            ShowTips("登录失败");
        }
    }

    public void OnHandleRegister(ReturnCode returnCode)
    {
        if (returnCode == ReturnCode.Success)
        {
            ShowTips("注册成功");
            register.gameObject.SetActive(false);
        }
        else
        {
            ShowTips("注册失败");
        }
    }

    private void ShowTips(string message)
    {
        tipsText.text = message;
        Invoke(nameof(HideTips), 1.5f);
    }

    private void HideTips()
    {
        tipsText.text = "";
    }
}