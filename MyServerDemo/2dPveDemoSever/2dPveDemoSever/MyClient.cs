using _2dPveDemoSever.DOA;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using MyCommon;
namespace _2dPveDemoSever;

public class MyClient:ClientPeer
{
    DataManager dataManager=new DataManager();
    public string currentPhoneNum= "";
    public MyClient(InitRequest initRequest) : base(initRequest)
    {
    }

    protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
    {
        switch (operationRequest.OperationCode)
        {
            case (byte)OperationCode.Login:
                OnHandLogin(operationRequest, sendParameters);
                break;
            case (byte)OperationCode.Register:
                OnHandRegister(operationRequest, sendParameters);
                break;
            case (byte)OperationCode.ChangePassword:
                OnHandChangePassword(operationRequest, sendParameters);
                break;
            case (byte)OperationCode.DeleteAccount:
                OnHandDeleteAccount(operationRequest, sendParameters);
                break;
            case (byte)OperationCode.PublicChat:
                OnHandPublicChat(operationRequest, sendParameters);
                break;
            case (byte)OperationCode.PrivateChat:
                OnHandPrivateChat(operationRequest, sendParameters);
                break;
            case (byte)OperationCode.RefreshOnlineUsers:
                OnHandRefreshOnlineUsers(operationRequest, sendParameters);
                break;
            case (byte)OperationCode.UpdateGameResult:
                OnHandUpdateGameResult(operationRequest, sendParameters);
                break;
            // 游戏同步
            case (byte)OperationCode.PlayerPosition:
                GameController.HandlePlayerPosition(this, operationRequest.Parameters);
                break;
            case (byte)OperationCode.PlayerAttack:
                GameController.HandlePlayerAttack(this, operationRequest.Parameters);
                break;
            case (byte)OperationCode.PlayerAttacked:
                GameController.HandlePlayerAttacked(this, operationRequest.Parameters);
                break;
            case (byte)OperationCode.PlayerDie:
                GameController.HandlePlayerDie(this);
                break;
            case (byte)OperationCode.EnemyAttacked:
                GameController.HandleEnemyAttacked(this, operationRequest.Parameters);
                break;
            case (byte)OperationCode.EnemyDie:
                GameController.HandleEnemyDie(this, operationRequest.Parameters);
                break;
            case (byte)OperationCode.WaveStart:
                GameController.HandleWaveStart(this, operationRequest.Parameters);
                break;
            case (byte)OperationCode.SyncGameState:
                GameController.SendSyncState(this);
                break;
            case (byte)OperationCode.EnemySync:
                GameController.HandleEnemySync(this, operationRequest.Parameters);
                break;
            case (byte)OperationCode.SkillEffect:
                GameController.HandleSkillEffect(this, operationRequest.Parameters);
                break;
            default:
                MyServer.log.Info("没有此操作码");
                break;
        }

    }
    public void OnHandLogin(OperationRequest operationRequest, SendParameters sendParameters)
    {
        
        object PhonerNum, PassWord;
        Dictionary<byte, object> parameters = operationRequest.Parameters;
        parameters.TryGetValue((byte)ParameterCode.PhoneNumber, out PhonerNum);
        parameters.TryGetValue((byte)ParameterCode.PassWord, out PassWord);
        
        OperationResponse response = new OperationResponse((byte)OperationCode.Login);
        // foreach (MyClient client in MyServer.clients)
        // {
        //     if (PhonerNum.ToString().Equals(client.currentPhoneNum))
        //     {
        //         response.OperationCode = (byte)OperationCode.Login;
        //     }
        // }

        if (dataManager.VerifyUser(int.Parse((string)PhonerNum), (string)PassWord))
        {
            MyServer.log.Info("用户登录成功");
            this.currentPhoneNum = (string)PhonerNum;
            string userName = dataManager.GetUserName(int.Parse((string)PhonerNum));
            response.ReturnCode = (short)ReturnCode.Success;
            response.Parameters = new Dictionary<byte, object>
            {
                { (byte)ParameterCode.UserName, userName }
            };
            this.SendOperationResponse(response, sendParameters);
            MyServer.log.Info($"已发送登录成功响应，返回码: {response.ReturnCode}");
            // 登录成功 → 广播在线用户列表给所有人
            ChatController.BroadcastOnlineUsers();
        }
        else
        {
            MyServer.log.Info("用户登录失败");
            response.ReturnCode = (short)ReturnCode.Fail;
            this.SendOperationResponse(response, sendParameters);
        }

    }
    public void OnHandRegister(OperationRequest operationRequest, SendParameters sendParameters)
    {

        object PhonerNum, PassWord, UserName;
        Dictionary<byte, object> parameters = operationRequest.Parameters;
        parameters.TryGetValue((byte)ParameterCode.PhoneNumber, out PhonerNum);
        parameters.TryGetValue((byte)ParameterCode.PassWord, out PassWord);
        parameters.TryGetValue((byte)ParameterCode.UserName, out UserName);

        string userNameStr = (UserName != null) ? (string)UserName : ((string)PhonerNum);

        OperationResponse response = new OperationResponse((byte)OperationCode.Register);

        if (dataManager.Register(int.Parse((string)PhonerNum), (string)PassWord, userNameStr))
        {
            MyServer.log.Info("用户注册成功");
            response.ReturnCode = (short)ReturnCode.Success;
            this.SendOperationResponse(response, sendParameters);
        }
        else
        {
            MyServer.log.Info("用户注册失败");
            response.ReturnCode = (short)ReturnCode.Fail;
            this.SendOperationResponse(response, sendParameters);
        }

    }
    public void OnHandChangePassword(OperationRequest operationRequest, SendParameters sendParameters)
    {
        object PhonerNum, PassWord, NewPassWord;
        Dictionary<byte, object> parameters = operationRequest.Parameters;
        parameters.TryGetValue((byte)ParameterCode.PhoneNumber, out PhonerNum);
        parameters.TryGetValue((byte)ParameterCode.PassWord, out PassWord);
        parameters.TryGetValue((byte)ParameterCode.NewPassWord, out NewPassWord);

        OperationResponse response = new OperationResponse((byte)OperationCode.ChangePassword);

        if (dataManager.ChangePassword(int.Parse((string)PhonerNum), (string)PassWord, (string)NewPassWord))
        {
            MyServer.log.Info("用户修改密码成功");
            response.ReturnCode = (short)ReturnCode.Success;
            this.SendOperationResponse(response, sendParameters);
        }
        else
        {
            MyServer.log.Info("用户修改密码失败");
            response.ReturnCode = (short)ReturnCode.Fail;
            this.SendOperationResponse(response, sendParameters);
        }
    }

