namespace MyCommon
{
    public enum OperationCode
    {
        None,
        Login,
        Register,
        ChangePassword,
        DeleteAccount,
        PublicChat,
        PrivateChat,
        RefreshOnlineUsers,
        UpdateGameResult,
        // 游戏同步
        SpawnPlayer,
        PlayerPosition,
        PlayerAttack,
        PlayerAttacked,
        PlayerDie,
        EnemySpawn,
        EnemyAttacked,
        EnemyDie,
        WaveStart,
        SyncGameState,
        EnemySync,
        SkillEffect
    }
}