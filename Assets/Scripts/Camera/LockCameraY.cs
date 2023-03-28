using Cinemachine;
using UnityEngine;

namespace Camera
{
	/// <summary>
	/// An add-on module for Cinemachine Virtual Camera that locks the camera's Y co-ordinate
	/// </summary>
	[SaveDuringPlay]
	[AddComponentMenu("")] // Hide in menu
	public class LockCameraY : CinemachineExtension
	{
		[Tooltip("Lock the camera's Y position to this value")]
		public float YPosition = 86;

		protected override void PostPipelineStageCallback(
			CinemachineVirtualCameraBase vcam,
			CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
		{
			if (stage == CinemachineCore.Stage.Finalize)
			{
				var pos = state.RawPosition;
				pos.y = YPosition;
				state.RawPosition = pos;
			}
		}
	}
}