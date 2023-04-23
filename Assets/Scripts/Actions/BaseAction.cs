using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BaseAction : MonoBehaviour
{
	public static event EventHandler OnAnyActionStart;
	public static event EventHandler OnAnyActionComplete;

	protected Unit Unit;
	protected Action OnActionComplete;
	protected bool IsActive;

	protected virtual void Awake() => Unit = GetComponent<Unit>();

	public abstract string GetActionName();

	public virtual int GetActionPointsCost() => 1;

	public abstract void TakeAction(GridPosition gridPosition, Action onActionComplete);

	public abstract List<GridPosition> GetValidGridPositions();

	public virtual bool IsValidActionGridPosition(GridPosition gridPosition)
	{
		var validGridPositions = GetValidGridPositions();
		return validGridPositions.Contains(gridPosition);
	}

	protected void ActionStart(Action onActionComplete, EventArgs eventArgs)
	{
		IsActive = true;
		OnActionComplete = onActionComplete;

		OnAnyActionStart?.Invoke(this, eventArgs);
	}

	protected void ActionComplete()
	{
		IsActive = false;
		OnActionComplete();

		OnAnyActionComplete?.Invoke(this, EventArgs.Empty);
	}

	public EnemyAIAction GetBestEnemyAIAction()
	{
		var enemyAIActions = new List<EnemyAIAction>();

		var validActionGridPositions = GetValidGridPositions();
		foreach (var gridPos in validActionGridPositions)
		{
			var enemyAIAction = GetEnemyAIAction(gridPos);
			enemyAIActions.Add(enemyAIAction);
		}

		if (enemyAIActions.Count > 0)
		{
			enemyAIActions.Sort((EnemyAIAction a, EnemyAIAction b) => b.ActionValue - a.ActionValue);
			return enemyAIActions.FirstOrDefault();
		}

		// no possible enemy actions
		return null;
	}

	public abstract EnemyAIAction GetEnemyAIAction(GridPosition gridPos);
}