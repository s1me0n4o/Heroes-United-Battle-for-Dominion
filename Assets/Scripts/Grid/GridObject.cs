using System.Collections.Generic;
using System.Linq;
using GridSystem;
using UnityEngine;

public class GridObject //: MonoBehaviour
{
	private readonly Grid<GridObject> _grid;
	private readonly GridPosition _gridPosition;
	private readonly List<Unit> _units;

	public GridObject(Grid<GridObject> grid, int x, int z)
	{
		_grid = grid;
		_gridPosition = new GridPosition(x, z);
		_units = new List<Unit>();
	}

	public void AddUnit(Unit unit) => _units.Add(unit);

	public List<Unit> GetUnitList() => _units;

	public void RemoveUnit(Unit unit) => _units.Remove(unit);

	public override string ToString()
	{
		var unitString = string.Empty;
		foreach (var unit in _units)
		{
			unitString += unit + "\n";
		}

		return _gridPosition + "\n" + unitString;
	}

	public bool HasAnyUnit() => _units.Any();

	public Unit GetUnit()
	{
		return HasAnyUnit() ? _units.FirstOrDefault() : null;
	}
}