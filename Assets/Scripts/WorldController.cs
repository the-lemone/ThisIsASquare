using UnityEngine;

public class WorldController : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 5f; // how fast rotation happens

    private Quaternion _startRotation;
    private Quaternion _targetRotation;
    private float _t;
    private bool _isRotating;

    void Update()
    {
        // Q = rotate left (-90)
        if (Input.GetKeyDown(KeyCode.Q) && !_isRotating)
        {
            StartRotation(-90f);
        }

        // E = rotate right (+90)
        if (Input.GetKeyDown(KeyCode.E) && !_isRotating)
        {
            StartRotation(90f);
        }

        if (_isRotating)
        {
            _t += Time.deltaTime * rotateSpeed;
            transform.rotation = Quaternion.Slerp(_startRotation, _targetRotation, _t);

            if (_t >= 1f)
                _isRotating = false;
        }
    }

    private void StartRotation(float angle)
    {
        _startRotation = transform.rotation;
        _targetRotation = transform.rotation * Quaternion.Euler(0f, 0f, angle);
        _t = 0f;
        _isRotating = true;
    }
}
