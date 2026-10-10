using UnityEngine;

namespace LongWayHome.Player
{
    /// <summary>Hold a mouse button to grab; release to let momentum continue.</summary>
    public sealed class HandInteractor : MonoBehaviour
    {
        [SerializeField] private Rigidbody playerBody;
        [SerializeField] private Transform view;
        [SerializeField] private Rigidbody handTarget;
        [SerializeField] private Vector3 restOffset = new Vector3(-0.25f, -0.25f, 0.65f);
        [SerializeField, Min(0.1f)] private float reach = 1.8f;
        [SerializeField] private LayerMask grabLayers = 1;
        [SerializeField, Range(0f, .4f)] private float ledgeAimTolerance = .28f;
        private readonly RaycastHit[] ledgeHits = new RaycastHit[64];
        private Grabbable held;
        private ConfigurableJoint joint;
        private Vector3 localAnchor;
        private Vector3 railNormal;
        private Collider[] heldColliders;
        private Collider[] playerColliders;
        private bool grabRequested;
        private bool releaseRequested;

        public bool IsHolding => held != null;
        public Grabbable HeldObject => held;
        public bool IsAnchored => held != null && held.IsHandrail;
        public Vector3 PushNormal => railNormal;
        public string HeldName => held == null ? "empty" : held.name;
        public LedgeRing HeldLedge => IsAnchored ? held.GetComponentInParent<LedgeRing>() : null;
        public Vector3 GripPoint => held == null ? Vector3.zero : HeldPhysicsMatrix().MultiplyPoint3x4(localAnchor);

        public void RequestGrab()
        {
            grabRequested = true;
            releaseRequested = false;
        }

        public void RequestRelease()
        {
            releaseRequested = true;
            grabRequested = false;
        }

        private void Awake()
        {
            if (playerBody == null || view == null || handTarget == null)
            {
                Debug.LogError("HandInteractor requires player, view and hand target references.", this);
                enabled = false;
                return;
            }
            playerColliders = playerBody.GetComponentsInChildren<Collider>();
        }

        public bool TryGrab()
        {
            if (!isActiveAndEnabled || held != null) return false;
            if (!FindGrab(out RaycastHit hit)) return false;
            var candidate = hit.collider.GetComponentInParent<Grabbable>();
            var ledge = candidate != null ? candidate.GetComponentInParent<LedgeRing>() : null;
            // Upper ledges are caught from a jump/fall, not by reaching through a ceiling
            // while standing. Ordinary rails and floor-level rings retain their behavior.
            if (ledge != null && Vector3.Dot(ledge.transform.position - playerBody.position, ledge.Up) > .25f &&
                playerBody.TryGetComponent<PlayerMovement>(out var motor) && motor.IsGrounded) return false;
            if (candidate == null || !candidate.TryClaim(this)) return false;

            held = candidate;
            // Raycasts use the physics pose. An interpolated Transform can lag behind
            // it, producing the wrong grip point on a moving (or repositioned) tool.
            localAnchor = HeldPhysicsMatrix().inverse.MultiplyPoint3x4(hit.point);
            railNormal = hit.normal;
            if (held.IsHandrail)
            {
                joint = playerBody.gameObject.AddComponent<ConfigurableJoint>();
                joint.autoConfigureConnectedAnchor = false;
                joint.anchor = Vector3.zero;
                joint.connectedAnchor = hit.point;
                joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Limited;
                joint.linearLimit = new SoftJointLimit { limit = reach };
                joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Free;
            }
            else
            {
                handTarget.position = hit.point;
                handTarget.rotation = view.rotation;
                // Joint creation captures Transform frames. Synchronize them once to
                // the current physics poses so interpolation cannot capture an old grip orientation.
                held.transform.SetPositionAndRotation(held.Body.position, held.Body.rotation);
                handTarget.transform.SetPositionAndRotation(handTarget.position, handTarget.rotation);
                joint = held.gameObject.AddComponent<ConfigurableJoint>();
                joint.connectedBody = handTarget;
                joint.autoConfigureConnectedAnchor = false;
                joint.anchor = localAnchor;
                joint.connectedAnchor = Vector3.zero;
                joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Free;
                var drive = new JointDrive { positionSpring = 1200f, positionDamper = 80f, maximumForce = 350f };
                joint.xDrive = joint.yDrive = joint.zDrive = drive;
                // A grip holds orientation relative to the hand, rather than behaving
                // like a ball-and-socket. Capture the pickup pose, then turn with the hand.
                joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Locked;
                heldColliders = held.GetComponentsInChildren<Collider>();
                SetPlayerCollisionIgnored(true);
            }
            joint.enableCollision = false;
            return true;
        }

