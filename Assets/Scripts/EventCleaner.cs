using UnityEngine;
using UnityEngine.SceneManagement;
using TheBlob.Events;

namespace TheBlob
{
    /// <summary>
    /// Clears static event bus on scene unload.
    /// Attach to any persistent object or put on a DontDestroyOnLoad manager.
    /// Prevents memory leaks from static event subscriptions.
    /// </summary>
    public class EventCleaner : MonoBehaviour
    {
        private void OnEnable()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private void OnSceneUnloaded(Scene scene)
        {
            BlobEvents.ClearAll();
        }
    }
}
