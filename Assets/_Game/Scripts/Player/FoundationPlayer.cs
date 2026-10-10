using UnityEngine;
using UnityEngine.InputSystem;

namespace LongWayHome.Player
{
    /// <summary>Grounded M1 controller. M2 will replace movement with ship-relative physics.</summary>
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public sealed class FoundationPlayer : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private Transform view;
        [SerializeField, Min(0f)] private float walkSpeed = 3f;
        [SerializeField, Min(0f)] private float lookSensitivity = 0.12f;
        [SerializeField] private LayerMask groundLayers = 1;

        private Rigidbody body;
        private InputActionAsset runtimeActions;
        private InputAction move;
        private InputAction look;
        private InputAction pause;
        private Vector2 movement;
        private float yaw;
        private float pitch;
        private bool paused;

        public bool IsPaused => paused;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            if (inputActions == null || view == null)
            {
                Debug.LogError("FoundationPlayer requires input actions and a view transform.", this);
                enabled = false;
                return;
            }

            runtimeActions = Instantiate(inputActions);
            move = runtimeActions.FindAction("Gameplay/Move", true);
            look = runtimeActions.FindAction("Gameplay/Look", true);
            pause = runtimeActions.FindAction("Gameplay/Pause", true);
            yaw = body.rotation.eulerAngles.y;
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
            movement = paused ? Vector2.zero : move.ReadValue<Vector2>();
            if (paused) return;

            // Mouse delta is already a per-frame displacement; do not multiply by deltaTime.
            Vector2 delta = look.ReadValue<Vector2>() * lookSensitivity;
            yaw += delta.x;
            pitch = Mathf.Clamp(pitch - delta.y, -85f, 85f);
            view.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        private void FixedUpdate()
        {
            if (paused) return;
            body.MoveRotation(Quaternion.Euler(0f, yaw, 0f));
            // The player is on layer 2, excluded from groundLayers. No air steering.
            if (!Physics.SphereCast(body.position, 0.28f, Vector3.down, out _,
                    0.8f, groundLayers, QueryTriggerInteraction.Ignore)) return;

            Vector3 target = Quaternion.Euler(0f, yaw, 0f) *
                new Vector3(movement.x, 0f, movement.y) * walkSpeed;
            body.linearVelocity = new Vector3(target.x, body.linearVelocity.y, target.z);
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
            GUI.Box(new Rect(16, 16, 440, paused ? 100 : 80),
                "THE LONG WAY HOME — FOUNDATION\n" +
                "WASD: walk   |   Mouse: look   |   Esc: pause / resume\n" +
                "Grounded test only. Hands and ship gravity arrive in Milestone 2." +
                (paused ? "\nPAUSED — press Escape to resume" : ""));
        }
    }
}
