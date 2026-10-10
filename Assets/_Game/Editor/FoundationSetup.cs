using System;
using System.IO;
using System.Linq;
using LongWayHome.Core;
using LongWayHome.Player;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace LongWayHome.Editor
{
    public static class FoundationSetup
    {
        private const string Root = "Assets/_Game/";
        private static readonly string[] Scenes = { "Bootstrap", "ShipInterior", "SpaceExterior" };

        [MenuItem("The Long Way Home/Create Foundation (once)")]
        public static void Create()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            if (Scenes.Any(name => File.Exists(Root + "Scenes/" + name + ".unity")))
                throw new InvalidOperationException("Foundation scenes already exist. Open Bootstrap; setup never overwrites scenes.");
            if (GraphicsSettings.defaultRenderPipeline == null)
                throw new InvalidOperationException("Create this project from Unity's Universal 3D template first.");

            string[] folders = {
                "Art/Materials", "Art/Models", "Art/Textures", "Art/VFX", "Audio",
                "Data/Planets", "Data/StarSystems", "Data/Plants", "Data/RobotModules",
                "Input", "Prefabs/Player", "Prefabs/Ship", "Prefabs/Robot", "Prefabs/Planets",
                "Scenes", "Scripts/Core", "Scripts/Player", "Scripts/Physics", "Scripts/Navigation",
                "Scripts/Exploration", "Scripts/Survival", "Scripts/UI", "Scripts/Save", "Tests"
            };
            foreach (string folder in folders) Directory.CreateDirectory(Root + folder);
            AssetDatabase.Refresh();

            EditorSettings.serializationMode = SerializationMode.ForceText;
            VersionControlSettings.mode = "Visible Meta Files";
            PlayerSettings.companyName = "LongWayHome";
            PlayerSettings.productName = "The Long Way Home";
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            settings.FindProperty("activeInputHandler").intValue = 1;
            settings.ApplyModifiedPropertiesWithoutUndo();

            InputActionAsset actions = CreateInput();
            Material floor = Material("Floor", new Color(0.14f, 0.19f, 0.22f));
            Material wall = Material("Wall", new Color(0.48f, 0.52f, 0.53f));
            Material marker = Material("Marker", new Color(0.85f, 0.44f, 0.15f));
            Scene interior = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Box("Floor", new Vector3(0, -0.25f, 0), new Vector3(10, 0.5f, 14), floor);
            Box("Ceiling", new Vector3(0, 4.25f, 0), new Vector3(10, 0.5f, 14), wall);
            Box("Port wall", new Vector3(-5, 2, 0), new Vector3(0.5f, 4, 14), wall);
            Box("Starboard wall", new Vector3(5, 2, 0), new Vector3(0.5f, 4, 14), wall);
            Box("Aft wall", new Vector3(0, 2, -7), new Vector3(10, 4, 0.5f), wall);
            Box("Forward wall", new Vector3(0, 2, 7), new Vector3(10, 4, 0.5f), wall);
            Box("Workshop placeholder", new Vector3(-3, 0.5f, 3), new Vector3(2, 1, 1), marker);
            Box("Greenhouse placeholder", new Vector3(3, 0.5f, 3), new Vector3(2, 1, 1), marker);
            for (int i = -4; i <= 4; i += 2)
                Box("Floor stripe " + i, new Vector3(0, 0.005f, i), new Vector3(0.12f, 0.01f, 1), marker);
            var light = new GameObject("Interior light").AddComponent<Light>();
            light.type = LightType.Point;
            light.transform.position = new Vector3(0, 3.4f, 0);
            light.range = 20;
            light.intensity = 5;
            light.color = new Color(1, 0.88f, 0.72f);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.25f, 0.28f, 0.32f);
            CreatePlayer(actions);
            EditorSceneManager.SaveScene(interior, Root + "Scenes/ShipInterior.unity");

            Scene exterior = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Space visuals reserved for Milestone 4");
            EditorSceneManager.SaveScene(exterior, Root + "Scenes/SpaceExterior.unity");
            Scene bootstrap = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Scene composition").AddComponent<SceneBootstrap>();
            EditorSceneManager.SaveScene(bootstrap, Root + "Scenes/Bootstrap.unity");
            EditorBuildSettings.scenes = Scenes.Select(name =>
                new EditorBuildSettingsScene(Root + "Scenes/" + name + ".unity", true)).ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("FOUNDATION_SETUP_OK");
            GravityPrototypeSetup.Upgrade();
        }

        private static InputActionAsset CreateInput()
        {
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = new InputActionMap("Gameplay");
            asset.AddActionMap(map);
            var move = map.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
            move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            map.AddAction("Look", InputActionType.Value, "<Mouse>/delta", expectedControlLayout: "Vector2");
            map.AddAction("LeftGrab", InputActionType.Button, "<Mouse>/leftButton");
            map.AddAction("RightGrab", InputActionType.Button, "<Mouse>/rightButton");
            map.AddAction("JumpPush", InputActionType.Button, "<Keyboard>/space");
            map.AddAction("Interact", InputActionType.Button, "<Keyboard>/e");
            map.AddAction("Pause", InputActionType.Button, "<Keyboard>/escape");
            asset.name = "ShipControls";
            const string path = Root + "Input/ShipControls.inputactions";
            File.WriteAllText(path, asset.ToJson());
            UnityEngine.Object.DestroyImmediate(asset);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var imported = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
            if (imported == null)
                throw new InvalidOperationException("ShipControls import failed; cannot create an unbound player.");
            return imported;
        }

        private static void CreatePlayer(InputActionAsset actions)
        {
            var player = new GameObject("Astronaut (foundation)");
            player.SetActive(false);
            player.layer = 2; // Ignore Raycast; excluded from the grounded probe.
            player.transform.position = new Vector3(0, 1.05f, -4);
            var capsule = player.AddComponent<CapsuleCollider>();
            capsule.height = 2;
            capsule.radius = 0.35f;
            var body = player.AddComponent<Rigidbody>();
            body.mass = 80;
            body.constraints = RigidbodyConstraints.FreezeRotation;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            var camera = new GameObject("First person view", typeof(Camera), typeof(AudioListener));
            camera.tag = "MainCamera";
            camera.transform.SetParent(player.transform, false);
            camera.transform.localPosition = new Vector3(0, 0.65f, 0);
            camera.GetComponent<Camera>().nearClipPlane = 0.05f;
            var controller = player.AddComponent<FoundationPlayer>();
            var data = new SerializedObject(controller);
            data.FindProperty("inputActions").objectReferenceValue = actions;
            data.FindProperty("view").objectReferenceValue = camera.transform;
            data.ApplyModifiedPropertiesWithoutUndo();
            player.SetActive(true);
            PrefabUtility.SaveAsPrefabAssetAndConnect(player, Root + "Prefabs/Player/FoundationPlayer.prefab", InteractionMode.AutomatedAction);
        }

        private static Material Material(string name, Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.name = name;
            material.color = color;
            AssetDatabase.CreateAsset(material, Root + "Art/Materials/" + name + ".mat");
            return material;
        }

        private static void Box(string name, Vector3 position, Vector3 scale, Material material)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.position = position;
            cube.transform.localScale = scale;
            cube.GetComponent<Renderer>().sharedMaterial = material;
        }

        public static void BuildWindows()
        {
            BuildReport report = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes,
                "Builds/Windows/TheLongWayHome.exe", BuildTarget.StandaloneWindows64, BuildOptions.Development);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Windows build failed: " + report.summary.result);
            Debug.Log("FOUNDATION_WINDOWS_BUILD_OK");
        }
    }
}
