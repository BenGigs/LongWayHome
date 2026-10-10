using UnityEngine;

namespace LongWayHome.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class LedgeClimber : MonoBehaviour
    {
        private enum Phase { None, Hanging, Rising, Crossing, Retreating, Lowering }
        [SerializeField, Min(0.1f)] private float climbSpeed = 2.4f;
        [SerializeField, Min(0.1f)] private float acceleration = 35f;
        [SerializeField, Min(0.01f)] private float pullResponseTime = .16f;
        private Rigidbody body;
        private Phase phase;
        private LedgeRing ring;
        private Vector3 grip;
        private Vector3 lateralOffset;
        private bool waitingForMoveRelease;
        public bool IsActive => phase != Phase.None;

        private void Awake() => body = GetComponent<Rigidbody>();

        // Called by the motor in FixedUpdate, after input and gravity state are known.
        public bool Tick(Vector2 movement, Vector3 gravity, Vector3 up, HandInteractor left, HandInteractor right)
        {
            // The player may still be looking toward the shaft when the mantle ends.
            // Consume that held W until released so the first frame cannot walk back in.
            if (waitingForMoveRelease)
            {
                if (movement.y > 0.5f) return true;
                waitingForMoveRelease = false;
            }
            HandInteractor hand = left != null && left.HeldLedge != null ? left : right;
            if (gravity.sqrMagnitude <= 0.01f || hand == null || hand.HeldLedge == null ||
                Vector3.Dot(hand.HeldLedge.Up, up) < 0.98f)
            {
                Cancel();
                return false;
            }
            if (ring != hand.HeldLedge)
            {
                Cancel();
                // Only catch a deck above the torso. Holding a floor ring while walking
                // or jumping does not start another mantle or open either hand.
                if (Vector3.Dot(hand.HeldLedge.transform.position - body.position, up) < 0.25f)
                    return false;
                ring = hand.HeldLedge;
                grip = hand.GripPoint;
                phase = Phase.Hanging;
            }
            ring.GetTargets(grip, out Vector3 hang, out Vector3 above, out Vector3 landing);
            // Shift the body under the fixed hand grip, within the open hatch and arm reach.
            lateralOffset = Vector3.ClampMagnitude(lateralOffset +
                Vector3.ProjectOnPlane(transform.right, up).normalized *
                (movement.x * 1.2f * Time.fixedDeltaTime), .35f);
            hang += lateralOffset;
            above += lateralOffset;
            landing += lateralOffset;
            if (movement.y < -.5f && phase != Phase.Retreating && phase != Phase.Lowering)
                phase = phase == Phase.Crossing ? Phase.Retreating : Phase.Lowering;
            if (movement.y > .5f && phase == Phase.Lowering) phase = Phase.Rising;
            if (movement.y > .5f && phase == Phase.Retreating) phase = Phase.Crossing;
            Vector3 target = hang;
            if (phase == Phase.Hanging && movement.y > 0.5f && Vector3.Distance(body.position, hang) < 0.18f)
                phase = Phase.Rising;
            if (phase == Phase.Rising)
            {
                target = above;
                if (Vector3.Distance(body.position, above) < 0.10f) phase = Phase.Crossing;
            }
            if (phase == Phase.Crossing)
            {
                target = landing;
                if (Vector3.Distance(body.position, landing) < 0.10f)
                {
                    Cancel();
                    waitingForMoveRelease = true;
                    return true;
                }
            }
            // Reverse a mantle by moving back into the opening before lowering.
            if (phase == Phase.Retreating)
            {
                target = above;
                if (Vector3.Distance(body.position, above) < .10f) phase = Phase.Lowering;
            }
            if (phase == Phase.Lowering) target = hang - up * .4f;
            Vector3 desired = Vector3.ClampMagnitude((target - body.position) * 6f, climbSpeed);
            bool verticalInput = Mathf.Abs(movement.y) > .5f;
            if (phase != Phase.Hanging && !verticalInput)
            {
                desired = Mathf.Abs(movement.x) > .01f
                    ? Vector3.Project(desired, Vector3.ProjectOnPlane(transform.right, up)) : Vector3.zero;
            }
            // Ease into pulling and around the lip without weakening the fall arrest.
            // Pausing W still brakes promptly; smoothing only applies to active pulling.
            if (phase != Phase.Hanging && (verticalInput || Mathf.Abs(movement.x) > .01f))
                desired = Vector3.Lerp(body.linearVelocity, desired,
                    1f - Mathf.Exp(-Time.fixedDeltaTime / pullResponseTime));
            // Muscle reaction through the held ring; remains dynamic and collidable.
            // Never move the Transform, make the body kinematic, or disable collision.
            body.AddForce(-gravity, ForceMode.Acceleration);
            body.AddForce(Vector3.ClampMagnitude(desired - body.linearVelocity,
                acceleration * Time.fixedDeltaTime), ForceMode.VelocityChange);
            return true;
        }

        public void Cancel() { phase = Phase.None; ring = null; lateralOffset = Vector3.zero; }
        private void OnDisable() { Cancel(); waitingForMoveRelease = false; }
    }
}
