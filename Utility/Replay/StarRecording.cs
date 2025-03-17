using System;
using UnityEngine;

public class StarRecording : MonoBehaviour
{
    //[SerializeField] private float speed = 1.0f;
    //[SerializeField] private float amplitude = 1.0f;

    private Vector2 _start;

    public static event Action OnGoalTriggered;
    private bool _running;

    private void Awake()
    {
        _start = transform.position;
    }

    private void OnEnable() => EndRecording.Crossed += FinishLineOnOnRunEvent;
    private void OnDisable() => EndRecording.Crossed -= FinishLineOnOnRunEvent;

    private void FinishLineOnOnRunEvent(bool running)
    {
        _running = running;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_running) return;
        OnGoalTriggered?.Invoke();
    }
}
