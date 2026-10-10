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
        private Transform handVisual;
        private Renderer originalHandRenderer;
        private Vector3 reachStartLocal;
        private Vector3 pickupLocal;
        private float pickupTime;
        private LedgeClimber climber;
        private int originalSolverIterations;
        private int originalVelocityIterations;
        private float carryRadius;
        private const float ReachDuration = .18f;
        private const float RetrieveDuration = .28f;

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
            climber = playerBody.GetComponent<LedgeClimber>();
            // Keep the visible hand separate from its physics target, so reaching can
            // be animated without snapping the hand renderer to the pickup point.
            originalHandRenderer = handTarget.GetComponent<Renderer>();
            var mesh = handTarget.GetComponent<MeshFilter>();
            if (originalHandRenderer != null && mesh != null)
            {
                var visual = new GameObject(name + " visible grip", typeof(MeshFilter), typeof(MeshRenderer));
                visual.GetComponent<MeshFilter>().sharedMesh = mesh.sharedMesh;
                visual.GetComponent<MeshRenderer>().sharedMaterials = originalHandRenderer.sharedMaterials;
                handVisual = visual.transform;
                handVisual.localScale = handTarget.transform.lossyScale;
                handVisual.position = handTarget.position;
                originalHandRenderer.enabled = false;
            }
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
            pickupTime = Time.time;
            reachStartLocal = playerBody.transform.InverseTransformPoint(handVisual != null ? handVisual.position : handTarget.position);
            pickupLocal = Quaternion.Inverse(playerBody.rotation) * (hit.point - playerBody.position);
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
                // A carried object has a firm grip, not a spring that oscillates and
                // stretches when the player accelerates. World collisions remain active.
                joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Locked;
                originalSolverIterations = held.Body.solverIterations;
                originalVelocityIterations = held.Body.solverVelocityIterations;
                held.Body.solverIterations = 16;
                held.Body.solverVelocityIterations = 8;
                // A grip holds orientation relative to the hand, rather than behaving
                // like a ball-and-socket. Capture the pickup pose, then turn with the hand.
                joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Locked;
                heldColliders = held.GetComponentsInChildren<Collider>();
                carryRadius = .1f;
                foreach (var item in heldColliders)
                    carryRadius = Mathf.Max(carryRadius, item.bounds.extents.magnitude + Vector3.Distance(item.bounds.center, hit.point));
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
                (IsAnchored && Vector3.Distance(playerBody.position, GripPoint) > reach + 0.8f)))
                Release();
            if (held == null && joint != null) Release();
            // Interpolated camera transforms describe the previous rendered pose.
            // Convert them back to the current Rigidbody pose before driving physics.
            Quaternion rotation = playerBody.rotation * Quaternion.Inverse(playerBody.transform.rotation) * view.rotation;
            Vector3 eye = playerBody.position + playerBody.rotation * playerBody.transform.InverseTransformPoint(view.position);
            Vector3 target = IsAnchored ? GripPoint : eye + rotation * restOffset;
            if (held != null && !IsAnchored)
            {
                Vector3 chest = playerBody.position + playerBody.rotation * Vector3.up * .25f;
                if (climber != null && climber.IsActive)
                    target = chest + playerBody.rotation * new Vector3(Mathf.Sign(restOffset.x) * .14f, 0, .12f);
                float t = Mathf.SmoothStep(0, 1, (Time.time - pickupTime - ReachDuration) / RetrieveDuration);
                target = Vector3.Lerp(playerBody.position + playerBody.rotation * pickupLocal, target, t);
                // Retract the grip toward the body before the object hits a wall/rim.
                // Conservative bounds include the offset from the contact point to its centre.
                Vector3 path = target - chest;
                float distance = path.magnitude;
                if (distance > .001f && t > 0)
                    foreach (var hit in UnityEngine.Physics.SphereCastAll(chest, carryRadius, path / distance, distance, grabLayers, QueryTriggerInteraction.Ignore))
                    {
                        if (hit.collider.GetComponentInParent<Grabbable>() == held || hit.collider.attachedRigidbody == playerBody) continue;
                        target = chest + path.normalized * Mathf.Min(Vector3.Distance(chest, target), Mathf.Max(0, hit.distance - .04f));
                    }
            }
            handTarget.MovePosition(target);
            handTarget.MoveRotation(rotation);
        }

        private void LateUpdate()
        {
            if (handVisual == null) return;
            Vector3 target = held != null ? held.transform.TransformPoint(localAnchor) : view.TransformPoint(restOffset);
            if (held != null)
            {
                float t = Mathf.SmoothStep(0, 1, (Time.time - pickupTime) / ReachDuration);
                target = Vector3.Lerp(playerBody.transform.TransformPoint(reachStartLocal), target, t);
            }
            handVisual.position = target;
            handVisual.rotation = view.rotation;
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
            if (held != null && !held.IsHandrail && held.Body != null)
            {
                held.Body.solverIterations = originalSolverIterations;
                held.Body.solverVelocityIterations = originalVelocityIterations;
            }
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

        private void OnEnable() { if (handVisual != null) handVisual.gameObject.SetActive(true); }
        private void OnDisable()
        {
            Release();
            if (handVisual != null) handVisual.gameObject.SetActive(false);
        }
        private void OnDestroy()
        {
            if (handVisual != null) Destroy(handVisual.gameObject);
            if (originalHandRenderer != null) originalHandRenderer.enabled = true;
        }
    }
}
