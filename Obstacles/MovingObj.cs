using System.Collections;
using System.Collections.Generic;

#if UnityEditor
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

public class MovingObj : MonoBehaviour
{
    enum MoveDir
    {
        up,
        down,
        left,
        right
    }
    [SerializeField] private MoveDir moveDir;
    [SerializeField] private float speed;
    [SerializeField] private float distance;
    [SerializeField] private bool isTurnBack;
    
    private float orignPos;

    void Start()
    {
        distance = Mathf.Abs(distance);

        Init();
    }

    private void Init()
    {
        switch (moveDir)
        {
            case MoveDir.up:
                orignPos = transform.position.y;
                break;
            case MoveDir.down:
                orignPos = transform.position.y;
                break;
            case MoveDir.left:
                orignPos = transform.position.x;
                break;
            case MoveDir.right:
                orignPos = transform.position.x;
                break;
        }
    }

    void Update()
    {
        switch (moveDir)
        {
            case MoveDir.up:
                transform.position = transform.position + new Vector3(0, speed * Time.deltaTime, 0);
                if (distance < Mathf.Abs(orignPos - transform.position.y))
                {
                    transform.position = new Vector3(transform.position.x, orignPos + distance, transform.position.z);
                    if (isTurnBack)
                    {
                        moveDir = MoveDir.down;
                        Init();
                    }
                }
                break;
            case MoveDir.down:
                transform.position = transform.position + new Vector3(0, -speed * Time.deltaTime, 0);
                if (distance < Mathf.Abs(orignPos - transform.position.y))
                {
                    transform.position = new Vector3(transform.position.x, orignPos - distance, transform.position.z);
                    if (isTurnBack)
                    {
                        moveDir = MoveDir.up;
                        Init();
                    }
                }
                break;

            case MoveDir.left:
                transform.position = transform.position + new Vector3(-speed * Time.deltaTime, 0, 0);
                if (distance < Mathf.Abs(orignPos - transform.position.x))
                {
                    transform.position = new Vector3(orignPos - distance, transform.position.y, transform.position.z);
                    if (isTurnBack)
                    {
                        moveDir = MoveDir.right;
                        Init();
                    }
                }
                break;
            case MoveDir.right:
                transform.position = transform.position + new Vector3(speed * Time.deltaTime, 0, 0);
                if (distance < Mathf.Abs(orignPos - transform.position.x))
                {
                    transform.position = new Vector3(orignPos + distance, transform.position.y, transform.position.z);
                    if (isTurnBack)
                    {
                        moveDir = MoveDir.left;
                        Init();
                    }
                }
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if(other.collider.CompareTag("Player") || other.collider.name == "StickUmbrella")
        {
            other.transform.SetParent(transform);
        }
    }
    private void OnCollisionExit2D(Collision2D other) {
        if(other.collider.CompareTag("Player") || other.collider.name == "StickUmbrella")
        {
            other.transform.SetParent(null);
            other.transform.localScale = new Vector3(1, 1, 1);
        }
    }
    private void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player") || other.name == "StickUmbrella")
        {
            other.transform.SetParent(transform);
        }
    }
    private void OnTriggerExit2D(Collider2D other) {
        if(other.CompareTag("Player") || other.name == "StickUmbrella")
        {
            other.transform.SetParent(null);
            other.transform.localScale = new Vector3(1, 1, 1);
        }
    }
}
