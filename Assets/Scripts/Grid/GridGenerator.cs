using System;
using System.Collections.Generic;
using GridSystem;
using UnityEngine;
using Utils;

public class GridGenerator : MonoSingleton<GridGenerator>
{
	public static event Action OnUnitMove;

	[SerializeField] private Transform debugGo;
	private Grid<GridObject> _grid;

	private void Start()
	{
		_grid = new Grid<GridObject>(10, 10, 2f, Vector3.zero, gameObject.transform,
			(grid, x, z) =>
				new GridObject(grid, x, z));
		_grid.CreateDebugObjects(debugGo, true);
	}

	/////////////////////////////////////////

	public void AddUnitAtGridPosition(GridPosition gridPosition, Unit unit)
	{
		var gridObj = _grid.GetGridObject(gridPosition);
		gridObj.AddUnit(unit);
	}

	public List<Unit> GetUnitListAtGridPosition(GridPosition gridPosition)
	{
		var gridObj = _grid.GetGridObject(gridPosition);
		return gridObj.GetUnitList();
	}

	public void RemoveUnitAtGridPosition(GridPosition gridPosition, Unit unit)
	{
		var gridObj = _grid.GetGridObject(gridPosition);
		gridObj.RemoveUnit(unit);
	}

	public void UnitMoveGridPosition(Unit unit, GridPosition fromPos, GridPosition toPos)
	{
		RemoveUnitAtGridPosition(fromPos, unit);
		AddUnitAtGridPosition(toPos, unit);

		OnUnitMove?.Invoke();
	}

	public GridPosition GetGridPosition(Vector3 worldPos) => _grid.GetGridPosition(worldPos);

	public Vector3 GetWorldPosition(GridPosition gridPosition) =>
		_grid.GetWorldPosition(gridPosition.x, gridPosition.z);

	public bool IsValidGridPosition(GridPosition gridPosition) => _grid.IsValidGridPosition(gridPosition);

	public bool HasAnyUnitOnPosition(GridPosition gridPosition)
	{
		var gridObj = _grid.GetGridObject(gridPosition);
		return gridObj.HasAnyUnit();
	}

	public Unit GetUnitOnGridPosition(GridPosition gridPosition)
	{
		var gridObj = _grid.GetGridObject(gridPosition);
		return gridObj.GetUnit();
	}

	public int GridHeight => _grid.GetGridHeight();
	public int GridWidth => _grid.GetGridWidth();
}