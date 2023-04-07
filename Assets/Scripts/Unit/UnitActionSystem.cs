using System;
using Camera;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;
using UnityEngine.EventSystems;

public class UnitActionSystem : MonoSingleton<UnitActionSystem>
{
	public static event Action<Unit> OnSelectedUnitChanged;
	public static event Action OnSelectedAction;
	public static event Action<bool> OnSystemBusy;
	public static event Action OnActionStarted;

	private Unit _selectedUnit;
	private DefaultActions _defaultActions;
	private DefaultActions DefaultActions => _defaultActions ??= new DefaultActions();
	private bool _isBusy;
	private BaseAction _selectedAction;

	public Unit SelectedUnit => _selectedUnit;
	public BaseAction SelectedAction => _selectedAction;

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
	private void OnUnitSelected(Unit unit)
	{
		if (_selectedUnit == unit)
			return;
		if (unit.IsEnemy)
			return;

		_selectedUnit = unit;
		_selectedAction = null;
		OnSelectedUnitChanged?.Invoke(unit);
	}

	private void OnRightClickPerformed(InputAction.CallbackContext ctx)
	{
		if (_isBusy)
			return;

		if (!TurnSystem.Instance.IsPlayerTurn)
			return;

		if (_selectedUnit == null)
			return;

		if (_selectedAction == null)
			return;

		if (EventSystem.current.IsPointerOverGameObject())
			return;

		HandleSelectedAction();
	}

	public void SetSelectedAction(BaseAction baseAction)
	{
		_selectedAction = baseAction;
		OnSelectedAction?.Invoke();
	}

	private void HandleSelectedAction()
	{
		var mousePosition = Mouse.current.position.ReadValue();
		var worldPositionOnPlane = CameraHandler.Instance.GetWorldPositionOnPlane(mousePosition);
		var mouseGridPosition = GridGenerator.Instance.GetGridPosition(worldPositionOnPlane);
		if (!_selectedAction.IsValidActionGridPosition(mouseGridPosition))
			return;

		if (!_selectedUnit.TrySpendActionPointsToTakeAction(_selectedAction))
			return;

		SetBusy();
		_selectedAction.TakeAction(mouseGridPosition, ClearBusy);
		OnActionStarted?.Invoke();
	}

	private void SetBusy()
	{
		_isBusy = true;
		OnSystemBusy?.Invoke(_isBusy);
	}

	private void ClearBusy()
	{
		_isBusy = false;
		OnSystemBusy?.Invoke(_isBusy);
	}
}