using System;
using Camera;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

public class UnitActionSystem : MonoSingleton<UnitActionSystem>
{
	[SerializeField] private Unit _selectedUnit;
	private DefaultActions _defaultActions;

	public Unit SelectedUnit => _selectedUnit;
	private DefaultActions DefaultActions => _defaultActions ??= new DefaultActions();

	private void OnEnable()
	{
		DefaultActions.Enable();
		DefaultActions.UI.RightClick.performed += OnRightClickPerformed;
		CameraHandler.OnInteractableSelected += OnUnitSelected;
	}

	private void OnDisable()
	{
		DefaultActions.UI.RightClick.performed -= OnRightClickPerformed;
		DefaultActions.Disable();
		CameraHandler.OnInteractableSelected -= OnUnitSelected;
	}

	private void OnUnitSelected(Unit unit) => _selectedUnit = unit;

	private void OnRightClickPerformed(InputAction.CallbackContext ctx)
	{
		if (_selectedUnit == null)
			return;

		var mousePosition = Mouse.current.position.ReadValue();
		var worldPositionOnPlane = CameraHandler.Instance.GetWorldPositionOnPlane(mousePosition);
		var mouseGridPosition = GridGenerator.Instance.GetGridPosition(worldPositionOnPlane);
		Debug.Log($"MouseGridPos - {mouseGridPosition}");
		if (_selectedUnit.GetMoveAction().IsValidActionGridPosition(mouseGridPosition))
			_selectedUnit.GetMoveAction().Move(mouseGridPosition);
	}

	// private void Update()
	// {
	// 	var screenPos = DefaultActions.UI.Point.ReadValue<Vector2>();
	// 	var worldPositionOnPlane = CameraHandler.Instance.GetWorldPositionOnPlane(screenPos);
	// 	var mouseGridPosition = GridGenerator.Instance.GetGridPosition(worldPositionOnPlane);
	// 	Debug.Log($"MouseGridPos - {mouseGridPosition}");
	// }
}