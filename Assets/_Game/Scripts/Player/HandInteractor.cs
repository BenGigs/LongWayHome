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
        private Transform carriedVisual;
        private Renderer[] carriedRenderers;
        private bool[] rendererStates;
        private Quaternion pickupRotation;
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
        public Vector3 VisibleGripPoint => carriedVisual != null ? carriedVisual.TransformPoint(localAnchor) : GripPoint;

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
                    carryRadius = Mathf.Max(carryRadius, item.bounds.extents.magnitude + Vector3.Distance(item.bounds.center, held.Body.position));
                SetPlayerCollisionIgnored(true);
                CreateCarriedVisual();
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
                float t = Mathf.SmoothStep(0, 1, (Time.time - pickupTime - ReachDuration) / RetrieveDuration);
                target = Vector3.Lerp(playerBody.position + playerBody.rotation * pickupLocal, target, t);
                target = ResolveCarryAnchor(target, rotation * pickupRotation, playerBody.rotation * Vector3.up);
            }
            handTarget.MovePosition(target);
            handTarget.MoveRotation(rotation);
        }

        private void LateUpdate()
        {
            if (handVisual == null) return;
            // Render the held meshes in the same frame as the camera. Independently
            // interpolated player, target and dynamic object poses can visibly beat
            // against one another even when their FixedUpdate grip error is small.
            if (carriedVisual != null && held != null)
            {
                float retrieve = Mathf.SmoothStep(0, 1, (Time.time - pickupTime - ReachDuration) / RetrieveDuration);
                Vector3 anchor = Vector3.Lerp(playerBody.transform.TransformPoint(pickupLocal), view.TransformPoint(restOffset), retrieve);
                Quaternion rotation = view.rotation * pickupRotation;
                anchor = ResolveCarryAnchor(anchor, rotation, playerBody.transform.up);
                Vector3 position = anchor - rotation * Vector3.Scale(localAnchor, carriedVisual.localScale);
                // Camera-relative rendering only hides small interpolation errors.
                // Never show a fictional grip on the other side of a blocked deck.
                if (Vector3.Distance(position, held.transform.position) > .12f)
                {
                    position = held.transform.position;
                    rotation = held.transform.rotation;
                }
                carriedVisual.SetPositionAndRotation(position, rotation);
            }
            Vector3 target = carriedVisual != null ? VisibleGripPoint : held != null ? held.transform.TransformPoint(localAnchor) : view.TransformPoint(restOffset);
            if (held != null)
            {
                float t = Mathf.SmoothStep(0, 1, (Time.time - pickupTime) / ReachDuration);
                target = Vector3.Lerp(playerBody.transform.TransformPoint(reachStartLocal), target, t);
            }
            handVisual.position = target;
            handVisual.rotation = view.rotation;
        }

        private Vector3 ResolveCarryAnchor(Vector3 anchor, Quaternion rotation, Vector3 up)
        {
            Vector3 offset = rotation * Vector3.Scale(localAnchor, held.transform.lossyScale);
            Vector3 position = anchor - offset;
            if (!CarryPenetration(position, rotation, out Vector3 initialCorrection)) return anchor;

            // A nearby tabletop should lift the pack over its edge, not shorten the
            // arm toward the stomach. Only actual shape overlap triggers avoidance.
            // An underside contact is a ceiling, not a tabletop. Searching upward
            // there can choose an empty pose on the far side of a thin deck/roof.
            for (int step = 1; step <= 14 && Vector3.Dot(initialCorrection.normalized, up) > -.1f; step++)
            {
                Vector3 raised = position + up * (step * .04f);
                if (!CarryPenetration(raised, rotation, out _))
                {
                    float low = (step - 1) * .04f;
                    float high = step * .04f;
                    for (int refine = 0; refine < 6; refine++)
                    {
                        float middle = (low + high) * .5f;
                        if (CarryPenetration(position + up * middle, rotation, out _)) low = middle;
                        else high = middle;
                    }
                    return position + up * (high + .008f) + offset;
                }
            }
            // Tall walls/ceilings cannot be cleared by lifting. Slide out along their
            // surface normal, preserving height at a wall instead of diving inward.
            for (int step = 0; step < 8; step++)
            {
                if (!CarryPenetration(position, rotation, out Vector3 correction)) break;
                position += correction;
            }
            return position + offset;
        }

        private bool CarryPenetration(Vector3 position, Quaternion rotation, out Vector3 correction)
        {
            correction = Vector3.zero;
            // The sphere is broad phase only: it never determines the carry pose.
            var nearby = UnityEngine.Physics.OverlapSphere(position, carryRadius, grabLayers, QueryTriggerInteraction.Ignore);
            float deepest = 0;
            foreach (var item in heldColliders)
            {
                if (item == null || !item.enabled || item.isTrigger) continue;
                Vector3 partOffset = Quaternion.Inverse(held.transform.rotation) * (item.transform.position - held.transform.position);
                Quaternion partRotation = Quaternion.Inverse(held.transform.rotation) * item.transform.rotation;
                foreach (var obstacle in nearby)
                {
                    if (obstacle.attachedRigidbody == held.Body || obstacle.attachedRigidbody == playerBody) continue;
                    if (UnityEngine.Physics.ComputePenetration(item, position + rotation * partOffset, rotation * partRotation,
                        obstacle, obstacle.transform.position, obstacle.transform.rotation, out Vector3 direction, out float depth) && depth > deepest)
                    {
                        deepest = depth;
                        correction = direction * (depth + .008f);
                    }
                }
            }
            return deepest > 0;
        }

        private void CreateCarriedVisual()
        {
            pickupRotation = Quaternion.Inverse(view.rotation) * held.Body.rotation;
            carriedVisual = new GameObject(held.name + " held presentation").transform;
            carriedVisual.SetPositionAndRotation(held.Body.position, held.Body.rotation);
            carriedVisual.localScale = held.transform.lossyScale;
            carriedRenderers = held.GetComponentsInChildren<MeshRenderer>();
            rendererStates = new bool[carriedRenderers.Length];
            for (int i = 0; i < carriedRenderers.Length; i++)
            {
                var source = carriedRenderers[i];
                rendererStates[i] = source.enabled;
                var filter = source.GetComponent<MeshFilter>();
                if (filter == null) continue;
                var part = new GameObject(source.name, typeof(MeshFilter), typeof(MeshRenderer));
                part.transform.SetParent(carriedVisual, false);
                part.transform.localPosition = held.transform.InverseTransformPoint(source.transform.position);
                part.transform.localRotation = Quaternion.Inverse(held.transform.rotation) * source.transform.rotation;
                Vector3 scale = source.transform.lossyScale;
                Vector3 rootScale = held.transform.lossyScale;
                part.transform.localScale = new Vector3(scale.x / rootScale.x, scale.y / rootScale.y, scale.z / rootScale.z);
                part.GetComponent<MeshFilter>().sharedMesh = filter.sharedMesh;
                var renderer = part.GetComponent<MeshRenderer>();
                renderer.sharedMaterials = source.sharedMaterials;
                renderer.enabled = source.enabled;
                source.enabled = false;
            }
        }

        private void ClearCarriedVisual()
        {
            if (carriedRenderers != null)
                for (int i = 0; i < carriedRenderers.Length; i++)
                    if (carriedRenderers[i] != null) carriedRenderers[i].enabled = rendererStates[i];
            if (carriedVisual != null)
            {
                carriedVisual.gameObject.SetActive(false);
                Destroy(carriedVisual.gameObject);
            }
            carriedVisual = null;
            carriedRenderers = null;
            rendererStates = null;
        }

        public void Release()
        {
            // Hand rendering may lead physics by one interpolation frame. Transfer
            // only that small, collision-free difference before restoring the mesh.
            if (held != null && !held.IsHandrail && held.Body != null && carriedVisual != null &&
                Vector3.Distance(held.Body.position, carriedVisual.position) <= .15f &&
                !CarryPenetration(carriedVisual.position, carriedVisual.rotation, out _))
            {
                held.Body.position = carriedVisual.position;
                held.Body.rotation = carriedVisual.rotation;
                held.transform.SetPositionAndRotation(carriedVisual.position, carriedVisual.rotation);
            }
            ClearCarriedVisual();
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
