using System.Collections;
using UnityEngine;

namespace TheBlob.Cutscene
{
    /// <summary>
    /// Hardcoded intro cutscene.
    /// Disables player input, pauses enthusiasm, drives camera and dialogue.
    /// Can be skipped at any point.
    /// 
    /// Camera zoom uses SmoothDamp in Update — no coroutine conflicts,
    /// always smooth regardless of how many times target zoom changes.
    /// </summary>
    public class CutsceneManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TypingManager _typingManager;
        [SerializeField] private CameraFollow _cameraFollow;
        [SerializeField] private Camera _camera;
        [SerializeField] private TheBlob.Physics.HandController[] _hands;
        [SerializeField] private EnthusiasmManager _enthusiasmManager;
        [SerializeField] private TutorialManager _tutorialManager;

        [Header("Camera Targets")]
        [SerializeField] private Transform _playerTarget;
        [SerializeField] private Transform _secondTarget;
        [SerializeField] private Transform _npcTarget;

        [Header("Camera Zoom")]
        [Tooltip("Normal camera orthographic size.")]
        [SerializeField] private float _normalZoom = 5f;

        [Tooltip("Zoomed-in size at midpoint of camera pan.")]
        [SerializeField] private float _zoomedSize = 3.5f;

        [Tooltip("How smooth the zoom transition is. Lower = faster.")]
        [SerializeField] private float _zoomSmoothTime = 0.5f;

        [Header("Characters")]
        [SerializeField] private string _npcName = "???";
        [SerializeField] private GameObject _npcPortrait;

        [SerializeField] private string _playerName = "You";
        [SerializeField] private GameObject _playerPortrait;

        [Header("Skip")]
        [SerializeField] private KeyCode _skipKey = KeyCode.Escape;
        [SerializeField] private GameObject _skipHintUI;

        [SerializeField] private GameObject _uiToHide;

        private Coroutine _cutsceneCoroutine;
        private bool _isPlaying;

        // ── Smooth zoom state ──
        private float _targetZoom;
        private float _zoomVelocity;

        public bool IsPlaying => _isPlaying;

        private void Start()
        {
            StartCutscene();
        }

        public void StartCutscene()
        {
            if (_isPlaying) return;

            if (_uiToHide != null)
                _uiToHide.SetActive(false);

            _targetZoom = _normalZoom;
            _camera.orthographicSize = _normalZoom;
            _zoomVelocity = 0f;

            _cutsceneCoroutine = StartCoroutine(PlayCutscene());
        }

        private void Update()
        {
            if (_isPlaying && UnityEngine.Input.GetKeyDown(_skipKey))
            {
                SkipCutscene();
            }

            // Smooth zoom — always running, no coroutine needed
            if (_isPlaying)
            {
                _camera.orthographicSize = Mathf.SmoothDamp(
                    _camera.orthographicSize,
                    _targetZoom,
                    ref _zoomVelocity,
                    _zoomSmoothTime
                );
            }
        }

        /// <summary>
        /// Set target zoom. Camera will smoothly transition to it.
        /// Call this instead of coroutine-based zoom.
        /// </summary>
        private void SetZoom(float targetSize)
        {
            _targetZoom = targetSize;
        }

        private IEnumerator PlayCutscene()
        {
            _isPlaying = true;
            BeginCutscene();
            yield return new WaitForSeconds(1f);

            // ── Line 1: "Hey you!" — 2 seconds ──
            _cameraFollow.SetTarget(_playerTarget);
            yield return ShowLineAndWait(_npcName, _npcPortrait, "Hey you!", 0.5f, 2f);

            // ── Camera pan: player → second target, zoom in — 3 seconds ──
            _typingManager.Hide();
            _cameraFollow.SetSmoothTime(0.8f);
            _cameraFollow.SetTarget(_secondTarget);
            SetZoom(_zoomedSize);
            yield return new WaitForSeconds(3f);

            // ── Camera to NPC, zoom back to normal ──
            SetZoom(_normalZoom);
            _cameraFollow.SetTarget(_npcTarget);

            // ── Line 2: "Go to town!" — 3 seconds ──
            yield return ShowLineAndWait(_npcName, _npcPortrait, "Go to town!", 0.8f, 3f);
            yield return new WaitForSeconds(2f);

            // ── Camera returns to player ──
            _typingManager.Hide();
            _cameraFollow.SetTarget(_playerTarget);
            _cameraFollow.ResetSmoothTime();
            yield return new WaitForSeconds(1f);

            // ── Line 3: "Blob" — 1 second ──
            yield return ShowLineAndWait(_playerName, _playerPortrait, "Blob", 0.3f, 1f);

            // ── End ──
            EndCutscene();
        }

        private IEnumerator ShowLineAndWait(string name, GameObject portrait, string text,
                                            float typingDuration, float totalDuration)
        {
            _typingManager.ShowLine(name, portrait, text, typingDuration);
            yield return new WaitForSeconds(totalDuration);
        }

        private void SkipCutscene()
        {
            if (_cutsceneCoroutine != null)
                StopCoroutine(_cutsceneCoroutine);

            _camera.orthographicSize = _normalZoom;
            _targetZoom = _normalZoom;
            _zoomVelocity = 0f;

            EndCutscene();
        }

        private void BeginCutscene()
        {
            foreach (var hand in _hands)
                hand.Pause();

            if (_enthusiasmManager != null)
                _enthusiasmManager.Pause();

            if (_skipHintUI != null)
                _skipHintUI.SetActive(true);
        }

        private void EndCutscene()
        {
            _isPlaying = false;

            if (_uiToHide != null)
                _uiToHide.SetActive(true);

            _typingManager.Hide();
            _cameraFollow.ResetSmoothTime();

            _cameraFollow.SetTarget(_playerTarget);
            _camera.orthographicSize = _normalZoom;
            _targetZoom = _normalZoom;

            foreach (var hand in _hands)
                hand.Unpause();

            if (_enthusiasmManager != null)
                _enthusiasmManager.Unpause();

            if (_skipHintUI != null)
                _skipHintUI.SetActive(false);

            _tutorialManager.StartTutorial();
        }
    }
}