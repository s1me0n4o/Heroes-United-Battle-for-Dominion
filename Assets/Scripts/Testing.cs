using UnityEngine;

public class Testing : MonoBehaviour
{
	[SerializeField] private Unit unit;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.T))
		{
			GridSystemVisuals.Instance.HideAllGridPositions();
			GridSystemVisuals.Instance.ShowGridPositionLists(unit.GetMoveAction().GetValidGridPositions());
		}
	}
}