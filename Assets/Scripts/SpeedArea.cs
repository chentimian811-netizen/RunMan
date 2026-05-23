using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedArea : MonoBehaviour
{
    public float speed = 5f;

    public float timer = 3f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag ("Player"))
        {
              PlayerController player =other.GetComponent<PlayerController>();

            player.speed = speed;
            Invoke("ReSpeed", timer);
        }
    }

    void ReSpeed()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        PlayerController playerController = player.GetComponent<PlayerController>();

        playerController.speed = 5f;

    }
}
