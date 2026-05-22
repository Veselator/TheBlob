using UnityEngine;
using TheBlob.Data;
using TheBlob.Events;

namespace TheBlob.Physics
{
    /// <summary>
    /// Controls one hand's physics.
    /// 
    /// NO JOINTS between hand and body. Instead:
    /// - Hand moves via AddForce toward cursor
    /// - When hand collides with world, reactive force is transferred to body via AddForce
    /// - When hand grabs a GrabSurface, a FixedJoint2D anchors hand to world,
    ///   and body is pulled toward hand via spring force
    /// - Max reach is enforced by clamping hand position each FixedUpdate
    /// 
    /// This avoids the joint spaghetti problem and gives direct control
    /// over how force transfers from hands to body.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class HandController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private HandSide _side;
        [SerializeField] private HandSettings _settings;

        [Header("References")]
        [SerializeField] private Rigidbody2D _blobBody;
        [SerializeField] private GameOverManager _gom;

        // ── Components ──
        private Rigidbody2D _handRb;
        private CircleCollider2D _collider;
        private FixedJoint2D _grabJoint;

        // ── State ──
        private enum HandState { Idle, Active, Grabbed, Stunned }
        private HandState _state = HandState.Idle;

        private Vector2 _targetPosition;
        private float _currentMaxReach;
        private GrabSurface _grabbedSurface;
        private float _idleSwayPhase;
        private float _stunTimer;

        // ── Public API (read by VisualHand) ──
        public Vector2 ShoulderPosition =>
            (Vector2)_blobBody.transform.position + _settings.shoulderOffset;

        public Vector2 TipPosition => (Vector2)_handRb.position;
        public float CurrentLength => Vector2.Distance(ShoulderPosition, TipPosition);

        public float NormalizedStretch
        {
            get
            {
                float range = _settings.maxLength - _settings.baseLength;
                if (range <= 0f) return 0f;
                return Mathf.Clamp01((CurrentLength - _settings.baseLength) / range);
            }
        }

        public bool IsGrabbing => _state == HandState.Grabbed;
        public bool IsActive => _state != HandState.Idle;
        public HandSettings Settings => _settings;

        private bool _isPaused;

        public void Pause()
        {
            _isPaused = true;

            if (_state == HandState.Grabbed)
                ReleaseGrab();

            _state = HandState.Idle;
        }

        public void Unpause()
        {
            _isPaused = false;
        }

        // ────────────────────────────────────
        //  Setup
        // ────────────────────────────────────

        private void Awake()
        {
            _handRb = GetComponent<Rigidbody2D>();
            _collider = GetComponent<CircleCollider2D>();

            // Hand Rigidbody
            _handRb.mass = _settings.handMass;
            _handRb.gravityScale = _settings.handGravityScale;
            _handRb.drag = _settings.handDrag;
            _handRb.angularDrag = _settings.handAngularDrag;
            _handRb.interpolation = RigidbodyInterpolation2D.Interpolate;
            _handRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _handRb.freezeRotation = true;

            // Hand Collider (real, not trigger)
            _collider.isTrigger = false;
            _collider.radius = _settings.handColliderRadius;
            var mat = new PhysicsMaterial2D($"HandTip_{_side}")
            {
                friction = _settings.handFriction,
                bounciness = _settings.handBounciness
            };
            _collider.sharedMaterial = mat;

            _currentMaxReach = _settings.baseLength;
            _idleSwayPhase = (_side == HandSide.Left) ? 0f : Mathf.PI * 0.7f;
        }

        private void Start()
        {
            // Place hand at idle dangle position
            var shoulder = ShoulderPosition;
            var dangleDir = _settings.idleDangleDirection.normalized;
            var startPos = shoulder + dangleDir * _settings.baseLength;
            _handRb.position = startPos;
            transform.position = (Vector3)startPos;
        }

        // ────────────────────────────────────
        //  Event subscriptions
        // ────────────────────────────────────

        private void OnEnable()
        {
            BlobEvents.OnHandGrabStart += HandleGrabStart;
            BlobEvents.OnHandMove += HandleMove;
            BlobEvents.OnHandGrabEnd += HandleGrabEnd;
            BlobEvents.OnKnockback += HandleKnockback;
        }

