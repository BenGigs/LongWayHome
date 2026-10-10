using UnityEngine;

namespace LongWayHome.Player
{
    // One component per ring. Segments share its centre and deck plane.
    public sealed class LedgeRing : MonoBehaviour
    {
        [SerializeField] private float openingRadius = 1.05f;
        public Vector3 Up => transform.up;

        public void GetTargets(Vector3 grip, out Vector3 hang, out Vector3 above, out Vector3 landing)
        {
            Vector3 radial = Vector3.ProjectOnPlane(grip - transform.position, Up).normalized;
            if (radial.sqrMagnitude < 0.5f) radial = transform.forward;
            // Ring origin is the deck's walking plane, not the raised grip mesh.
            Vector3 shaft = transform.position + radial * (openingRadius - 0.55f);
            hang = shaft - Up * 0.95f;
            above = shaft + Up * 1.08f;
            landing = transform.position + radial * (openingRadius + 0.60f) + Up * 1.08f;
        }
    }
}