    public void OnHandDeleteAccount(OperationRequest operationRequest, SendParameters sendParameters)
    {
        object PhonerNum, PassWord;
        Dictionary<byte, object> parameters = operationRequest.Parameters;
        parameters.TryGetValue((byte)ParameterCode.PhoneNumber, out PhonerNum);
        parameters.TryGetValue((byte)ParameterCode.PassWord, out PassWord);

        OperationResponse response = new OperationResponse((byte)OperationCode.DeleteAccount);

        if (dataManager.DeleteUser(int.Parse((string)PhonerNum), (string)PassWord))
        {
            MyServer.log.Info("用户注销成功");
            this.currentPhoneNum = "";
            response.ReturnCode = (short)ReturnCode.Success;
            this.SendOperationResponse(response, sendParameters);
        }
        else
        {
            MyServer.log.Info("用户注销失败");
            response.ReturnCode = (short)ReturnCode.Fail;
            this.SendOperationResponse(response, sendParameters);
        }
    }

    protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
    {
        MyServer.log.Info("有一个客户端断开了连接");
        MyServer.clients.Remove(this);
        MyServer.log.Info("当前在线人数: " + MyServer.clients.Count);
        // 断线后广播在线用户列表更新
        ChatController.BroadcastOnlineUsers();
    }

    #region 聊天

    public void OnHandPublicChat(OperationRequest operationRequest, SendParameters sendParameters)
    {
        object message;
        operationRequest.Parameters.TryGetValue((byte)ParameterCode.ChatMessage, out message);

        ChatController.HandlePublicChat(this, (string)message);

        // 发送确认响应
        OperationResponse response = new OperationResponse((byte)OperationCode.PublicChat);
        response.ReturnCode = (short)ReturnCode.Success;
        this.SendOperationResponse(response, sendParameters);
    }

    public void OnHandPrivateChat(OperationRequest operationRequest, SendParameters sendParameters)
    {
        object targetName, message;
        operationRequest.Parameters.TryGetValue((byte)ParameterCode.TargetName, out targetName);
        operationRequest.Parameters.TryGetValue((byte)ParameterCode.ChatMessage, out message);

        OperationResponse response = new OperationResponse((byte)OperationCode.PrivateChat);

        if (ChatController.HandlePrivateChat(this, (string)targetName, (string)message))
        {
            response.ReturnCode = (short)ReturnCode.Success;
        }
        else
        {
            response.ReturnCode = (short)ReturnCode.Fail;
        }
        this.SendOperationResponse(response, sendParameters);
    }

    public void OnHandRefreshOnlineUsers(OperationRequest operationRequest, SendParameters sendParameters)
    {
        ChatController.SendOnlineUsers(this);

        OperationResponse response = new OperationResponse((byte)OperationCode.RefreshOnlineUsers);
        response.ReturnCode = (short)ReturnCode.Success;
        this.SendOperationResponse(response, sendParameters);
    }

    #endregion

    #region 更新游戏结果

    public void OnHandUpdateGameResult(OperationRequest operationRequest, SendParameters sendParameters)
    {
        object PhonerNum, passResult, passTime, passScore;
        operationRequest.Parameters.TryGetValue((byte)ParameterCode.PhoneNumber, out PhonerNum);
        operationRequest.Parameters.TryGetValue((byte)ParameterCode.PassResult, out passResult);
        operationRequest.Parameters.TryGetValue((byte)ParameterCode.PassTime, out passTime);
        operationRequest.Parameters.TryGetValue((byte)ParameterCode.PassScore, out passScore);

        int score = 0;
        if (passScore != null) int.TryParse((string)passScore, out score);

        OperationResponse response = new OperationResponse((byte)OperationCode.UpdateGameResult);

        if (dataManager.UpdateGameResult(int.Parse((string)PhonerNum), (string)passResult, (string)passTime, score))
        {
            MyServer.log.Info("游戏结果更新成功");
            response.ReturnCode = (short)ReturnCode.Success;
        }
        else
        {
            MyServer.log.Info("游戏结果更新失败");
            response.ReturnCode = (short)ReturnCode.Fail;
        }
        this.SendOperationResponse(response, sendParameters);
    }

    #endregion
}