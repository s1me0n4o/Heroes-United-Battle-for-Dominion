using System.Collections.Generic;
using GridSystem;
using UnityEngine;

public class GridObject : MonoBehaviour
{
	private readonly Grid<GridObject> _grid;
	private readonly GridPosition _gridPosition;
	private readonly List<Unit> _units;

	public GridObject(Grid<GridObject> grid, GridPosition gridPosition)
	{
		_grid = grid;
		_gridPosition = gridPosition;
		_units = new List<Unit>();
	}

	public void AddUnit(Unit unit)
	{
		_units.Add(unit);
	}

	public List<Unit> GetUnitList()
	{
		return _units;
	}

	public void RemoveUnit(Unit unit)
	{
		_units.Remove(unit);
	}

	public override string ToString()
	{
		var unitString = string.Empty;
		foreach (var unit in _units) unitString += unit + "\n";
		return _gridPosition + "\n" + unitString;
	}
}