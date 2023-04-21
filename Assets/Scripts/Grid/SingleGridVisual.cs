using UnityEngine;

public class SingleGridVisual : MonoBehaviour
{
	[SerializeField] private MeshRenderer _meshRenderer;

	public void Show(Material material)
	{
		_meshRenderer.enabled = true;
		_meshRenderer.material = material;
	}

	public void Hide() => _meshRenderer.enabled = false;
}