using UnityEngine;

public class GameBonusSfx : MonoBehaviour
{
    public static GameBonusSfx Instance { get; private set; }
    [SerializeField] private AudioSource _as;

    [SerializeField] private float _minPitch = 0.7f;
    [SerializeField] private float _maxPitch = 1.2f;

    private void Awake()
    {
        Instance = this;
    }

    public void PlaySound()
    {
        _as.pitch = Random.Range(_minPitch, _maxPitch);
        _as.Play();
    }
}
