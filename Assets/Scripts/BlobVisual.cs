using UnityEngine;

namespace TheBlob.Visual
{
    /// <summary>
    /// Squash-and-stretch visual deformation for the blob body.
    /// 
    /// Attach to a CHILD of BlobBody that holds the SpriteRenderer.
    /// This way freezeRotation on the parent Rigidbody2D keeps physics stable,
    /// but the visual can wobble freely.
    /// 
    /// Hierarchy:
    ///   BlobBody (Rigidbody2D, freezeRotation = true)
    ///     └── BlobSprite (SpriteRenderer + this script)
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class BlobVisual : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody2D _blobRigidbody;

        [Header("Squash & Stretch")]
        [SerializeField, Range(0f, 5.5f)] private float _deformAmount = 0.15f;
        [SerializeField, Range(1f, 20f)] private float _deformSpeed = 8f;
        [SerializeField, Range(1f, 2f)] private float _maxDeform = 1.3f;

        [Header("Visual Wobble")]
        [Tooltip("How much the sprite tilts based on horizontal velocity.")]
        [SerializeField, Range(0f, 15f)] private float _tiltAmount = 5f;
        [SerializeField, Range(1f, 20f)] private float _tiltSpeed = 6f;

        private Vector3 _baseScale;
        private float _currentStretchX = 1f;
        private float _currentStretchY = 1f;
        private float _currentTilt;

        private void Awake()
        {
            _baseScale = transform.localScale;
        }

        private void LateUpdate()
        {
            var velocity = _blobRigidbody.velocity;
            float speed = velocity.magnitude;

            // ── Squash & Stretch ──
            float stretchFactor = 1f + Mathf.Clamp(speed * _deformAmount, 0f, _maxDeform - 1f);
            float squashFactor = 1f / stretchFactor;

            float targetY = speed > 0.1f ? stretchFactor : 1f;
            float targetX = speed > 0.1f ? squashFactor : 1f;

            _currentStretchX = Mathf.Lerp(_currentStretchX, targetX, _deformSpeed * Time.deltaTime);
            _currentStretchY = Mathf.Lerp(_currentStretchY, targetY, _deformSpeed * Time.deltaTime);

            transform.localScale = new Vector3(
                _baseScale.x * _currentStretchX,
                _baseScale.y * _currentStretchY,
                _baseScale.z
            );

            // ── Tilt based on horizontal velocity ──
            float targetTilt = -velocity.x * _tiltAmount;
            _currentTilt = Mathf.Lerp(_currentTilt, targetTilt, _tiltSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Euler(0, 0, _currentTilt);
        }
    }
}