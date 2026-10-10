using UnityEngine;

namespace LongWayHome.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class LedgeClimber : MonoBehaviour
    {
        private enum Phase { None, Hanging, Rising, Crossing }
        [SerializeField, Min(0.1f)] private float climbSpeed = 2.4f;
        [SerializeField, Min(0.1f)] private float acceleration = 35f;
        [SerializeField, Min(0.01f)] private float pullResponseTime = .16f;
        private Rigidbody body;
        private Phase phase;
        private LedgeRing ring;
        private Vector3 grip;
        private bool waitingForMoveRelease;
        public bool IsActive => phase != Phase.None;
        public string Status => waitingForMoveRelease ? "On deck — release W, then move normally" : phase == Phase.None ? "Jump, aim up and hold a grab button" :
            phase == Phase.Hanging ? "Hanging — hold W to pull up; release grip to fall" : "Pulling up — keep W and grip held";

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
            Vector3 desired = Vector3.ClampMagnitude((target - body.position) * 6f, climbSpeed);
            if (phase != Phase.Hanging && movement.y <= 0.5f) desired = Vector3.zero;
            // Ease into pulling and around the lip without weakening the fall arrest.
            // Pausing W still brakes promptly; smoothing only applies to active pulling.
            if (phase != Phase.Hanging && movement.y > .5f)
                desired = Vector3.Lerp(body.linearVelocity, desired,
                    1f - Mathf.Exp(-Time.fixedDeltaTime / pullResponseTime));
            // Muscle reaction through the held ring; remains dynamic and collidable.
            // Never move the Transform, make the body kinematic, or disable collision.
            body.AddForce(-gravity, ForceMode.Acceleration);
            body.AddForce(Vector3.ClampMagnitude(desired - body.linearVelocity,
                acceleration * Time.fixedDeltaTime), ForceMode.VelocityChange);
            return true;
        }

        public void Cancel() { phase = Phase.None; ring = null; }
        private void OnDisable() { Cancel(); waitingForMoveRelease = false; }
    }
}
