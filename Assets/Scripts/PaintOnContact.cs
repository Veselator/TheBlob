using System.Collections.Generic;
using UnityEngine;
using TheBlob.Physics;

namespace TheBlob.Visual
{
    /// <summary>
    /// Spawns paint splat prefabs where the hand touches surfaces.
    /// Tracks hand state directly (not via events) to catch both
    /// grabbable and non-grabbable surface contacts.
    /// 
    /// Attach to the same GameObject as HandController.
    /// </summary>
    public class PaintOnContact : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HandController _handController;

        [Header("Paint")]
        [SerializeField] private GameObject _paintPrefab;

        [Tooltip("Maximum paint splats in the scene. Oldest removed when exceeded.")]
        [SerializeField] private int _maxPaintCount = 50;

        [Tooltip("Minimum distance between paint splats (prevents stacking).")]
        [SerializeField] private float _minDistance = 0.3f;

        [Tooltip("Random rotation applied to each splat (degrees).")]
        [SerializeField] private float _randomRotation = 360f;

        [Tooltip("Random scale range applied to each splat.")]
        [SerializeField] private Vector2 _scaleRange = new Vector2(0.8f, 1.2f);

        private static readonly Queue<GameObject> _paintPool = new Queue<GameObject>();
        private Vector2 _lastPaintPos;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TrySpawnPaint(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            TrySpawnPaint(collision);
        }

        private void TrySpawnPaint(Collision2D collision)
        {
            if (!_handController.IsActive && !_handController.IsGrabbing) return;

            for (int i = 0; i < collision.contactCount; i++)
            {
                var point = collision.GetContact(i).point;

                if (Vector2.Distance(point, _lastPaintPos) < _minDistance) continue;

                SpawnPaint(point, collision.GetContact(i).normal);
                _lastPaintPos = point;
                return;
            }
        }

        private void SpawnPaint(Vector2 position, Vector2 normal)
        {
            // Remove oldest if at limit
            while (_paintPool.Count >= _maxPaintCount)
            {
                var oldest = _paintPool.Dequeue();
                if (oldest != null)
                    Destroy(oldest);
            }

            float angle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg - 90f;
            angle += Random.Range(-_randomRotation * 0.5f, _randomRotation * 0.5f);

            var paint = Instantiate(
                _paintPrefab,
                position,
                Quaternion.Euler(0f, 0f, angle)
            );

            float scale = Random.Range(_scaleRange.x, _scaleRange.y);
            paint.transform.localScale *= scale;

            _paintPool.Enqueue(paint);
        }
    }
}