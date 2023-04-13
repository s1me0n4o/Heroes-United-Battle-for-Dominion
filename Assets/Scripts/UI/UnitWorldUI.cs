using UnityEngine;
using UnityEngine.UI;

public class UnitWorldUI : MonoBehaviour
{
	[SerializeField] private Image _healthBar;
	[SerializeField] private HealthSystem _healthSystem;

	private void OnEnable() => _healthSystem.OnHit += OnDamaged;

	private void OnDisable() => _healthSystem.OnHit -= OnDamaged;

	private void OnDamaged() => UpdateHealthBar();

	private void Start() => UpdateHealthBar();

	private void UpdateHealthBar() => _healthBar.fillAmount = _healthSystem.GetHealthNormalized();
}