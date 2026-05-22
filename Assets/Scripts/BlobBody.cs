using UnityEngine;

namespace TheBlob.Physics
{
    /// <summary>
    /// Main blob body. Freeze rotation so the blob stays upright.
    /// Visual wobble is handled by BlobVisual on a separate child sprite.
    /// 
    /// Hierarchy:
    ///   BlobBody (this script + Rigidbody2D + CircleCollider2D)
    ///     └── BlobSprite (SpriteRenderer + BlobVisual)
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class BlobBody : MonoBehaviour
    {
        [Header("Physics")]
        [SerializeField] private float _gravityScale = 1.8f;
        [SerializeField] private float _drag = 0.8f;
        [SerializeField] private float _angularDrag = 10f;
        [SerializeField] private float _mass = 2f;

        [Header("Collider")]
        [SerializeField] private float _bodyRadius = 0.5f;
        [SerializeField] private float _friction = 0.6f;
        [SerializeField] private float _bounciness = 0.05f;

        [Header("Climbing Friction")]
        [Tooltip("Friction used when any hand is grabbing a surface. " +
                 "Low value lets body slide up walls while climbing.")]
        [SerializeField] private float _climbingFriction = 0.05f;

        private Rigidbody2D _rb;
        private PhysicsMaterial2D _mat;
        private int _activeGrabCount;

        public Rigidbody2D Rigidbody => _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            var col = GetComponent<CircleCollider2D>();

            _rb.gravityScale = _gravityScale;
            _rb.drag = _drag;
            _rb.angularDrag = _angularDrag;
            _rb.mass = _mass;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rb.freezeRotation = true;

            col.radius = _bodyRadius;
            _mat = new PhysicsMaterial2D("BlobBodyMat")
            {
                friction = _friction,
                bounciness = _bounciness
            };
            col.sharedMaterial = _mat;
        }

        private void OnEnable()
        {
            Events.BlobEvents.OnHandGrabbedSurface += HandleGrabbed;
            Events.BlobEvents.OnHandReleasedSurface += HandleReleased;
            Events.BlobEvents.OnKnockback += HandleKnockback;
        }

        private void OnDisable()
        {
            Events.BlobEvents.OnHandGrabbedSurface -= HandleGrabbed;
            Events.BlobEvents.OnHandReleasedSurface -= HandleReleased;
            Events.BlobEvents.OnKnockback -= HandleKnockback;
        }

        private void HandleGrabbed(Events.HandGrabbedSurfaceEvent e)
        {
            _activeGrabCount++;
            UpdateFriction();
        }

        private void HandleReleased(Events.HandReleasedSurfaceEvent e)
        {
            _activeGrabCount = Mathf.Max(0, _activeGrabCount - 1);
            UpdateFriction();
        }

        private void HandleKnockback(Events.KnockbackEvent e)
        {
            _activeGrabCount = 0;
            UpdateFriction();
        }

        private void UpdateFriction()
        {
            _mat.friction = _activeGrabCount > 0 ? _climbingFriction : _friction;
        }
    }
}