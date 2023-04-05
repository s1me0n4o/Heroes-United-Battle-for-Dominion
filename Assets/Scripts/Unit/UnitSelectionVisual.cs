using System;
using System.Collections;
using System.Collections.Generic;
using Camera;
using UnityEngine;

public class UnitSelectionVisual : MonoBehaviour
{
	private Unit _currentUnit;
	private MeshRenderer _meshRenderer;

	private void Awake()
	{
		_meshRenderer = GetComponent<MeshRenderer>();
		_meshRenderer.enabled = false;
	}

	private void Start()
	{
		_currentUnit = GetComponentInParent<Unit>();
	}

	private void OnEnable()
	{
		UnitActionSystem.OnSelectedUnitChanged += OnUnitSelected;
	}

	private void OnDisable()
	{
		UnitActionSystem.OnSelectedUnitChanged -= OnUnitSelected;
	}

	private void OnUnitSelected(Unit unit)
	{
		_meshRenderer.enabled = unit == _currentUnit;
	}
}