        private void OnDisable()
        {
            BlobEvents.OnHandGrabStart -= HandleGrabStart;
            BlobEvents.OnHandMove -= HandleMove;
            BlobEvents.OnHandGrabEnd -= HandleGrabEnd;
            BlobEvents.OnKnockback -= HandleKnockback;
        }

        private void HandleGrabStart(HandGrabStartEvent e)
        {
            if (e.Side != _side) return;
            if (_state == HandState.Stunned || _isPaused) return;

            // If currently grabbed — release grab, then start new Active reach
            if (_state == HandState.Grabbed)
            {
                ReleaseGrab();
            }

            _state = HandState.Active;
            _targetPosition = e.WorldPosition;
        }

        private void HandleMove(HandMoveEvent e)
        {
            if (e.Side != _side) return;
            _targetPosition = e.WorldPosition;
        }

        private void HandleGrabEnd(HandGrabEndEvent e)
        {
            if (e.Side != _side) return;
            if (_state == HandState.Stunned) return;

            // If grabbed — STAY grabbed. Don't release on mouse up.
            if (_state == HandState.Grabbed) return;

            // Only go idle if we were in Active (reaching but didn't grab anything)
            _state = HandState.Idle;
        }

        private void HandleKnockback(KnockbackEvent e)
        {
            // Release grab if holding
            if (_state == HandState.Grabbed)
                ReleaseGrab();

            _state = HandState.Stunned;
            _stunTimer = e.Duration;

            // Fling hand outward for visual effect
            var flingDir = (TipPosition - ShoulderPosition).normalized;
            if (flingDir.sqrMagnitude < 0.01f)
                flingDir = Random.insideUnitCircle.normalized;
            _handRb.velocity = flingDir * 5f;
        }

        public void ResetPos()
        {
            // Release any grab — DestroyImmediate so joint is gone THIS frame
            if (_state == HandState.Grabbed)
            {
                _grabbedSurface = null;
                if (_grabJoint != null)
                {
                    DestroyImmediate(_grabJoint);
                    _grabJoint = null;
                }
            }

            // Reset state
            _state = HandState.Idle;
            _currentMaxReach = _settings.baseLength;
            _stunTimer = 0f;
            _isTouchingSurface = false;

            // Teleport hand to idle dangle position relative to current body position
            var shoulder = ShoulderPosition;
            var dangleDir = _settings.idleDangleDirection.normalized;
            var idlePos = shoulder + dangleDir * _settings.baseLength;

            _handRb.position = idlePos;
            transform.position = (Vector3)idlePos;
            _handRb.velocity = Vector2.zero;
        }

        // ────────────────────────────────────
        //  Physics tick
        // ────────────────────────────────────

        private void FixedUpdate()
        {
            if (_gom.IsGameOver || _isPaused) return;

            switch (_state)
            {
                case HandState.Idle:
                    UpdateIdle();
                    break;
                case HandState.Active:
                    UpdateActive();
                    break;
                case HandState.Grabbed:
                    UpdateGrabbed();
                    break;
                case HandState.Stunned:
                    UpdateStunned();
                    break;
            }

            EnforceMaxReach();
        }

        private void UpdateStunned()
        {
            _stunTimer -= Time.fixedDeltaTime;
            if (_stunTimer <= 0f)
            {
                _state = HandState.Idle;
            }
            // During stun: no forces on body, hand just dangles with gravity
        }

        private void UpdateIdle()
        {
            _currentMaxReach = Mathf.MoveTowards(
                _currentMaxReach, _settings.baseLength,
                _settings.retractRate * Time.fixedDeltaTime);

            var shoulder = ShoulderPosition;
            var dangleDir = _settings.idleDangleDirection.normalized;
            float sway = Mathf.Sin(Time.time * _settings.idleSwayFrequency + _idleSwayPhase)
                         * _settings.idleSwayAmplitude;
            var swayPerp = new Vector2(-dangleDir.y, dangleDir.x);
            var idleTarget = shoulder + (dangleDir + swayPerp * sway).normalized * _currentMaxReach;

            // Velocity-based steering
            var toTarget = idleTarget - TipPosition;
            var desiredVel = toTarget * _settings.idleReturnSpeed;
            var correction = desiredVel - _handRb.velocity;
            _handRb.AddForce(correction * _handRb.mass, ForceMode2D.Force);
        }

