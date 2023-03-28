using UnityEngine;

public class Unit : MonoBehaviour
{
    private Vector3 _targetPosition;
    private float _movementSpeed = 4f;
    private float _stoppingDistance = .1f;

    private void Update()
    {
        if (Vector3.Distance(transform.position, _targetPosition) > _stoppingDistance)
        {
            // we dont want the magnitute that is why we normalize the vector.
            var moveDir = (_targetPosition - transform.position).normalized;
            transform.position += moveDir * _movementSpeed * Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("T");

            Move(new Vector3(4, 0, 4));
        }
    }

    private void Move(Vector3 targetPos)
    {
        _targetPosition = targetPos;
    }
}
