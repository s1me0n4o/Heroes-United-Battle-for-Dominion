using System;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class MoveAction : BaseAction
{
	public Action OnStartMoving;
	public Action OnStopMoving;

	[SerializeField] private int _maxMoveDistance = 5;

	private readonly float _movementSpeed = 4f;
	private readonly float _rotationSpeed = 10f;
	private readonly float _stoppingDistance = .03f;
	private Vector3 _targetPosition;
	private AIPath _aiPath;
	private AIDestinationSetter _aiDestinationSetter;

	public Vector3 TargetPosition => _targetPosition;

	protected override void Awake()
	{
		base.Awake();
		_targetPosition = transform.position;
		_aiPath = GetComponent<AIPath>();
		_aiDestinationSetter = GetComponent<AIDestinationSetter>();
	}

	private void Update()
	{
		if (!IsActive)
			return;
		if (Vector3.Distance(transform.position, _targetPosition) > _stoppingDistance)
			return;
		OnStopMoving?.Invoke();
		ActionComplete();
	}

	/////////////////////////////////////////

	public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
	{
		_targetPosition = GridGenerator.Instance.GetWorldPosition(gridPosition);
		_aiPath.destination = _targetPosition;

		OnStartMoving?.Invoke();
		ActionStart(onActionComplete, EventArgs.Empty);
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

	protected override EnemyAIAction GetEnemyAIAction(GridPosition gridPos)
	{
		// prioritise position with a lot of enemies
		var targetCountAtGridPos = Unit.GetAction<ShootAction>().GetTargetCountAtPosition(gridPos);
		return new EnemyAIAction()
		{
			GridPosition = gridPos,
			ActionValue = targetCountAtGridPos * 10,
		};
	}

	public override string GetActionName()
	{
		return "Move";
	}
}