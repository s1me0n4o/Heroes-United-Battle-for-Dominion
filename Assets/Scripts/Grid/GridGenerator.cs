using System.Collections.Generic;
using GridSystem;
using UnityEngine;
using Utils;

public class GridGenerator : MonoSingleton<GridGenerator>
{
	[SerializeField] private Transform debugGo;
	private Grid<GridObject> _grid;

	private void Start()
	{
		_grid = new Grid<GridObject>(10, 10, 2f, new Vector3(0, 0, 0), gameObject.transform,
			(grid, x, z) =>
				new GridObject(_grid, new GridPosition(x, z)));
		_grid.CreateDebugObjects(debugGo, true);
	}

	// private void Update()
	// {
	// 	Debug.Log(_grid.GetGridPosition(CameraHandler.Instance.GetWorldPositionOnPlane(Input.mousePosition)));
	// }


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
	}

	public GridPosition GetGridPosition(Vector3 worldPos)
	{
		return _grid.GetGridPosition(worldPos);
	}
}