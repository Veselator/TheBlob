using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private Animation _anim;

    public void StartTutorial()
    {
        _anim.Play();
    }
}
