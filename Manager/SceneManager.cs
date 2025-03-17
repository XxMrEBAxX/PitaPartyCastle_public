using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace UB
{
    public class SceneManager : Singleton<SceneManager>
    {
        [SerializeField] private float _fadeDuration = 1.5f;
        [SerializeField] private Shader _shader;
        private Image _image;
        private Material _material;
        private const int DELAY_TIME = 1000;
        private float _radius;
        private float _originRadius;
        private bool _isFading;

        private void Start()
        {
            
            _image = GetComponentInChildren<Image>();
            _image.material = new Material(_shader);
            _material = _image.material;

            _material.SetColor("_Color", Color.black);
            _material.SetFloat("_BlurAmount", 0.1f);

            InitializeToSceneFade();
            _material.SetFloat("_Radius", _radius);
            _image.enabled = false;
        }

        private void OnDestroy()
        {
            OnDisable();
        }
        private void OnDisable()
        {
            DOTween.Kill(this);
        }

        private float GetScreenDiagonal()
        {
            float width = Screen.width;
            float height = Screen.height;
            return Mathf.Sqrt(width * width + height * height);
        }

        public async UniTaskVoid LoadSceneWithFade(string sceneName)
        {
            if (_isFading)
                return;

            _isFading = true;
            await FadeIn();
            await LoadScene(sceneName);
            await FadeOut();
            _isFading = false;
        }

        public async UniTaskVoid LoadSceneWithFadeAtPlayer(string sceneName)
        {
            if (_isFading)
                return;

            _isFading = true;
            await FadeInAtPlayer(() => { LoadScene(sceneName).Forget();});
            _isFading = false;
        }

        public void OnButtonNextScene()
        {
            NextSceneWithFade().Forget();
        }

        public async UniTaskVoid NextSceneWithFade()
        {
            if (_isFading)
                return;
            _isFading = true;
            await FadeIn();
            await NextScene();
            await FadeOut();
            _isFading = false;
        }

        public async UniTask LoadScene(string sceneName)
        {
            InitializeToSceneFade();
            await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
            await UniTask.Delay(DELAY_TIME, true);
        }

        public async UniTask NextScene()
        {
            InitializeToSceneFade();
            await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);
            await UniTask.Delay(DELAY_TIME, true);
        }

        public async UniTask FadeInAtPlayer(Action action)
        {
            Vector2 pos = Player.Instance.transform.position;
            InitializeToSceneFade();
            pos = Camera.main.WorldToScreenPoint(pos);
            pos.y = Screen.height - pos.y;
            Vector2 originPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            float distance = Vector2.Distance(originPosition, pos);
            _radius = _originRadius + distance;
            _material.SetFloat("_Radius", _radius);

            _material.SetVector("_Position", new Vector4(pos.x, pos.y, 0, 0));
            await FadeIn();
            action();
            await UniTask.Delay(DELAY_TIME, true);

            try
            {
                pos = Player.Instance.transform.position;    
            }
            catch
            {
                await FadeOut();
                return;
            }
            pos = Camera.main.WorldToScreenPoint(pos);
            pos.y = Screen.height - pos.y;
            _material.SetVector("_Position", new Vector4(pos.x, pos.y, 0, 0));

            await FadeOut();
        }

        private void InitializeToSceneFade()
        {
            _material.SetVector("_Position", new Vector4(Screen.width * 0.5f, Screen.height * 0.5f, 0, 0));
            _radius = GetScreenDiagonal();
            _originRadius = _radius;
        }

        private async UniTask FadeIn()
        {
            _image.enabled = true;
            DOTween.To(() =>
            _material.GetFloat("_Radius"), x =>
            _material.SetFloat("_Radius", x), 0, _fadeDuration);
            await UniTask.Delay((int)(_fadeDuration * 1000), true);
        }

        private async UniTask FadeOut()
        {
            DOTween.To(() =>
            _material.GetFloat("_Radius"), x =>
            _material.SetFloat("_Radius", x), _radius, _fadeDuration);
            await UniTask.Delay((int)(_fadeDuration * 1000), true);
            _image.enabled = false;
        }
    }
}
