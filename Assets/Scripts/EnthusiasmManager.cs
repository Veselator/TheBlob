using System;
using TheBlob;
using TheBlob.Input;
using UnityEngine;

public class EnthusiasmManager : MonoBehaviour
{
    public static EnthusiasmManager Instance { get; private set; }

    [SerializeField] private float _startEnthusiasm = 100f;
    [SerializeField] private float _maxEnthusiasm = 100f;

    [SerializeField] private float _enthusiasmConcumptionRate = 0.3f;
    [SerializeField] private float _movingFactor = 1.2f;

    // Yes, it's highly coupled code. But dude, it's a game jam and 48 hours
    [SerializeField] private InputRouter _router; // To detect any hand movement
    [SerializeField] private GameOverManager _gom;

    private float _enthusiasm;

    private float enthusiasm
    {
        get => _enthusiasm;
        set
        {
            _enthusiasm = Mathf.Clamp(value, 0f, _maxEnthusiasm);
            if (_enthusiasm <= 0f)
            {
                Pause();
                LivesManager.Instance.TakeLife();
            }

            OnEnthusiamsChanged?.Invoke(_enthusiasm);
        }
    }

    public float Enthusiasm => _enthusiasm;
    public float EnthusiasmInPercentage => _enthusiasm / _maxEnthusiasm;
    private bool _isPaused = false;

    public event Action<float> OnEnthusiamsChanged;

    private void Awake()
    {
        _enthusiasm = _startEnthusiasm;
        Instance = this;
    }

    public void AddEnthusiasm(float value)
    {
        enthusiasm += value;
    }

    public void ResetEnthusiasm()
    {
        enthusiasm = _startEnthusiasm;
        Unpause();
    }

    public void Pause()
    {
        _isPaused = true;
    }

    public void Unpause()
    {
        _isPaused = false;
    }

    private void Update()
    {
        if (_isPaused || _gom.IsGameOver) return;

        enthusiasm -= _router.IsAnyHandMoving ? _enthusiasmConcumptionRate * _movingFactor * Time.deltaTime : _enthusiasmConcumptionRate * Time.deltaTime;
    }
}
