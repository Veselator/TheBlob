using UnityEngine;

namespace TheBlob.UI
{
    /// <summary>
    /// Controls a single heart icon in the lives display.
    /// Hides Top when current lives drop below this heart's index.
    /// 
    /// Example: 3 hearts with ids 0, 1, 2.
    /// 2 lives remaining → id 2 hides Top, ids 0 and 1 stay visible.
    /// </summary>
    public class LivesUI : MonoBehaviour
    {
        [SerializeField] private int _id;
        [SerializeField] private GameObject _top;

        private LivesManager _livesManager;

        private void Start()
        {
            _livesManager = LivesManager.Instance;
            if (_livesManager != null)
                _livesManager.OnLivesChanged += UpdateHeart;

            UpdateHeart(_livesManager != null ? _livesManager.CurrentLives : 0);
        }

        private void OnDestroy()
        {
            if (_livesManager != null)
                _livesManager.OnLivesChanged -= UpdateHeart;
        }

        private void UpdateHeart(int currentLives)
        {
            _top.SetActive(currentLives >= _id + 1);
        }
    }
}