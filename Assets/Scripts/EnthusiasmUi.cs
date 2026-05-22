using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnthusiasmUi : MonoBehaviour
{
    [SerializeField] private Image _filledImage;
    private EnthusiasmManager _eManager;

    private void Start()
    {
        _eManager = EnthusiasmManager.Instance;
        _eManager.OnEnthusiamsChanged += Fill;

        Fill(0f);
    }

    private void OnDestroy()
    {
        _eManager.OnEnthusiamsChanged -= Fill;
    }

    private void Fill(float _)
    {
        _filledImage.fillAmount = _eManager.EnthusiasmInPercentage;
    }
}
