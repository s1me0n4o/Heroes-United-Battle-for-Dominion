using System;
using UnityEngine;

public class TurnSystem : MonoBehaviour
{
	public static event Action OnTurnChanged;
	public int TurnNumber => _turnNumber;
	private int _turnNumber = 1;

	public void NextTurn()
	{
		_turnNumber++;
		OnTurnChanged?.Invoke();
	}
}