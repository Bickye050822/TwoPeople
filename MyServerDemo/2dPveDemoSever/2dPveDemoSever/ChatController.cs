using System.Collections.Generic;
using System.Text;
using MyCommon;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;

namespace _2dPveDemoSever
{
    public static class ChatController
    {
        /// <summary>
        /// 发送公频消息给所有已登录客户端（除了发送者自己）
        /// </summary>
        public static void HandlePublicChat(MyClient sender, string message)
        {
            string senderName = GetDisplayName(sender);
            string payload = senderName + "|" + message;

            EventData eventData = new EventData((byte)EventCode.PublicChatEvent);
            eventData.Parameters = new Dictionary<byte, object>
            {
                { (byte)ParameterCode.ChatMessage, payload }
            };

            SendParameters sendParams = new SendParameters();
            foreach (var client in MyServer.clients)
            {
                if (!string.IsNullOrEmpty(client.currentPhoneNum))
                {
                    client.SendEvent(eventData, sendParams);
                }
            }

            MyServer.log.Info($"公频消息 [{senderName}]: {message}");
        }

        /// <summary>
        /// 发送私聊消息给指定目标客户端
        /// </summary>
        public static bool HandlePrivateChat(MyClient sender, string targetName, string message)
        {
            string senderName = GetDisplayName(sender);

            // 查找目标客户端
            MyClient target = null;
            foreach (var client in MyServer.clients)
            {
                if (GetDisplayName(client) == targetName)
                {
                    target = client;
                    break;
                }
            }

            if (target == null)
            {
                MyServer.log.Info($"私聊失败：目标用户 {targetName} 未找到");
                return false;
            }

            string payload = senderName + "|" + message;
            EventData eventData = new EventData((byte)EventCode.PrivateChatEvent);
            eventData.Parameters = new Dictionary<byte, object>
            {
                { (byte)ParameterCode.ChatMessage, payload }
            };

            SendParameters sendParams = new SendParameters();
            target.SendEvent(eventData, sendParams);

            MyServer.log.Info($"私聊消息 [{senderName}] -> [{targetName}]: {message}");
            return true;
        }

        /// <summary>
        /// 获取在线用户列表
        /// </summary>
        public static void SendOnlineUsers(MyClient requester)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var client in MyServer.clients)
            {
                string name = GetDisplayName(client);
                if (!string.IsNullOrEmpty(name))
                {
                    if (sb.Length > 0) sb.Append("*");
                    sb.Append(name);
                }
            }

            EventData eventData = new EventData((byte)EventCode.OnlineUsersEvent);
            eventData.Parameters = new Dictionary<byte, object>
            {
                { (byte)ParameterCode.ChatMessage, sb.ToString() }
            };

            SendParameters sendParams = new SendParameters();
            requester.SendEvent(eventData, sendParams);
        }

        /// <summary>
        /// 广播在线用户列表给所有客户端
        /// </summary>
        public static void BroadcastOnlineUsers()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var client in MyServer.clients)
            {
                string name = GetDisplayName(client);
                if (!string.IsNullOrEmpty(name))
                {
                    if (sb.Length > 0) sb.Append("*");
                    sb.Append(name);
                }
            }

            EventData eventData = new EventData((byte)EventCode.OnlineUsersEvent);
            eventData.Parameters = new Dictionary<byte, object>
            {
                { (byte)ParameterCode.ChatMessage, sb.ToString() }
            };

            SendParameters sendParams = new SendParameters();
            foreach (var client in MyServer.clients)
            {
                if (!string.IsNullOrEmpty(client.currentPhoneNum))
                {
                    client.SendEvent(eventData, sendParams);
                }
            }
        }

        /// <summary>
        /// 获取客户端显示名称（用手机号作为聊天名称）
        /// </summary>
        private static string GetDisplayName(MyClient client)
        {
            return client.currentPhoneNum;
        }
    }
}
