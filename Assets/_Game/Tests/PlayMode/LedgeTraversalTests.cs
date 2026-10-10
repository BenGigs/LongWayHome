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
    public sealed class LedgeTraversalTests : PrototypeTestContext
    {
        protected override string StartupScene => "FiveDeckGameplay";
        private FoundationPlayer Player => Object.FindFirstObjectByType<FoundationPlayer>();
        private static readonly Vector3 Shaft = new Vector3(.9f, 0, -.85f);

        private IEnumerator Catch(int handIndex, int nextDeck)
        {
            var body = Player.GetComponent<Rigidbody>();
            var motor = Player.GetComponent<PlayerMovement>();
            var view = Player.GetComponentInChildren<Camera>().transform;
            var hand = Player.GetComponentsInChildren<HandInteractor>()[handIndex];
            Vector3 grip = Shaft + new Vector3(0, nextDeck * 3.2f - .18f, -1.03f);
            motor.RequestPush();
            bool caught = false;
            string blocker = "none";
            for (int step = 0; step < 28; step++)
            {
                view.LookAt(grip);
                if (UnityEngine.Physics.Raycast(view.position, view.forward, out var hit, 1.8f, 1)) blocker = hit.collider.name;
                if (!hand.IsHolding) caught |= hand.TryGrab();
                yield return new WaitForFixedUpdate();
                if (hand.HeldLedge != null) break;
            }
            Assert.That(caught && hand.HeldLedge != null, Is.True, $"Catch deck {nextDeck}; player={body.position}; grip={grip}; ray hit={blocker}");
        }

        [UnityTest]
        public IEnumerator JumpCatchHangPullUpAllFourDecksAndDropToBottom()
        {
            Player.enabled = false;
            var body = Player.GetComponent<Rigidbody>();
            var motor = Player.GetComponent<PlayerMovement>();
            var hands = Player.GetComponentsInChildren<HandInteractor>();
            for (int deck = 1; deck <= 4; deck++)
            {
                yield return Catch(deck % 2, deck);
                yield return new WaitForSeconds(.8f);
                Assert.That(Player.GetComponent<LedgeClimber>().IsActive, Is.True);
                Assert.That(body.position.y, Is.InRange(deck * 3.2f - 1.2f, deck * 3.2f - .7f), "Hang below the caught floor.");
                motor.SetMovement(Vector2.up);
                float end = Time.time + 4;
                while (Time.time < end && (Player.GetComponent<LedgeClimber>().IsActive || !motor.IsGrounded))
                    yield return new WaitForFixedUpdate();
                motor.SetMovement(Vector2.zero);
                Assert.That(body.position.y, Is.InRange(deck * 3.2f + .95f, deck * 3.2f + 1.2f), "Stand on the next deck.");
                Assert.That(hands[deck % 2].IsHolding, Is.True, "Pull-up must not forcibly release the grip.");
                hands[deck % 2].Release();
                yield return new WaitForSeconds(.15f);
            }
            // Independently verify the entire fall path with no lateral momentum.
            // The ascent above uses real jumps/grabs and never teleports the player.
            body.position = Shaft + Vector3.up * (12.8f + 1.1f);
            body.linearVelocity = Vector3.zero;
            yield return new WaitForSeconds(2.4f);
            Assert.That(body.position.y, Is.InRange(.95f, 1.15f), "No invisible floors or barriers in the shaft.");
        }

        [UnityTest]
        public IEnumerator ReleasingGripDropsPlayerAndGravityToggleCancelsClimb()
        {
            Player.enabled = false;
            yield return Catch(0, 1);
            yield return new WaitForSeconds(.8f);
            var body = Player.GetComponent<Rigidbody>();
            float before = body.position.y;
            Player.GetComponentsInChildren<HandInteractor>()[0].Release();
            yield return new WaitForSeconds(.25f);
            Assert.That(body.position.y, Is.LessThan(before - .15f));
            Assert.That(Player.GetComponent<LedgeClimber>().IsActive, Is.False);
            yield return new WaitForSeconds(.6f);
            // Return using a real jump from below the same opening.
            yield return Catch(1, 1);
            Object.FindFirstObjectByType<ShipGravityController>().AccelerationLocal = Vector3.zero;
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.That(Player.GetComponent<LedgeClimber>().IsActive, Is.False);
            Assert.That(Player.GetComponentsInChildren<HandInteractor>()[1].IsHolding, Is.True);
        }

        [UnityTest]
        public IEnumerator HeldButtonCatchesWhenRingEntersReachAndResetReloads()
        {
            // Input System path: hold grab in advance, jump, and wait for the catch.
            var view = Player.GetComponentInChildren<Camera>().transform;
            Vector3 target = Shaft + new Vector3(0, 3.02f, -1.03f);
            // FoundationPlayer owns pitch; feed mouse delta to look approximately 72 degrees up.
            InputSystem.QueueStateEvent(mouse, new MouseState { delta = new Vector2(0, 575) }.WithButton(MouseButton.Left));
            yield return null;
            InputSystem.QueueStateEvent(mouse, new MouseState().WithButton(MouseButton.Left));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
            yield return new WaitForSeconds(.4f);
            Assert.That(Player.GetComponentsInChildren<HandInteractor>()[0].HeldLedge, Is.Not.Null, $"view={view.forward} target={target}");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.R));
            yield return new WaitForSeconds(.6f);
            Assert.That(Object.FindObjectsByType<FoundationPlayer>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            Assert.That(Player.GetComponent<Rigidbody>().position.y, Is.InRange(.95f, 1.15f));
        }

        [UnityTest]
        public IEnumerator TwoHandCatchCanPausePullAndContinueWithOneHand()
        {
            Player.enabled = false;
            yield return Catch(0, 1);
            yield return new WaitForSeconds(.8f);
            var hands = Player.GetComponentsInChildren<HandInteractor>();
            Player.GetComponentInChildren<Camera>().transform.LookAt(hands[0].GripPoint);
            Assert.That(hands[1].TryGrab(), Is.True);
            var motor = Player.GetComponent<PlayerMovement>();
            motor.SetMovement(Vector2.up);
            yield return new WaitForSeconds(.5f);
            motor.SetMovement(Vector2.zero);
            yield return new WaitForSeconds(.2f);
            var body = Player.GetComponent<Rigidbody>();
            Vector3 stopped = body.position;
            yield return new WaitForSeconds(.3f);
            Assert.That(Vector3.Distance(stopped, body.position), Is.LessThan(.06f), "Releasing W pauses a partial pull-up.");
            hands[0].Release();
            yield return new WaitForFixedUpdate();
            Assert.That(Player.GetComponent<LedgeClimber>().IsActive, Is.True, "The second grip still supports the body.");
            motor.SetMovement(Vector2.up);
            yield return new WaitForSeconds(3);
            Assert.That(body.position.y, Is.InRange(4.15f, 4.4f));
            Assert.That(hands[1].IsHolding, Is.True);
        }

        [UnityTest]
        public IEnumerator ZeroGravityRingPullAndReleasePreserveCoasting()
        {
            Player.enabled = false;
            Object.FindFirstObjectByType<ShipGravityController>().AccelerationLocal = Vector3.zero;
            var body = Player.GetComponent<Rigidbody>();
            body.position = Shaft + Vector3.up * 1.8f;
            body.linearVelocity = Vector3.zero;
            yield return new WaitForFixedUpdate();
            var view = Player.GetComponentInChildren<Camera>().transform;
            view.LookAt(Shaft + new Vector3(0, 3.02f, -1.03f));
            var hand = Player.GetComponentsInChildren<HandInteractor>()[0];
            Assert.That(hand.TryGrab(), Is.True);
            view.rotation = Quaternion.LookRotation(Vector3.up, Vector3.forward);
            Player.GetComponent<PlayerMovement>().SetMovement(Vector2.up);
            yield return new WaitForSeconds(.5f);
            Assert.That(body.position.y, Is.GreaterThan(2.3f));
            Assert.That(Player.GetComponent<LedgeClimber>().IsActive, Is.False);
            hand.Release();
            Vector3 speed = body.linearVelocity;
            yield return new WaitForSeconds(.2f);
            Assert.That(Vector3.Distance(body.linearVelocity, speed), Is.LessThan(.03f), "W must not add thrust after release.");
        }

        [UnityTest]
        public IEnumerator NearMissDescentCatchRequiresGrabAndClearReach()
        {
            Player.enabled = false;
            var body = Player.GetComponent<Rigidbody>();
            body.position = Shaft + Vector3.up * 3.65f;
            body.linearVelocity = Vector3.down * 2;
            yield return new WaitForFixedUpdate();
            yield return null;
            var view = Player.GetComponentInChildren<Camera>().transform;
            var hand = Player.GetComponentsInChildren<HandInteractor>()[0];
            Vector3 nearRing = Shaft + new Vector3(0, 3.02f, -.80f);
            view.LookAt(nearRing);
            Assert.That(hand.IsHolding, Is.False, "Passing a ring alone must not auto-catch.");
            bool exact = UnityEngine.Physics.Raycast(view.position, view.forward, out var direct, 1.8f, 1) &&
                direct.collider.GetComponentInParent<LedgeRing>() != null;
            Assert.That(exact, Is.False, "Exercise a genuine aiming miss.");
            Assert.That(hand.TryGrab(), Is.True, "A nearby visible ring should tolerate this aiming miss.");
            Assert.That(hand.HeldLedge.transform.position.y, Is.EqualTo(3.2f).Within(.01f));
            hand.Release();

            var blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.transform.position = view.position + view.forward * .45f;
            blocker.transform.localScale = Vector3.one * .65f;
            UnityEngine.Physics.SyncTransforms();
            Assert.That(hand.TryGrab(), Is.False, "Aim assistance cannot see through a blocker.");
            Object.Destroy(blocker);
            yield return null;
            view.rotation = Quaternion.LookRotation(Vector3.up);
            Assert.That(hand.TryGrab(), Is.False, "The next deck above is beyond reach.");
        }

        [UnityTest]
        public IEnumerator PullUpBuildsSpeedGradually()
        {
            Player.enabled = false;
            yield return Catch(0, 1);
            yield return new WaitForSeconds(.8f);
            var body = Player.GetComponent<Rigidbody>();
            Player.GetComponent<PlayerMovement>().SetMovement(Vector2.up);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocity.y, Is.InRange(.05f, .85f), "Start the pull with an eased acceleration (including test coroutine scheduling).");
            yield return new WaitForSeconds(.3f);
            Assert.That(body.linearVelocity.y, Is.GreaterThan(1.3f), "Ease-in must still reach a useful climbing speed.");
        }

        [UnityTest]
        public IEnumerator PullUpCannotPassThroughAnObstruction()
        {
            Player.enabled = false;
            yield return Catch(0, 1);
            yield return new WaitForSeconds(.8f);
            var blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.name = "Test hatch obstruction";
            blocker.transform.position = Shaft + Vector3.up * 3.8f;
            blocker.transform.localScale = new Vector3(2.5f, .2f, 2.5f);
            Player.GetComponent<PlayerMovement>().SetMovement(Vector2.up);
            yield return new WaitForSeconds(2);
            var body = Player.GetComponent<Rigidbody>();
            Assert.That(body.position.y, Is.LessThan(2.8f), "The dynamic capsule must stay below the blocking slab.");
            Assert.That(body.isKinematic, Is.False);
            Assert.That(body.GetComponent<Collider>().enabled, Is.True);
            Object.Destroy(blocker);
        }
    }
}
