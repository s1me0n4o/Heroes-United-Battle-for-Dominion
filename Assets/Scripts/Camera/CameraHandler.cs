using System;
using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using Utils;

namespace Camera
{
	public class CameraHandler : MonoSingleton<CameraHandler>
	{
		public enum State
		{
			None,
			Dragg,
			Click,
			LongPress,
			PinchZoom
		}

		/////////////////////////////////////////////////////
		private const float _kScreenXY = 0.5f;
		private const string _kInteractableObj = "InteractableObject";

		[Header("GameObjects")] [SerializeField]
		private Transform _cameraFollowTarget;

		[Header("Movement Settings")] [SerializeField]
		private float _camSpeed = 10f;

		[SerializeField] private Vector3 _cameraRotation = new(45, -50, 0);
		[SerializeField] private Vector2 _limit = new(60, 60);

		[Header("Damping")] [Range(0f, 20f)] [SerializeField]
		private float _zDamping = 1f;

		[Range(0f, 20f)] [SerializeField] private float _yDamping;

		[Range(0f, 20f)] [SerializeField] private float _xDamping;

		[Header("DeadZone")] [Range(0f, 1f)] [SerializeField]
		private float _deadZoneHeight = .7f;

		[Range(0f, 1f)] [SerializeField] private float _deadZoneWidth = .7f;

		[Range(0f, 1f)] [SerializeField] private float _deadZoneDepth;

		[Header("Zoom Settings")] [SerializeField]
		private float _zoomStep = 50f;

		[Range(30, 500)] [SerializeField] private float _zoomCameraDistance = 50f;

		[SerializeField] private float _minZoomDistance = 30f;
		[SerializeField] private float _maxZoomDistance = 100f;
		[SerializeField] private float _zoomInCameraAngleX = 25f;
		[SerializeField] private float _zoomOutCameraAngleX = 45f;
		[SerializeField] private float _zoomInCameraAngleY;
		[SerializeField] private float _zoomOutCameraAngleY;
		[SerializeField] private float _zoomInCameraAngleZ;
		[SerializeField] private float _zoomOutCameraAngleZ;
		[SerializeField] private float _mobileZoomInCameraAngleX = 25f;
		[SerializeField] private float _mobileZoomOutCameraAngleX = 45f;
		[SerializeField] private float _mobileZoomInCameraAngleY;
		[SerializeField] private float _mobileZoomOutCameraAngleY;
		[SerializeField] private float _mobileZoomInCameraAngleZ;
		[SerializeField] private float _mobileZoomOutCameraAngleZ;
		[SerializeField] private bool _enableCameraZoomRotation = true;

		[SerializeField] private bool _cameraSmoothness = true;
		[SerializeField] private float _zeroSmoothnes;

		[SerializeField] private float
			_mouseDragSpeedPhone = .3f; // lower value than .1f create snapping because removing the smoothness

		[SerializeField]
		private float _mouseDragSpeed = .1f; // lower value than .1f create snapping because removing the smoothness

		private readonly float _elapsedTimeMax = 2f;
		private readonly float _globalMultiplayer = 2f;
		private readonly float _hitRayMaxDistance = 5000f;
		private readonly float _threshold = 30f;
		private LockCameraY _cameraLockXY;
		private DefaultActions _defaultActions;
		private Vector3 _dragCurrentPosition;
		private Vector3 _dragOrigin;
		private float _elapsedTime;
		private Vector2 _initialScreenPoint;
		private bool _isPressed;
		private int _layerInteractable;
		private UnityEngine.Camera _mainCamera;
		private bool _pinchZoom;
		private Plane _plane;
		private Vector3 _targetCurrentPos;
		private CinemachineFramingTransposer _transposer;
		private Vector3 _velocity = Vector3.zero;


		/////////////////////////////////////////////////////
		private CinemachineVirtualCamera _virtualCamera;
		private Coroutine _zoomCoroutine;

		/////////////////////////////////////////////////////
		public float ZoomCameraDistance => _zoomCameraDistance;
		public float MaxCameraDistance => _maxZoomDistance;
		public float MinCameraDistance => _minZoomDistance;
		public float ZoomStepPerc { get; private set; }

