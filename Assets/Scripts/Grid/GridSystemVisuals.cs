using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Utils;

public class GridSystemVisuals : MonoSingleton<GridSystemVisuals>
{
	[SerializeField] private Transform visualPrefab;

	private SingleGridVisual[,] _gridSystemVisuals;

	private void Start()
	{
		_gridSystemVisuals = new SingleGridVisual[
			GridGenerator.Instance.GridWidth,
			GridGenerator.Instance.GridHeight];
		for (int i = 0; i < GridGenerator.Instance.GridWidth; i++)
		{
			for (int j = 0; j < GridGenerator.Instance.GridHeight; j++)
			{
				var gridPos = new GridPosition(i, j);
				var go = Instantiate(visualPrefab, GridGenerator.Instance.GetWorldPosition(gridPos),
					quaternion.identity);
				go.SetParent(gameObject.transform);

				_gridSystemVisuals[i, j] = go.GetComponent<SingleGridVisual>();
			}
		}
	}

	private void Update()
	{
		UpgradeGridVisual();
	}

	public void HideAllGridPositions()
	{
		for (int i = 0; i < GridGenerator.Instance.GridWidth; i++)
		{
			for (int j = 0; j < GridGenerator.Instance.GridHeight; j++)
			{
				_gridSystemVisuals[i, j].Hide();
			}
		}
	}

	public void ShowGridPositionLists(List<GridPosition> gridPositions)
	{
		foreach (var gridPos in gridPositions)
		{
			_gridSystemVisuals[gridPos.x, gridPos.z].Show();
		}
	}

	public void UpgradeGridVisual()
	{
		HideAllGridPositions();
		var unit = UnitActionSystem.Instance.SelectedUnit;
		ShowGridPositionLists(unit.GetMoveAction().GetValidGridPositions());
	}
}