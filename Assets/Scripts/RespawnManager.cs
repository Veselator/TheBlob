using UnityEngine;
using TheBlob.Physics;

namespace TheBlob
{
    /// <summary>
    /// Stores the last claimed respawn point.
    /// On respawn: teleports body and hands to checkpoint, resets enthusiasm.
    /// 
    /// Singleton — Respawn checkpoints call SetRespawnPoint() on this.
    /// </summary>
    public class RespawnManager : MonoBehaviour
    {
        public static RespawnManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private Rigidbody2D _blobBody;
        [SerializeField] private HandController[] _hands;
        [SerializeField] private Transform _defaultRespawnPoint;

        [Header("Respawn Settings")]
        [Tooltip("Brief pause before respawning (seconds). Lets death VFX play.")]
        [SerializeField] private float _respawnDelay = 0.5f;

        private Vector2 _lastRespawnPosition;
        private LivesManager _livesManager;

        private void Awake()
        {
            Instance = this;

            if (_defaultRespawnPoint != null)
                _lastRespawnPosition = _defaultRespawnPoint.position;
            else
                _lastRespawnPosition = _blobBody.position;
        }

        private void Start()
        {
            _livesManager = LivesManager.Instance;
            if (_livesManager != null)
                _livesManager.OnRespawn += HandleRespawn;
        }

        private void OnDisable()
        {
            if (_livesManager != null)
                _livesManager.OnRespawn -= HandleRespawn;
        }

        public void SetRespawnPoint(Vector2 position)
        {
            _lastRespawnPosition = position;
        }

        private void HandleRespawn()
        {
            if (_respawnDelay > 0f)
                StartCoroutine(RespawnAfterDelay());
            else
                ExecuteRespawn();
        }

        private System.Collections.IEnumerator RespawnAfterDelay()
        {
            // Freeze body AND hands during delay
            _blobBody.simulated = false;
            foreach (var hand in _hands)
                hand.GetComponent<Rigidbody2D>().simulated = false;

            yield return new WaitForSeconds(_respawnDelay);

            ExecuteRespawn();

            // Re-enable simulation
            _blobBody.simulated = true;
            foreach (var hand in _hands)
                hand.GetComponent<Rigidbody2D>().simulated = true;
        }

        private void ExecuteRespawn()
        {
            // 1. Teleport body FIRST (hands read ShoulderPosition from body)
            _blobBody.position = _lastRespawnPosition;
            _blobBody.velocity = Vector2.zero;
            _blobBody.angularVelocity = 0f;
            _blobBody.transform.position = (Vector3)_lastRespawnPosition;

            // 2. Reset hands — releases grabs, teleports to idle dangle
            foreach (var hand in _hands)
            {
                hand.ResetPos();
            }

            // 3. Reset enthusiasm
            var enthusiasm = EnthusiasmManager.Instance;
            if (enthusiasm != null)
                enthusiasm.ResetEnthusiasm();
        }
    }
}