		public State CameraState { get; private set; } = State.None;

		public bool IsScrolling { get; private set; }
		public Vector3 WorldTargetPosition { get; }

		private DefaultActions DefaultActions => _defaultActions ??= new DefaultActions();

		private void Start()
		{
			Assert.IsNotNull(_cameraFollowTarget);

			_mainCamera = UnityEngine.Camera.main;
			_virtualCamera = GetComponent<CinemachineVirtualCamera>();
			_transposer = _virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
			_cameraLockXY = _virtualCamera.GetComponent<LockCameraY>();

			_zoomCameraDistance = (_minZoomDistance + _maxZoomDistance) / 2;
			_transposer.m_CameraDistance = _zoomCameraDistance;

			_plane = new Plane(Vector3.up, Vector3.zero);

			SetDefaultCameraOffset();
			OnEnableCameraCameraRotation(true);
			OnEnableSmoothness(true);

			// initial camera position;
			_cameraFollowTarget.position = Vector3.zero;

			_cameraLockXY.YPosition = _virtualCamera.gameObject.transform.position.y;
			_layerInteractable = LayerMask.GetMask(_kInteractableObj);
			SetDefaultScreenXY();
			//CameraLockDisable();

			// events
			DefaultActions.UI.SecondaryTouchContact.started += _ => PinchZoomStart();
			DefaultActions.UI.SecondaryTouchContact.canceled += _ => PinchZoomEnd();
		}

		private void LateUpdate()
		{
			if (_pinchZoom)
				return;

			if (_isPressed)
			{
				//if (UI.UIManager.InsideUI)
				//    return;

				// Detect difference between screen touches
				var currentScreenPoint = DefaultActions.UI.Point.ReadValue<Vector2>();
				var pointsDiffrance = currentScreenPoint - _initialScreenPoint;

				// If pressed but not moving
				if (pointsDiffrance.x < _threshold || pointsDiffrance.x > -_threshold ||
				    pointsDiffrance.y < _threshold || (pointsDiffrance.y > -_threshold &&
				                                       CameraState == State.None))
				{
					// Check for long press
					_elapsedTime += Time.deltaTime;
					if (_elapsedTime > _elapsedTimeMax)
						//Debug.Log("LongPress");
						CameraState = State.LongPress;
				}

				// If its not in the treshhold -> dragging
				if ((pointsDiffrance.x > _threshold || pointsDiffrance.x < -_threshold ||
				     pointsDiffrance.y > _threshold || pointsDiffrance.y < -_threshold) &&
				    CameraState != State.LongPress)
				{
					_elapsedTime = 0;
					CameraState = State.Dragg;

					// Update dragg
					_dragCurrentPosition = GetWorldPositionOnPlane(DefaultActions.UI.Point.ReadValue<Vector2>());
					var newPos = _targetCurrentPos + _dragOrigin - _dragCurrentPosition;
					AdjustPositionWithSmoothDamp(newPos);
				}
				// If we are not moving -> clicking
				else if (CameraState == State.None)
				{
					CameraState = State.Click;
				}

				if (CameraState is State.Dragg or State.LongPress)
					CameraState = State.None;
			}
			else if (!_isPressed && CameraState == State.Click)
			{
				//if (UI.UIManager.InsideUI)
				//    return;

				// If we are clicking check for interactable objects
				if (Physics.Raycast(
					    _mainCamera.ScreenPointToRay(DefaultActions.UI.Point.ReadValue<Vector2>()),
					    out var hit,
					    _hitRayMaxDistance,
					    _layerInteractable))
				{
					if (hit.transform.TryGetComponent<Unit>(out var unit))
						OnInteractableSelected?.Invoke(unit);

					InteractableSelected(hit.transform.position);
				}

				CameraState = State.None;
			}
		}

		private void OnEnable()
		{
			DefaultActions.Enable();

			DefaultActions.UI.Click.performed += OnClickPerformed;
			DefaultActions.UI.ScrollClick.performed += OnClickPerformed;
			DefaultActions.UI.Navigate.performed += OnNavigatePerformed;
			DefaultActions.UI.ScrollWheel.performed += OnScrollPerformed;
		}