        private void UpdateActive()
        {
            _currentMaxReach = Mathf.MoveTowards(
                _currentMaxReach, _settings.maxLength,
                _settings.growthRate * Time.fixedDeltaTime);

            var shoulder = ShoulderPosition;
            var toTarget = _targetPosition - shoulder;
            float clampedDist = Mathf.Min(toTarget.magnitude, _currentMaxReach);
            var clampedTarget = shoulder + toTarget.normalized * clampedDist;

            var toClamped = clampedTarget - TipPosition;
            var desiredForce = toClamped * _settings.moveForce;
            _handRb.AddForce(desiredForce, ForceMode2D.Force);

            // When hand is touching a surface, apply two forces to body:
            if (_isTouchingSurface)
            {
                // 1. Reactive: blocked hand force mirrors onto body
                TransferReactiveForce(desiredForce);

                // 2. Cursor intent: player pushes mouse past the hand → body moves
                ApplyCursorIntentForce();
            }
        }

        private void UpdateGrabbed()
        {
            if (_grabbedSurface == null) return;

            float grip = _grabbedSurface.gripStrength;
            var bodyPos = (Vector2)_blobBody.position;
            var grabPos = TipPosition;
            var toGrab = grabPos - bodyPos;
            float distToGrab = toGrab.magnitude;

            var totalForce = Vector2.zero;

            // ── 1. Tether constraint: body moves FREELY within arm length ──
            //    Only pulls back when body exceeds max reach from grab point.
            //    No constant spring — the body can swing, be pushed by other hand, etc.
            if (distToGrab > _currentMaxReach * 0.9f)
            {
                // How far past the soft limit (0 = at limit, 1 = way past)
                float overstretch = (distToGrab - _currentMaxReach * 0.9f) / (_currentMaxReach * 0.1f);
                overstretch = Mathf.Clamp01(overstretch);

                // Pull body back toward grab, scaling with overstretch
                totalForce += toGrab.normalized * (_settings.grabSpringForce * overstretch * grip);

                // Damping only on the component moving AWAY from grab point
                float awaySpeed = Vector2.Dot(_blobBody.velocity, -toGrab.normalized);
                if (awaySpeed > 0f)
                {
                    totalForce += toGrab.normalized * (awaySpeed * _settings.grabDamping);
                }
            }

            // ── 2. Pull toward grab: works in ANY direction ──
            //    When body is far from grab point, apply force toward it.
            //    Not just upward — horizontal grabs pull horizontally.
            if (distToGrab > 0.3f)
            {
                float proximityFactor = 1f - Mathf.Clamp01(
                    distToGrab / _currentMaxReach) * _settings.pullUpDistanceFalloff;

                // Force direction: toward the grab point
                var pullDir = toGrab.normalized;

                // Scale by distance — further away = stronger pull
                float distScale = Mathf.Clamp01(distToGrab / _currentMaxReach);

                totalForce += pullDir * (_settings.pullUpForce * distScale * proximityFactor * grip);
            }

            // ── 3. Cursor steering: push body from grab point toward cursor ──
            var grabToCursor = _targetPosition - grabPos;
            if (grabToCursor.sqrMagnitude > 0.01f)
            {
                totalForce += grabToCursor.normalized * (_settings.cursorSteerForce * grip);
            }

            _blobBody.AddForce(totalForce);

            // ── 4. Cursor intent: player muscle ──
            ApplyCursorIntentForce();

            // ── Clamp upward velocity to prevent launches ──
            if (_blobBody.velocity.y > _settings.maxPullUpSpeed)
            {
                var v = _blobBody.velocity;
                v.y = _settings.maxPullUpSpeed;
                _blobBody.velocity = v;
            }

            // ── Slippery surfaces ──
            if (_grabbedSurface.isSlippery)
            {
                var pos = _handRb.position;
                pos += Vector2.down * (_grabbedSurface.slideSpeed * Time.fixedDeltaTime);
                _handRb.MovePosition(pos);
            }
        }

        // ────────────────────────────────────
        //  Max reach enforcement
        // ────────────────────────────────────

