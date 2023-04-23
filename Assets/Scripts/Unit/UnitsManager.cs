using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

[DefaultExecutionOrder(-100)]
public class UnitsManager : MonoSingleton<UnitsManager>
{
	private List<Unit> _unitsList = new List<Unit>();
	private List<Unit> _friendlyUnitsList = new List<Unit>();
	private List<Unit> _enemyUnitsList = new List<Unit>();

	private void Start()
	{
		Unit.OnAnyUnitSpawn += OnUnitSpawn;
		Unit.OnAnyUnitDead += OnUnitDead;
	}

	private void OnDestroy()
	{
		Unit.OnAnyUnitSpawn -= OnUnitSpawn;
		Unit.OnAnyUnitDead -= OnUnitDead;
	}

	private void OnUnitSpawn(object sender, EventArgs e)
	{
		var unit = sender as Unit;

		_unitsList.Add(unit);
		if (unit.IsEnemy)
			_enemyUnitsList.Add(unit);
		else
			_friendlyUnitsList.Add(unit);
	}

	private void OnUnitDead(object sender, EventArgs e)
	{
		var unit = sender as Unit;

		_unitsList.Remove(unit);
		if (unit.IsEnemy)
			_enemyUnitsList.Remove(unit);
		else
			_friendlyUnitsList.Remove(unit);
	}

	public List<Unit> GetUnitsList() => _unitsList;

	public List<Unit> GetEnemyUnitList() => _enemyUnitsList;

	public List<Unit> GetFriendlyUnitsList() => _friendlyUnitsList;
}