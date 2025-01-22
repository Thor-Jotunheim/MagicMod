using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public bool IsPlacementKeyHeld()
    {
        return Input.GetKey(KeyCode.E);  // ✅ Handles input properly inside MonoBehaviour
    }
}