        private void EnforceMaxReach()
        {
            var shoulder = ShoulderPosition;
            var offset = TipPosition - shoulder;
            float dist = offset.magnitude;

            // Auto-release grab if body is too far from hand
            if (_state == HandState.Grabbed && dist > _settings.maxLength * 2f)
            {
                ReleaseGrab();
                _state = HandState.Idle;
                return;
            }

            if (dist > _currentMaxReach && dist > 0.001f)
            {
                var dir = offset / dist;
                _handRb.MovePosition(shoulder + dir * _currentMaxReach);

                float outVel = Vector2.Dot(_handRb.velocity, dir);
                if (outVel > 0f)
                    _handRb.velocity -= dir * outVel;
            }
        }

        // ────────────────────────────────────
        //  Collision: reactive force to body + grab detection
        // ────────────────────────────────────

        private bool _isTouchingSurface;
        private Vector2 _lastDesiredForce;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            _isTouchingSurface = true;

            // Grab detection
            if (_state != HandState.Active) return;

            var surface = collision.collider.GetComponent<GrabSurface>();
            if (surface == null) return;

            AttachGrab(surface);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            _isTouchingSurface = true;
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            _isTouchingSurface = false;
        }

        /// <summary>
        /// Called in UpdateActive. When the hand is pressing against a surface,
        /// the force the hand is trying to apply gets mirrored onto the body.
        /// 
        /// Hand tries to move RIGHT but is blocked by wall →
        /// body receives force to the LEFT (Newton's 3rd law, manually).
        /// Works for ANY direction — horizontal, vertical, diagonal.
        /// </summary>
        private void TransferReactiveForce(Vector2 desiredForce)
        {
            if (!_isTouchingSurface) return;

            // The hand wants to go in `desiredForce` direction but is blocked.
            // The blocked portion of that force pushes the body in the opposite direction.
            // We detect "blocked" by checking if the hand isn't moving despite force being applied.
            var handSpeed = _handRb.velocity.magnitude;
            var forceDir = desiredForce.normalized;
            float forceMag = desiredForce.magnitude;

            // How much the hand is blocked: if hand barely moves despite high force, it's fully blocked
            float blockFactor = Mathf.Clamp01(1f - handSpeed / (forceMag * 0.1f + 0.01f));

            var reactiveForce = -forceDir * forceMag * blockFactor * _settings.reactiveForceMult;
            _blobBody.AddForce(reactiveForce * Time.fixedDeltaTime, ForceMode2D.Impulse);
        }

        /// <summary>
        /// PIVOT MECHANIC: Hand is the pivot point (planted or grabbed).
        /// 
        /// Body is pushed in the direction FROM cursor TO grab point, extended past it.
        /// Works for ANY angle — vertical, horizontal, diagonal.
        /// 
        /// Horizontal grab: cursor is to the right of grab → body pushed left.
        /// Vertical grab: cursor is below grab → body pushed up.
        /// The further the cursor from the grab point, the stronger the push.
        /// </summary>
        private void ApplyCursorIntentForce()
        {
            var grabPos = TipPosition;

            // Direction: from cursor through grab point and beyond
            var cursorToGrab = grabPos - _targetPosition;
            float dist = cursorToGrab.magnitude;
            if (dist < 0.1f) return;

            var pushDir = cursorToGrab.normalized;
            float intensity = Mathf.Clamp01(dist / _settings.cursorIntentMaxDistance);

            var intentForce = pushDir * (_settings.cursorIntentForce * intensity * intensity);
            _blobBody.AddForce(intentForce, ForceMode2D.Force);
        }

        private void AttachGrab(GrabSurface surface)
        {
            _state = HandState.Grabbed;
            _grabbedSurface = surface;

            _grabJoint = gameObject.AddComponent<FixedJoint2D>();
            _grabJoint.connectedBody = null;
            _grabJoint.autoConfigureConnectedAnchor = false;
            _grabJoint.connectedAnchor = TipPosition;
            _grabJoint.dampingRatio = 1f;
            _grabJoint.frequency = 0f;

            _handRb.velocity = Vector2.zero;

            BlobEvents.FireHandGrabbedSurface(
                new HandGrabbedSurfaceEvent(_side, _grabbedSurface.GetComponent<Collider2D>(), TipPosition));
        }

        private void ReleaseGrab()
        {
            _grabbedSurface = null;

            if (_grabJoint != null)
            {
                Destroy(_grabJoint);
                _grabJoint = null;
            }

            BlobEvents.FireHandReleasedSurface(new HandReleasedSurfaceEvent(_side));
        }
    }
}