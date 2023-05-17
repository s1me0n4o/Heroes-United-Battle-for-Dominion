using System;
using Cinemachine;
using Utils;

public class ScreenShake : MonoSingleton<ScreenShake>
{
	private CinemachineImpulseSource _cinemachineImpulse;

	protected override void Awake()
	{
		_cinemachineImpulse = GetComponent<CinemachineImpulseSource>();
	}

	public void Shake(float intensity = 1f)
	{
		_cinemachineImpulse.GenerateImpulse(intensity);
	}
}