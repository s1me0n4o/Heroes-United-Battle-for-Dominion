using System;
using Camera;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitActionSystem : MonoBehaviour
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

	private void OnUnitSelected(Unit unit)
	{
		_selectedUnit = unit;
	}

	private void OnRightClickPerformed(InputAction.CallbackContext ctx)
	{
		if (_selectedUnit == null)
			return;

		var screenPos = DefaultActions.UI.Point.ReadValue<Vector2>();
		var worldPositionOnPlane = CameraHandler.Instance.GetWorldPositionOnPlane(screenPos);
		Debug.Log(worldPositionOnPlane);
		var mouseGridPosition = GridGenerator.Instance.GetGridPosition(worldPositionOnPlane);
		Debug.Log($"MouseGridPos - {mouseGridPosition}");
		if (_selectedUnit.MoveAction.IsValidActionGridPosition(mouseGridPosition))
			_selectedUnit.MoveAction.Move(mouseGridPosition);
	}

	// private void Update()
	// {
	// 	var screenPos = DefaultActions.UI.Point.ReadValue<Vector2>();
	// 	var worldPositionOnPlane = CameraHandler.Instance.GetWorldPositionOnPlane(screenPos);
	// 	var mouseGridPosition = GridGenerator.Instance.GetGridPosition(worldPositionOnPlane);
	// 	Debug.Log($"MouseGridPos - {mouseGridPosition}");
	// }
}