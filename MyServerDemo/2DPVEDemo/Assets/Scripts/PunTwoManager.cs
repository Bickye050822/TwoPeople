using Photon.Pun;
using UnityEngine;

public class PunTwoManager : MonoBehaviourPunCallbacks
{
    void Start()
    {
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 60;
        
        PhotonNetwork.AutomaticallySyncScene = true;
    }
}
