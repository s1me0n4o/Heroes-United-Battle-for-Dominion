using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class AOEAction : BaseAction
{
	public Action OnAoeShoot;

	[FormerlySerializedAs("aoeSkillPrefab")] [SerializeField]
	private GameObject _aoeSkillPrefab;

	private int _maxAOEDistance = 7;

	private void Update()
	{
		if (!IsActive)
		{
			return;
		}
	}

	public override string GetActionName()
	{
		return "AOEAction";
	}

	public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
	{
		var projectileTransform = Instantiate(_aoeSkillPrefab, Unit.GetWorldPosition(), Quaternion.identity);
		var aoeProjectile = projectileTransform.GetComponent<AOEProjectile>();
		aoeProjectile.Setup(gridPosition, OnAoeActionComplete);

		OnAoeShoot?.Invoke();
		ActionStart(onActionComplete, EventArgs.Empty);
	}

	private void OnAoeActionComplete()
	{
		ActionComplete();
	}

	public override List<GridPosition> GetValidGridPositions()
	{
		var validPositions = new List<GridPosition>();
		var unitGridPos = Unit.GetCurrentGridPosition();

		// current unit to be in the middle of the search
		for (var x = -_maxAOEDistance; x <= _maxAOEDistance; x++)
		{
			for (var z = -_maxAOEDistance; z <= _maxAOEDistance; z++)
			{
				var offsetGridPos = new GridPosition(x, z);
				var testGridPos = unitGridPos + offsetGridPos;

				if (!GridGenerator.Instance.IsValidGridPosition(testGridPos))
					continue;

				// check diamante aria not a square
				var testDistance = Mathf.Abs(x) + Mathf.Abs(z);
				if (testDistance > _maxAOEDistance)
					continue;

				validPositions.Add(testGridPos);
			}
		}

		return validPositions;
	}

	protected override EnemyAIAction GetEnemyAIAction(GridPosition gridPos)
	{
		return new EnemyAIAction()
		{
			GridPosition = gridPos,
			ActionValue = 0
		};
	}
}