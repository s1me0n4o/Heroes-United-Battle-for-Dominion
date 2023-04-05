using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitActionSystemUI : MonoBehaviour
{
	[SerializeField] private Transform _actionButtonPrefab;
	[SerializeField] private Transform _actionButtonContainer;
	[SerializeField] private TextMeshProUGUI _actionPointsText;

	private readonly List<ActionButtonUI> _buttons = new List<ActionButtonUI>();

	private void OnEnable()
	{
		UnitActionSystem.OnSelectedUnitChanged += OnSelectedUnitChanged;
		UnitActionSystem.OnSelectedAction += OnSelectedAction;
		UnitActionSystem.OnSystemBusy += OnSystemBusy;
		UnitActionSystem.OnActionStarted += OnActionStarted;
	}

	private void OnDisable()
	{
		UnitActionSystem.OnSelectedUnitChanged -= OnSelectedUnitChanged;
		UnitActionSystem.OnSelectedAction -= OnSelectedAction;
		UnitActionSystem.OnSystemBusy -= OnSystemBusy;
		UnitActionSystem.OnActionStarted -= OnActionStarted;
	}

	private void Start()
	{
		CreateUnitActionButtons();
		UpgradeSelectedVisual();
		UpdateRemainingActionPoints();
	}

	private void UpgradeSelectedVisual()
	{
		foreach (var btn in _buttons)
		{
			btn.UpdateSelectedVisual();
		}
	}

	/////////////////////////////////////////

	private void CreateUnitActionButtons()
	{
		DestroyAllButtons();
		_buttons.Clear();

		var selectedUnit = UnitActionSystem.Instance.SelectedUnit;
		if (selectedUnit == null)
			return;
		foreach (var baseAction in selectedUnit.BaseActions)
		{
			var btn = Instantiate(_actionButtonPrefab, _actionButtonContainer);
			var btnUI = btn.GetComponent<ActionButtonUI>();
			btnUI.SetBaseAction(baseAction);

			_buttons.Add(btnUI);
		}
	}

	private void DestroyAllButtons()
	{
		foreach (Transform item in _actionButtonContainer)
		{
			Destroy(item.gameObject);
		}
	}

	private void OnSelectedUnitChanged(Unit unit)
	{
		_actionPointsText.gameObject.SetActive(true);

		CreateUnitActionButtons();
		UpdateRemainingActionPoints();
	}

	private void OnSelectedAction() => UpgradeSelectedVisual();

	private void OnActionStarted() => UpdateRemainingActionPoints();

	private void OnSystemBusy(bool isBusy)
	{
		_actionButtonContainer.gameObject.SetActive(!isBusy);
		_actionPointsText.gameObject.SetActive(!isBusy);
	}

	private void UpdateRemainingActionPoints()
	{
		var unit = UnitActionSystem.Instance.SelectedUnit;
		if (unit != null)
		{
			_actionPointsText.SetText($"Remaining Actions: {unit.GetRemainingActionsCount()}");
		}
	}
}