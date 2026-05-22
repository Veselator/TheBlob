using UnityEngine;
using TheBlob.Events;

namespace TheBlob.Input
{
    /// <summary>
    /// Reads mouse input and fires hand events.
    /// LMB → Left hand, RMB → Right hand.
    /// Attach to the Blob root object.
    /// </summary>
    public class InputRouter : MonoBehaviour
    {
        private Camera _mainCam;
        private bool _isAnyHandMoving = false;

        public bool IsAnyHandMoving => _isAnyHandMoving; 

        private void Awake()
        {
            _mainCam = Camera.main;
        }

        private void Update()
        {
            _isAnyHandMoving = false;

            ProcessHand(0, HandSide.Left);  // LMB
            ProcessHand(1, HandSide.Right); // RMB
        }

        private void ProcessHand(int mouseButton, HandSide side)
        {
            if (UnityEngine.Input.GetMouseButtonDown(mouseButton))
            {
                var worldPos = GetMouseWorldPosition();
                BlobEvents.FireHandGrabStart(new HandGrabStartEvent(side, worldPos));
            }

            if (UnityEngine.Input.GetMouseButton(mouseButton))
            {
                var worldPos = GetMouseWorldPosition();
                BlobEvents.FireHandMove(new HandMoveEvent(side, worldPos));
                _isAnyHandMoving = true;
            }

            if (UnityEngine.Input.GetMouseButtonUp(mouseButton))
            {
                BlobEvents.FireHandGrabEnd(new HandGrabEndEvent(side));
            }
        }

        private Vector2 GetMouseWorldPosition()
        {
            var mousePos = UnityEngine.Input.mousePosition;
            mousePos.z = -_mainCam.transform.position.z;
            return _mainCam.ScreenToWorldPoint(mousePos);
        }
    }
}
