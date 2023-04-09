using Unity.Mathematics;
using UnityEngine;

public class Bullet : MonoBehaviour
{
	[SerializeField] private Transform _bulletHitVfx;

	private Vector3 _targetPosition;
	private readonly float _moveSpeed = 200f;

	public void Setup(Vector3 targetPos)
	{
		_targetPosition = targetPos;
	}

	private void Update()
	{
		var moveDir = (_targetPosition - transform.position).normalized;
		var distanceBeforeMove = Vector3.Distance(transform.position, _targetPosition);
		transform.position += moveDir * _moveSpeed * Time.deltaTime;
		var distanceAfterMove = Vector3.Distance(transform.position, _targetPosition);
		if (distanceBeforeMove < distanceAfterMove)
		{
			Destroy(gameObject);
			Instantiate(_bulletHitVfx, _targetPosition, quaternion.identity);
		}
	}
}