		private void OnDisable()
		{
			DefaultActions.UI.Click.performed -= OnClickPerformed;
			DefaultActions.UI.ScrollClick.performed -= OnClickPerformed;
			DefaultActions.UI.Navigate.performed -= OnNavigatePerformed;
			DefaultActions.UI.ScrollWheel.performed -= OnScrollPerformed;

			DefaultActions.Disable();
			StopAllCoroutines();
		}

		public static event Action<Unit> OnInteractableSelected;

		/////////////////////////////////////////////////////
		private void OnEnableCameraTargetMesh(bool active)
		{
			var mr = _cameraFollowTarget.GetComponentInChildren<MeshRenderer>();
			if (mr != null)
				mr.enabled = active;
		}

		private void OnEnableCameraCameraRotation(bool status)
		{
			_enableCameraZoomRotation = status;
		}

		private void OnEnableSmoothness(bool condition)
		{
			_cameraSmoothness = condition;
		}

		private void OnScrollPerformed(InputAction.CallbackContext context)
		{
			IsScrolling = true;
			//if (UI.UIManager.InsideUI)
			//    return;

			_ = StartCoroutine(PerformScroll(context));
			IsScrolling = false;
		}

		private IEnumerator PerformScroll(InputAction.CallbackContext context)
		{
			//_cameraCollisionDetection.enabled = false;

			var dir = context.ReadValue<Vector2>();
			while (dir != Vector2.zero)
			{
				dir = context.ReadValue<Vector2>();
				// Camera zoom in
				if (dir.y > 0)
				{
					CameraLockDisable();
					StopDampingAndDeadZone();

					var currentDistance = _zoomCameraDistance;
					_zoomCameraDistance -= _zoomStep * _globalMultiplayer * Time.deltaTime;
					var step = currentDistance - _zoomCameraDistance;
					ZoomStepPerc = step / (_maxZoomDistance - _minZoomDistance);

					RotateCamera(false);
				}

				// Camera zoom out
				if (dir.y < 0)
				{
					CameraLockDisable();
					StopDampingAndDeadZone();

					var prevDist = _zoomCameraDistance;
					_zoomCameraDistance += _zoomStep * _globalMultiplayer * Time.deltaTime;
					var step = _zoomCameraDistance - prevDist;
					ZoomStepPerc = step / (_maxZoomDistance - _minZoomDistance);

					RotateCamera(true);
				}

				_zoomCameraDistance = Mathf.Clamp(_zoomCameraDistance, _minZoomDistance, _maxZoomDistance);

				SetCameraDistance(_zoomCameraDistance);

				yield return null;
			}
		}

		private void OnClickPerformed(InputAction.CallbackContext context)
		{
			_isPressed = context.ReadValue<float>() == 1 && CameraState != State.PinchZoom;

			AdjustDamping(false);
			// Getting initial data
			_initialScreenPoint = DefaultActions.UI.Point.ReadValue<Vector2>();
			_dragOrigin = GetWorldPositionOnPlane(_initialScreenPoint);
			_elapsedTime = 0;
		}

		private IEnumerator WaitForUIEvent()
		{
			// Await UI event to be triggered
			yield return new WaitForEndOfFrame();
		}

		private void OnNavigatePerformed(InputAction.CallbackContext context)
		{
			//if (UI.UIManager.InsideUI)
			//    return;

			AdjustDamping(true);
			_ = StartCoroutine(PerformVectorNavigate(context));
			AdjustDamping(false);
		}

