using UnityEngine;

public class FpsSetting : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 60;
    }
}
