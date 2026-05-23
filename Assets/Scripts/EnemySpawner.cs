using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    public GameObject enemyPrefab;
    public Transform[] spawnPoint;


    public override void OnStartServer()
    {
        foreach(Transform point in spawnPoint)
        {
            GameObject enemy = Instantiate(enemyPrefab, point.position, point.rotation);
            NetworkServer.Spawn(enemy);
        }
    }
   
}
