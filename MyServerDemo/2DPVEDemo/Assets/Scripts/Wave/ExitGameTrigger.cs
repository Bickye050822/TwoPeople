using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitGameTrigger : MonoBehaviour
{
    public bool canExit;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            canExit = true;
        }
    }
    
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            canExit = true;
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            canExit = false;
        }
    }
    
    void Update()
    {
        
        if (canExit && Input.GetKeyDown(KeyCode.W))
        {
            string time = PlayerManager.instance ? PlayerManager.instance.GetGameTime() : "00:00";
            string score = PlayerManager.instance ? PlayerManager.instance.Score.ToString() : "0";
            GameManager.instance.GameOver("通关成功", time, score);
        }
    }
    
}
