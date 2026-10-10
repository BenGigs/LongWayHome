using System.Collections;
using LongWayHome.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace LongWayHome.Tests
{
    public abstract class PrototypeTestContext
    {
        protected Keyboard keyboard;
        protected Mouse mouse;
        private InputSettings.BackgroundBehavior previousBackgroundBehavior;
#if UNITY_EDITOR
        private InputSettings.EditorInputBehaviorInPlayMode previousEditorBehavior;
#endif

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Batch mode has no focused Game view. Route synthetic device input explicitly.
            previousBackgroundBehavior = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            previousEditorBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.editorInputBehaviorInPlayMode =
                InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            keyboard = InputSystem.AddDevice<Keyboard>();
            mouse = InputSystem.AddDevice<Mouse>();
            yield return SceneManager.LoadSceneAsync("Bootstrap", LoadSceneMode.Single);
            float timeout = Time.realtimeSinceStartup + 20;
            while (!SceneManager.GetSceneByName("SpaceExterior").isLoaded && Time.realtimeSinceStartup < timeout)
                yield return null;
            Assert.That(SceneManager.GetSceneByName("SpaceExterior").isLoaded, Is.True);
            yield return new WaitForSecondsRealtime(0.3f);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            InputSystem.RemoveDevice(keyboard);
            InputSystem.RemoveDevice(mouse);
            InputSystem.settings.backgroundBehavior = previousBackgroundBehavior;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode = previousEditorBehavior;
#endif
            Time.timeScale = 1;
            yield return null;
        }
    }

    public sealed class FoundationTests : PrototypeTestContext
    {
        [UnityTest]
        public IEnumerator BootstrapComposesOnePlayerAndCamera()
        {
            Assert.That(SceneManager.GetSceneByName("Bootstrap").isLoaded, Is.True);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("ShipInterior"));
            Assert.That(Object.FindObjectsByType<FoundationPlayer>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            yield return null;
        }

        [UnityTest]
        public IEnumerator WalksAndStopsAtEnclosedWall()
        {
            var body = Object.FindFirstObjectByType<FoundationPlayer>().GetComponent<Rigidbody>();
            float start = body.position.z;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W));
            yield return new WaitForSeconds(0.5f);
            Assert.That(body.position.z, Is.GreaterThan(start + 0.5f), "W must drive the Rigidbody through the Input System.");
            yield return new WaitForSeconds(4);
            Assert.That(body.position.z, Is.InRange(5.8f, 6.5f), "The player must stop inside the forward wall.");
            Assert.That(body.position.y, Is.InRange(0.9f, 1.1f), "The floor must support the capsule.");
        }

        [UnityTest]
        public IEnumerator EscapePausesAndResumesPhysics()
        {
            var player = Object.FindFirstObjectByType<FoundationPlayer>();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Escape));
            yield return null;
            yield return null;
            Assert.That(player.IsPaused, Is.True);
            Assert.That(Time.timeScale, Is.Zero);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Escape));
            yield return null;
            yield return null;
            Assert.That(player.IsPaused, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1));
        }
    }
}
