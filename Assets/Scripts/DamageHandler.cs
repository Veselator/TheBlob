using UnityEngine;
using TheBlob.Events;
using System;

namespace TheBlob.Physics
{
    /// <summary>
    /// Handles damage from obstacles with SomethingThatDealsDamage.
    /// On collision: fires KnockbackEvent (releases all hand grabs),
    /// THEN directly sets body velocity for reliable knockback.
    /// 
    /// Attach to BlobBody (same object as Rigidbody2D).
    /// </summary>
    public class DamageHandler : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rb;

        [Header("Knockback")]
        [Tooltip("Knockback velocity (units/sec). Body will fly at this speed.")]
        [SerializeField] private float _knockbackSpeed = 12f;

        [Tooltip("Minimum upward component of knockback. Ensures the blob always pops up.")]
        [SerializeField] private float _minUpwardKnockback = 5f;

        [Tooltip("How long hands are disabled after being hit (seconds).")]
        [SerializeField] private float _stunDuration = 0.5f;

        [Tooltip("Random angle spread added to knockback direction (degrees). " +
                 "0 = always exact, 30 = up to ±30° deviation.")]
        [SerializeField, Range(0f, 90f)] private float _randomSpreadAngle = 15f;

        public event Action<float> OnDamage;
        private EnthusiasmManager _em;

        private void Start()
        {
            _em = EnthusiasmManager.Instance;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.gameObject.TryGetComponent<SomethingThatDealsDamage>(out var damager))
                return;

            // 1. Notify EnthusiasmManager
            _em.AddEnthusiasm(-damager.Damage);
            OnDamage?.Invoke(damager.Damage);

            // 2. Release all hand grabs and stun hands
            BlobEvents.FireKnockback(new KnockbackEvent(_stunDuration));

            // 3. Calculate knockback direction (away from contact point)
            Vector2 contactCenter = Vector2.zero;
            for (int i = 0; i < collision.contactCount; i++)
            {
                contactCenter += collision.GetContact(i).point;
            }
            contactCenter /= Mathf.Max(collision.contactCount, 1);

            Vector2 knockbackDir = ((Vector2)transform.position - contactCenter).normalized;

            // 4. Apply random spread
            float randomAngle = UnityEngine.Random.Range(-_randomSpreadAngle, _randomSpreadAngle) * Mathf.Deg2Rad;
            float cos = Mathf.Cos(randomAngle);
            float sin = Mathf.Sin(randomAngle);
            knockbackDir = new Vector2(
                knockbackDir.x * cos - knockbackDir.y * sin,
                knockbackDir.x * sin + knockbackDir.y * cos
            );

            // 5. Ensure there's always some upward component
            if (knockbackDir.y < _minUpwardKnockback / _knockbackSpeed)
                knockbackDir.y = _minUpwardKnockback / _knockbackSpeed;
            knockbackDir.Normalize();

            // 6. SET velocity directly
            _rb.velocity = knockbackDir * _knockbackSpeed;
        }
    }
}