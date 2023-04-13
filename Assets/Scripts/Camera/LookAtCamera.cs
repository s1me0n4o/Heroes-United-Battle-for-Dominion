using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
	[SerializeField] private bool _invert;

	private Transform _cameraTransform;
	private void Awake() => _cameraTransform = UnityEngine.Camera.main.transform;

	private void LateUpdate()
	{
		if (_invert)
		{
			var dirToCamera = (_cameraTransform.position - transform.position).normalized;
			transform.LookAt(transform.position + dirToCamera * -1);
		}
		else
		{
			transform.LookAt(_cameraTransform);
		}
	}
}