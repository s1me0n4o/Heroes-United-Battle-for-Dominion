using UnityEngine;

public class Unit : MonoBehaviour
{
	// public MoveAction MoveAction { get; private set; }
	public GridPosition CurrentGridPosition { get; private set; }

	private MoveAction _moveAction;
	private void Awake() => _moveAction = GetComponent<MoveAction>();

	public MoveAction GetMoveAction() => _moveAction;

	private void Start()
	{
		CurrentGridPosition = GridGenerator.Instance.GetGridPosition(transform.position);
		GridGenerator.Instance.AddUnitAtGridPosition(CurrentGridPosition, this);
	}

	private void Update()
	{
		var newGridPos = GridGenerator.Instance.GetGridPosition(transform.position);

		if (newGridPos == CurrentGridPosition) return;
		if (!_moveAction.IsValidActionGridPosition(newGridPos)) return;
		GridGenerator.Instance.UnitMoveGridPosition(this, CurrentGridPosition, newGridPos);
		CurrentGridPosition = newGridPos;
	}
}