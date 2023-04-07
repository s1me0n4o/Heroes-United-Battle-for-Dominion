using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
	private float _timer;

	private void OnEnable()
	{
		TurnSystem.OnTurnChanged += OnTurnChanged;
	}

	private void OnDisable()
	{
		TurnSystem.OnTurnChanged -= OnTurnChanged;
	}

	private void OnTurnChanged()
	{
		_timer = 2f;
	}

	private void Update()
	{
		if (TurnSystem.Instance.IsPlayerTurn)
			return;

		_timer -= Time.deltaTime;
		if (_timer <= 0f)
		{
			TurnSystem.Instance.NextTurn();
		}
	}
}