		private IEnumerator PerformVectorNavigate(InputAction.CallbackContext context)
		{
			var dir = context.ReadValue<Vector2>();

			while (dir != Vector2.zero)
			{
				// Need to update the dir in order to check if the player has released the button
				dir = context.ReadValue<Vector2>();

				if (dir.x < 0)
				{
					_targetCurrentPos -= transform.right * _camSpeed * Time.deltaTime;
					AdjustPositionWithSmoothDamp(_targetCurrentPos);
				}

				if (dir.x > 0)
				{
					_targetCurrentPos += transform.right * _camSpeed * Time.deltaTime;
					AdjustPositionWithSmoothDamp(_targetCurrentPos);
				}

				if (dir.y > 0)
				{
					_targetCurrentPos += transform.forward * _camSpeed * Time.deltaTime;
					AdjustPositionWithSmoothDamp(_targetCurrentPos);
				}

				if (dir.y < 0)
				{
					_targetCurrentPos -= transform.forward * _camSpeed * Time.deltaTime;
					AdjustPositionWithSmoothDamp(_targetCurrentPos);
				}

				yield return null;
			}
		}

		private void InteractableSelected(Vector3 pos, bool withDeadZone = true, bool withDamping = true,
			bool fromScript = false)
		{
			// TODO: add check if it is inside the dead zone and then enable the damping
			if (!fromScript)
			{
				var inside = CheckIfInDeadZone(DefaultActions.UI.Point.ReadValue<Vector2>());
				if (inside)
					return;
			}

			AdjustDamping(withDamping);
			AdjustDeadZone(withDeadZone);
			AdjustPosition(pos);
			_ = StartCoroutine(WaitASecAndAdjustTheCamInMidScreen());
		}

		private bool CheckIfInDeadZone(Vector2 screenPos)
		{
			var widht = _deadZoneWidth * Screen.width;
			var height = _deadZoneHeight * Screen.height;
			return screenPos.x < widht && screenPos.y < height;
		}

		private IEnumerator WaitASecAndAdjustTheCamInMidScreen()
		{
			yield return new WaitForSeconds(.5f);

			var pos = GetWorldPositionOnPlaneFromMidScreen();
			AdjustPosition(pos);

			AdjustDamping(false);
			AdjustDeadZone(false);
		}

		public void MovementInvoked(Vector3 pos)
		{
			CameraLockDisable();
			InteractableSelected(pos, false, true, true);
			SetCameraDistance(200f);
		}

		private void PinchZoomStart()
		{
			//_cameraCollisionDetection.enabled = false;
			_zoomCoroutine = StartCoroutine(ZoomPinchDetection());
		}

		private void PinchZoomEnd()
		{
			StopCoroutine(_zoomCoroutine);
			_pinchZoom = false;
			CameraState = State.None;
			//_cameraCollisionDetection.enabled = true;
		}

		private IEnumerator ZoomPinchDetection()
		{
			//if (UI.UIManager.InsideUI)
			//    yield break;

			_pinchZoom = true;
			CameraState = State.PinchZoom;

			var prevDist = 0f;
			var initialScreenDistance = Vector2.Distance(DefaultActions.UI.PrimaryFingerPosition.ReadValue<Vector2>(),
				DefaultActions.UI.SecondaryFingerPosition.ReadValue<Vector2>());
			var initialCameraDistance = _zoomCameraDistance;

			// Тhis will stop when we stop the coroutine - lift finger
			while (true)
			{
				// Check the input distance
				var currentScreenDistance = Vector2.Distance(
					DefaultActions.UI.PrimaryFingerPosition.ReadValue<Vector2>(),
					DefaultActions.UI.SecondaryFingerPosition.ReadValue<Vector2>());
				if (prevDist == 0)
					prevDist = currentScreenDistance;

				// Zoom out
				if (currentScreenDistance < prevDist)
					CalculateCameraDistance(currentScreenDistance, initialScreenDistance, initialCameraDistance, true);
				// Zoom in
				else if (currentScreenDistance > prevDist)
					CalculateCameraDistance(currentScreenDistance, initialScreenDistance, initialCameraDistance, false);

				_zoomCameraDistance = Mathf.Clamp(_zoomCameraDistance, _minZoomDistance, _maxZoomDistance);
				SetCameraDistance(_zoomCameraDistance);

				prevDist = currentScreenDistance;
				yield return null;
			}
		}

