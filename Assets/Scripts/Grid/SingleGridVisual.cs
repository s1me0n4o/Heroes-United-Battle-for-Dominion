using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleGridVisual : MonoBehaviour
{
	[SerializeField] private MeshRenderer _meshRenderer;

	public void Show() => _meshRenderer.enabled = true;

	public void Hide() => _meshRenderer.enabled = false;
}