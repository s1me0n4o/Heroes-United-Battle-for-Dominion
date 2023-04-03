using UnityEngine;

public class Testing : MonoBehaviour
{
	[SerializeField] private Unit unit;

	// Update is called once per frame
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.T)) unit.MoveAction.GetValidGridPositions();
	}
}