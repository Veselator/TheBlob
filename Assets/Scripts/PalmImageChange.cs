using UnityEngine;
using TheBlob.Events;

namespace TheBlob.Visual
{
    /// <summary>
    /// Swaps the hand tip sprite between open and grabbing states.
    /// Listens to BlobEvents for grab/release, filtered by hand side.
    /// </summary>
    public class PalmImageChange : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private HandSide _side;

        [Header("References")]
        [SerializeField] private SpriteRenderer _spriteRenderer;

        [Header("Sprites")]
        [SerializeField] private Sprite _openSprite;
        [SerializeField] private Sprite _grabbingSprite;

        private void OnEnable()
        {
            BlobEvents.OnHandGrabbedSurface += HandleGrabbed;
            BlobEvents.OnHandReleasedSurface += HandleReleased;
            BlobEvents.OnKnockback += HandleKnockback;
        }

        private void OnDisable()
        {
            BlobEvents.OnHandGrabbedSurface -= HandleGrabbed;
            BlobEvents.OnHandReleasedSurface -= HandleReleased;
            BlobEvents.OnKnockback -= HandleKnockback;
        }

        private void HandleGrabbed(HandGrabbedSurfaceEvent e)
        {
            if (e.Side != _side) return;
            _spriteRenderer.sprite = _grabbingSprite;
        }

        private void HandleReleased(HandReleasedSurfaceEvent e)
        {
            if (e.Side != _side) return;
            _spriteRenderer.sprite = _openSprite;
        }

        private void HandleKnockback(KnockbackEvent e)
        {
            _spriteRenderer.sprite = _openSprite;
        }
    }
}