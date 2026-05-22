using UnityEngine;
using TheBlob.Physics;

namespace TheBlob.Visual
{
    /// <summary>
    /// Shakes the camera's parent object on damage.
    /// Subscribes to DamageHandler.OnDamage.
    /// Shake intensity decays over time.
    /// </summary>
    public class CameraShakeOnDamage : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DamageHandler _damageHandler;

        [Header("Shake Settings")]
        [Tooltip("Base shake intensity (units of displacement).")]
        [SerializeField] private float _baseIntensity = 0.3f;

        [Tooltip("Multiplier applied to damage value for intensity scaling. " +
                 "Final intensity = _baseIntensity + damage * _damageScale.")]
        [SerializeField] private float _damageScale = 0.05f;

        [Tooltip("How long the shake lasts (seconds).")]
        [SerializeField] private float _duration = 0.4f;

        [Tooltip("How fast the shake oscillates (shakes per second).")]
        [SerializeField] private float _frequency = 25f;

        [Tooltip("Decay curve. Higher = faster falloff at the start.")]
        [SerializeField] private float _decayExponent = 2f;

        private Vector3 _originalLocalPos;
        private float _shakeTimer;
        private float _currentIntensity;

        private void Awake()
        {
            _originalLocalPos = transform.localPosition;
        }

        private void OnEnable()
        {
            if (_damageHandler != null)
                _damageHandler.OnDamage += HandleDamage;
        }

        private void OnDisable()
        {
            if (_damageHandler != null)
                _damageHandler.OnDamage -= HandleDamage;

            transform.localPosition = _originalLocalPos;
        }

        private void HandleDamage(float damage)
        {
            float intensity = _baseIntensity + damage * _damageScale;

            // If already shaking, take the stronger value
            _currentIntensity = Mathf.Max(_currentIntensity, intensity);
            _shakeTimer = _duration;
        }

        private void LateUpdate()
        {
            if (_shakeTimer <= 0f)
            {
                transform.localPosition = _originalLocalPos;
                return;
            }

            _shakeTimer -= Time.deltaTime;

            // Decay: 1 at start, 0 at end, shaped by exponent
            float progress = Mathf.Clamp01(_shakeTimer / _duration);
            float decay = Mathf.Pow(progress, _decayExponent);

            float currentMag = _currentIntensity * decay;

            // Perlin-based offset for smooth randomness
            float time = Time.time * _frequency;
            float offsetX = (Mathf.PerlinNoise(time, 0f) - 0.5f) * 2f * currentMag;
            float offsetY = (Mathf.PerlinNoise(0f, time) - 0.5f) * 2f * currentMag;

            transform.localPosition = _originalLocalPos + new Vector3(offsetX, offsetY, 0f);

            if (_shakeTimer <= 0f)
            {
                _currentIntensity = 0f;
                transform.localPosition = _originalLocalPos;
            }
        }
    }
}