using System.Collections.Generic;
using MyCommon;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;

namespace _2dPveDemoSever
{
    /// <summary>
    /// 游戏消息中继器：接收一个客户端的操作，用 Event 广播/转发给其他客户端
    /// </summary>
    public static class GameController
    {
        private static SendParameters unreliable = new SendParameters { Unreliable = true };

        private static void Broadcast(MyClient sender, byte eventCode, Dictionary<byte, object> data)
        {
            EventData evt = new EventData(eventCode) { Parameters = data };
            foreach (var c in MyServer.clients)
            {
                if (c != sender && !string.IsNullOrEmpty(c.currentPhoneNum))
                    c.SendEvent(evt, unreliable);
            }
        }

        private static void BroadcastAll(byte eventCode, Dictionary<byte, object> data)
        {
            EventData evt = new EventData(eventCode) { Parameters = data };
            foreach (var c in MyServer.clients)
            {
                if (!string.IsNullOrEmpty(c.currentPhoneNum))
                    c.SendEvent(evt, unreliable);
            }
        }

        private static void SendTo(MyClient target, byte eventCode, Dictionary<byte, object> data)
        {
            EventData evt = new EventData(eventCode) { Parameters = data };
            target.SendEvent(evt, unreliable);
        }

        // ==================== 玩家 ====================

        public static void HandlePlayerPosition(MyClient sender, Dictionary<byte, object> data)
        {
            Broadcast(sender, (byte)EventCode.PlayerPosEvent, data);
        }

        public static void HandlePlayerAttack(MyClient sender, Dictionary<byte, object> data)
        {
            Broadcast(sender, (byte)EventCode.PlayerAttackEvent, data);
        }

        public static void HandlePlayerAttacked(MyClient sender, Dictionary<byte, object> data)
        {
            // 转发攻击到被攻击者方向（data 包含 target info）
            Broadcast(sender, (byte)EventCode.PlayerAttackedEvent, data);
        }

        public static void HandlePlayerDie(MyClient sender)
        {
            Broadcast(sender, (byte)EventCode.PlayerDieEvent, null);
        }

        public static void HandleSpawnPlayer(MyClient sender)
        {
            // 新玩家生成，通知所有人
            Dictionary<byte, object> data = new Dictionary<byte, object>
            {
                { (byte)ParameterCode.PhoneNumber, sender.currentPhoneNum }
            };
            Broadcast(sender, (byte)EventCode.PlayerSpawnEvent, data);
        }

        // ==================== 敌人 ====================

        public static void HandleEnemyAttacked(MyClient sender, Dictionary<byte, object> data)
        {
            Broadcast(sender, (byte)EventCode.EnemyAttackedEvent, data);
        }

        public static void HandleEnemyDie(MyClient sender, Dictionary<byte, object> data)
        {
            Broadcast(sender, (byte)EventCode.EnemyDieEvent, data);
        }

        // ==================== 敌人状态 ====================

        public static void HandleEnemySync(MyClient sender, Dictionary<byte, object> data)
        {
            Broadcast(sender, (byte)EventCode.EnemySyncEvent, data);
        }

        public static void HandleSkillEffect(MyClient sender, Dictionary<byte, object> data)
        {
            Broadcast(sender, (byte)EventCode.SkillEffectEvent, data);
        }

        // ==================== 波次 ====================

        public static void HandleWaveStart(MyClient sender, Dictionary<byte, object> data)
        {
            Broadcast(sender, (byte)EventCode.WaveStartEvent, data);
        }

        // ==================== 完整状态同步 ====================

        public static void SendSyncState(MyClient requester)
        {
            // 告诉新加入的客户端当前已连接的玩家列表
            string playerList = "";
            foreach (var c in MyServer.clients)
            {
                if (!string.IsNullOrEmpty(c.currentPhoneNum))
                {
                    if (playerList.Length > 0) playerList += "*";
                    playerList += c.currentPhoneNum;
                }
            }
            Dictionary<byte, object> data = new Dictionary<byte, object>
            {
                { (byte)ParameterCode.ChatMessage, playerList }
            };
            SendTo(requester, (byte)EventCode.SyncGameStateEvent, data);
        }
    }
}
