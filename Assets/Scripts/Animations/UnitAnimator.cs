using UnityEngine;

public class UnitAnimator : MonoBehaviour
{
	private static readonly int IsMoving = Animator.StringToHash("IsMoving");
	private static readonly int Shooting = Animator.StringToHash("Shoot");
	private static readonly int GetHit = Animator.StringToHash("IsHit");
	private static readonly int Dead = Animator.StringToHash("IsDead");
	private static readonly int AOEShoot = Animator.StringToHash("AOEShoot");

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

		if (TryGetComponent<AOEAction>(out var aoeAction))
		{
			aoeAction.OnAoeShoot += OnAoeSkill;
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

		if (TryGetComponent<AOEAction>(out var aoeAction))
		{
			aoeAction.OnAoeShoot -= OnAoeSkill;
		}
	}

	private void OnAoeSkill() => _animator.SetTrigger(AOEShoot);

	private void OnStopMoving()
	{
		Debug.Log($"ISMoving {false}");
		_animator.SetBool(IsMoving, false);
	}

	private void OnStartMoving()
	{
		Debug.Log($"ISMoving {true}");
		_animator.SetBool(IsMoving, true);
	}

	private void OnShootStart() => _animator.SetTrigger(Shooting);

	private void OnGetHit() => _animator.SetTrigger(GetHit);
	private void OnDead() => _animator.SetTrigger(Dead);
}