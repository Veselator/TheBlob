using UnityEngine;
using TheBlob.Physics;

namespace TheBlob.Audio
{
    /// <summary>
    /// Plays a random sound with random pitch when the blob takes damage.
    /// Subscribes to DamageHandler.OnDamage.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioOnDamage : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DamageHandler _damageHandler;

        [Header("Sounds")]
        [SerializeField] private AudioClip[] _clips;

        [Header("Pitch")]
        [SerializeField] private float _minPitch = 0.8f;
        [SerializeField] private float _maxPitch = 1.2f;

        private AudioSource _source;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _source.playOnAwake = false;
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
        }

        private void HandleDamage(float damage)
        {
            if (_clips == null || _clips.Length == 0) return;

            _source.pitch = Random.Range(_minPitch, _maxPitch);
            _source.PlayOneShot(_clips[Random.Range(0, _clips.Length)]);
        }
    }
}