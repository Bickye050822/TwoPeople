using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveTriggerData : MonoBehaviour
{
  public string waveName;
  private void Update()
  {
    
  }
  public void OnTriggerEnter2D(Collider2D other)
  {
    if (other.tag == "Player")
    {
      WaveManager.instance.StartWave(waveName);
      Destroy(gameObject);
    }
  }

 
}
