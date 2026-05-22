using UnityEngine;

namespace TheBlob.Visual
{
    /// <summary>
    /// Parallax effect for background layers.
    /// Moves this object relative to camera movement at a configurable ratio.
    /// Also scrolls the DiagonalStripes shader offset for seamless tiling.
    /// 
    /// Works with any number of layers — put one script per background object,
    /// set different parallax amounts (0 = static, 1 = moves with camera).
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _cameraTransform;

        [Header("Parallax")]
        [Tooltip("How much this layer moves relative to camera. " +
                 "0 = fixed background, 0.5 = half speed, 1 = moves with camera (foreground).")]
        [SerializeField, Range(0f, 1f)] private float _parallaxAmount = 0.2f;

        [Tooltip("Enable vertical parallax as well.")]
        [SerializeField] private bool _verticalParallax = true;

        [Header("Shader Scroll (optional)")]
        [Tooltip("If assigned, scrolls the _Offset property on the material for seamless tiling.")]
        [SerializeField] private Renderer _renderer;

        [Tooltip("How fast the stripe pattern scrolls relative to parallax movement.")]
        [SerializeField] private float _shaderScrollScale = 1f;

        private Vector3 _startPos;
        private Vector3 _camStartPos;
        private Material _material;
        private static readonly int OffsetID = Shader.PropertyToID("_Offset");

        private void Start()
        {
            _startPos = transform.position;
            _camStartPos = _cameraTransform.position;

            if (_renderer != null)
                _material = _renderer.material;
        }

        private void LateUpdate()
        {
            var camDelta = _cameraTransform.position - _camStartPos;

            float x = camDelta.x * _parallaxAmount;
            float y = _verticalParallax ? camDelta.y * _parallaxAmount : 0f;

            transform.position = _startPos + new Vector3(x, y, 0f);

            // Scroll shader offset for seamless stripe movement
            if (_material != null)
            {
                var offset = new Vector4(
                    camDelta.x * _parallaxAmount * _shaderScrollScale,
                    camDelta.y * _parallaxAmount * _shaderScrollScale,
                    0f, 0f
                );
                _material.SetVector(OffsetID, offset);
            }
        }
    }
}
