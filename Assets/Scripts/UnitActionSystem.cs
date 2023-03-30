using Camera;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

public class UnitActionSystem : MonoBehaviour
{
    [SerializeField] private Unit _selectedUnit;

    public Unit SelectedUnit => _selectedUnit;
    private DefaultActions _defaultActions;
    private DefaultActions DefaultActions => _defaultActions ??= new DefaultActions();

    private void OnEnable()
    {
        DefaultActions.Enable();
        DefaultActions.UI.RightClick.performed += OnRightClickPerformed;
        CameraHandler.OnInteractableSelected += OnUnitSelected;
    }

    private void OnDisable()
    {
        DefaultActions.UI.RightClick.performed -= OnRightClickPerformed;
        DefaultActions.Disable();
        CameraHandler.OnInteractableSelected -= OnUnitSelected;

    }

    private void OnUnitSelected(Unit unit) => _selectedUnit = unit;

    private void OnRightClickPerformed(InputAction.CallbackContext ctx)
    {
        var screenPos = DefaultActions.UI.Point.ReadValue<Vector2>();
        var worldPositionOnPlane = CameraHandler.Instance.GetWorldPositionOnPlane(screenPos);
        _selectedUnit.Move(worldPositionOnPlane);
    }
}
