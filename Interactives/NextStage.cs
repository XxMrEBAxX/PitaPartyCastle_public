using UnityEngine;

namespace UB
{
    public class NextStage : MonoBehaviour
    {
        [SerializeField] string sceneName;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.transform.CompareTag("Player"))
            {
                SceneManager.Instance.LoadSceneWithFadeAtPlayer(sceneName).Forget();
            }
        }
    }
}