		private void CalculateCameraDistance(float currentScreenDistance,
			float initialScreenDistance,
			float initialCameraDistance,
			bool rotateCamera)
		{
			CameraLockDisable();
			StopDampingAndDeadZone();

			var previousDistance = _zoomCameraDistance;
			_zoomCameraDistance = initialCameraDistance * initialScreenDistance / currentScreenDistance;
			var step = _zoomCameraDistance - previousDistance;
			ZoomStepPerc = step / (_maxZoomDistance - _minZoomDistance);

			if (_enableCameraZoomRotation)
				RotateCamera(rotateCamera, true);
		}

		/////////////////////////////////////////////////////
		public void SetCameraDistance(float distance)
		{
			//_zoomCameraDistance = Mathf.Clamp(distance, _minZoomDistance, _maxZoomDistance);
			// _transposer.m_CameraDistance = Mathf.Lerp(_transposer.m_CameraDistance, distance, Time.deltaTime);
			_transposer.m_CameraDistance = distance;
		}

		private void RotateCamera(bool positive, bool useMobile = false)
		{
			float stepX;
			float stepY;
			float stepZ;
			float targetRotationX;
			float targetRotationY;
			float targetRotationZ;
			if (useMobile)
			{
				if (_mobileZoomInCameraAngleX > _mobileZoomOutCameraAngleX)
				{
					stepX = ZoomStepPerc * (_mobileZoomInCameraAngleX - _mobileZoomOutCameraAngleX);
					targetRotationX = positive
						? Mathf.Clamp(_cameraRotation.x - stepX, _mobileZoomOutCameraAngleX, _mobileZoomInCameraAngleX)
						: Mathf.Clamp(_cameraRotation.x + stepX, _mobileZoomOutCameraAngleX, _mobileZoomInCameraAngleX);
				}
				else
				{
					stepX = ZoomStepPerc * (_mobileZoomOutCameraAngleX - _mobileZoomInCameraAngleX);
					targetRotationX = positive
						? Mathf.Clamp(_cameraRotation.x + stepX, _mobileZoomInCameraAngleX, _mobileZoomOutCameraAngleX)
						: Mathf.Clamp(_cameraRotation.x - stepX, _mobileZoomInCameraAngleX, _mobileZoomOutCameraAngleX);
				}

				if (_mobileZoomInCameraAngleY > _mobileZoomOutCameraAngleY)
				{
					stepY = ZoomStepPerc *
					        (Mathf.Abs(_mobileZoomInCameraAngleY) - Mathf.Abs(_mobileZoomOutCameraAngleY));
					targetRotationY = positive
						? Mathf.Clamp(_cameraRotation.y - Mathf.Abs(stepY), _mobileZoomOutCameraAngleY,
							_mobileZoomInCameraAngleY)
						: Mathf.Clamp(_cameraRotation.y + Mathf.Abs(stepY), _mobileZoomOutCameraAngleY,
							_mobileZoomInCameraAngleY);
				}
				else
				{
					stepY = ZoomStepPerc *
					        (Mathf.Abs(_mobileZoomOutCameraAngleY) - Mathf.Abs(_mobileZoomInCameraAngleY));
					targetRotationY = positive
						? Mathf.Clamp(-Mathf.Abs(_cameraRotation.y + Mathf.Abs(stepY)), _mobileZoomInCameraAngleY,
							_mobileZoomOutCameraAngleY)
						: Mathf.Clamp(-Mathf.Abs(_cameraRotation.y - Mathf.Abs(stepY)), _mobileZoomInCameraAngleY,
							_mobileZoomOutCameraAngleY);
				}

				if (_mobileZoomInCameraAngleZ > _mobileZoomOutCameraAngleZ)
				{
					stepZ = ZoomStepPerc * (_mobileZoomInCameraAngleZ - _mobileZoomOutCameraAngleZ);
					targetRotationZ = positive
						? Mathf.Clamp(_cameraRotation.z - stepZ, _mobileZoomOutCameraAngleZ, _mobileZoomInCameraAngleZ)
						: Mathf.Clamp(_cameraRotation.z + stepZ, _mobileZoomOutCameraAngleZ, _mobileZoomInCameraAngleZ);
				}
				else
				{
					stepZ = ZoomStepPerc * (_mobileZoomOutCameraAngleZ - _mobileZoomInCameraAngleZ);
					targetRotationZ = positive
						? Mathf.Clamp(_cameraRotation.z + stepZ, _mobileZoomInCameraAngleZ, _mobileZoomOutCameraAngleZ)
						: Mathf.Clamp(_cameraRotation.z - stepZ, _mobileZoomInCameraAngleZ, _mobileZoomOutCameraAngleZ);
				}
			}
			else
			{
				if (_zoomInCameraAngleX > _zoomOutCameraAngleX)
				{
					stepX = ZoomStepPerc * (_zoomInCameraAngleX - _zoomOutCameraAngleX);
					targetRotationX = positive
						? Mathf.Clamp(_cameraRotation.x - stepX, _zoomOutCameraAngleX, _zoomInCameraAngleX)
						: Mathf.Clamp(_cameraRotation.x + stepX, _zoomOutCameraAngleX, _zoomInCameraAngleX);
				}
				else
				{
					stepX = ZoomStepPerc * (_zoomOutCameraAngleX - _zoomInCameraAngleX);
					targetRotationX = positive
						? Mathf.Clamp(_cameraRotation.x + stepX, _zoomInCameraAngleX, _zoomOutCameraAngleX)
						: Mathf.Clamp(_cameraRotation.x - stepX, _zoomInCameraAngleX, _zoomOutCameraAngleX);
				}

				if (_zoomInCameraAngleY > _zoomOutCameraAngleY)
				{
					stepY = ZoomStepPerc * (Mathf.Abs(_zoomInCameraAngleY) - Mathf.Abs(_zoomOutCameraAngleY));
					targetRotationY = positive
						? Mathf.Clamp(_cameraRotation.y - Mathf.Abs(stepY), _zoomOutCameraAngleY, _zoomInCameraAngleY)
						: Mathf.Clamp(_cameraRotation.y + Mathf.Abs(stepY), _zoomOutCameraAngleY, _zoomInCameraAngleY);
				}
				else
				{
					stepY = ZoomStepPerc * (Mathf.Abs(_zoomOutCameraAngleY) - Mathf.Abs(_zoomInCameraAngleY));
					targetRotationY = positive
						? Mathf.Clamp(-Mathf.Abs(_cameraRotation.y + Mathf.Abs(stepY)), _zoomInCameraAngleY,
							_zoomOutCameraAngleY)
						: Mathf.Clamp(-Mathf.Abs(_cameraRotation.y - Mathf.Abs(stepY)), _zoomInCameraAngleY,
							_zoomOutCameraAngleY);
				}

				if (_zoomInCameraAngleZ > _zoomOutCameraAngleZ)
				{
					stepZ = ZoomStepPerc * (_zoomInCameraAngleZ - _zoomOutCameraAngleZ);
					targetRotationZ = positive
						? Mathf.Clamp(_cameraRotation.z - stepZ, _zoomOutCameraAngleZ, _zoomInCameraAngleZ)
						: Mathf.Clamp(_cameraRotation.z + stepZ, _zoomOutCameraAngleZ, _zoomInCameraAngleZ);
				}
				else
				{
					stepZ = ZoomStepPerc * (_zoomOutCameraAngleZ - _zoomInCameraAngleZ);
					targetRotationZ = positive
						? Mathf.Clamp(_cameraRotation.z + stepZ, _zoomInCameraAngleZ, _zoomOutCameraAngleZ)
						: Mathf.Clamp(_cameraRotation.z - stepZ, _zoomInCameraAngleZ, _zoomOutCameraAngleZ);
				}
			}

			var newCamRot = new Vector3(targetRotationX, targetRotationY, targetRotationZ);
			_virtualCamera.transform.rotation = Quaternion.Slerp(
				Quaternion.Euler(_cameraRotation),
				Quaternion.Euler(newCamRot),
				Time.time);
			_cameraRotation = newCamRot;
		}

