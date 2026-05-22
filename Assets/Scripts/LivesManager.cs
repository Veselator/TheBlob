using System;
using UnityEngine;

namespace TheBlob
{
    /// <summary>
    /// Tracks player lives. When a life is lost, fires OnRespawn.
    /// When all lives are spent, triggers game over.
    /// 
    /// Call TakeLife() from EnthusiasmManager (when enthusiasm hits 0)
    /// or from any death trigger.
    /// </summary>
    public class LivesManager : MonoBehaviour
    {
        public static LivesManager Instance { get; private set; }

        [SerializeField] private int _startLives = 3;
        [SerializeField] private GameOverManager _gom;

        private int _currentLives;

        public int CurrentLives => _currentLives;
        public int MaxLives => _startLives;

        /// <summary>
        /// Fired when the player loses a life but still has lives remaining.
        /// RespawnManager listens to this.
        /// </summary>
        public event Action OnRespawn;

        /// <summary>
        /// Fired whenever life count changes. UI can listen to this.
        /// </summary>
        public event Action<int> OnLivesChanged;

        private void Awake()
        {
            Instance = this;
            _currentLives = _startLives;
        }

        public void TakeLife()
        {
            if (_gom.IsGameOver) return;

            _currentLives--;
            OnLivesChanged?.Invoke(_currentLives);

            if (_currentLives <= 0)
            {
                _gom.GameOver();
            }
            else
            {
                OnRespawn?.Invoke();
            }
        }

        public void ResetLives()
        {
            _currentLives = _startLives;
            OnLivesChanged?.Invoke(_currentLives);
        }
    }
}