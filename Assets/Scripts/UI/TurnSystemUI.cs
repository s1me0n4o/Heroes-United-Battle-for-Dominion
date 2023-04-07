using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TurnSystemUI : MonoBehaviour
{
	[SerializeField] private Button _buttonEndTurn;
	[SerializeField] private TextMeshProUGUI _currentTurnText;
	[SerializeField] private TurnSystem _turnSystem;
	[SerializeField] private GameObject _enemyTurnUI;

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
		UpdateEnemyTurnUI();
		UpdateEndTurnBtnVisibility();
	}

	/////////////////////////////////////////

	private void OnTurnChanged()
	{
		UpdateEnemyTurnUI();
		UpdateTurnNumber();
		UpdateEndTurnBtnVisibility();
	}

	private void UpdateTurnNumber() => _currentTurnText.SetText($"Turn: {_turnSystem.TurnNumber}".ToUpper());

	private void UpdateEnemyTurnUI() => _enemyTurnUI.SetActive(!TurnSystem.Instance.IsPlayerTurn);

	private void UpdateEndTurnBtnVisibility() => _buttonEndTurn.gameObject.SetActive(TurnSystem.Instance.IsPlayerTurn);
}