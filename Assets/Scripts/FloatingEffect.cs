using UnityEngine;

namespace TheBlob.Visual
{
    /// <summary>
    /// Smooth random floating around the starting position.
    /// Works with both Transform (world objects) and RectTransform (UI elements).
    /// </summary>
    public class FloatingEffect : MonoBehaviour
    {
        [Header("Movement")]
        [Tooltip("Maximum distance from start position.")]
        [SerializeField] private float _radius = 0.5f;

        [Tooltip("How fast it moves toward new target points. Higher = snappier.")]
        [SerializeField] private float _smoothSpeed = 2f;

        [Tooltip("How often a new random target is picked (seconds).")]
        [SerializeField] private float _retargetInterval = 1.5f;

        [Header("Axis Lock")]
        [SerializeField] private bool _moveX = true;
        [SerializeField] private bool _moveY = true;

        private Vector3 _origin;
        private Vector3 _targetOffset;
        private Vector3 _currentOffset;
        private float _retargetTimer;
        private bool _isRectTransform;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _isRectTransform = _rectTransform != null;

            _origin = _isRectTransform
                ? (Vector3)_rectTransform.anchoredPosition
                : transform.localPosition;

            PickNewTarget();
        }

        private void Update()
        {
            _retargetTimer -= Time.deltaTime;
            if (_retargetTimer <= 0f)
                PickNewTarget();

            _currentOffset = Vector3.Lerp(_currentOffset, _targetOffset, _smoothSpeed * Time.deltaTime);

            if (_isRectTransform)
                _rectTransform.anchoredPosition = (Vector2)(_origin + _currentOffset);
            else
                transform.localPosition = _origin + _currentOffset;
        }

        private void PickNewTarget()
        {
            var random = Random.insideUnitCircle * _radius;

            _targetOffset = new Vector3(
                _moveX ? random.x : 0f,
                _moveY ? random.y : 0f,
                0f
            );

            _retargetTimer = _retargetInterval * Random.Range(0.7f, 1.3f);
        }
    }
}