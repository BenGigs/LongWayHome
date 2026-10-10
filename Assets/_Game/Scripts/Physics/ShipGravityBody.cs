using UnityEngine;

namespace LongWayHome.ShipPhysics
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ShipGravityBody : MonoBehaviour
    {
        [SerializeField] private ShipGravityController source;
        private Rigidbody body;
        public ShipGravityController Source { get => source; set => source = value; }
        public Vector3 Gravity => source != null && source.isActiveAndEnabled ? source.EffectiveGravity : Vector3.zero;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = false;
        }

        private void Start()
        {
            if (source == null)
            {
                Debug.LogError("ShipGravityBody requires an explicit ship gravity source.", this);
                enabled = false;
            }
        }

        private void FixedUpdate()
        {
            // Acceleration mode keeps apparent gravity independent of astronaut/object mass.
            body.AddForce(Gravity, ForceMode.Acceleration);
        }
    }
}
