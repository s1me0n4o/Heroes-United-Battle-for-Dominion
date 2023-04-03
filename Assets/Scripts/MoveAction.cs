using System.Collections.Generic;
using UnityEngine;

public class MoveAction : MonoBehaviour
{
	private static readonly int IsMoving = Animator.StringToHash("IsMoving");
	[SerializeField] private Animator _unitAnimator;
	[SerializeField] private int _maxMoveDistance = 5;
	private readonly float _movementSpeed = 4f;
	private readonly float _rotationSpeed = 10f;
	private readonly float _stoppingDistance = .1f;

	private Vector3 _targetPosition;

	private Unit _unit;

	private void Awake()
	{
		_targetPosition = transform.position;
		_unit = GetComponent<Unit>();
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

	public void Move(GridPosition gridPosition) =>
		_targetPosition = GridGenerator.Instance.GetWorldPosition(gridPosition);

	public bool IsValidActionGridPosition(GridPosition gridPosition)
	{
		var validGridPositions = GetValidGridPositions();
		return validGridPositions.Contains(gridPosition);
	}

	public List<GridPosition> GetValidGridPositions()
	{
		var validPositions = new List<GridPosition>();

		var unitGridPos = _unit.CurrentGridPosition;
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
}