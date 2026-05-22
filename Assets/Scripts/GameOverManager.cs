using System.Collections;
using UnityEngine;

namespace TheBlob
{
    public class GameOverManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera _camera;
        [SerializeField] private GameObject _gameOverText;

        [Header("Camera Zoom")]
        [Tooltip("Target orthographic size when zooming in on death.")]
        [SerializeField] private float _zoomedSize = 2.5f;

        [Tooltip("How long the zoom-in takes (seconds).")]
        [SerializeField] private float _zoomDuration = 1f;

        [Tooltip("Delay before showing game over text (seconds).")]
        [SerializeField] private float _textDelay = 0.5f;

        private float _originalSize;
        private bool _isGameOver;

        public bool IsGameOver => _isGameOver;

        private void Awake()
        {
            _originalSize = _camera.orthographicSize;

            if (_gameOverText != null)
                _gameOverText.SetActive(false);
        }

        public void GameOver()
        {
            if (_isGameOver) return;
            _isGameOver = true;

            StartCoroutine(GameOverSequence());
        }

        public void ResetGameOver()
        {
            _isGameOver = false;
            _camera.orthographicSize = _originalSize;

            if (_gameOverText != null)
                _gameOverText.SetActive(false);
        }

        private IEnumerator GameOverSequence()
        {
            // Zoom in
            float elapsed = 0f;
            float startSize = _camera.orthographicSize;

            while (elapsed < _zoomDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / _zoomDuration);
                _camera.orthographicSize = Mathf.Lerp(startSize, _zoomedSize, t);
                yield return null;
            }

            _camera.orthographicSize = _zoomedSize;

            // Delay before text
            yield return new WaitForSeconds(_textDelay);

            // Show game over text
            if (_gameOverText != null)
                _gameOverText.SetActive(true);
        }
    }
}