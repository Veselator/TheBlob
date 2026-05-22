using UnityEngine;

namespace TheBlob.Data
{
    [CreateAssetMenu(fileName = "HandSettings", menuName = "TheBlob/Hand Settings")]
    public class HandSettings : ScriptableObject
    {
        [Header("Shoulder Offset")]
        [Tooltip("Local offset from body center where this arm visually attaches.")]
        public Vector2 shoulderOffset = new Vector2(-0.3f, 0.2f);

        [Header("Reach & Movement")]
        [Tooltip("Force applied toward cursor target.")]
        public float moveForce = 40f;

        [Header("Growth")]
        [Tooltip("How fast the max reach extends while mouse is held (units/sec).")]
        public float growthRate = 1.5f;

        [Tooltip("Starting / default arm length.")]
        public float baseLength = 1.5f;

        [Tooltip("Maximum length the arm can grow to.")]
        public float maxLength = 6f;

        [Tooltip("How fast the arm retracts to baseLength when released (units/sec).")]
        public float retractRate = 4f;

        [Header("Grab Physics")]
        [Tooltip("Spring force pulling the body toward grab point.")]
        public float grabSpringForce = 40f;

        [Tooltip("Damping on the spring to prevent oscillation.")]
        public float grabDamping = 8f;

        [Header("Grab: Pull-Up & Vault")]
        [Tooltip("Extra upward force applied when body is below the grab point. " +
                 "This is what lets the blob pull itself up onto ledges.")]
        public float pullUpForce = 25f;

        [Tooltip("Force that pushes body in the direction from grab point to cursor. " +
                 "Lets the player steer the body while hanging — move mouse past the ledge to vault over.")]
        public float cursorSteerForce = 15f;

        [Tooltip("How much closer to the grab point = stronger pull-up. " +
                 "At max distance the pull-up is weakest.")]
        [Range(0f, 1f)]
        public float pullUpDistanceFalloff = 0.5f;

        [Tooltip("Maximum speed the body can be pulled upward. Prevents launches.")]
        public float maxPullUpSpeed = 8f;

        [Header("Hand Rigidbody")]
        public float handMass = 0.5f;
        public float handGravityScale = 0.3f;
        public float handDrag = 3f;
        public float handAngularDrag = 5f;

        [Header("Hand Collider")]
        public float handColliderRadius = 0.25f;
        public float handFriction = 0.8f;
        public float handBounciness = 0f;

        [Header("Reactive Push (when hand collides with world)")]
        [Tooltip("Multiplier for the reactive force transferred to body when hand pushes against a surface.")]
        public float reactiveForceMult = 1.5f;

        [Header("Cursor Intent (player muscle)")]
        [Tooltip("When the hand is blocked (touching surface or grabbed), " +
                 "the direction from hand tip to cursor applies force to the body. " +
                 "This is the player's 'muscle' — pushing the mouse further = pushing harder.")]
        public float cursorIntentForce = 20f;

        [Tooltip("How far the cursor needs to be from the hand tip for full force (units).")]
        public float cursorIntentMaxDistance = 3f;

        [Header("Idle Dangle")]
        public Vector2 idleDangleDirection = new Vector2(-0.3f, -1f);
        public float idleReturnSpeed = 8f;
        public float idleSwayAmplitude = 0.15f;
        public float idleSwayFrequency = 1.5f;

        [Header("Visual")]
        public float handWidth = 0.4f;

        [Range(0f, 1f)]
        public float stretchThinning = 0.3f;

        [Range(4, 32)]
        public int meshSegments = 12;

        public float uvTilingY = 2f;
    }
}