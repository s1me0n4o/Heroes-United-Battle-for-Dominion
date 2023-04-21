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

	[SerializeField] private List<GridVisualColorMaterial> _gridVisualColorMaterials;

	private SingleGridVisual[,] _gridSystemVisuals;

	[Serializable]
	public struct GridVisualColorMaterial
	{
		public GridVisualColors Color;
		public Material Material;
	}

	public enum GridVisualColors
	{
		Green,
		Blue,
		Red,
		Yellow,
		RedSoft,
	}


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

	/////////////////////////////////////////

	private void HideAllGridPositions()
	{
		for (int i = 0; i < GridGenerator.Instance.GridWidth; i++)
		{
			for (int j = 0; j < GridGenerator.Instance.GridHeight; j++)
			{
				_gridSystemVisuals[i, j].Hide();
			}
		}
	}

	private void ShowGridPositionLists(List<GridPosition> gridPositions, GridVisualColors color)
	{
		foreach (var gridPos in gridPositions)
		{
			_gridSystemVisuals[gridPos.x, gridPos.z].Show(GetMaterialByColor(color));
		}
	}

	private void UpdateGridVisual()
	{
		HideAllGridPositions();
		var selectedAction = UnitActionSystem.Instance.SelectedAction;
		if (selectedAction == null)
			return;
		var selectedUnit = UnitActionSystem.Instance.SelectedUnit;
		if (selectedUnit == null)
			return;

		GridVisualColors color;
		switch (selectedAction)
		{
			default:
			case MoveAction moveAction:
				color = GridVisualColors.Green;
				break;
			case ShootAction shootAction:
				color = GridVisualColors.Red;

				ShowGridPositionRange(selectedUnit.GetCurrentGridPosition(), shootAction.MaxShootRange,
					GridVisualColors.RedSoft);
				break;
		}

		ShowGridPositionLists(selectedAction.GetValidGridPositions(), color);
	}

	private Material GetMaterialByColor(GridVisualColors color)
	{
		foreach (var gridVisualColorMaterial in _gridVisualColorMaterials)
		{
			if (gridVisualColorMaterial.Color == color)
			{
				return gridVisualColorMaterial.Material;
			}
		}

		Debug.LogError($"Could not find material for color {color}");
		return null;
	}

	private void ShowGridPositionRange(GridPosition gridPosition, int range, GridVisualColors color)
	{
		var gridPositionList = new List<GridPosition>();

		for (int x = -range; x <= range; x++)
		{
			for (int z = -range; z <= range; z++)
			{
				var testGridPosition = gridPosition + new GridPosition(x, z);

				if (!GridGenerator.Instance.IsValidGridPosition(testGridPosition))
				{
					continue;
				}

				var testDistance = Mathf.Abs(x) + Mathf.Abs(z);
				if (testDistance > range)
				{
					continue;
				}

				gridPositionList.Add(testGridPosition);
			}
		}

		ShowGridPositionLists(gridPositionList, color);
	}
}