using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint : MonoBehaviour
{
    private GameObject spawnPoint;

    private void Start()
    {
        spawnPoint = GameObject.Find("Level").transform.Find("SpawnPoint").gameObject;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(spawnPoint!=null && collision.CompareTag("Player"))
        {
            spawnPoint.transform.position = transform.position;
        }
    }
}
