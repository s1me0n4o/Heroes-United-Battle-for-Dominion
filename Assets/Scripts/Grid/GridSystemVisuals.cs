using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

public class GridSystemVisuals : MonoSingleton<GridSystemVisuals>
{
	[FormerlySerializedAs("visualPrefab")] [SerializeField]
	private Transform _visualPrefab;

	private SingleGridVisual[,] _gridSystemVisuals;

	private void OnEnable()
	{
		UnitActionSystem.OnSelectedAction += OnActionSelected;
		GridGenerator.OnUnitMove += OnUnitMove;
		UnitActionSystem.OnSelectedUnitChanged += OnUnitSelected;
	}

	private void OnDisable()
	{
		UnitActionSystem.OnSelectedAction -= OnActionSelected;
		GridGenerator.OnUnitMove -= OnUnitMove;
		UnitActionSystem.OnSelectedUnitChanged -= OnUnitSelected;
	}

	private void OnUnitMove()
	{
		UpdateGridVisual();
	}

	private void OnActionSelected()
	{
		UpdateGridVisual();
	}

	private void OnUnitSelected(Unit obj)
	{
		HideAllGridPositions();
	}

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
				var go = Instantiate(_visualPrefab, GridGenerator.Instance.GetWorldPosition(gridPos),
					quaternion.identity);
				go.SetParent(gameObject.transform);

				_gridSystemVisuals[i, j] = go.GetComponent<SingleGridVisual>();
			}
		}

		UpdateGridVisual();
	}

	// private void Update()
	// {
	// 	UpdateGridVisual();
	// }

	/////////////////////////////////////////

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

	private void UpdateGridVisual()
	{
		HideAllGridPositions();
		var selectedAction = UnitActionSystem.Instance.SelectedAction;
		if (selectedAction == null)
			return;
		ShowGridPositionLists(selectedAction.GetValidGridPositions());
	}
}