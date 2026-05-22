using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatePrefabOnDestroy : MonoBehaviour
{
    [SerializeField] private GameObject _prefab;

    private void OnDestroy()
    {
        Instantiate(_prefab, transform.position, Quaternion.identity);
    }
}
