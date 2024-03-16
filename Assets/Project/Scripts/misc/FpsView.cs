using TMPro;
using UnityEngine;

public class FpsView : MonoBehaviour
{
    [SerializeField] private TMP_Text _view;

    private int _count;
    private float _prevTime;

    private void Start()
    {
        _count = 0;
        _prevTime = 0f;
    }

    private void Update()
    {
        _count++;
        float time = Time.realtimeSinceStartup - _prevTime;

        if (time >= 0.5f)
        {
            float fps = _count / time;
            _view.text = $"FPS: {fps:F2}";
            _count = 0;
            _prevTime = Time.realtimeSinceStartup;
        }
    }
}