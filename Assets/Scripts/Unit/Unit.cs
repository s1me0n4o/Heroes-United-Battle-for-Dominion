using UnityEngine;

public class Unit : MonoBehaviour
{
	// public MoveAction MoveAction { get; private set; }
	// public GridPosition CurrentGridPosition { get; private set; }
	private GridPosition _currentGridPosition;
	private MoveAction _moveAction;
	private void Awake() => _moveAction = GetComponent<MoveAction>();

	public MoveAction GetMoveAction() => _moveAction;

	private void Start()
	{
		_currentGridPosition = GridGenerator.Instance.GetGridPosition(transform.position);
		GridGenerator.Instance.AddUnitAtGridPosition(_currentGridPosition, this);
	}

	private void Update()
	{
		//var newGridPos = GridGenerator.Instance.GetGridPosition(transform.position);
		var targetGridPosition = GridGenerator.Instance.GetGridPosition(_moveAction.TargetPosition);
		if (targetGridPosition == _currentGridPosition)
			return;
		if (!_moveAction.IsValidActionGridPosition(targetGridPosition))
			return;
		GridGenerator.Instance.UnitMoveGridPosition(this, _currentGridPosition, targetGridPosition);
		_currentGridPosition = targetGridPosition;
	}

	public GridPosition GetCurrentGridPosition() => _currentGridPosition;
}