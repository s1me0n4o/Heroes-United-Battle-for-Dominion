using System;
using Cinemachine;
using UnityEngine;

namespace Game.Camera
{
	public class CameraManager : MonoBehaviour
	{
		public enum GameCameraType
		{
			TopDown,
			Action
		}

		[SerializeField] private CinemachineVirtualCamera[] _cameras;
		private CinemachineBrain _brain;

		private void Start()
		{
			var mainCameraTrans = UnityEngine.Camera.main.transform;
			_brain = mainCameraTrans.GetComponent<CinemachineBrain>();

			SwitchCamera(GameCameraType.TopDown);
		}

		private void OnEnable()
		{
			BaseAction.OnAnyActionStart += OnAnyActionStart;
			BaseAction.OnAnyActionComplete += OnAnyActionComplete;
		}

		private void OnDisable()
		{
			BaseAction.OnAnyActionStart -= OnAnyActionStart;
			BaseAction.OnAnyActionComplete -= OnAnyActionComplete;
		}

		private void OnAnyActionStart(object sender, EventArgs e)
		{
			switch (sender)
			{
				case ShootAction shootAction:
					var blendTime = .5f;
					var eventArgs = e as ShootAction.OnShootEventArgs;

					var shootingUnit = eventArgs.shootingUnit;
					var targetUnit = eventArgs.targgetUnit;

					var charHeight = 2.2f;
					var cameraHeight = Vector3.up * charHeight;
					var shootDir = (targetUnit.GetWorldPosition() - shootingUnit.GetWorldPosition()).normalized;
					var shoulderOffsetAmount = -1.5f; // left shoulder, positive number right shoulder
					var shoulderOffset = Quaternion.Euler(0, 90, 0) * shootDir * shoulderOffsetAmount;
					var actionCamPos =
						shootingUnit.GetWorldPosition() // at character feed 
						+ cameraHeight + shoulderOffset // lifting it up and adding shoulder offset
						+ (shootDir * 5 * -1); // pulling it back behind the character
					var actionCamera = _cameras[(int)GameCameraType.Action].gameObject;

					actionCamera.transform.position = actionCamPos;
					actionCamera.transform.LookAt(targetUnit.GetWorldPosition() + cameraHeight);

					SwitchCamera(GameCameraType.Action, blendTime);
					break;
			}
		}

		private void OnAnyActionComplete(object sender, EventArgs e)
		{
			switch (sender)
			{
				case ShootAction shootAction:
					SwitchCamera(GameCameraType.TopDown);
					break;
			}
		}

		private void SwitchCamera(GameCameraType cameraType, float blendTime = 1.0f)
		{
			foreach (var c in _cameras)
				c.Priority = 0;

			_cameras[(int)cameraType].Priority = 1;

			_brain.m_DefaultBlend.m_Time = blendTime;
			_brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
		}
	}
}