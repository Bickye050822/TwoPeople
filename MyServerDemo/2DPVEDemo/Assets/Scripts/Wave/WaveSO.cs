using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Wave", menuName = "Enemy/Wave Data")]
public class WaveSO :ScriptableObject
{
    [Header("波次基础设置")]
    [Tooltip("波次名")]
    public string waveName; // 波次名
    [Tooltip("开始前等待时间")]
    public float delayBeforeWave = 2f; // 波次开始前等待时间
    [Tooltip("等待下一波时间")]
    public float waveEndDelay = 3f; // 波次结束后等待下一波时间
    [Tooltip("敌人总量")]
    public int enemyTotalCount;// 敌人总量
    [Tooltip("敌人难度")]
    public int enemyDifficulty;

    [Header("敌人生成设置")]
    [Tooltip("敌人预制体")]
    public List<GameObject> enemyPrefabList; // 敌人预制体
    [Tooltip("敌人数量")]
    public List<int> enemyCount ; // 本波生成数量
    [Tooltip("生成间隔")]
    public float spawnInterval = 1f; // 生成间隔（秒）

    [Tooltip("生成点位置")] public List<Vector3> spawnPoint; // 生成点位置
}
