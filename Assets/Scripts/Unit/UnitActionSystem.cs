using System;
using Camera;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

public class UnitActionSystem : MonoSingleton<UnitActionSystem>
{
	[SerializeField] private Unit _selectedUnit;
	private DefaultActions _defaultActions;
	private DefaultActions DefaultActions => _defaultActions ??= new DefaultActions();
	private bool _isBusy;

	public Unit SelectedUnit => _selectedUnit;

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

	//////////////////////////////////////////
	private void OnUnitSelected(Unit unit) => _selectedUnit = unit;

	private void OnRightClickPerformed(InputAction.CallbackContext ctx)
	{
		if (_isBusy)
			return;

		if (_selectedUnit == null)
			return;

		var mousePosition = Mouse.current.position.ReadValue();
		var worldPositionOnPlane = CameraHandler.Instance.GetWorldPositionOnPlane(mousePosition);
		var mouseGridPosition = GridGenerator.Instance.GetGridPosition(worldPositionOnPlane);
		Debug.Log($"MouseGridPos - {mouseGridPosition}");
		if (_selectedUnit.GetMoveAction().IsValidActionGridPosition(mouseGridPosition))
		{
			SetBusy();
			_selectedUnit.GetMoveAction().Move(mouseGridPosition, ClearBusy);
		}
	}

	private void SetBusy() => _isBusy = true;

	private void ClearBusy() => _isBusy = false;
	// private void Update()
	// {
	// 	var screenPos = DefaultActions.UI.Point.ReadValue<Vector2>();
	// 	var worldPositionOnPlane = CameraHandler.Instance.GetWorldPositionOnPlane(screenPos);
	// 	var mouseGridPosition = GridGenerator.Instance.GetGridPosition(worldPositionOnPlane);
	// 	Debug.Log($"MouseGridPos - {mouseGridPosition}");
	// }
}