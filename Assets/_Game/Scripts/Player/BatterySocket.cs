using UnityEngine;
using UnityEngine.InputSystem;
using LongWayHome.ShipPhysics;

namespace LongWayHome.Player
{
    /// <summary>A local, reusable battery latch for the workshop delivery playtest.</summary>
    public sealed class BatterySocket : MonoBehaviour
    {
        [SerializeField] private Grabbable battery;
        [SerializeField] private Transform mount;
        [SerializeField] private FoundationPlayer player;
        [SerializeField] private Transform view;
        [SerializeField] private HandInteractor leftHand;
        [SerializeField] private HandInteractor rightHand;
        [SerializeField] private Light poweredLight;
        [SerializeField] private Renderer indicator;
        [SerializeField] private Material offMaterial;
        [SerializeField] private Material onMaterial;
        [SerializeField] private Transform fan;
        private bool powered;
        private bool gravityWasEnabled;
        public bool IsPowered => powered;
        public Grabbable Battery => battery;
        public Transform Mount => mount;

        private void Start() => SetPower(false);

        private void Update()
        {
            if (powered && fan != null) fan.Rotate(0, 0, 180 * Time.deltaTime, Space.Self);
            if (player == null || player.IsPaused || Time.timeScale == 0 || Keyboard.current == null ||
                !Keyboard.current.eKey.wasPressedThisFrame || !IsAimedAt()) return;
            if (powered) RemoveBattery();
            else if (!TryInstall(leftHand)) TryInstall(rightHand);
        }

        private bool IsAimedAt()
        {
            // The carried pack can obscure the socket, but walls and other objects cannot.
            var hits = UnityEngine.Physics.RaycastAll(view.position, view.forward, 1.8f, 1, QueryTriggerInteraction.Ignore);
            float closest = float.PositiveInfinity;
            Collider target = null;
            foreach (var hit in hits)
            {
                if ((leftHand.HeldObject == battery || rightHand.HeldObject == battery) &&
                    hit.collider.GetComponentInParent<Grabbable>() == battery) continue;
                if (hit.distance >= closest) continue;
                closest = hit.distance;
                target = hit.collider;
            }
            return target != null && (target.GetComponentInParent<BatterySocket>() == this ||
                (powered && target.GetComponentInParent<Grabbable>() == battery));
        }

        public bool TryInstall(HandInteractor hand)
        {
            if (!isActiveAndEnabled || powered || hand == null || hand.HeldObject != battery ||
                Vector3.Distance(battery.Body.position, mount.position) > .8f) return false;
            // Reject obstructions between the carried pack and the latch.
            Vector3 travel = mount.position - battery.Body.position;
            foreach (var hit in UnityEngine.Physics.RaycastAll(battery.Body.position, travel.normalized,
                travel.magnitude, 1, QueryTriggerInteraction.Ignore))
                if (hit.collider.GetComponentInParent<Grabbable>() != battery &&
                    hit.collider.GetComponentInParent<BatterySocket>() != this) return false;
            hand.Release();
            var body = battery.Body;
            body.linearVelocity = body.angularVelocity = Vector3.zero;
            var gravity = battery.GetComponent<ShipGravityBody>();
            gravityWasEnabled = gravity != null && gravity.enabled;
            if (gravity != null) gravity.enabled = false;
            body.isKinematic = true;
            body.position = mount.position;
            body.rotation = mount.rotation;
            body.transform.SetPositionAndRotation(mount.position, mount.rotation);
            SetPower(true);
            return true;
        }

        public void RemoveBattery()
        {
            if (!powered) return;
            if (battery == null || battery.Body == null) { SetPower(false); return; }
            battery.Body.isKinematic = false;
            var gravity = battery.GetComponent<ShipGravityBody>();
            if (gravity != null) gravity.enabled = gravityWasEnabled;
            // A small latch-ejection impulse exposes the pack for another pickup.
            battery.Body.linearVelocity = -mount.forward * .35f;
            SetPower(false);
        }

        private void SetPower(bool value)
        {
            powered = value;
            if (poweredLight != null) poweredLight.enabled = value;
            if (indicator != null) indicator.sharedMaterial = value ? onMaterial : offMaterial;
        }

        private void OnDisable() { if (powered) RemoveBattery(); }
    }
}
