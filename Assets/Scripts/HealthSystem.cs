using System;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
	public event Action OnDead;
	public event Action OnHit;

	[SerializeField] private int _health = 100;
	private int _healthMax;

	private void Awake()
	{
		_healthMax = _health;
	}

	public void TakeDamage(int damage)
	{
		_health -= damage;
		if (_health <= 0)
			_health = 0;
		else
			OnHit?.Invoke();

		if (_health == 0)
		{
			Die();
		}

		Debug.Log("Current health: " + _health);
	}

	private void Die()
	{
		OnDead?.Invoke();
	}

	public float GetHealthNormalized() => (float)_health / _healthMax;
}