namespace MyCommon
{
    public enum EventCode
    {
        None,
        PublicChatEvent,
        PrivateChatEvent,
        OnlineUsersEvent,
        // 游戏同步
        PlayerSpawnEvent,
        PlayerPosEvent,
        PlayerAttackEvent,
        PlayerAttackedEvent,
        PlayerDieEvent,
        EnemySpawnEvent,
        EnemyAttackedEvent,
        EnemyDieEvent,
        WaveStartEvent,
        SyncGameStateEvent,
        EnemySyncEvent,
        SkillEffectEvent
    }
}
