using System.Collections;
using UnityEngine;

namespace TheBlob
{
    /// <summary>
    /// A respawn checkpoint. When the blob touches it for the first time,
    /// it registers itself as the active respawn point,
    /// plays a color transition animation and spawns a particle effect.
    /// 
    /// Needs a Collider2D set to isTrigger = true.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Respawn : MonoBehaviour
    {
        [Header("Respawn")]
        [SerializeField] private Transform _respawnPoint;

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer _spriteRenderer;

        [Tooltip("Color before activation.")]
        [SerializeField] private Color _inactiveColor = Color.gray;

        [Tooltip("Color after activation.")]
        [SerializeField] private Color _activeColor = Color.green;

        [Tooltip("How long the color transition takes (seconds).")]
        [SerializeField] private float _colorTransitionDuration = 0.5f;

        [Header("Particle Effect")]
        [Tooltip("Particle system to play on activation. Can be a child object.")]
        [SerializeField] private ParticleSystem _activationParticle;

        private bool _isClaimed;

        public bool IsClaimed => _isClaimed;

        private void Awake()
        {
            if (_spriteRenderer != null)
                _spriteRenderer.color = _inactiveColor;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isClaimed) return;

            if (other.GetComponent<TheBlob.Visual.BlobVisual>() == null) return;

            Claim();
        }

        private void Claim()
        {
            _isClaimed = true;

            var respawnPos = _respawnPoint != null
                ? (Vector2)_respawnPoint.position
                : (Vector2)transform.position;

            RespawnManager.Instance.SetRespawnPoint(respawnPos);

            // Color animation
            if (_spriteRenderer != null)
                StartCoroutine(AnimateColor());

            // Particle effect
            if (_activationParticle != null)
                _activationParticle.Play();
        }

        private IEnumerator AnimateColor()
        {
            float elapsed = 0f;
            var startColor = _spriteRenderer.color;

            while (elapsed < _colorTransitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / _colorTransitionDuration);
                _spriteRenderer.color = Color.Lerp(startColor, _activeColor, t);
                yield return null;
            }

            _spriteRenderer.color = _activeColor;
        }
    }
}