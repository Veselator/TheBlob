using TheBlob.Visual;
using UnityEngine;

public class PickupableFood : MonoBehaviour
{
    [SerializeField] private float _addValue = 10f;
    [SerializeField] private GameObject _particle;
    private bool _isPickuped = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isPickuped) return;
        if (!collision.TryGetComponent<BlobVisual>(out _)) return;

        _isPickuped = true;
        EnthusiasmManager.Instance.AddEnthusiasm(_addValue);
        GameBonusSfx.Instance.PlaySound();
        if (_particle != null) Instantiate(_particle, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
