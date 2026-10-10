using System;
using UnityEngine;

namespace LongWayHome.ShipPhysics
{
    /// <summary>Translational ship-frame approximation; excludes rotational inertial forces.</summary>
    public sealed class ShipGravityController : MonoBehaviour
    {
        [SerializeField] private Vector3 accelerationLocal = new Vector3(0f, 9.81f, 0f);

        public Vector3 AccelerationLocal
        {
            get => accelerationLocal;
            set
            {
                if (!IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(value));
                accelerationLocal = value;
            }
        }

        public Vector3 EffectiveGravity => -transform.TransformDirection(accelerationLocal);

        private static bool IsFinite(Vector3 value) =>
            !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
            !float.IsNaN(value.y) && !float.IsInfinity(value.y) &&
            !float.IsNaN(value.z) && !float.IsInfinity(value.z);

        private void OnValidate()
        {
            if (!IsFinite(accelerationLocal)) accelerationLocal = Vector3.zero;
        }
    }
}
