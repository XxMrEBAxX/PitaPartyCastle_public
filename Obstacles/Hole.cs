using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UB;

public class Hole : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>().SubtractHP(1000000);
        }
    }
}
