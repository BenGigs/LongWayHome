using UnityEngine;
using UnityEngine.InputSystem;
using LongWayHome.ShipPhysics;

namespace LongWayHome.Player
{
    /// <summary>Input, view, pause and prototype HUD; locomotion and grabbing have separate components.</summary>
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public sealed class FoundationPlayer : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private Transform view;
        [SerializeField, Min(0f)] private float lookSensitivity = 0.12f;
        [SerializeField] private PlayerMovement motor;
        [SerializeField] private HandInteractor leftHand;
        [SerializeField] private HandInteractor rightHand;
        [SerializeField] private ShipGravityController gravitySource;

        private InputActionAsset runtimeActions;
        private InputAction move;
        private InputAction look;
        private InputAction pause;
        private InputAction leftGrab;
        private InputAction rightGrab;
        private InputAction jumpPush;
        private InputAction toggleGravity;
        private float pitch;
        private bool paused;

        public bool IsPaused => paused;

        private void Awake()
        {
            if (inputActions == null || view == null || motor == null || leftHand == null || rightHand == null || gravitySource == null)
            {
                Debug.LogError("Player requires input, view, motor, hands and gravity references. Run the M2 upgrade once.", this);
                enabled = false;
                return;
            }

            runtimeActions = Instantiate(inputActions);
            move = runtimeActions.FindAction("Gameplay/Move", true);
            look = runtimeActions.FindAction("Gameplay/Look", true);
            pause = runtimeActions.FindAction("Gameplay/Pause", true);
            leftGrab = runtimeActions.FindAction("Gameplay/LeftGrab", true);
            rightGrab = runtimeActions.FindAction("Gameplay/RightGrab", true);
            jumpPush = runtimeActions.FindAction("Gameplay/JumpPush", true);
            toggleGravity = runtimeActions.FindAction("Gameplay/ToggleGravity", true);
        }

        private void OnEnable()
        {
            if (runtimeActions == null) return;
            runtimeActions.Enable();
            SetPaused(false);
        }

        private void Update()
        {
            if (pause.WasPressedThisFrame()) SetPaused(!paused);
            motor.SetMovement(paused ? Vector2.zero : move.ReadValue<Vector2>());
            if (paused) return;

            if (toggleGravity.WasPressedThisFrame())
                gravitySource.AccelerationLocal = gravitySource.AccelerationLocal.sqrMagnitude > 0.01f ? Vector3.zero : Vector3.up * 9.81f;
            if (leftGrab.WasPressedThisFrame()) leftHand.RequestGrab();
            if (!leftGrab.IsPressed()) leftHand.RequestRelease();
            if (rightGrab.WasPressedThisFrame()) rightHand.RequestGrab();
            if (!rightGrab.IsPressed()) rightHand.RequestRelease();
            if (jumpPush.WasPressedThisFrame()) motor.RequestPush();

            // Mouse delta is already a per-frame displacement; do not multiply by deltaTime.
            Vector2 delta = look.ReadValue<Vector2>() * lookSensitivity;
            motor.AddYaw(delta.x);
            pitch = Mathf.Clamp(pitch - delta.y, -85f, 85f);
            view.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void SetPaused(bool value)
        {
            paused = value;
            Time.timeScale = paused ? 0f : 1f;
            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = paused;
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused && runtimeActions != null) SetPaused(true);
        }

        private void OnDisable()
        {
            runtimeActions?.Disable();
            if (motor != null) motor.SetMovement(Vector2.zero);
            leftHand?.Release();
            rightHand?.Release();
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnDestroy()
        {
            if (runtimeActions != null) Destroy(runtimeActions);
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(16, 16, 560, paused ? 160 : 140),
                "THE LONG WAY HOME — GRAVITY PROTOTYPE\n" +
                "WASD: walk / pull while holding rail   |   Mouse: look\n" +
                "Hold LMB / RMB: grab with left / right hand   |   Release: let go\n" +
                "Space: jump / push from contact   |   G: 1g / coast   |   Esc: pause\n" +
                $"Gravity: {gravitySource.EffectiveGravity.magnitude:F2} m/s²   |   No contact = no WASD thrust\n" +
                $"Left: {leftHand.HeldName}   |   Right: {rightHand.HeldName}" +
                (paused ? "\nPAUSED — press Escape to resume" : ""));
            GUI.Label(new Rect(Screen.width / 2f - 5, Screen.height / 2f - 10, 20, 20), "+");
        }
    }
}
