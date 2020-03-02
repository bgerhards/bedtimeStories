using UnityEngine;

public class VirtualJoystickEnabler : MonoBehaviour
{
    void Start()
    {
        if (Application.isMobilePlatform)
        {
            gameObject.SetActive(true);
        }
            
    }
}
