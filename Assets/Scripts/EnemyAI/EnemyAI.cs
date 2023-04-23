using System;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
	private float _timer;
	private State _state;

	private enum State
	{
		WaitingForEnemyTurn,
		TakingTurn,
		Busy,
	}

	private void Awake()
	{
		_state = State.WaitingForEnemyTurn;
	}

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
		if (TurnSystem.Instance.IsPlayerTurn)
			return;

		_state = State.TakingTurn;
		_timer = 2f;
	}

	private void Update()
	{
		if (TurnSystem.Instance.IsPlayerTurn)
			return;

		switch (_state)
		{
			case State.WaitingForEnemyTurn:
				break;
			case State.TakingTurn:
				_timer -= Time.deltaTime;
				if (_timer <= 0f)
				{
					if (TryTakeEnemyAIAction(SetStateTakingTurn))
						_state = State.Busy;
					else
						TurnSystem.Instance.NextTurn();
				}

				break;
			case State.Busy:
				break;
		}
	}

	private void SetStateTakingTurn()
	{
		_timer = .5f;
		_state = State.TakingTurn;
	}

	private bool TryTakeEnemyAIAction(Action onEnemyAIActionComplete)
	{
		foreach (var enemyUnit in UnitsManager.Instance.GetEnemyUnitList())
		{
			if (TryTakeEnemyAIAction(enemyUnit, onEnemyAIActionComplete))
				return true;
		}

		return false;
	}

	private bool TryTakeEnemyAIAction(Unit enemyUnit, Action onEnemyAIActionComplete)
	{
		EnemyAIAction bestEnemyAIAction = null;
		BaseAction bestBaseAction = null;
		foreach (var baseAction in enemyUnit.BaseActions)
		{
			if (!enemyUnit.CanSpendActionPointsToTakeAction(baseAction))
			{
				// enemy cannot afford this action
				continue;
			}

			if (bestEnemyAIAction == null)
			{
				bestEnemyAIAction = baseAction.GetBestEnemyAIAction();
				bestBaseAction = baseAction;
			}
			else
			{
				var testEnemyAIAction = baseAction.GetBestEnemyAIAction();
				if (testEnemyAIAction != null && testEnemyAIAction.ActionValue > bestEnemyAIAction.ActionValue)
				{
					bestEnemyAIAction = testEnemyAIAction;
					bestBaseAction = baseAction;
				}
			}
		}

		if (bestEnemyAIAction != null && enemyUnit.TrySpendActionPointsToTakeAction(bestBaseAction))
		{
			bestBaseAction.TakeAction(bestEnemyAIAction.GridPosition, onEnemyAIActionComplete);
			return true;
		}

		return false;
	}
}