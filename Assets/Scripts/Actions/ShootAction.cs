using System;
using System.Collections.Generic;
using UnityEngine;

public class ShootAction : BaseAction
{
	public static Action OnAnyShoot;
	public Action OnShootStart;

	public class OnShootEventArgs : EventArgs
	{
		public Unit targgetUnit;
		public Unit shootingUnit;
	}

	public int MaxShootRange => _maxShootDistance;

	private enum ShootingState
	{
		Aiming,
		Shooting,
		Cooloff
	}

	private readonly int _maxShootDistance = 7;
	private readonly float _rotationSpeed = 10f;
	private float _stateTimer;
	private bool _canShoot;
	private ShootingState _state;
	private Unit _targetUnit;

	private void Update()
	{
		if (!IsActive)
			return;

		_stateTimer -= Time.deltaTime;
		switch (_state)
		{
			case ShootingState.Aiming:
				// rotate
				var aimDir = (_targetUnit.GetWorldPosition() - Unit.GetWorldPosition()).normalized;
				transform.forward = Vector3.Lerp(transform.forward, aimDir, Time.deltaTime * _rotationSpeed);

				break;
			case ShootingState.Shooting:
				if (_canShoot)
				{
					Shoot();
					_canShoot = false;
				}

				break;
			case ShootingState.Cooloff:
				break;
			default:
				break;
		}

		if (_stateTimer < 0)
		{
			NextState();
		}
	}

	private void Shoot()
	{
		var dmg = 40;
		_targetUnit.Damage(dmg);

		var targetUnitShootAtPos = _targetUnit.GetWorldPosition();
		targetUnitShootAtPos.y = 1f;
		OnShootStart?.Invoke();
		OnAnyShoot?.Invoke();
		Unit.InitBullet(targetUnitShootAtPos);
	}

	private void NextState()
	{
		switch (_state)
		{
			case ShootingState.Aiming:
				var shootingTime = 0.1f;
				SetStateData(ShootingState.Shooting, shootingTime);
				break;
			case ShootingState.Shooting:
				var coolOffTime = 0.5f;
				SetStateData(ShootingState.Cooloff, coolOffTime);
				break;
			case ShootingState.Cooloff:
				ActionComplete();
				break;
			default:
				break;
		}
	}

	public override string GetActionName()
	{
		return "Shoot";
	}

	public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
	{
		_targetUnit = GridGenerator.Instance.GetUnitOnGridPosition(gridPosition);

		SetStateData(ShootingState.Aiming, 1f);
		_canShoot = true;
		ActionStart(onActionComplete, new OnShootEventArgs
		{
			shootingUnit = Unit,
			targgetUnit = _targetUnit
		});
	}

	private void SetStateData(ShootingState state, float time)
	{
		_state = state;
		var aimingStateTime = time;
		_stateTimer = aimingStateTime;
	}

	public override List<GridPosition> GetValidGridPositions()
	{
		var unitGridPos = Unit.GetCurrentGridPosition();

		return GetValidGridPositions(unitGridPos);
	}

	public List<GridPosition> GetValidGridPositions(GridPosition unitGridPos)
	{
		var validPositions = new List<GridPosition>();

		// current unit to be in the middle of the search
		for (var x = -_maxShootDistance; x <= _maxShootDistance; x++)
		{
			for (var z = -_maxShootDistance; z <= _maxShootDistance; z++)
			{
				var offsetGridPos = new GridPosition(x, z);
				var testGridPos = unitGridPos + offsetGridPos;

				if (!GridGenerator.Instance.IsValidGridPosition(testGridPos))
					continue;

				// check diamante aria not a square
				var testDistance = Mathf.Abs(x) + Mathf.Abs(z);
				if (testDistance > _maxShootDistance)
					continue;

				if (!GridGenerator.Instance.HasAnyUnitOnPosition(testGridPos))
				{
					// grid pos is empty, no unit
					continue;
				}

				var targetUnit = GridGenerator.Instance.GetUnitOnGridPosition(testGridPos);

				if (targetUnit.IsEnemy == Unit.IsEnemy)
				{
					// both units on the same team
					continue;
				}

				validPositions.Add(testGridPos);
			}
		}

		return validPositions;
	}

	protected override EnemyAIAction GetEnemyAIAction(GridPosition gridPos)
	{
		var targetUnit = GridGenerator.Instance.GetUnitOnGridPosition(gridPos);

		return new EnemyAIAction()
		{
			GridPosition = gridPos,
			ActionValue = 100 + Mathf.RoundToInt((1 - targetUnit.GetHealthNormalized) * 100f),
		};
	}

	public int GetTargetCountAtPosition(GridPosition gridPosition)
	{
		return GetValidGridPositions(gridPosition).Count;
	}
}