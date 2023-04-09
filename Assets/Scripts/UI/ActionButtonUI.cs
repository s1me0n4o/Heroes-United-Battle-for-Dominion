using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ActionButtonUI : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _textMeshPro;
	[SerializeField] private Button _button;
	[SerializeField] private GameObject _selectedBtnImg;
	private BaseAction _baseAction;

	private void OnDestroy()
	{
		_button.onClick.RemoveAllListeners();
	}

	/////////////////////////////////////////

	public void SetBaseAction(BaseAction baseAction)
	{
		_baseAction = baseAction;
		_textMeshPro.SetText(baseAction.GetActionName().ToUpper());
		_button.onClick.AddListener(() => { UnitActionSystem.Instance.SetSelectedAction(baseAction); });
	}

	public void UpdateSelectedVisual()
	{
		var selectedBaseAction = UnitActionSystem.Instance.SelectedAction;
		_selectedBtnImg.SetActive(selectedBaseAction == _baseAction);
	}
}