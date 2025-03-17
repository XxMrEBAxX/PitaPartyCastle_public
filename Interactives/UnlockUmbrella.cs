using UB.UI;
using UnityEngine;

namespace UB
{
    public class UnlockUmbrella : MonoBehaviour
    {
        [SerializeField] private bool _unlockUmbrella = false;
        [SerializeField] private bool _unlockTeleport = false;
        [SerializeField] private bool _unlockGliding = false;
        [SerializeField] private bool _unlockDoubleJump = false;

        private Player _player;
        private UnlockUI _unlockUI;

        private void Start()
        {
            _player = Player.Instance;
            _unlockUI = FindObjectOfType<UnlockUI>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                _player.UnlockUmbrella = _player.UnlockUmbrella | _unlockUmbrella;
                _player.UnlockTeleport = _player.UnlockTeleport | _unlockTeleport;
                _player.UnlockGliding = _player.UnlockGliding | _unlockGliding;
                _player.UnlockDoubleJump = _player.UnlockDoubleJump | _unlockDoubleJump;

                if (_unlockUI != null)
                {
                    if (_unlockDoubleJump)
                    {
                        _unlockUI.ShowPanel(UnlockUI.UnlockType.DOUBLEJUMP);
                    }
                    else if (_unlockGliding)
                    {
                        _unlockUI.ShowPanel(UnlockUI.UnlockType.GLIDING);
                    }
                    else if (_unlockTeleport)
                    {
                        _unlockUI.ShowPanel(UnlockUI.UnlockType.TELEPORT);
                    }
                    else if (_unlockUmbrella)
                    {
                        _unlockUI.ShowPanel(UnlockUI.UnlockType.UMBRELLA);
                    }
                }

                Destroy(gameObject);
            }
        }
    }
}
