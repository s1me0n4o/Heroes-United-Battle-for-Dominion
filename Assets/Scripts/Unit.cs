using System;
using Camera;
using UnityEngine;
using UnityEngine.InputSystem;

public class Unit : MonoBehaviour
{
    [SerializeField] private Animator _unitAnimator;
    
    private Vector3 _targetPosition;
    private readonly float _movementSpeed = 4f;
    private readonly float _stoppingDistance = .1f;
    private readonly float _rotationSpeed = 10f;
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");

    private void Awake()
    {
        _targetPosition = transform.position;
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, _targetPosition) > _stoppingDistance)
        {
            // we dont want the magnitude that is why we normalize the vector.
            var moveDir = (_targetPosition - transform.position).normalized;
            transform.position += moveDir * _movementSpeed * Time.deltaTime;

            // rotate
            transform.forward = Vector3.Lerp(transform.forward, moveDir, Time.deltaTime * _rotationSpeed);
            _unitAnimator.SetBool(IsMoving, true);
        }
        else
        {
            _unitAnimator.SetBool(IsMoving, false);
        }
    }

    public void Move(Vector3 targetPos) => _targetPosition = targetPos;
}
