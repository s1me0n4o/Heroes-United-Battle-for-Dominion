using UnityEngine;

public class Unit : MonoBehaviour
{
	private static readonly int IsMoving = Animator.StringToHash("IsMoving");
	[SerializeField] private Animator _unitAnimator;
	private readonly float _movementSpeed = 4f;
	private readonly float _rotationSpeed = 10f;
	private readonly float _stoppingDistance = .1f;

	private GridPosition _currentGridPosition;

	private Vector3 _targetPosition;

	private void Awake()
	{
		_targetPosition = transform.position;
	}

	private void Start()
	{
		_currentGridPosition = GridGenerator.Instance.GetGridPosition(transform.position);
		GridGenerator.Instance.AddUnitAtGridPosition(_currentGridPosition, this);
	}

	private void Update()
	{
		if (Vector3.Distance(transform.position, _targetPosition) > _stoppingDistance)
		{
			// we dont want the magnitude that is why we normalize the vector.
			var moveDir = (_targetPosition - transform.position).normalized;
			transform.position += moveDir * _movementSpeed * Time.deltaTime;

			// rotate
			transform.forward = Vector3.Lerp(transform.forward, moveDir, Time.deltaTime * _rotationSpeed);
			_unitAnimator.SetBool(IsMoving, true);
		}
		else
		{
			_unitAnimator.SetBool(IsMoving, false);
		}

		var newGridPos = GridGenerator.Instance.GetGridPosition(transform.position);

		if (newGridPos != _currentGridPosition)
		{
			GridGenerator.Instance.UnitMoveGridPosition(this, _currentGridPosition, newGridPos);
			_currentGridPosition = newGridPos;
		}
	}

	public void Move(Vector3 targetPos)
	{
		_targetPosition = targetPos;
	}
}