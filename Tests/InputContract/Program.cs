using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

// Офлайн-проверка реального Update -> FixedUpdate. Legacy API намеренно недоступен.
static class Program
{
    static void Call(object target, string method, params object[] arguments) =>
        target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, arguments);

    static void Check(bool value, string message)
    {
        if (!value) throw new Exception(message);
    }

    static void Main()
    {
        Keyboard.current = new Keyboard();
        Mouse.current = new Mouse();
        var player = new SpacePlayerController();
        Call(player, "Awake");
        Call(player, "Start");
        Keyboard.current.wKey.isPressed = true;
        Mouse.current.delta.value = new Vector2(10, -5);
        Call(player, "Update");
        Call(player, "FixedUpdate");
        Check(player.testBody.lastForce.z > 0, "W must accelerate forward");
        Check(player.transform.rotation.y > 0, "Mouse must turn player");
        Keyboard.current.dKey.isPressed = true;
        Check(Math.Abs(SpaceGameInput.Movement.Magnitude - 1) < 0.001f, "Diagonal speed must be bounded");
        Keyboard.current.escapeKey.wasPressedThisFrame = true;
        Call(player, "Update");
        Call(player, "FixedUpdate");
        Check(Cursor.lockState == CursorLockMode.None && player.testBody.lastForce.Magnitude == 0, "Escape must release control");
        Keyboard.current.escapeKey.wasPressedThisFrame = false;
        Mouse.current.leftButton.wasPressedThisFrame = true;
        Mouse.current.leftButton.isPressed = true;
        Call(player, "Update");
        Check(Cursor.lockState == CursorLockMode.Locked, "Click must recapture cursor");
        Check(SpaceGameInput.MousePressed(0) && SpaceGameInput.MouseHeld(0), "Left grab input missing");
        Check(!SpaceGameInput.MouseHeld(1), "Hands must be independent");
        Mouse.current.rightButton.isPressed = true;
        Check(SpaceGameInput.MouseHeld(1), "Right grab input missing");
        Mouse.current.leftButton.isPressed = false;
        Check(!SpaceGameInput.MouseHeld(0), "Releasing button must release grab input");
        Call(player, "OnApplicationFocus", false);
        Check(Cursor.lockState == CursorLockMode.None, "Focus loss must release cursor");
        Keyboard.current = null;
        Mouse.current = null;
        Check(SpaceGameInput.Movement.Magnitude == 0 && SpaceGameInput.Look.x == 0
            && !SpaceGameInput.EscapePressed && !SpaceGameInput.MouseHeld(0), "Disconnected devices must be safe");
        Keyboard.current = new Keyboard();
        Keyboard.current.spaceKey.isPressed = true;
        Check(SpaceGameInput.Movement.y == 1, "Device reconnection must recover");
        Console.WriteLine("PASS: flight, look, cursor, hand input, disconnect and reconnect");
    }
}

namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.Class)] public class RequireComponent : Attribute { public RequireComponent(Type type) {} }
    [AttributeUsage(AttributeTargets.Field)] public class Header : Attribute { public Header(string text) {} }
    [AttributeUsage(AttributeTargets.Field)] public class SerializeField : Attribute {}
    public class MonoBehaviour
    {
        public Rigidbody testBody = new Rigidbody();
        public Transform transform = new Transform();
        public T GetComponent<T>() where T : class => testBody as T;
    }
    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 zero => new Vector2();
        public static Vector2 operator *(Vector2 v, float f) => new Vector2(v.x * f, v.y * f);
    }
    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 zero => new Vector3();
        public float sqrMagnitude => x * x + y * y + z * z;
        public float Magnitude => MathF.Sqrt(sqrMagnitude);
        public Vector3 normalized => Magnitude > 0 ? this * (1 / Magnitude) : zero;
        public static Vector3 operator *(Vector3 v, float f) => new Vector3(v.x * f, v.y * f, v.z * f);
        public static Vector3 ClampMagnitude(Vector3 v, float max) => v.Magnitude > max ? v.normalized * max : v;
    }
    public struct Quaternion
    {
        public float x, y, z;
        public static Quaternion Euler(float x, float y, float z) => new Quaternion { x = x, y = y, z = z };
    }
    public class Transform
    {
        public Vector3 eulerAngles;
        public Quaternion rotation;
        public Vector3 TransformDirection(Vector3 value) => value;
    }
    public enum ForceMode { Acceleration }
    public class Rigidbody
    {
        public bool useGravity, freezeRotation;
        public float linearDamping, angularDamping;
        public Vector3 linearVelocity, lastForce;
        public void AddForce(Vector3 value, ForceMode mode)
        {
            if (mode != ForceMode.Acceleration) throw new Exception("Unexpected force mode");
            lastForce = value;
        }
    }
    public enum CursorLockMode { None, Locked }
    public static class Cursor { public static CursorLockMode lockState; public static bool visible; }
    public static class Application { public static bool isFocused = true; }
    public static class Mathf { public static float Clamp(float value, float min, float max) => Math.Clamp(value, min, max); }
    public enum KeyCode { Space, LeftControl, C, Escape }
    public static class Input
    {
        static Exception Disabled() => new InvalidOperationException("Legacy Input backend is disabled");
        public static float GetAxis(string name) => throw Disabled();
        public static float GetAxisRaw(string name) => throw Disabled();
        public static bool GetKey(KeyCode key) => throw Disabled();
        public static bool GetKeyDown(KeyCode key) => throw Disabled();
        public static bool GetMouseButton(int button) => throw Disabled();
        public static bool GetMouseButtonDown(int button) => throw Disabled();
    }
}
namespace UnityEngine.InputSystem
{
    public class Button { public bool isPressed, wasPressedThisFrame; }
    public class Delta { public UnityEngine.Vector2 value; public UnityEngine.Vector2 ReadValue() => value; }
    public class Keyboard
    {
        public static Keyboard current;
        public Button wKey = new Button(), aKey = new Button(), sKey = new Button(), dKey = new Button(),
            upArrowKey = new Button(), downArrowKey = new Button(), leftArrowKey = new Button(), rightArrowKey = new Button(),
            spaceKey = new Button(), leftCtrlKey = new Button(), cKey = new Button(), escapeKey = new Button();
    }
    public class Mouse
    {
        public static Mouse current;
        public Button leftButton = new Button(), rightButton = new Button();
        public Delta delta = new Delta();
    }
}
