using LongWayHome.ShipPhysics;
using UnityEngine;

namespace LongWayHome.Player
{
    [RequireComponent(typeof(Rigidbody), typeof(ShipGravityBody))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private Transform view;
        [SerializeField] private HandInteractor leftHand;
        [SerializeField] private HandInteractor rightHand;
        [SerializeField, Min(0f)] private float walkSpeed = 3f;
        [SerializeField, Min(0f)] private float contactAcceleration = 12f;
        [SerializeField, Min(0f)] private float pullSpeed = 2f;
        [SerializeField, Min(0f)] private float pushSpeed = 2.5f;
        [SerializeField] private LayerMask groundLayers = 1;
        private Rigidbody body;
        private ShipGravityBody gravityBody;
        private Vector2 movement;
        private float pendingYaw;
        private bool pushRequested;
        private Vector3 contactNormal;
        private Collider contactSurface;
        private CapsuleCollider capsule;
        public bool IsGrounded { get; private set; }
        public bool HasRail => (leftHand != null && leftHand.IsAnchored) || (rightHand != null && rightHand.IsAnchored);

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            gravityBody = GetComponent<ShipGravityBody>();
            capsule = GetComponent<CapsuleCollider>();
            if (view == null)
            {
                Debug.LogError("PlayerMovement requires a view transform.", this);
                enabled = false;
            }
        }

        public void SetMovement(Vector2 value) => movement = Vector2.ClampMagnitude(value, 1f);
        public void AddYaw(float degrees) => pendingYaw += degrees;
        public void RequestPush() => pushRequested = true;

        private void FixedUpdate()
        {
            Vector3 gravity = gravityBody.Gravity;
            bool powered = gravity.sqrMagnitude > 0.01f;
            Vector3 up = powered ? -gravity.normalized : body.rotation * Vector3.up;
            Quaternion aligned = Quaternion.FromToRotation(body.rotation * Vector3.up, up) * body.rotation;
            body.MoveRotation(Quaternion.AngleAxis(pendingYaw, up) * Quaternion.RotateTowards(body.rotation, aligned, 120f * Time.fixedDeltaTime));
            pendingYaw = 0f;
            IsGrounded = powered && UnityEngine.Physics.SphereCast(body.position, 0.28f, -up,
                out RaycastHit hit, 0.8f, groundLayers, QueryTriggerInteraction.Ignore) && Vector3.Dot(hit.normal, up) > 0.65f;

            bool pushed = pushRequested && TryPush(powered, up);
            pushRequested = false;
            if (pushed) return;
            if (IsGrounded)
            {
                Vector3 forward = Vector3.ProjectOnPlane(view.forward, up).normalized;
                Vector3 right = Vector3.Cross(up, forward);
                Vector3 desired = (forward * movement.y + right * movement.x) * walkSpeed;
                Vector3 current = Vector3.ProjectOnPlane(body.linearVelocity, up);
                body.AddForce(Vector3.ClampMagnitude(desired - current, contactAcceleration * Time.fixedDeltaTime), ForceMode.VelocityChange);
            }
            else if (HasRail)
            {
                Vector3 desired = (view.forward * movement.y + view.right * movement.x) * pullSpeed;
                // Reaction comes from the held static rail. No contact => no locomotion force.
                body.AddForce(Vector3.ClampMagnitude(desired - body.linearVelocity,
                    contactAcceleration * Time.fixedDeltaTime), ForceMode.VelocityChange);
            }
        }

        private bool TryPush(bool powered, Vector3 up)
        {
            Vector3 normal;
            if (IsGrounded) normal = up;
            else if (!powered && HasRail) normal = leftHand != null && leftHand.IsAnchored ? leftHand.PushNormal : rightHand.PushNormal;
            else if (!powered && HasSurfaceContact()) normal = contactNormal;
            else return false;
            leftHand?.Release();
            rightHand?.Release();
            body.AddForce(normal.normalized * pushSpeed, ForceMode.VelocityChange);
            IsGrounded = false;
            return true;
        }

        private bool HasSurfaceContact()
        {
            // Sleeping bodies stop receiving OnCollisionStay. Retain the contact, but
            // verify geometry so teleporting or disabling a surface cannot grant thrust.
            if (contactSurface == null || !contactSurface.enabled || !contactSurface.gameObject.activeInHierarchy) return false;
            Vector3 surfacePoint = contactSurface.ClosestPoint(body.position);
            return Vector3.Distance(surfacePoint, capsule.ClosestPoint(surfacePoint)) <= 0.08f;
        }

        private void OnCollisionEnter(Collision collision) => RememberContact(collision);
        private void OnCollisionStay(Collision collision) => RememberContact(collision);
        private void OnCollisionExit(Collision collision)
        {
            if (contactSurface == collision.collider) contactSurface = null;
        }

        private void RememberContact(Collision collision)
        {
            // Loose objects do not count as an immovable surface for a free push impulse.
            if (collision.rigidbody != null && !collision.rigidbody.isKinematic) return;
            contactNormal = collision.GetContact(0).normal;
            contactSurface = collision.collider;
        }

        private void OnDisable()
        {
            movement = Vector2.zero;
            pendingYaw = 0f;
            pushRequested = false;
        }
    }
}
