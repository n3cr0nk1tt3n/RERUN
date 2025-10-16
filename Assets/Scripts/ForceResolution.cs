using UnityEngine;

public class ForceResolution : MonoBehaviour
{
    void Start()
    {
        // Set resolution to 1920x1080 and fullscreen mode
        Screen.SetResolution(1920, 1080, FullScreenMode.ExclusiveFullScreen);
    }
}
