using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WaveManager : MonoBehaviourPun
{
    public List<WaveSO> waveSO;
    public List<GameObject> waveWallPrefab;
    public static WaveManager instance;
    public int currentEnemyKillNum = 0;
    public int currentWaveEnemyNum = 0;
    public bool currentWaveStart = false;
    public int currentWaveNum = 0;
    private int lastClearedWaveNum = 0;

    public void Start()
    {
        if (instance == null) instance = this;
    }

    public void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (currentWaveStart && currentWaveEnemyNum > 0 && currentEnemyKillNum >= currentWaveEnemyNum)
        {
            Debug.Log($"===== 第 {currentWaveNum} 波完成！准备下一波 =====");

            if (currentWaveNum > lastClearedWaveNum)
                WallClear();

            currentWaveStart = false;
            currentEnemyKillNum = 0;
        }
    }

    public void WallClear()
    {
        if (lastClearedWaveNum >= currentWaveNum) return;
        lastClearedWaveNum = currentWaveNum;
        photonView.RPC(nameof(SyncWallClear), RpcTarget.All, currentWaveNum);
    }

    [PunRPC]
    public void SyncWallClear(int waveNum)
    {
        int index = waveNum - 1;
        if (waveNum == 2)
        {
            waveWallPrefab[1].SetActive(true);
            return;
        }
        if (index >= 0 && index < waveWallPrefab.Count && waveWallPrefab[index] != null)
            Destroy(waveWallPrefab[index]);
    }

    [PunRPC]
    public void StartWave(string str)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        currentWaveNum++;
        Debug.Log($"[StartWave] 开始第 {currentWaveNum} 波: {str}");
        SpawnEnemy(str);
    }

    [PunRPC]
    public void UpKill()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        currentEnemyKillNum++;
        Debug.Log($"击杀数更新为: {currentEnemyKillNum}");
    }

    public void UpKillDirect()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        currentEnemyKillNum++;
        Debug.Log($"[直接调用] 击杀数更新为: {currentEnemyKillNum}");
    }

    public void SpawnEnemy(string str)
    {
        currentWaveStart = true;
        int spawnPointIndex = 0;
        WaveSO currentWaveDate = FindWaveSO(str);
        for (int i = 0; i < currentWaveDate.enemyPrefabList.Count; i++)
        {
            for (int k = 0; k < currentWaveDate.enemyCount[i]; k++)
            {
                Vector3 pos = currentWaveDate.spawnPoint[spawnPointIndex];
                spawnPointIndex = (spawnPointIndex + 1) % currentWaveDate.spawnPoint.Count;

                PhotonNetwork.Instantiate(currentWaveDate.enemyPrefabList[i].name, pos, Quaternion.identity);
            }
        }
    }

    public WaveSO FindWaveSO(string str)
    {
        foreach (WaveSO item in waveSO)
        {
            if (item.waveName == str)
            {
                currentWaveEnemyNum = item.enemyTotalCount;
                return item;
            }
        }
        return null;
    }
}
