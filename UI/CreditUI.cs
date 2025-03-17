using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace UB.UI
{
    public class CreditUI : MonoBehaviour
    {
        [SerializeField] private int _duration = 25;
        private Animator _animator;
        private void Start()
        {
            StartCoroutine(Credit());
            _animator = GetComponent<Animator>();
            Time.timeScale = 1;
        }

        private void Update()
        {
            if (Keyboard.current[Key.Space].isPressed)
            {
                Time.timeScale = 10;
            }
            else
            {
                Time.timeScale = 1;
            }
        }

        private IEnumerator Credit()
        {
            yield return new WaitForSeconds(_duration);
            UnityEngine.SceneManagement.SceneManager.LoadScene("sn-Main");
            Time.timeScale = 1;
        }
    }
}
