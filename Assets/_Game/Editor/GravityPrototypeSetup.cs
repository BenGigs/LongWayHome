using System;
using System.IO;
using LongWayHome.Player;
using LongWayHome.ShipPhysics;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LongWayHome.Editor
{
    public static class GravityPrototypeSetup
    {
        private const string Root = "Assets/_Game/";

        [MenuItem("The Long Way Home/Upgrade to Gravity Prototype (once)")]
        public static void Upgrade()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(Root + "Scenes/ShipInterior.unity");
            if (UnityEngine.Object.FindFirstObjectByType<ShipGravityController>() != null)
                throw new InvalidOperationException("Gravity prototype already exists. This migration never resets your scene.");

            const string inputPath = Root + "Input/ShipControls.inputactions";
            var input = InputActionAsset.FromJson(File.ReadAllText(inputPath));
            var map = input.FindActionMap("Gameplay", true);
            if (map.FindAction("ToggleGravity") == null)
                map.AddAction("ToggleGravity", InputActionType.Button, "<Keyboard>/g");
            File.WriteAllText(inputPath, input.ToJson());
            UnityEngine.Object.DestroyImmediate(input);
            AssetDatabase.ImportAsset(inputPath, ImportAssetOptions.ForceSynchronousImport);
            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(inputPath);
            if (actions == null) throw new InvalidOperationException("Cannot bind input asset.");

            var source = new GameObject("Ship acceleration (edit vector in Inspector)").AddComponent<ShipGravityController>();
            var inputDriver = UnityEngine.Object.FindFirstObjectByType<FoundationPlayer>();
            GameObject player = inputDriver.gameObject;
            PrefabUtility.UnpackPrefabInstance(player, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            player.name = "Astronaut";
            var body = player.GetComponent<Rigidbody>();
            body.useGravity = false;
            body.linearDamping = 0;
            body.solverIterations = 12;
            body.solverVelocityIterations = 8;
            Transform view = player.GetComponentInChildren<Camera>().transform;
            var gravity = player.AddComponent<ShipGravityBody>();
            var motor = player.AddComponent<PlayerMovement>();
            var left = CreateHand(player, body, view, "Left", -0.25f, new Color(0.2f, 0.65f, 0.9f));
            var right = CreateHand(player, body, view, "Right", 0.25f, new Color(0.9f, 0.65f, 0.2f));
            SetReference(motor, "view", view);
            SetReference(motor, "leftHand", left);
            SetReference(motor, "rightHand", right);
            SetReference(inputDriver, "inputActions", actions);
            SetReference(inputDriver, "motor", motor);
            SetReference(inputDriver, "leftHand", left);
            SetReference(inputDriver, "rightHand", right);
            // Scene dependencies are assigned after saving the reusable prefab.
            PrefabUtility.SaveAsPrefabAssetAndConnect(player, Root + "Prefabs/Player/FoundationPlayer.prefab", InteractionMode.AutomatedAction);
            SetReference(gravity, "source", source);
            SetReference(inputDriver, "gravitySource", source);
            PrefabUtility.RecordPrefabInstancePropertyModifications(gravity);
            PrefabUtility.RecordPrefabInstancePropertyModifications(inputDriver);

            var rail = Cube("Handrail", new Vector3(-1.25f, 1.45f, -3.3f), new Vector3(0.12f, 0.12f, 2.1f));
            var railGrab = rail.AddComponent<Grabbable>();
            var railData = new SerializedObject(railGrab);
            railData.FindProperty("handrail").boolValue = true;
            railData.ApplyModifiedPropertiesWithoutUndo();
            Cube("Rail support A", new Vector3(-1.25f, 0.7f, -4.25f), new Vector3(0.12f, 1.4f, 0.12f));
            Cube("Rail support B", new Vector3(-1.25f, 0.7f, -2.35f), new Vector3(0.12f, 1.4f, 0.12f));
            Cube("Tool shelf", new Vector3(1.2f, 0.55f, -3f), new Vector3(1f, 1.1f, 1.2f));
            CreateWrench(source);
            var sample = Cube("Sample canister", new Vector3(1.45f, 1.3f, -2.8f), Vector3.one * 0.22f);
            ConfigureLooseBody(sample, source, 0.5f);

            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(Root + "Scenes/Bootstrap.unity");
            Debug.Log("GRAVITY_PROTOTYPE_SETUP_OK");
        }

        private static HandInteractor CreateHand(GameObject player, Rigidbody body, Transform view, string name, float x, Color color)
        {
            var logic = new GameObject(name + " hand interaction");
            logic.transform.SetParent(player.transform, false);
            var hand = logic.AddComponent<HandInteractor>();
            var target = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            target.name = name + " hand";
            UnityEngine.Object.DestroyImmediate(target.GetComponent<Collider>());
            target.transform.SetParent(player.transform, false);
            target.transform.position = view.TransformPoint(new Vector3(x, -0.25f, 0.65f));
            target.transform.localScale = Vector3.one * 0.12f;
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color };
            AssetDatabase.CreateAsset(material, Root + "Art/Materials/" + name + "Hand.mat");
            target.GetComponent<Renderer>().sharedMaterial = material;
            var targetBody = target.AddComponent<Rigidbody>();
            targetBody.isKinematic = true;
            targetBody.useGravity = false;
            targetBody.interpolation = RigidbodyInterpolation.Interpolate;
            SetReference(hand, "playerBody", body);
            SetReference(hand, "view", view);
            SetReference(hand, "handTarget", targetBody);
            var data = new SerializedObject(hand);
            data.FindProperty("restOffset").vector3Value = new Vector3(x, -0.25f, 0.65f);
            data.ApplyModifiedPropertiesWithoutUndo();
            return hand;
        }

        private static void CreateWrench(ShipGravityController source)
        {
            var root = new GameObject("Wrench");
            root.transform.position = new Vector3(1.05f, 1.2f, -3.15f);
            Part(root.transform, "Handle", Vector3.zero, new Vector3(0.08f, 0.06f, 0.35f));
            Part(root.transform, "Head bridge", new Vector3(0, 0, 0.18f), new Vector3(0.23f, 0.06f, 0.08f));
            Part(root.transform, "Jaw A", new Vector3(-0.085f, 0, 0.24f), new Vector3(0.06f, 0.06f, 0.14f));
            Part(root.transform, "Jaw B", new Vector3(0.085f, 0, 0.24f), new Vector3(0.06f, 0.06f, 0.14f));
            ConfigureLooseBody(root, source, 1f);
        }

        private static void ConfigureLooseBody(GameObject target, ShipGravityController source, float mass)
        {
            var body = target.AddComponent<Rigidbody>();
            body.mass = mass;
            body.useGravity = false;
            body.linearDamping = body.angularDamping = 0;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.solverIterations = 12;
            target.AddComponent<Grabbable>();
            SetReference(target.AddComponent<ShipGravityBody>(), "source", source);
        }

        private static void Part(Transform parent, string name, Vector3 local, Vector3 scale)
        {
            var part = Cube(name, Vector3.zero, scale);
            part.transform.SetParent(parent, false);
            part.transform.localPosition = local;
        }

        private static GameObject Cube(string name, Vector3 position, Vector3 scale)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.position = position;
            cube.transform.localScale = scale;
            cube.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Root + "Art/Materials/Marker.mat");
            return cube;
        }

        private static void SetReference(UnityEngine.Object target, string property, UnityEngine.Object value)
        {
            var data = new SerializedObject(target);
            data.FindProperty(property).objectReferenceValue = value;
            data.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
