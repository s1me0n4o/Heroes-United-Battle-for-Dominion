using UnityEngine;

public class UnitAnimator : MonoBehaviour
{
	private static readonly int IsMoving = Animator.StringToHash("IsMoving");
	private static readonly int Shooting = Animator.StringToHash("Shoot");
	private static readonly int GetHit = Animator.StringToHash("IsHit");
	private static readonly int Dead = Animator.StringToHash("IsDead");

	[SerializeField] private Animator _animator;

	private MoveAction _moveAction;

	private void Awake()
	{
		if (TryGetComponent<MoveAction>(out var moveAction))
		{
			moveAction.OnStartMoving += OnStartMoving;
			moveAction.OnStopMoving += OnStopMoving;
		}

		if (TryGetComponent<ShootAction>(out var shootAction))
		{
			shootAction.OnShootStart += OnShootStart;
		}

		if (TryGetComponent<HealthSystem>(out var healthAction))
		{
			healthAction.OnDead += OnDead;
			healthAction.OnHit += OnGetHit;
		}
	}

	private void OnDestroy()
	{
		if (TryGetComponent<MoveAction>(out var moveAction))
		{
			moveAction.OnStartMoving -= OnStartMoving;
			moveAction.OnStartMoving -= OnStopMoving;
		}

		if (TryGetComponent<ShootAction>(out var shootAction))
		{
			shootAction.OnShootStart -= OnShootStart;
		}

		if (TryGetComponent<HealthSystem>(out var healthAction))
		{
			healthAction.OnDead -= OnDead;
			healthAction.OnHit -= OnGetHit;
		}
	}

	private void OnStopMoving() => _animator.SetBool(IsMoving, false);

	private void OnStartMoving() => _animator.SetBool(IsMoving, true);

	private void OnShootStart() => _animator.SetTrigger(Shooting);

	private void OnGetHit() => _animator.SetTrigger(GetHit);
	private void OnDead() => _animator.SetTrigger(Dead);
}