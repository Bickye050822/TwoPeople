using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalUserData : MonoBehaviour
{
    public static LocalUserData instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
     DontDestroyOnLoad(gameObject);
    }
    public bool isGameStart = false;
    public float Hp=100;
    public string currentUserName="用户4231";
    public string currentUserId="11111";
}
