using UnityEngine;

namespace LongWayHome.Player
{
    public sealed class Grabbable : MonoBehaviour
    {
        [SerializeField] private bool handrail;
        private HandInteractor owner;
        public bool IsHandrail => handrail;
        public Rigidbody Body => GetComponent<Rigidbody>();

        public bool TryClaim(HandInteractor hand)
        {
            if (!isActiveAndEnabled) return false;
            // Rails admit both hands. A loose object can only belong to one hand.
            if (handrail) return true;
            if (owner != null || Body == null || Body.isKinematic) return false;
            owner = hand;
            return true;
        }

        public void Release(HandInteractor hand)
        {
            if (owner == hand) owner = null;
        }
    }
}
