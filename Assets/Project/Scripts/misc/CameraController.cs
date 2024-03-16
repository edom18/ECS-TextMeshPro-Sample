using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _target = null;
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _rotateSpeed = 20f;
    [SerializeField] private float _boost = 2f;

    private bool _isMoveMode = false;
    private Vector3 _prevPos = Vector3.zero;

    private float MoveSpeed
    {
        get
        {
            float speed = _moveSpeed * Time.deltaTime;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                speed *= _boost;
            }

            return speed;
        }
    }

    private float RotateSpeed => _rotateSpeed * Time.deltaTime;

    #region ### MonoBehaviour ###

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            StartMove();
        }

        if (Input.GetMouseButtonUp(1))
        {
            EndMove();
        }

        if (_isMoveMode)
        {
            TryMove();
            TryRotate();
        }
    }

    private void Reset()
    {
        _target = transform;
    }

    #endregion ### MonoBehaviour ###

    private void StartMove()
    {
        _isMoveMode = true;
        _prevPos = Input.mousePosition;
    }

    private void EndMove()
    {
        _isMoveMode = false;
    }

    private void TryMove()
    {
        if (Input.GetKey(KeyCode.W))
        {
            _target.position += _target.forward * MoveSpeed;
        }

        if (Input.GetKey(KeyCode.A))
        {
            _target.position += -_target.right * MoveSpeed;
        }

        if (Input.GetKey(KeyCode.S))
        {
            _target.position += -_target.forward * MoveSpeed;
        }

        if (Input.GetKey(KeyCode.D))
        {
            _target.position += _target.right * MoveSpeed;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            _target.position += -_target.up * MoveSpeed;
        }

        if (Input.GetKey(KeyCode.E))
        {
            _target.position += _target.up * MoveSpeed;
        }
    }

    private void TryRotate()
    {
        Vector3 delta = Input.mousePosition - _prevPos;

        transform.Rotate(Vector3.up, delta.x * RotateSpeed, Space.World);

        Vector3 rightAxis = Vector3.Cross(transform.forward, Vector3.up);
        transform.Rotate(rightAxis.normalized, delta.y * RotateSpeed, Space.World);

        _prevPos = Input.mousePosition;
    }
}