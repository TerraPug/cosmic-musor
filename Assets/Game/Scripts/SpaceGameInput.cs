using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Один контракт ввода для полёта и клешней; новый backend имеет приоритет в режиме Both.
public static class SpaceGameInput
{
    public static Vector3 Movement
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keys = Keyboard.current;
            if (keys == null) return Vector3.zero;
            float x = (keys.dKey.isPressed || keys.rightArrowKey.isPressed ? 1f : 0f)
                - (keys.aKey.isPressed || keys.leftArrowKey.isPressed ? 1f : 0f);
            float z = (keys.wKey.isPressed || keys.upArrowKey.isPressed ? 1f : 0f)
                - (keys.sKey.isPressed || keys.downArrowKey.isPressed ? 1f : 0f);
            float y = (keys.spaceKey.isPressed ? 1f : 0f)
                - (keys.leftCtrlKey.isPressed || keys.cKey.isPressed ? 1f : 0f);
            return Vector3.ClampMagnitude(new Vector3(x, y, z), 1f);
#elif ENABLE_LEGACY_INPUT_MANAGER
            float y = (Input.GetKey(KeyCode.Space) ? 1f : 0f)
                - (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C) ? 1f : 0f);
            return Vector3.ClampMagnitude(new Vector3(Input.GetAxisRaw("Horizontal"), y,
                Input.GetAxisRaw("Vertical")), 1f);
#else
            return Vector3.zero;
#endif
        }
    }

    public static Vector2 Look
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            // Delta нового backend измеряется в пикселях, старые оси имеют множитель 0.1.
            return Mouse.current != null ? Mouse.current.delta.ReadValue() * 0.1f : Vector2.zero;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#else
            return Vector2.zero;
#endif
        }
    }

    public static bool EscapePressed
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Escape);
#else
            return false;
#endif
        }
    }

    public static bool MouseHeld(int button)
    {
#if ENABLE_INPUT_SYSTEM
        Mouse mouse = Mouse.current;
        return mouse != null && (button == 0 ? mouse.leftButton.isPressed : mouse.rightButton.isPressed);
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetMouseButton(button);
#else
        return false;
#endif
    }

    public static bool MousePressed(int button)
    {
#if ENABLE_INPUT_SYSTEM
        Mouse mouse = Mouse.current;
        return mouse != null && (button == 0 ? mouse.leftButton.wasPressedThisFrame : mouse.rightButton.wasPressedThisFrame);
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetMouseButtonDown(button);
#else
        return false;
#endif
    }
}
