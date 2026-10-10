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
        public IEnumerator RailAllowsPullAndPushWhileKeepingBothHands()
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
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return new WaitForSeconds(0.25f);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
            yield return new WaitForSeconds(0.1f);
            Assert.That(hands[0].IsAnchored && hands[1].IsAnchored, Is.True);
            Assert.That(body.linearVelocity.z, Is.LessThan(0), "Push away from the forward rail.");
            Assert.That(body.GetComponents<ConfigurableJoint>().Length, Is.EqualTo(2));
            InputSystem.QueueStateEvent(mouse, new MouseState());
            yield return new WaitForSeconds(0.1f);
            Assert.That(hands[0].IsHolding || hands[1].IsHolding, Is.False);
            Assert.That(body.GetComponents<ConfigurableJoint>().Length, Is.Zero);
        }

        [UnityTest]
        public IEnumerator PoweredJumpKeepsRailAndToolAndHasStrongerArc()
        {
            Player.enabled = false;
            var body = Player.GetComponent<Rigidbody>();
            var motor = Player.GetComponent<PlayerMovement>();
            var view = Player.GetComponentInChildren<Camera>().transform;
            var hands = Player.GetComponentsInChildren<HandInteractor>();
            var rail = GameObject.Find("Handrail").GetComponent<Collider>();
            view.LookAt(rail.ClosestPoint(view.position));
            Assert.That(hands[0].TryGrab(), Is.True);
            var tool = CreateBody("Jump test tool", 1, view.position + Vector3.forward * 0.8f);
            tool.gameObject.AddComponent<Grabbable>();
            UnityEngine.Physics.SyncTransforms();
            view.LookAt(tool.position);
            Assert.That(hands[1].TryGrab(), Is.True);
            yield return new WaitForSeconds(0.2f);
            Assert.That(motor.IsGrounded, Is.True);
            float start = body.position.y;
            float peak = start;
            motor.RequestPush();
            for (int step = 0; step < 45; step++)
            {
                yield return new WaitForFixedUpdate();
                peak = Mathf.Max(peak, body.position.y);
                Assert.That(hands[0].IsAnchored, Is.True, "Jump must keep the rail grip.");
                Assert.That(hands[1].IsHolding, Is.True, "Jump must keep the carried tool.");
            }
            Assert.That(peak - start, Is.InRange(0.65f, 0.95f), "An unobstructed 4 m/s jump should rise about 0.8 m at 1g.");
            Assert.That(body.position.y, Is.LessThan(start + 0.15f), "The player should return to the deck.");
            hands[0].Release();
            hands[1].Release();
            Object.Destroy(tool.gameObject);
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
        public IEnumerator LeftHandHoldsWrenchOrientationAndReleasesSpin() => CheckWrenchGrip(0);

        [UnityTest]
        public IEnumerator RightHandHoldsWrenchOrientationAndReleasesSpin() => CheckWrenchGrip(1);

        private IEnumerator CheckWrenchGrip(int handIndex)
        {
            Source.AccelerationLocal = Vector3.zero;
            var player = Player;
            player.enabled = false; // Drive the view directly; isolate the grip from locomotion/input.
            player.GetComponent<PlayerMovement>().enabled = false;
            var astronaut = player.GetComponent<Rigidbody>();
            astronaut.isKinematic = true;
            astronaut.position = new Vector3(0, 2, 0);
            var view = player.GetComponentInChildren<Camera>().transform;
            yield return new WaitForSeconds(0.1f);
            var wrench = GameObject.Find("Wrench").GetComponent<Rigidbody>();
            wrench.position = view.position + view.forward * 1.1f;
            wrench.rotation = Quaternion.Euler(20, 25, 10);
            wrench.linearVelocity = Vector3.zero;
            wrench.angularVelocity = new Vector3(2, 3, 1);
            UnityEngine.Physics.SyncTransforms();
            Quaternion pickupRotation = Quaternion.Inverse(view.rotation) * wrench.rotation;
            var hand = player.GetComponentsInChildren<HandInteractor>()[handIndex];
            Assert.That(hand.TryGrab(), Is.True, "Aim at the real compound wrench collider.");

            foreach (float acceleration in new[] { 9.81f, 0f })
            {
                Source.AccelerationLocal = Vector3.up * acceleration;
                yield return new WaitForSeconds(0.6f);
                Assert.That(Quaternion.Angle(view.rotation * pickupRotation, wrench.rotation), Is.LessThan(4f), $"A stationary grip must resist spinning under gravity or coasting. Held={hand.IsHolding}, spin={wrench.angularVelocity}, pose={wrench.rotation.eulerAngles}, expected={(view.rotation * pickupRotation).eulerAngles}, position={wrench.position}");
                Assert.That(wrench.angularVelocity.magnitude, Is.LessThan(0.2f));
                for (int i = 0; i < 30; i++)
                {
                    view.rotation = Quaternion.Euler(0.4f, 1.5f, 0) * view.rotation;
                    yield return new WaitForFixedUpdate();
                }
                yield return new WaitForSeconds(0.3f);
                Assert.That(hand.IsHolding, Is.True);
                Assert.That(Quaternion.Angle(view.rotation * pickupRotation, wrench.rotation), Is.LessThan(4f), "The held pose must follow hand rotation, not freeze in world space.");
                var grip = wrench.GetComponent<ConfigurableJoint>();
                Assert.That(Vector3.Distance(wrench.transform.TransformPoint(grip.anchor), grip.connectedBody.position), Is.LessThan(0.12f));
            }

            Vector3 releaseSpin = new Vector3(0.1f, 0.7f, -0.1f);
            Vector3 releaseVelocity = new Vector3(0.2f, 0, 0.3f);
            wrench.angularVelocity = releaseSpin;
            wrench.linearVelocity = releaseVelocity;
            hand.Release();
            Assert.That(wrench.angularVelocity, Is.EqualTo(releaseSpin));
            Assert.That(wrench.linearVelocity, Is.EqualTo(releaseVelocity));
            wrench.position = new Vector3(3, 2, 1); // Clear the astronaut for an isolated inertia check.
            Quaternion releasedRotation = wrench.rotation;
            yield return new WaitForSeconds(0.2f);
            Assert.That(wrench.GetComponent<ConfigurableJoint>(), Is.Null);
            Assert.That(Quaternion.Angle(releasedRotation, wrench.rotation), Is.GreaterThan(3f));
            Assert.That(Vector3.Distance(wrench.angularVelocity, releaseSpin), Is.LessThan(0.02f));
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
