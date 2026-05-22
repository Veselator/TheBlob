using TheBlob.Visual;
using UnityEngine;

public class ShowWin : MonoBehaviour
{
    [SerializeField] private GameObject _winCanvas;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent<BlobVisual>(out _)) return;

        _winCanvas.SetActive(true);
    }
}
