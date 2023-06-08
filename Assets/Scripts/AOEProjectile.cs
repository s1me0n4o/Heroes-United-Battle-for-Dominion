using System;
using Unity.Mathematics;
using UnityEngine;

public class AOEProjectile : MonoBehaviour
{
	[SerializeField] private GameObject _aoeSkillPrefab;
	private Vector3 _targetPos;
	private readonly float _reachedTargetDistance = .2f;
	private readonly float _damageRadius = 4f;
	private readonly int _AOEDamage = 30;
	private Action _onAOEActionComplete;

	private void Update()
	{
		var moveDir = (_targetPos - transform.position).normalized;
		var moveSpeed = 4f;
		transform.position += moveDir * moveSpeed * Time.deltaTime;

		if (Vector3.Distance(transform.position, _targetPos) < _reachedTargetDistance)
		{
			var colliderArray = Physics.OverlapSphere(_targetPos, _damageRadius);
			foreach (var collider in colliderArray)
			{
				if (collider.TryGetComponent<Unit>(out var targetUnit))
				{
					targetUnit.Damage(_AOEDamage);
				}
			}

			Destroy(gameObject);
			if (_aoeSkillPrefab != null)
				Instantiate(_aoeSkillPrefab, _targetPos, quaternion.identity);
			_onAOEActionComplete();
		}
	}

	public void Setup(GridPosition targetGridPosition, Action onAOEActionComplete)
	{
		_onAOEActionComplete = onAOEActionComplete;
		_targetPos = GridGenerator.Instance.GetWorldPosition(targetGridPosition);
	}
}