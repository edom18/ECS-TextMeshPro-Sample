using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CameraPosition : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private Button _button;
    [SerializeField] private Vector3 _targetPosition = new Vector3(0, 0, -1f);

    private bool _isFar = true;

    private Vector3 _initPosition;
    private TMP_Text _text;

    private void Start()
    {
        _text = _button.GetComponentInChildren<TMP_Text>();

        _initPosition = _camera.transform.position;

        _button.onClick.AddListener(() =>
        {
            if (_isFar)
            {
                _camera.transform.position = _targetPosition;
            }
            else
            {
                _camera.transform.position = _initPosition;
            }

            _isFar = !_isFar;

            UpdateText();
        });

        UpdateText();
    }

    private void UpdateText()
    {
        _text.text = _isFar ? "To Close" : "To Far";
    }
}