using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
                Init();
            return _instance;
        }
    }

    private static void Init(T gameObject = null)
    {
        if (_instance == null)
            _instance = FindObjectOfType<T>();
        else
        {
            if(_instance != gameObject.GetComponent<T>())
                Destroy(gameObject);
        }
    }

    protected virtual void Awake()
    {
        Init(this.gameObject.GetComponent<T>());
    }
}