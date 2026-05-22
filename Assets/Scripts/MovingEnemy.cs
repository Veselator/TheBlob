using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingEnemy : MonoBehaviour
{
    [SerializeField] private Transform[] _points;
    [SerializeField] private float _speed = 3f;
    [SerializeField] private float _threshold = 0.01f;

    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField] private bool _flipSpriteOnPoints = false;

    private int _curentPoint;

    private Vector2 _target;

    private void Start()
    {
        transform.position = _points[0].position;
        NextPoint();
    }

    private void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, _target, _speed * Time.deltaTime);
        if (Vector2.Distance(transform.position, _target) < _threshold) NextPoint();
    }

    private void NextPoint()
    {
        _curentPoint = (_curentPoint + 1) % _points.Length;
        _target = _points[_curentPoint].position;

        if(_flipSpriteOnPoints) _sprite.flipX = !_sprite.flipX;
    }
}
