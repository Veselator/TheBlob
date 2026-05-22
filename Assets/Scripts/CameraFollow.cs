using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _smoothTime = 0.5f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

    private Vector3 velocity;
    private float _startSmoothTime;

    private void Start()
    {
        _startSmoothTime = _smoothTime;
    }

    private void FixedUpdate()
    {
        Vector3 newPos = Vector3.SmoothDamp(transform.position, _target.position, ref velocity, _smoothTime);
        newPos.z = offset.z;
        transform.position = newPos;
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public void SetSmoothTime(float smooth)
    {
        _smoothTime = smooth;
    }

    public void ResetSmoothTime()
    {
        _smoothTime = _startSmoothTime;
    }
}
