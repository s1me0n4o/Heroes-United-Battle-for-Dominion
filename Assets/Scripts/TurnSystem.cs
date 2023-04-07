using System;
using UnityEngine;
using Utils;

public class TurnSystem : MonoSingleton<TurnSystem>
{
	public static event Action OnTurnChanged;
	public int TurnNumber => _turnNumber;
	public bool IsPlayerTurn => _isPlayerTurn;

	private int _turnNumber = 1;
	private bool _isPlayerTurn = true;

	public void NextTurn()
	{
		_turnNumber++;
		_isPlayerTurn = !_isPlayerTurn;
		OnTurnChanged?.Invoke();
	}
}