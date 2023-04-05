using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnSystemUI : MonoBehaviour
{
	[SerializeField] private Button _buttonEndTurn;
	[SerializeField] private TextMeshProUGUI _currentTurnText;
	[SerializeField] private TurnSystem _turnSystem;

	private void OnEnable()
	{
		TurnSystem.OnTurnChanged += OnTurnChanged;
	}

	private void OnDisable()
	{
		TurnSystem.OnTurnChanged -= OnTurnChanged;
	}

	private void OnDestroy()
	{
		_buttonEndTurn.onClick.RemoveAllListeners();
	}

	private void Start()
	{
		_buttonEndTurn.onClick.AddListener(() => _turnSystem.NextTurn());
		UpdateTurnNumber();
	}

	/////////////////////////////////////////

	private void OnTurnChanged() => UpdateTurnNumber();

	private void UpdateTurnNumber()
	{
		_currentTurnText.SetText($"Turn: {_turnSystem.TurnNumber}".ToUpper());
	}
}