using System.Collections;
using LongWayHome.Player;
using LongWayHome.ShipPhysics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace LongWayHome.Tests
{
    public sealed class BatteryDeliveryTests : PrototypeTestContext
    {
        protected override string StartupScene => "FiveDeckGameplay";
        private FoundationPlayer Player => Object.FindFirstObjectByType<FoundationPlayer>();
        private BatterySocket Socket => Object.FindFirstObjectByType<BatterySocket>();
        private static readonly Vector3 Shaft = new Vector3(.9f, 0, -.85f);

        private IEnumerator GrabAt(Vector3 position)
        {
            var battery = Socket.Battery;
            battery.Body.position = position;
            battery.Body.linearVelocity = Vector3.zero;
            UnityEngine.Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            Player.GetComponentInChildren<Camera>().transform.LookAt(battery.Body.position);
            Assert.That(Player.GetComponentsInChildren<HandInteractor>()[0].TryGrab(), Is.True, "Battery pickup must use the real hand ray/joint.");
        }

        [UnityTest]
        public IEnumerator BatteryStaysHeldDuringTwoOneHandedClimbs()
        {
            Player.enabled = false;
            var body = Player.GetComponent<Rigidbody>();
            var view = Player.GetComponentInChildren<Camera>().transform;
            var motor = Player.GetComponent<PlayerMovement>();
            var hands = Player.GetComponentsInChildren<HandInteractor>();
            // Place only the pickup fixture; ascent itself uses real jumps and grabs.
            yield return GrabAt(view.position + Vector3.forward * .65f);
            for (int deck = 1; deck <= 2; deck++)
            {
                motor.RequestPush();
                for (int step = 0; step < 30 && hands[1].HeldLedge == null; step++)
                {
                    view.LookAt(Shaft + new Vector3(0, deck * 3.2f - .09f, -1.05f));
                    hands[1].TryGrab();
                    yield return new WaitForFixedUpdate();
                }
                Assert.That(hands[1].HeldLedge, Is.Not.Null);
                motor.SetMovement(Vector2.up);
                Quaternion climbingView = view.rotation;
                for (int step = 0; step < 200; step++)
                {
                    view.rotation = climbingView * Quaternion.Euler(0, Mathf.Sin(step * .1f) * 30, 0);
                    yield return new WaitForFixedUpdate();
                    Assert.That(hands[0].HeldObject, Is.EqualTo(Socket.Battery), "Looking around mid-climb must not break the grip.");
                    if (step > 30)
                    {
                        var grip = Socket.Battery.GetComponent<ConfigurableJoint>();
                        Assert.That(Vector3.Distance(hands[0].GripPoint, grip.connectedBody.position), Is.LessThan(.15f), "The pack must remain at the physics hand through the rim.");
                    }
                }
                motor.SetMovement(Vector2.zero);
                Assert.That(body.position.y, Is.InRange(deck * 3.2f + .95f, deck * 3.2f + 1.2f));
                Assert.That(hands[0].HeldObject, Is.EqualTo(Socket.Battery), "The carrying hand must keep its battery through the hatch.");
                hands[1].Release();
                yield return new WaitForSeconds(.15f);
            }
        }

        [UnityTest]
        public IEnumerator CockpitCeilingContactAndReleaseKeepTheVisibleBoxOnThisDeck()
        {
            Player.enabled = false;
            var body = Player.GetComponent<Rigidbody>();
            var view = Player.GetComponentInChildren<Camera>().transform;
            var hand = Player.GetComponentsInChildren<HandInteractor>()[0];
            body.position = new Vector3(-1, 13.85f, -2.4f);
            yield return new WaitForFixedUpdate();
            yield return null;
            yield return GrabAt(view.position + Vector3.forward * .6f);
            yield return new WaitForSeconds(.7f);
            view.rotation = Quaternion.Euler(-80, 0, 0);
            Player.GetComponent<PlayerMovement>().RequestPush();
            for (int i = 0; i < 70; i++)
            {
                yield return new WaitForFixedUpdate();
                yield return null;
                var mesh = GameObject.Find("Workshop battery held presentation").transform;
                Assert.That(mesh.position.y, Is.LessThan(15.9f), "A ceiling contact must not lift the visible box through the roof.");
                Assert.That(Vector3.Distance(mesh.position, Socket.Battery.transform.position), Is.LessThan(.13f), "The visible box must remain near its physical body.");
            }
            view.rotation = Quaternion.identity;
            yield return new WaitForSeconds(.5f);
            Vector3 visible = GameObject.Find("Workshop battery held presentation").transform.position;
            hand.Release();
            Assert.That(Vector3.Distance(Socket.Battery.Body.position, visible), Is.LessThan(.15f), "Dropping must not reveal a box on a different deck.");
            yield return new WaitForSeconds(1.5f);
            Assert.That(Socket.Battery.Body.position.y, Is.GreaterThan(12.8f), "A drop over solid cockpit floor must stay on the cockpit deck.");
        }

        [UnityTest]
        public IEnumerator PickupReachesVisiblyAndWalkingKeepsGripStable()
        {
            Player.enabled = false;
            var view = Player.GetComponentInChildren<Camera>().transform;
            var hand = Player.GetComponentsInChildren<HandInteractor>()[0];
            yield return GrabAt(view.position + Vector3.forward * 1.2f);
            var visual = GameObject.Find(hand.name + " visible grip").transform;
            yield return new WaitForSeconds(.06f);
            Assert.That(Vector3.Distance(visual.position, hand.GripPoint), Is.GreaterThan(.15f), "The visible hand should still be reaching, not snapped onto the object.");
            yield return new WaitForSeconds(.6f);
            var motor = Player.GetComponent<PlayerMovement>();
            motor.SetMovement(Vector2.right);
            for (int i = 0; i < 25; i++)
            {
                yield return new WaitForFixedUpdate();
                var grip = Socket.Battery.GetComponent<ConfigurableJoint>();
                Assert.That(Vector3.Distance(hand.GripPoint, grip.connectedBody.position), Is.LessThan(.06f), "Walking must not make the object oscillate away from the hand.");
            }
            motor.SetMovement(Vector2.zero);
            Assert.That(Socket.Battery.Body.isKinematic, Is.False);
            Assert.That(Socket.Battery.GetComponent<Collider>().enabled, Is.True);
            hand.Release();
            yield return null;
            Assert.That(Socket.Battery.GetComponent<ConfigurableJoint>(), Is.Null);
        }

        [UnityTest]
        public IEnumerator RenderedGripStaysStillRelativeToCameraWhileWalking()
        {
            Player.enabled = false;
            var view = Player.GetComponentInChildren<Camera>().transform;
            var hand = Player.GetComponentsInChildren<HandInteractor>()[0];
            yield return GrabAt(view.position + Vector3.forward * 1.2f);
            yield return new WaitForSeconds(.7f);
            var probe = Player.gameObject.AddComponent<GripRenderProbe>();
            probe.View = view;
            probe.Hand = hand;
            Player.GetComponent<PlayerMovement>().SetMovement(Vector2.right);
            yield return new WaitForSeconds(.5f);
            Assert.That(probe.Samples, Is.GreaterThan(5));
            Assert.That(probe.MaxError, Is.LessThan(.002f), "The rendered grip must remain within 2 mm of the camera-relative hold pose, independent of physics/render timing.");
            Player.GetComponent<PlayerMovement>().SetMovement(Vector2.zero);
            hand.Release();
            yield return null;
            Assert.That(GameObject.Find("Workshop battery held presentation"), Is.Null);
            foreach (var renderer in Socket.Battery.GetComponentsInChildren<MeshRenderer>())
                Assert.That(renderer.enabled, Is.True, "Release restores the physical object's rendering.");
        }

        [UnityTest]
        public IEnumerator TableApproachKeepsPackForwardAndClearOfSurface()
        {
            Player.enabled = false;
            var body = Player.GetComponent<Rigidbody>();
            var view = Player.GetComponentInChildren<Camera>().transform;
            var hand = Player.GetComponentsInChildren<HandInteractor>()[0];
            var motor = Player.GetComponent<PlayerMovement>();
            body.position = new Vector3(0, 1.05f, 1.1f);
            yield return new WaitForFixedUpdate();
            yield return null;
            yield return GrabAt(view.position + Vector3.forward * .6f);
            yield return new WaitForSeconds(.7f);
            foreach (float pitch in new[] { 0f, 40f })
            {
                view.rotation = Quaternion.Euler(pitch, 0, 0);
                motor.SetMovement(Vector2.up);
                for (int step = 0; step < 60; step++)
                {
                    yield return new WaitForFixedUpdate();
                    yield return null;
                    Vector3 visible = view.InverseTransformPoint(hand.VisibleGripPoint);
                    Assert.That(visible.z, Is.GreaterThan(.4f), "The table must not push the pack into the player's body.");
                    Assert.That(visible.y, Is.GreaterThan(-.28f), "Avoidance must not lower the pack under the normal hold position.");
                    var presentation = GameObject.Find("Workshop battery held presentation").transform;
                    var shape = Socket.Battery.GetComponent<Collider>();
                    foreach (var obstacle in UnityEngine.Physics.OverlapSphere(presentation.position, .4f))
                    {
                        if (obstacle.attachedRigidbody == body || obstacle.attachedRigidbody == Socket.Battery.Body || obstacle.isTrigger) continue;
                        bool overlap = UnityEngine.Physics.ComputePenetration(shape, presentation.position, presentation.rotation,
                            obstacle, obstacle.transform.position, obstacle.transform.rotation, out _, out float depth);
                        Assert.That(overlap && depth > .002f, Is.False, "Visible battery must clear the actual table collider.");
                    }
                }
            }
            motor.SetMovement(Vector2.zero);
            Assert.That(hand.HeldObject, Is.EqualTo(Socket.Battery));
        }

        [UnityTest]
        public IEnumerator InputInstallsSecuresRemovesAndResetRestoresBattery()
        {
            Player.enabled = false;
            var source = Object.FindFirstObjectByType<ShipGravityController>();
            source.AccelerationLocal = Vector3.zero;
            Player.GetComponent<Rigidbody>().position = new Vector3(-.55f, 7.45f, 1.3f);
            yield return new WaitForFixedUpdate();
            yield return null;
            yield return GrabAt(Socket.Mount.position - Vector3.forward * .5f);
            var view = Player.GetComponentInChildren<Camera>().transform;
            view.LookAt(Socket.Mount.position);
            yield return new WaitForSeconds(.25f);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            yield return null;
            yield return null;
            Assert.That(Socket.IsPowered, Is.True);
            Assert.That(Socket.Battery.Body.isKinematic, Is.True);
            Assert.That(Socket.GetComponentInChildren<Light>().enabled, Is.True);
            var fan = Socket.transform.Find("Workshop fan rotor");
            Quaternion initialFan = fan.localRotation;
            Assert.That(Player.GetComponentsInChildren<HandInteractor>()[0].IsHolding, Is.False);
            Vector3 latched = Socket.Battery.Body.position;
            source.AccelerationLocal = Vector3.up * 9.81f;
            yield return new WaitForSeconds(.3f);
            Assert.That(Vector3.Distance(Socket.Battery.Body.position, latched), Is.LessThan(.001f));
            Assert.That(Quaternion.Angle(initialFan, fan.localRotation), Is.GreaterThan(20), "Installed battery powers the fan.");
            source.AccelerationLocal = Vector3.zero;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
            view.LookAt(Socket.Mount.position);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            yield return null;
            yield return null;
            Assert.That(Socket.IsPowered, Is.False);
            Assert.That(Socket.GetComponentInChildren<Light>().enabled, Is.False);
            Assert.That(Socket.Battery.Body.isKinematic, Is.False);
            Assert.That(Socket.Battery.GetComponent<ShipGravityBody>().enabled, Is.True);
            yield return new WaitForSeconds(.7f);
            Assert.That(Socket.Battery.Body.position.z, Is.LessThan(latched.z - .1f), "An unlatched battery must be free to drift.");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.R));
            yield return new WaitForSeconds(.7f);
            Assert.That(Socket.IsPowered, Is.False);
            Assert.That(Socket.Battery.Body.position.y, Is.LessThan(1.5f));
            Assert.That(Object.FindObjectsByType<BatterySocket>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator SocketRejectsDistantAndObstructedBattery()
        {
            Player.enabled = false;
            Object.FindFirstObjectByType<ShipGravityController>().AccelerationLocal = Vector3.zero;
            var view = Player.GetComponentInChildren<Camera>().transform;
            yield return GrabAt(view.position + Vector3.forward * .65f);
            var hand = Player.GetComponentsInChildren<HandInteractor>()[0];
            Assert.That(Socket.TryInstall(hand), Is.False, "No remote delivery across decks.");
            hand.Release();
            Player.GetComponent<Rigidbody>().position = new Vector3(-.55f, 7.45f, 1.3f);
            yield return new WaitForFixedUpdate();
            yield return null;
            yield return GrabAt(Socket.Mount.position - Vector3.forward * .5f);
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = Socket.Mount.position - Vector3.forward * .25f;
            wall.transform.localScale = new Vector3(.7f, .7f, .06f);
            UnityEngine.Physics.SyncTransforms();
            Assert.That(Socket.TryInstall(hand), Is.False, "The battery must not latch through an obstruction.");
            Assert.That(hand.HeldObject, Is.EqualTo(Socket.Battery));
            Object.Destroy(wall);
            hand.Release();
            Assert.That(Socket.TryInstall(hand), Is.False, "A free hand cannot install anything.");
        }
    }

    [DefaultExecutionOrder(10000)]
    public sealed class GripRenderProbe : MonoBehaviour
    {
        public Transform View;
        public HandInteractor Hand;
        public float MaxError;
        public int Samples;
        private void LateUpdate()
        {
            if (View == null || Hand == null || !Hand.IsHolding) return;
            MaxError = Mathf.Max(MaxError, Vector3.Distance(View.InverseTransformPoint(Hand.VisibleGripPoint), new Vector3(-.25f, -.25f, .65f)));
            Samples++;
        }
    }
}
