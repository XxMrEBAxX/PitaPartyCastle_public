using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace UB.UI
{
    public class UnlockUI : UIView
    {
        public enum UnlockType
        {
            UMBRELLA,
            TELEPORT,
            GLIDING,
            DOUBLEJUMP
        }

        [SerializeField] private RectTransform _upLine;
        [SerializeField] private RectTransform _downLine;
        [SerializeField] private RectMask2D _rectMask2D;
        [SerializeField] private Ease _ease;
        [SerializeField] private float _duration = 1;

        [SerializeField] private Image _iconImage;
        [SerializeField] private Sprite[] _spriteList;
        [SerializeField] private LocalizeStringEvent _headText;
        [SerializeField] private LocalizeStringEvent _explanationText;
        [SerializeField] private GameObject _continueText;

        private float _upLineOriginY;
        private float _downLineOriginY;
        private PauseUI _pauseUI;
        private RectTransform _headTextRect;
        private RectTransform _bodyTextRect;
        private Vector3 _headTextOriginPos;
        private Vector3 _bodyTextOriginPos;
        private bool _isHidePanel;

        private void Start()
        {
            _upLineOriginY = _upLine.anchoredPosition.y;
            _downLineOriginY = _downLine.anchoredPosition.y;
            _pauseUI = FindObjectOfType<PauseUI>();
            _headTextRect = _headText.GetComponent<RectTransform>();
            _headTextOriginPos = _headTextRect.anchoredPosition;
            _bodyTextRect = _explanationText.GetComponent<RectTransform>();
            _bodyTextOriginPos = _bodyTextRect.anchoredPosition;
        }

        private void Update()
        {
            if (_panel.activeSelf)
            {
                if (Keyboard.current[Key.Space].wasPressedThisFrame && _isHidePanel)
                {
                    _pauseUI.HidePanel(this);
                }
            }
        }

        public override void EnableAction()
        {
            _rectMask2D.padding = new Vector4(0, Mathf.Abs(_downLineOriginY), 0, Mathf.Abs(_upLineOriginY));
            _upLine.anchoredPosition = new Vector3(_upLine.anchoredPosition.x, 0);
            _downLine.anchoredPosition = new Vector3(_downLine.anchoredPosition.x, 0);
            _headTextRect.anchoredPosition = new Vector3(-2000, -2000, 0);
            _bodyTextRect.anchoredPosition = new Vector3(-2000, -2000, 0);
            _isHidePanel = false;
            _continueText.SetActive(false);
        }

        public void ShowPanel(UnlockType unlockType)
        {
            if (_pauseUI != null)
                _pauseUI.ShowPanel(this);

            switch (unlockType)
            {
                case UnlockType.UMBRELLA:
                    _iconImage.sprite = _spriteList[0];
                    _headText.StringReference.TableEntryReference = "UmbrellaThrowHead";
                    _explanationText.StringReference.TableEntryReference = "UmbrellaThrowExplanation";
                    break;
                case UnlockType.TELEPORT:
                    _iconImage.sprite = _spriteList[1];
                    _headText.StringReference.TableEntryReference = "UmbrellaTeleportHead";
                    _explanationText.StringReference.TableEntryReference = "UmbrellaTeleportExplanation";
                    break;
                case UnlockType.GLIDING:
                    _iconImage.sprite = _spriteList[2];
                    _headText.StringReference.TableEntryReference = "UmbrellaGlidingHead";
                    _explanationText.StringReference.TableEntryReference = "UmbrellaGlidingExplanation";
                    break;
                case UnlockType.DOUBLEJUMP:
                    _iconImage.sprite = _spriteList[3];
                    _headText.StringReference.TableEntryReference = "UmbrellaDoubleJumpHead";
                    _explanationText.StringReference.TableEntryReference = "UmbrellaDoubleJumpExplanation";
                    break;
            }

            StartCoroutine(RectTransformMovePosition(_duration * 0.3f));

            DOTween.To(x =>
            {
                _upLine.anchoredPosition = new Vector2(_upLine.anchoredPosition.x, x);
            }, 0, _upLineOriginY, _duration).SetEase(_ease).SetUpdate(true);
            DOTween.To(x =>
            {
                _rectMask2D.padding = new Vector4(0, _rectMask2D.padding.y, 0, x);
            }, Mathf.Abs(_upLineOriginY), 0, _duration).SetEase(_ease).SetUpdate(true);

            DOTween.To(() => _downLine.anchoredPosition, x => _downLine.anchoredPosition = x,
                new Vector2(_downLine.anchoredPosition.x, _downLineOriginY), _duration).SetEase(_ease).SetUpdate(true);
            DOTween.To(x =>
            {
                _rectMask2D.padding = new Vector4(0, x, 0, _rectMask2D.padding.w);
            }, Mathf.Abs(_downLineOriginY), 0, _duration).SetEase(_ease).SetUpdate(true)
            .OnComplete(() =>
            {
                _isHidePanel = true;
                _continueText.SetActive(true);
            });
        }

        // Bold TextMeshPro의 텍스트가 변동되면 RectMask2D가 있음에도 깜빡 나왔다가 사라지는 현상 방지
        private IEnumerator RectTransformMovePosition(float duration)
        {
            yield return new WaitForSecondsRealtime(duration);
            _headTextRect.anchoredPosition = _headTextOriginPos;
            _bodyTextRect.anchoredPosition = _bodyTextOriginPos;
        }
    }
}
