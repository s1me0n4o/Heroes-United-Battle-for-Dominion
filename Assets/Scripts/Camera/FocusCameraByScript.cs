using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Camera
{
	public class FocusCameraByScript : MonoBehaviour
	{
		// This could be changed with SO that holds the slot id, transform, etc.
		[SerializeField] private List<Transform> _slots = new();

		public event Action<Vector3> OnMovementInvoked;

		private int _currentSlotIndex = 0;

		// Test purposes
		private void Update()
		{
			if (_slots.Count == 0)
				return;

			if (Keyboard.current.spaceKey.wasPressedThisFrame)
			{
				var slot = _slots[_currentSlotIndex];

				OnMovementInvoked?.Invoke(slot.transform.position);

				if (_currentSlotIndex == _slots.Count - 1)
				{
					_currentSlotIndex = 0;
				}
				else
				{
					_currentSlotIndex++;
				}
			}
		}
	}
}
