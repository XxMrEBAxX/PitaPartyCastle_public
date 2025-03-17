using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    [SerializeField] private Collider2D _collider;
    void Start()
    {
        _collider.usedByEffector = true;
        PlatformEffector2D effector = _collider.gameObject.GetComponent<PlatformEffector2D>();
        if (effector == null) _collider.gameObject.AddComponent<PlatformEffector2D>();
    }
}
