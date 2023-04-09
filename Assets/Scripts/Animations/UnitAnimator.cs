using UnityEngine;

public class UnitAnimator : MonoBehaviour
{
	private static readonly int IsMoving = Animator.StringToHash("IsMoving");
	private static readonly int Shooting = Animator.StringToHash("Shoot");

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
	}

	private void OnStopMoving()
	{
		_animator.SetBool(IsMoving, false);
	}

	private void OnStartMoving()
	{
		_animator.SetBool(IsMoving, true);
	}

	private void OnShootStart()
	{
		_animator.SetTrigger(Shooting);
	}
}