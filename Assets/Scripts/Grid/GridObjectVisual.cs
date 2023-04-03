using TMPro;
using UnityEngine;

public class GridObjectVisual : MonoBehaviour
{
	[SerializeField] private TextMeshPro _textMeshPro;
	private GridObject _gridObject;

	private void Update() => _textMeshPro.text = _gridObject.ToString();

	public void SetGridObject(GridObject gridObject)
	{
		_gridObject = gridObject;
	}
}