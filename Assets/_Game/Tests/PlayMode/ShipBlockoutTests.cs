using System.Collections;
using System.Linq;
using LongWayHome.Player;
using LongWayHome.ShipPhysics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LongWayHome.Tests
{
    public sealed class ShipBlockoutTests : PrototypeTestContext
    {
        [UnityTest]
        public IEnumerator PassageFitsWalkingAndFloatingCapsuleAndWindowRemainsSolid()
        {
            foreach (float height in new[] { 1.05f, 2.25f })
            {
                Vector3 center = new Vector3(0, height, -6);
                bool blocked = UnityEngine.Physics.CapsuleCast(center - Vector3.up * 0.65f,
                    center + Vector3.up * 0.65f, 0.35f, Vector3.forward, 11.5f, 1, QueryTriggerInteraction.Ignore);
                Assert.That(blocked, Is.False, "Both hatch openings must admit the astronaut along the full central passage.");
            }
            Assert.That(UnityEngine.Physics.Raycast(new Vector3(0, 2, 6), Vector3.forward, out var hit, 2, 1), Is.True);
            Assert.That(hit.collider.name, Is.EqualTo("Observation window collision pane"), "Clear windows must not let the astronaut leave the ship.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator RailHandoffsTraverseEveryAreaInBothDirectionsWithoutThrust()
        {
            var player = Object.FindFirstObjectByType<FoundationPlayer>();
            player.enabled = false; // Use actual grabs and motor input, without cursor/keyboard timing.
            var source = Object.FindFirstObjectByType<ShipGravityController>();
            source.AccelerationLocal = Vector3.zero;
            var body = player.GetComponent<Rigidbody>();
            var motor = player.GetComponent<PlayerMovement>();
            var view = player.GetComponentInChildren<Camera>().transform;
            var hand = player.GetComponentsInChildren<HandInteractor>()[0];
            var rails = Object.FindObjectsByType<Grabbable>(FindObjectsSortMode.None)
                .Where(item => item.name == "Handrail" || item.name.StartsWith("Transit rail -1:"))
                .Select(item => item.GetComponent<Collider>()).ToArray();
            body.linearVelocity = Vector3.zero;

            foreach (int direction in new[] { 1, -1 })
            {
                float destination = direction == 1 ? 5f : -4f;
                int steps = 0;
                while (direction * (destination - body.position.z) > 0 && steps++ < 30)
                {
                    Vector3 desired = new Vector3(-1.25f, 1.45f, body.position.z + direction * 0.65f);
                    Collider rail = rails.OrderBy(item => (item.ClosestPoint(desired) - desired).sqrMagnitude).First();
                    view.LookAt(rail.ClosestPoint(desired));
                    Assert.That(hand.TryGrab(), Is.True, $"Rail coverage gap near z={body.position.z:F2}, direction={direction}");
                    view.rotation = Quaternion.LookRotation(Vector3.forward * direction);
                    motor.SetMovement(Vector2.up);
                    yield return new WaitForSeconds(0.35f);
                    Assert.That(hand.IsAnchored, Is.True, "A short pull must stay within rail reach.");
                    hand.Release();
                    motor.SetMovement(Vector2.zero);
                    yield return new WaitForFixedUpdate();
                }
                Assert.That(direction * (body.position.z - destination), Is.GreaterThanOrEqualTo(0), "Reach the far room using contact forces only.");
            }
            Assert.That(body.position.y, Is.InRange(0.9f, 1.2f));
        }
    }
}