		private void AdjustPosition(Vector3 newPosition)
		{
			var xClamp = Mathf.Clamp(newPosition.x, -_limit.x, +_limit.x);
			var zClamp = Mathf.Clamp(newPosition.z, -_limit.y, +_limit.y);

			var target = new Vector3(xClamp, 0, zClamp);

			if (target == _cameraFollowTarget.position)
				return;

			_cameraFollowTarget.position = target;

			_targetCurrentPos = _cameraFollowTarget.position;
		}

		private void AdjustPositionWithSmoothDamp(Vector3 newPosition)
		{
			var xClamp = Mathf.Clamp(newPosition.x, -_limit.x, +_limit.x);
			var zClamp = Mathf.Clamp(newPosition.z, -_limit.y, +_limit.y);

			var target = new Vector3(xClamp, 0, zClamp);

			if (target == _cameraFollowTarget.position)
				return;

#if UNITY_IOS
			var smoothness = _cameraSmoothness ? _mouseDragSpeedPhone : _zeroSmoothnes;
#elif UNITY_ANDROID
			var smoothness = _cameraSmoothness ? _mouseDragSpeedPhone : _zeroSmoothnes;
#else
			var smoothness = _cameraSmoothness ? _mouseDragSpeed : _zeroSmoothnes;
#endif
			_cameraFollowTarget.position =
				Vector3.SmoothDamp(_cameraFollowTarget.position, target, ref _velocity, smoothness);

			_targetCurrentPos = _cameraFollowTarget.position;
		}

