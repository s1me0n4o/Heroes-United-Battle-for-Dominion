using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Camera
{
	public class CameraMovementBasedOnFOV : MonoBehaviour
	{
		[SerializeField] private float _minFov = 25;
		[SerializeField] private float _maxFov = 45;

		private CinemachineVirtualCamera _virtualCamera;
		private CameraHandler _cameraHandler;
		private float _initialFov;
		private float _initialCameraZpos;
		private float _initialCameraYpos;
		private float _initialCameraXpos;
		private float _step = 1f;
		private DefaultActions _defaultActions;

		private DefaultActions DefaultActions => _defaultActions ??= new DefaultActions();

		private void Start()
		{
			_virtualCamera = GetComponent<CinemachineVirtualCamera>();
			_cameraHandler = GetComponent<CameraHandler>();

			_initialCameraZpos = _virtualCamera.transform.position.z;
			_initialCameraYpos = _virtualCamera.transform.position.y;
			_initialCameraXpos = _virtualCamera.transform.position.x;

			_virtualCamera.m_Lens.FieldOfView = (_minFov + _maxFov) / 2;
			_initialFov = _virtualCamera.m_Lens.FieldOfView;
		}

		private void OnEnable()
		{
			DefaultActions.Enable();
			DefaultActions.UI.ScrollWheel.performed += OnScrollPerformed;
		}

		private void OnDisable()
		{
			DefaultActions.UI.ScrollWheel.performed -= OnScrollPerformed;
			DefaultActions.Disable();
		}

		private void OnScrollPerformed(InputAction.CallbackContext context) => StartCoroutine(PerformScroll(context));

		private IEnumerator PerformScroll(InputAction.CallbackContext ctx)
		{
			var dir = ctx.ReadValue<Vector2>();
			while (dir != Vector2.zero)
			{
				dir = ctx.ReadValue<Vector2>();

				if (dir.y > 0)
				{
					var perc = _cameraHandler.ZoomStepPerc;
					_step = perc * (_maxFov - _minFov);
					_virtualCamera.m_Lens.FieldOfView -= _step;
				}

				if (dir.y < 0)
				{
					var perc = _cameraHandler.ZoomStepPerc;
					_step = perc * (_maxFov - _minFov);
					_virtualCamera.m_Lens.FieldOfView += _step;
				}

				_virtualCamera.m_Lens.FieldOfView = Mathf.Clamp(_virtualCamera.m_Lens.FieldOfView, _minFov, _maxFov);

				var initDepthSize = Mathf.Tan(_initialFov / 2f * Mathf.PI / 180f) * 2f;
				var currentDepthSize = Mathf.Tan(_virtualCamera.m_Lens.FieldOfView / 2f * Mathf.PI / 180f) * 2f;

				_virtualCamera.transform.position = new Vector3(
					_initialCameraXpos * initDepthSize / currentDepthSize,
					_initialCameraYpos * initDepthSize / currentDepthSize,
					_initialCameraZpos * initDepthSize / currentDepthSize);

				yield return null;
			}
		}
	}
}