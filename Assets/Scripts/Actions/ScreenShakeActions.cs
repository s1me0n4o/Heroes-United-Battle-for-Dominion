using UnityEngine;

public class ScreenShakeActions : MonoBehaviour
{
	private void OnEnable() => ShootAction.OnAnyShoot += OnAnyShoot;

	private void OnDisable() => ShootAction.OnAnyShoot -= OnAnyShoot;

	private void OnAnyShoot() => ScreenShake.Instance.Shake();
}