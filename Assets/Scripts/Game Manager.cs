using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool enableDebug = false;

    private bool lockCursor = true;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            enableDebug = !enableDebug;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            lockCursor = false;
        }

        if (Application.isFocused)
        {
            lockCursor = true;
        }

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
