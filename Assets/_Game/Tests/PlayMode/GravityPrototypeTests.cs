using System.Collections;
using LongWayHome.Player;
using LongWayHome.ShipPhysics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace LongWayHome.Tests
{
    public sealed class GravityPrototypeTests : PrototypeTestContext
    {
        private static ShipGravityController Source => Object.FindFirstObjectByType<ShipGravityController>();
        private static FoundationPlayer Player => Object.FindFirstObjectByType<FoundationPlayer>();

        [UnityTest]
        public IEnumerator AccelerationUsesShipFrameAndIsIndependentOfMass()
        {
            Rigidbody light = CreateBody("Light test body", 1, new Vector3(100, 100, 100));
            Rigidbody heavy = CreateBody("Heavy test body", 80, new Vector3(110, 100, 100));
            Source.transform.rotation = Quaternion.Euler(0, 0, 90);
            Vector3[] values = { Vector3.zero, Vector3.up * 3, Vector3.up * 9.81f, Vector3.down * 9.81f, Vector3.right * 2 };
            foreach (Vector3 acceleration in values)
            {
                Source.AccelerationLocal = acceleration;
                light.linearVelocity = heavy.linearVelocity = Vector3.zero;
                yield return new WaitForFixedUpdate();
                Vector3 before = light.linearVelocity;
                for (int i = 0; i < 10; i++) yield return new WaitForFixedUpdate();
                Vector3 expected = -Source.transform.TransformDirection(acceleration) * (10 * Time.fixedDeltaTime);
                Assert.That(Vector3.Distance(light.linearVelocity - before, expected), Is.LessThan(0.005f));
                Assert.That(Vector3.Distance(light.linearVelocity, heavy.linearVelocity), Is.LessThan(0.005f));
                Assert.That(light.useGravity, Is.False);
            }
            Object.Destroy(light.gameObject);
            Object.Destroy(heavy.gameObject);
        }

        [UnityTest]
        public IEnumerator CoastingPreservesMomentumAndIgnoresFreeFlightInput()
        {
            Source.AccelerationLocal = Vector3.zero;
            Rigidbody body = Player.GetComponent<Rigidbody>();
            body.position = new Vector3(0, 2, 0);
            body.linearVelocity = new Vector3(0.2f, 0, 0.3f);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Vector3 before = body.linearVelocity;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W, Key.Space));
            yield return new WaitForSeconds(0.4f);
            Assert.That(Vector3.Distance(body.linearVelocity, before), Is.LessThan(0.01f), "W and Space must not add thrust without a rail or surface.");
        }

        [UnityTest]
        public IEnumerator GravityToggleAndRepeatedTransitionsRemainStable()
        {
            Rigidbody body = Player.GetComponent<Rigidbody>();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G));
            yield return null;
            yield return null;
            Assert.That(Source.AccelerationLocal, Is.EqualTo(Vector3.zero));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G));
            yield return null;
            yield return null;
            Assert.That(Source.AccelerationLocal.y, Is.EqualTo(9.81f).Within(0.01f));
            for (int i = 0; i < 10; i++)
            {
                Source.AccelerationLocal = i % 2 == 0 ? Vector3.zero : Vector3.up * 9.81f;
                yield return new WaitForFixedUpdate();
            }
            yield return new WaitForSeconds(0.4f);
            Assert.That(body.position.y, Is.InRange(0.9f, 1.1f));
            Assert.That(Player.GetComponent<PlayerMovement>().IsGrounded, Is.True);
        }

        [UnityTest]
        public IEnumerator BothHandsCanHoldSeparateObjectsAndReleaseWithMomentum()
        {
            Source.AccelerationLocal = Vector3.zero;
            var body = Player.GetComponent<Rigidbody>();
            body.position = new Vector3(0, 2, 0);
            body.linearVelocity = Vector3.zero;
            Transform view = Player.GetComponentInChildren<Camera>().transform;
            yield return new WaitForFixedUpdate();
            Rigidbody first = CreateBody("First tool", 1, view.position + view.forward * 1.1f);
            first.gameObject.AddComponent<Grabbable>();
            var hands = Player.GetComponentsInChildren<HandInteractor>();
            InputSystem.QueueStateEvent(mouse, new MouseState().WithButton(MouseButton.Left));
            yield return new WaitForSeconds(0.06f);
            Assert.That(hands[0].IsHolding, Is.True);
            Assert.That(first.GetComponent<Grabbable>().TryClaim(hands[1]), Is.False, "The same loose object cannot be claimed by both hands.");
            yield return new WaitForSeconds(0.25f);
            Rigidbody second = CreateBody("Second tool", 1, view.position + view.forward * 1.1f);
            second.gameObject.AddComponent<Grabbable>();
            InputSystem.QueueStateEvent(mouse, new MouseState().WithButton(MouseButton.Left).WithButton(MouseButton.Right));
            yield return new WaitForSeconds(0.06f);
            Assert.That(hands[1].IsHolding, Is.True);
            yield return new WaitForSeconds(0.25f);
            Assert.That(Vector3.Distance(first.position, body.position), Is.LessThan(2.5f));
            hands[0].Release();
            first.position = new Vector3(3, 2, 0);
            first.linearVelocity = new Vector3(0, 0, 0.6f);
            InputSystem.QueueStateEvent(mouse, new MouseState());
            yield return new WaitForSeconds(0.2f);
            Assert.That(hands[0].IsHolding || hands[1].IsHolding, Is.False);
            Assert.That(first.linearVelocity.z, Is.EqualTo(0.6f).Within(0.01f));
            Assert.That(first.GetComponent<ConfigurableJoint>(), Is.Null);
            Assert.That(UnityEngine.Physics.GetIgnoreCollision(first.GetComponent<Collider>(), body.GetComponent<Collider>()), Is.False);
            Object.Destroy(first.gameObject);
            Object.Destroy(second.gameObject);
        }

        [UnityTest]
        public IEnumerator RailAllowsPullAndPushOffReleasesBothHands()
        {
            Source.AccelerationLocal = Vector3.zero;
            var body = Player.GetComponent<Rigidbody>();
            body.position = new Vector3(0, 2, 0);
            body.linearVelocity = Vector3.zero;
            var view = Player.GetComponentInChildren<Camera>().transform;
            yield return new WaitForFixedUpdate();
            var rail = GameObject.Find("Handrail");
            rail.transform.position = view.position + view.forward * 1.2f;
            rail.transform.localScale = new Vector3(2, 0.12f, 0.12f);
            UnityEngine.Physics.SyncTransforms();
            InputSystem.QueueStateEvent(mouse, new MouseState().WithButton(MouseButton.Left).WithButton(MouseButton.Right));
            yield return new WaitForSeconds(0.06f);
            var hands = Player.GetComponentsInChildren<HandInteractor>();
            Assert.That(hands[0].IsAnchored && hands[1].IsAnchored, Is.True);
            float start = body.position.z;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W));
            yield return new WaitForSeconds(0.2f);
            Assert.That(body.position.z, Is.GreaterThan(start + 0.05f));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
            yield return new WaitForSeconds(0.1f);
            Assert.That(hands[0].IsHolding || hands[1].IsHolding, Is.False);
            Assert.That(body.linearVelocity.z, Is.LessThan(0), "Push away from the forward rail.");
            Assert.That(body.GetComponents<ConfigurableJoint>().Length, Is.Zero);
        }

        private static Rigidbody CreateBody(string name, float mass, Vector3 position)
        {
            var item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            item.SetActive(false);
            item.name = name;
            item.transform.position = position;
            item.transform.localScale = Vector3.one * 0.15f;
            var body = item.AddComponent<Rigidbody>();
            body.mass = mass;
            body.linearDamping = body.angularDamping = 0;
            item.AddComponent<ShipGravityBody>().Source = Source;
            item.SetActive(true);
            return body;
        }

        [UnityTest]
        public IEnumerator SleepingSurfaceContactStillAllowsPush()
        {
            var body = Player.GetComponent<Rigidbody>();
            Source.AccelerationLocal = Vector3.zero;
            yield return new WaitForSeconds(0.2f);
            body.Sleep();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
            yield return new WaitForSeconds(0.1f);
            Assert.That(body.linearVelocity.y, Is.GreaterThan(2), "Contact must remain usable after the astronaut rests against a surface.");
        }

        [UnityTest]
        public IEnumerator WallOccludesToolsAndDisablingHandCleansUpJoint()
        {
            Source.AccelerationLocal = Vector3.zero;
            var body = Player.GetComponent<Rigidbody>();
            body.position = new Vector3(0, 2, 0);
            body.linearVelocity = Vector3.zero;
            yield return new WaitForSeconds(0.1f);
            var view = Player.GetComponentInChildren<Camera>().transform;
            Rigidbody tool = CreateBody("Occluded tool", 1, view.position + view.forward * 1.2f);
            tool.gameObject.AddComponent<Grabbable>();
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = view.position + view.forward * 0.6f;
            wall.transform.localScale = new Vector3(0.8f, 0.8f, 0.1f);
            UnityEngine.Physics.SyncTransforms();
            var hand = Player.GetComponentsInChildren<HandInteractor>()[0];
            Assert.That(hand.TryGrab(), Is.False, "Grabs must not pass through walls.");
            Object.Destroy(wall);
            yield return null;
            Assert.That(hand.TryGrab(), Is.True);
            hand.enabled = false;
            yield return null;
            Assert.That(tool.GetComponent<ConfigurableJoint>(), Is.Null);
            Assert.That(UnityEngine.Physics.GetIgnoreCollision(tool.GetComponent<Collider>(), body.GetComponent<Collider>()), Is.False);
            Object.Destroy(tool.gameObject);
        }
    }
}
