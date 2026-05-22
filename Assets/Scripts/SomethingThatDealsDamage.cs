using UnityEngine;

public class SomethingThatDealsDamage : MonoBehaviour
{
    [SerializeField] private float _damage = 20f;
    public float Damage => _damage;
}
