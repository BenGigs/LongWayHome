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
        private Grabbable held;
        private ConfigurableJoint joint;
        private Vector3 localAnchor;
        private Vector3 railNormal;
        private Collider[] heldColliders;
        private Collider[] playerColliders;
        private bool grabRequested;
        private bool releaseRequested;

        public bool IsHolding => held != null;
        public bool IsAnchored => held != null && held.IsHandrail;
        public Vector3 PushNormal => railNormal;
        public string HeldName => held == null ? "empty" : held.name;

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
            // Raycast, not sphere cast: a wall must occlude anything behind it.
            if (!UnityEngine.Physics.Raycast(view.position, view.forward, out RaycastHit hit,
                    reach, grabLayers, QueryTriggerInteraction.Ignore)) return false;
            var candidate = hit.collider.GetComponentInParent<Grabbable>();
            if (candidate == null || !candidate.TryClaim(this)) return false;

            held = candidate;
            localAnchor = held.transform.InverseTransformPoint(hit.point);
            railNormal = hit.normal;
            if (held.IsHandrail)
            {
                joint = playerBody.gameObject.AddComponent<ConfigurableJoint>();
                joint.autoConfigureConnectedAnchor = false;
                joint.anchor = Vector3.zero;
                joint.connectedAnchor = hit.point;
                joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Limited;
                joint.linearLimit = new SoftJointLimit { limit = reach };
            }
            else
            {
                handTarget.position = hit.point;
                joint = held.gameObject.AddComponent<ConfigurableJoint>();
                joint.connectedBody = handTarget;
                joint.autoConfigureConnectedAnchor = false;
                joint.anchor = localAnchor;
                joint.connectedAnchor = Vector3.zero;
                joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Free;
                var drive = new JointDrive { positionSpring = 1200f, positionDamper = 80f, maximumForce = 350f };
                joint.xDrive = joint.yDrive = joint.zDrive = drive;
                // Leave angular motion free: a tool can swing around the grasp point.
                heldColliders = held.GetComponentsInChildren<Collider>();
                SetPlayerCollisionIgnored(true);
            }
            joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Free;
            joint.enableCollision = false;
            return true;
        }

        private void FixedUpdate()
        {
            if (releaseRequested) Release();
            else if (grabRequested) TryGrab();
            grabRequested = releaseRequested = false;
            if (held != null && (!held.isActiveAndEnabled || joint == null ||
                Vector3.Distance(playerBody.position, held.transform.TransformPoint(localAnchor)) > reach + 0.8f))
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
                Destroy(joint);
            }
            SetPlayerCollisionIgnored(false);
            if (held != null) held.Release(this);
            held = null;
            joint = null;
            heldColliders = null;
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
