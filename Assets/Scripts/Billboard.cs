using UnityEngine;

public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main != null)
        {
            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camUp = Camera.main.transform.up;
            transform.rotation = Quaternion.LookRotation(camForward, camUp);
        }
    }
}
