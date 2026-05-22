using UnityEngine;

namespace TheBlob.Physics
{
    /// <summary>
    /// Attach to any collider the blob can grab onto.
    /// Controls grip properties per-surface.
    /// </summary>
    public class GrabSurface : MonoBehaviour
    {
        [Header("Grip Properties")]
        [Tooltip("How much force the hand can exert while gripping this surface. 0 = no grip, 1 = perfect grip.")]
        [Range(0f, 1f)]
        public float gripStrength = 1f;

        [Tooltip("If true, the hand slides slowly along the surface while gripping (like ice).")]
        public bool isSlippery;

        [Tooltip("Slide speed when slippery. Higher = faster slide.")]
        [Range(0f, 5f)]
        public float slideSpeed = 1f;

        [Header("Visual Feedback")]
        [Tooltip("Color tint to apply to the hand when grabbing this surface.")]
        public Color gripTint = Color.white;
    }
}
