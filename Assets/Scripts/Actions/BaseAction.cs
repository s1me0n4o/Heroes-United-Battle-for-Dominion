using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAction : MonoBehaviour
{
	protected Unit Unit;
	protected Action OnActionComplete;
	protected bool IsActive;

	protected virtual void Awake()
	{
		Unit = GetComponent<Unit>();
	}

	public abstract string GetActionName();

	public virtual int GetActionPointsCost()
	{
		return 1;
	}

	public abstract void TakeAction(GridPosition gridPosition, Action onActionComplete);

	public abstract List<GridPosition> GetValidGridPositions();

	public virtual bool IsValidActionGridPosition(GridPosition gridPosition)
	{
		var validGridPositions = GetValidGridPositions();
		return validGridPositions.Contains(gridPosition);
	}

	protected void ActionStart(Action onActionComplete)
	{
		IsActive = true;
		OnActionComplete = onActionComplete;
	}

	protected void ActionComplete()
	{
		IsActive = false;
		OnActionComplete();
	}
}