		private void StopDampingAndDeadZone()
		{
			AdjustDamping(false);
			AdjustDeadZone(false);
		}

		private void AdjustDamping(bool haveDamping)
		{
			if (!haveDamping &&
			    (_transposer.m_XDamping != 0f ||
			     _transposer.m_YDamping != 0f ||
			     _transposer.m_ZDamping != 0f))
			{
				_transposer.m_XDamping = 0f;
				_transposer.m_YDamping = 0f;
				_transposer.m_ZDamping = 0f;
			}

			if (haveDamping)
			{
				_transposer.m_XDamping = _xDamping;
				_transposer.m_YDamping = _yDamping;
				_transposer.m_ZDamping = _zDamping;
			}
		}

		private void SetDefaultScreenXY()
		{
			_transposer.m_ScreenX = _kScreenXY;
			_transposer.m_ScreenY = _kScreenXY;
		}

		private void AdjustDeadZone(bool haveDeadZone)
		{
			if (!haveDeadZone &&
			    (_transposer.m_DeadZoneHeight != 0f ||
			     _transposer.m_DeadZoneWidth != 0f ||
			     _transposer.m_DeadZoneDepth != 0f))
			{
				_transposer.m_DeadZoneHeight = 0f;
				_transposer.m_DeadZoneWidth = 0f;
				_transposer.m_DeadZoneDepth = 0f;
				CameraLockDisable();
			}

			if (haveDeadZone)
			{
				_transposer.m_DeadZoneHeight = _deadZoneHeight;
				_transposer.m_DeadZoneWidth = _deadZoneWidth;
				_transposer.m_DeadZoneDepth = 0f;
				CameraLockEnable();
			}
		}

		private void CameraLockEnable()
		{
			_cameraLockXY.YPosition = _virtualCamera.gameObject.transform.position.y;
			_cameraLockXY.enabled = true;
		}

		private void CameraLockDisable()
		{
			_cameraLockXY.enabled = false;
		}

		/////////////////////////////////////////////////////
		private void SetDefaultCameraOffset()
		{
			_virtualCamera.transform.rotation = Quaternion.Euler(_cameraRotation);
			StopDampingAndDeadZone();
		}

		public Vector3 GetWorldPositionOnPlane(Vector3 screenPosition)
		{
			var ray = _mainCamera.ScreenPointToRay(screenPosition);
			return _plane.Raycast(ray, out var entry) ? ray.GetPoint(entry) : Vector3.zero;
		}

		private Vector3 GetWorldPositionOnPlaneFromMidScreen()
		{
			var midWidth = Screen.width / 2;
			var MidHeight = Screen.height / 2;
			var point = new Vector2(midWidth, MidHeight);
			var ray = _mainCamera.ScreenPointToRay(point);
			return _plane.Raycast(ray, out var entry) ? ray.GetPoint(entry) : Vector3.zero;
		}
	}
}