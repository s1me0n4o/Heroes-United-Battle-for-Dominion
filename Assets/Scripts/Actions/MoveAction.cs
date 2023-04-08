using System;
using System.Collections.Generic;
using UnityEngine;

public class MoveAction : BaseAction
{
	private static readonly int IsMoving = Animator.StringToHash("IsMoving");

	[SerializeField] private Animator _unitAnimator;
	[SerializeField] private int _maxMoveDistance = 5;

	private readonly float _movementSpeed = 4f;
	private readonly float _rotationSpeed = 10f;
	private readonly float _stoppingDistance = .01f;
	private Vector3 _targetPosition;

	public Vector3 TargetPosition => _targetPosition;

	protected override void Awake()
	{
		base.Awake();
		_targetPosition = transform.position;
	}

	private void Update()
	{
		if (!IsActive)
			return;
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
			ActionComplete();
		}
	}

	/////////////////////////////////////////

	public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
	{
		ActionStart(OnActionComplete);
		_targetPosition = GridGenerator.Instance.GetWorldPosition(gridPosition);
	}

	public override List<GridPosition> GetValidGridPositions()
	{
		var validPositions = new List<GridPosition>();

		var unitGridPos = Unit.GetCurrentGridPosition();
		// current unit to be in the middle of the search
		for (var x = -_maxMoveDistance; x <= _maxMoveDistance; x++)
		{
			for (var z = -_maxMoveDistance; z <= _maxMoveDistance; z++)
			{
				var offsetGridPos = new GridPosition(x, z);
				var testGridPos = unitGridPos + offsetGridPos;

				if (!GridGenerator.Instance.IsValidGridPosition(testGridPos))
					continue;

				if (unitGridPos == testGridPos)
				{
					// same grid pos that the unit is
					continue;
				}

				if (GridGenerator.Instance.HasAnyUnitOnPosition(testGridPos))
				{
					// grid pos already taken from another unit
					continue;
				}

				validPositions.Add(testGridPos);
			}
		}

		return validPositions;
	}

	public override string GetActionName()
	{
		return "Move";
	}
}