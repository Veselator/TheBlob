using UnityEngine;
using TheBlob.Events;
using TheBlob.Visual;

namespace TheBlob.Physics
{
    /// <summary>
    /// Bounces the blob on contact. Applies force along the surface normal.
    /// Fires KnockbackEvent to release hand grabs so the body can fly freely.
    /// 
    /// Attach to the trampoline object (needs a Collider2D).
    /// </summary>
    public class Trampoline : MonoBehaviour
    {
        [Tooltip("Launch speed applied to the blob (units/sec).")]
        [SerializeField] private float _jumpForce = 15f;

        [Tooltip("If true, releases hand grabs on bounce so blob flies freely.")]
        [SerializeField] private bool _releaseGrabsOnBounce = true;

        [Tooltip("Stun duration when releasing grabs. 0 = hands recover instantly.")]
        [SerializeField] private float _stunDuration = 0.2f;

        [Tooltip("If true, overrides blob velocity. If false, adds to it.")]
        [SerializeField] private bool _overrideVelocity = true;

        [SerializeField] private AudioSource _as;
        [SerializeField] private float _minPitch = 0.7f;
        [SerializeField] private float _maxPitch = 1.2f;

        [SerializeField] private Vector2 _bounceDir = new Vector2(0, 1f);

        private void OnCollisionEnter2D(Collision2D collision)
        {
            var blobBody = collision.rigidbody;
            if (blobBody == null) return;

            // Only bounce the blob body, not hands
            if (blobBody.GetComponent<BlobVisual>() == null) return;

            // Release grabs so body can fly
            if (_releaseGrabsOnBounce)
            {
                BlobEvents.FireKnockback(new KnockbackEvent(_stunDuration));
            }

            // Apply bounce
            if (_overrideVelocity)
            {
                blobBody.velocity = _bounceDir * _jumpForce;
            }
            else
            {
                blobBody.velocity += _bounceDir * _jumpForce;
            }

            _as.pitch = Random.Range(_minPitch, _maxPitch);
            _as.Play();
        }
    }
}