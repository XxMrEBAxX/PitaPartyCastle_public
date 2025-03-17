using System;
using UnityEngine;

public class EndRecording : MonoBehaviour
{
    [SerializeField] private GameObject _startObject;
    [SerializeField] private GameObject _endObject;

    public static event Action<bool> Crossed;

    private bool _running;

    private void Awake()
    {
        _startObject.SetActive(true);
        _endObject.SetActive(false);
    }

    private void OnEnable() => StarRecording.OnGoalTriggered += GoalOnOnGoalTriggered;
    private void OnDisable() => StarRecording.OnGoalTriggered -= GoalOnOnGoalTriggered;

    private void GoalOnOnGoalTriggered()
    {
        _endObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_running && !_endObject.activeSelf) return;
        _running = !_running;
        _startObject.SetActive(true);
        _endObject.SetActive(false);
        Crossed?.Invoke(_running);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        _startObject.SetActive(!_running);
    }
}
