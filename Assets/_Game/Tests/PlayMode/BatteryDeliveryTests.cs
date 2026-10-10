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
                yield return new WaitForSeconds(4);
                motor.SetMovement(Vector2.zero);
                Assert.That(body.position.y, Is.InRange(deck * 3.2f + .95f, deck * 3.2f + 1.2f));
                Assert.That(hands[0].HeldObject, Is.EqualTo(Socket.Battery), "The carrying hand must keep its battery through the hatch.");
                hands[1].Release();
                yield return new WaitForSeconds(.15f);
            }
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
}
