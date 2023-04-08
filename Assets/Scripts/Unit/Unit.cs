using System;
using UnityEngine;

public class Unit : MonoBehaviour
{
	public static event Action OnAnyActionPointsChanged;

	[SerializeField] private bool _isEnemy;

	private const int ActionPointsMax = 2;
	private GridPosition _currentGridPosition;
	private MoveAction _moveAction;
	private BaseAction[] _baseActions;
	private int _actionPoints = ActionPointsMax;

	private void OnEnable()
	{
		TurnSystem.OnTurnChanged += OnTurnChanged;
	}

	private void OnDisable()
	{
		TurnSystem.OnTurnChanged -= OnTurnChanged;
	}

	private void Awake()
	{
		_moveAction = GetComponent<MoveAction>();
		_baseActions = GetComponents<BaseAction>();
	}

	private void Start()
	{
		_currentGridPosition = GridGenerator.Instance.GetGridPosition(transform.position);
		GridGenerator.Instance.AddUnitAtGridPosition(_currentGridPosition, this);
	}

	private void Update()
	{
		var targetGridPosition = GridGenerator.Instance.GetGridPosition(_moveAction.TargetPosition);
		if (targetGridPosition == _currentGridPosition)
			return;
		if (!_moveAction.IsValidActionGridPosition(targetGridPosition))
			return;
		GridGenerator.Instance.UnitMoveGridPosition(this, _currentGridPosition, targetGridPosition);
		_currentGridPosition = targetGridPosition;
	}

	/////////////////////////////////////////

	public GridPosition GetCurrentGridPosition() => _currentGridPosition;
	public MoveAction GetMoveAction() => _moveAction;
	public int GetRemainingActionsCount() => _actionPoints;
	public BaseAction[] BaseActions => _baseActions;
	public bool IsEnemy => _isEnemy;

	public bool TrySpendActionPointsToTakeAction(BaseAction baseAction)
	{
		if (CanSpendActionPointsToTakeAction(baseAction))
		{
			SpendActionPoints(baseAction.GetActionPointsCost());
			return true;
		}

		return false;
	}

	private bool CanSpendActionPointsToTakeAction(BaseAction baseAction)
	{
		return _actionPoints >= baseAction.GetActionPointsCost();
	}

	private void SpendActionPoints(int amount)
	{
		_actionPoints -= amount;
		OnAnyActionPointsChanged?.Invoke();
	}

	private void OnTurnChanged()
	{
		if (_isEnemy && !TurnSystem.Instance.IsPlayerTurn || !_isEnemy && TurnSystem.Instance.IsPlayerTurn)
		{
			_actionPoints = ActionPointsMax;
			OnAnyActionPointsChanged?.Invoke();
		}
	}

	public void Damage()
	{
		Debug.Log("Damage");
	}

	public Vector3 GetWorldPosition()
	{
		return transform.position;
	}
}