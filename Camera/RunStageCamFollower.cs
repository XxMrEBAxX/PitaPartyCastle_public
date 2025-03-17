using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunStageCamFollower : MonoBehaviour
{
    public float minX;
    public float maxX;
    private Bounds _cameraBounds;

    GameObject player;
    Camera _mainCamera;

    float width;

    void Awake()
    {
        player = GameObject.FindWithTag("Player");
        _mainCamera = Camera.main;

        var height = _mainCamera.orthographicSize;
        width = height * _mainCamera.aspect;

        var map_minX = minX + width;
        var map_maxX = maxX - width;

        _cameraBounds = new Bounds();
        _cameraBounds.SetMinMax(
            new Vector3(map_minX, transform.position.y, 0f),
            new Vector3(map_maxX, transform.position.y, 0f));
    }

    void LateUpdate()
    {
        Vector3 targetPos = new Vector3(player.transform.position.x + (width / 2), player.transform.position.y, player.transform.position.z);
        transform.position = GetCameraBounds(targetPos);
    }

    private Vector3 GetCameraBounds(Vector3 targetPos)
    {
        return new Vector3(
            Mathf.Clamp(targetPos.x, _cameraBounds.min.x, _cameraBounds.max.x),
            Mathf.Clamp(targetPos.y, _cameraBounds.min.y, _cameraBounds.max.y),
            transform.position.z);
    }
}