        private bool FindGrab(out RaycastHit hit)
        {
            // Precise targeting always wins, including loose objects and ordinary rails.
            if (UnityEngine.Physics.Raycast(view.position, view.forward, out hit,
                reach, grabLayers, QueryTriggerInteraction.Ignore) &&
                hit.collider.GetComponentInParent<Grabbable>() != null) return true;

            int count = UnityEngine.Physics.SphereCastNonAlloc(view.position, ledgeAimTolerance,
                view.forward, ledgeHits, reach, grabLayers, QueryTriggerInteraction.Ignore);
            float bestError = float.PositiveInfinity;
            bool found = false;
            for (int i = 0; i < count; i++)
            {
                var candidate = ledgeHits[i];
                if (candidate.collider.GetComponentInParent<LedgeRing>() == null) continue;
                Vector3 offset = candidate.point - view.position;
                float distance = offset.magnitude;
                if (distance < .001f || distance > reach || Vector3.Dot(offset, view.forward) <= 0) continue;
                float error = Vector3.Angle(view.forward, offset);
                if (error >= bestError) continue;
                // A broad search never permits a grip through a wall or deck slab.
                if (!UnityEngine.Physics.Raycast(view.position, offset / distance, out var visible,
                    distance + .02f, grabLayers, QueryTriggerInteraction.Ignore) ||
                    visible.collider != candidate.collider) continue;
                bestError = error;
                hit = visible;
                found = true;
            }
            return found;
        }

        private void FixedUpdate()
        {
            if (releaseRequested) Release();
            else if (grabRequested) TryGrab();
            grabRequested = releaseRequested = false;
            if (held != null && (!held.isActiveAndEnabled || joint == null ||
                Vector3.Distance(playerBody.position, HeldPhysicsMatrix().MultiplyPoint3x4(localAnchor)) > reach + 0.8f))
                Release();
            if (held == null && joint != null) Release();
            Vector3 target = IsAnchored ? held.transform.TransformPoint(localAnchor) : view.TransformPoint(restOffset);
            handTarget.MovePosition(target);
            handTarget.MoveRotation(view.rotation);
        }

        public void Release()
        {
            grabRequested = releaseRequested = false;
            if (joint != null)
            {
                // Destroy is deferred; free the constraint now so push-off works this step.
                joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Free;
                joint.xDrive = joint.yDrive = joint.zDrive = default;
                joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Free;
                Destroy(joint);
            }
            SetPlayerCollisionIgnored(false);
            if (held != null) held.Release(this);
            held = null;
            joint = null;
            heldColliders = null;
        }

        private Matrix4x4 HeldPhysicsMatrix()
        {
            if (held.IsHandrail) return held.transform.localToWorldMatrix;
            Rigidbody body = held.Body;
            return Matrix4x4.TRS(body.position, body.rotation, held.transform.lossyScale);
        }

        private void SetPlayerCollisionIgnored(bool ignored)
        {
            if (heldColliders == null || playerColliders == null) return;
            foreach (Collider item in heldColliders)
                foreach (Collider player in playerColliders)
                    if (item != null && player != null) UnityEngine.Physics.IgnoreCollision(item, player, ignored);
        }

        private void OnDisable() => Release();
    }
}
