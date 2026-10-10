using System;
using System.IO;
using LongWayHome.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LongWayHome.Editor
{
    /// <summary>One-time, Editor-generated M3 layout. Does not regenerate the player or physics.</summary>
    public static class ShipBlockoutSetup
    {
        private const string Root = "Assets/_Game/";
        private static Transform layout;
        private static Material shell;
        private static Material trim;
        private static Material green;
        private static Material blue;
        private static Material amber;
        private static Material dark;

        [MenuItem("The Long Way Home/Build Ship Blockout (once)")]
        public static void Create()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(Root + "Scenes/ShipInterior.unity");
            if (GameObject.Find("Ship blockout") != null)
                throw new InvalidOperationException("The blockout already exists. Edit the scene; do not regenerate it.");
            string backup = Root + "Scenes/GravityPrototype.unity";
            if (File.Exists(backup)) throw new InvalidOperationException("The preserved prototype scene already exists.");
            if (!AssetDatabase.CopyAsset(Root + "Scenes/ShipInterior.unity", backup))
                throw new InvalidOperationException("Could not preserve the movement prototype scene.");

            layout = new GameObject("Ship blockout").transform;
            shell = MakeMaterial("ShipShell", new Color(0.38f, 0.42f, 0.44f));
            trim = MakeMaterial("ShipTrim", new Color(0.11f, 0.15f, 0.19f));
            green = MakeMaterial("HabitatGreen", new Color(0.2f, 0.42f, 0.3f));
            blue = MakeMaterial("CockpitBlue", new Color(0.2f, 0.4f, 0.55f));
            amber = MakeMaterial("WorkshopAmber", new Color(0.75f, 0.46f, 0.18f));
            dark = MakeMaterial("ConsoleScreen", new Color(0.015f, 0.06f, 0.08f));

            // Only replace known M1/M2 graybox fixtures; all player/physics references stay intact.
            Remove("Workshop placeholder");
            Remove("Greenhouse placeholder");
            Remove("Forward wall");
            Remove("Rail support A");
            Remove("Rail support B");
            var originalRail = GameObject.Find("Handrail");
            originalRail.transform.position = new Vector3(-1.25f, 1.45f, -4.5f);
            originalRail.transform.localScale = new Vector3(0.1f, 0.1f, 1.2f);
            originalRail.transform.SetParent(layout, true);
            originalRail.GetComponent<Renderer>().sharedMaterial = amber;
            var shelf = GameObject.Find("Tool shelf");
            Vector3 moveTools = new Vector3(1.4f, 0, 4f);
            shelf.transform.position += moveTools;
            shelf.transform.SetParent(layout, true);
            shelf.GetComponent<Renderer>().sharedMaterial = amber;
            GameObject.Find("Wrench").transform.position += moveTools;
            GameObject.Find("Sample canister").transform.position += moveTools;

            Hatch("Habitat / Workshop hatch", -1f, "WORKSHOP", "HABITAT", amber, green);
            Hatch("Workshop / Cockpit hatch", 3f, "COCKPIT", "WORKSHOP", blue, amber);
            Habitat();
            Workshop();
            Cockpit();
            Rails();
            Lighting();

            var camera = UnityEngine.Object.FindFirstObjectByType<FoundationPlayer>().GetComponentInChildren<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.004f, 0.008f, 0.02f);
            PrefabUtility.RecordPrefabInstancePropertyModifications(camera);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(Root + "Scenes/Bootstrap.unity");
            Debug.Log("SHIP_BLOCKOUT_SETUP_OK");
        }

        private static void Hatch(string name, float z, string forward, string backward, Material frontColor, Material backColor)
        {
            var root = new GameObject(name).transform;
            root.SetParent(layout, false);
            Box("Port bulkhead", new Vector3(-3.3f, 2, z), new Vector3(3.4f, 4, 0.22f), shell, root);
            Box("Starboard bulkhead", new Vector3(3.3f, 2, z), new Vector3(3.4f, 4, 0.22f), shell, root);
            Box("Hatch header", new Vector3(0, 3.7f, z), new Vector3(3.2f, 0.6f, 0.3f), trim, root);
            Box("Retracted door port", new Vector3(-1.8f, 1.7f, z - 0.15f), new Vector3(0.28f, 3.4f, 0.08f), frontColor, root);
            Box("Retracted door starboard", new Vector3(1.8f, 1.7f, z - 0.15f), new Vector3(0.28f, 3.4f, 0.08f), frontColor, root);
            Label(forward, new Vector3(0, 3.65f, z - 0.17f), Quaternion.identity, 0.06f, root);
            Label(backward, new Vector3(0, 3.65f, z + 0.17f), Quaternion.Euler(0, 180, 0), 0.06f, root);
            Box("Deck threshold", new Vector3(0, 0.008f, z), new Vector3(3.1f, 0.015f, 0.3f), backColor, root);
        }

        private static void Habitat()
        {
            var root = Area("Habitat — greenhouse and living");
            for (int i = 0; i < 3; i++)
            {
                float z = -5.8f + i * 1.35f;
                Box("Hydroponic tray", new Vector3(-3.6f, 0.65f, z), new Vector3(1.6f, 1.3f, 1.05f), green, root);
                Box("Enclosed irrigation lid", new Vector3(-3.6f, 1.32f, z), new Vector3(1.5f, 0.08f, 0.95f), trim, root);
                for (int plant = 0; plant < 3; plant++)
                    Box("Crop placeholder", new Vector3(-4.1f + plant * 0.5f, 1.48f, z), new Vector3(0.18f, 0.26f, 0.25f), green, root);
                Box("Grow light housing", new Vector3(-3.6f, 2.55f, z), new Vector3(1.65f, 0.1f, 0.9f), shell, root);
            }
            Box("Water reservoir", new Vector3(-3.6f, 0.9f, -1.7f), new Vector3(1.4f, 1.8f, 0.6f), blue, root);
            Box("Sleeping berth", new Vector3(3.65f, 0.5f, -5.3f), new Vector3(1.5f, 1, 2.4f), trim, root);
            Box("Berth cushion", new Vector3(3.65f, 1.04f, -5.3f), new Vector3(1.35f, 0.12f, 2.25f), green, root);
            Box("Food preparation counter", new Vector3(3.6f, 0.6f, -2.8f), new Vector3(1.5f, 1.2f, 1.2f), shell, root);
            Box("Food storage", new Vector3(4.2f, 2.05f, -2.8f), new Vector3(0.4f, 1.2f, 1.2f), green, root);
            Label("HABITAT / GREENHOUSE", new Vector3(0, 2.65f, -6.7f), Quaternion.Euler(0, 180, 0), 0.055f, root);
            Label("WATER", new Vector3(-3.6f, 1.8f, -2.02f), Quaternion.identity, 0.04f, root);
            AccessRail("Grow trays rail", new Vector3(-2.6f, 1.45f, -4.5f), new Vector3(0.1f, 0.1f, 3.8f), root);
            AccessRail("Berth rail", new Vector3(2.65f, 1.45f, -5.3f), new Vector3(0.1f, 0.1f, 2.2f), root);
        }

        private static void Workshop()
        {
            var root = Area("Workshop — tools and robot bay");
            Box("Robot docking plinth", new Vector3(-3.25f, 0.2f, 0.6f), new Vector3(2.1f, 0.4f, 2.3f), trim, root);
            Box("Robot bay placeholder", new Vector3(-3.25f, 0.8f, 0.6f), new Vector3(1.2f, 0.8f, 1.4f), shell, root);
            Box("Robot bay backboard", new Vector3(-4.5f, 1.5f, 0.6f), new Vector3(0.2f, 3f, 2.5f), amber, root);
            Label("ROBOT BAY", new Vector3(-3.25f, 2.3f, -0.5f), Quaternion.identity, 0.045f, root);
            Box("Workbench", new Vector3(3.6f, 0.55f, 1.3f), new Vector3(1.6f, 1.1f, 2.6f), amber, root);
            Box("Tool cabinet", new Vector3(4.4f, 2.15f, 1.3f), new Vector3(0.4f, 1.3f, 2.5f), trim, root);
            Label("TOOLS / MAINTENANCE", new Vector3(3.4f, 2.2f, -0.3f), Quaternion.identity, 0.035f, root);
            AccessRail("Workshop access rail", new Vector3(-2f, 1.45f, 1f), new Vector3(0.1f, 0.1f, 2.1f), root);
        }

        private static void Cockpit()
        {
            var root = Area("Cockpit — observation and piloting");
            Box("Window sill", new Vector3(0, 0.45f, 7), new Vector3(10, 0.9f, 0.5f), shell, root);
            Box("Window lintel", new Vector3(0, 3.7f, 7), new Vector3(10, 0.6f, 0.5f), shell, root);
            Box("Window port surround", new Vector3(-4.45f, 2.1f, 7), new Vector3(1.1f, 2.4f, 0.5f), shell, root);
            Box("Window starboard surround", new Vector3(4.45f, 2.1f, 7), new Vector3(1.1f, 2.4f, 0.5f), shell, root);
            var pane = Box("Observation window collision pane", new Vector3(0, 2.15f, 7), new Vector3(7.8f, 2.5f, 0.5f), dark, root);
            pane.GetComponent<Renderer>().enabled = false; // Clear graybox window; solid pressure boundary.
            for (int side = -1; side <= 1; side += 2)
            {
                Box("Window mullion", new Vector3(side * 2.2f, 2.15f, 6.72f), new Vector3(0.08f, 2.5f, 0.12f), trim, root);
                Box("Flight console", new Vector3(side * 2.85f, 0.65f, 5.65f), new Vector3(1.9f, 1.3f, 1.25f), blue, root);
                Box("Display placeholder", new Vector3(side * 2.85f, 1.32f, 5.65f), new Vector3(1.5f, 0.05f, 0.8f), dark, root);
                if (side == -1)
                {
                    Box("Pilot seat base", new Vector3(-2.85f, 0.45f, 4.5f), new Vector3(0.8f, 0.9f, 0.85f), trim, root);
                    Box("Pilot seat back", new Vector3(-2.85f, 1.15f, 4.15f), new Vector3(0.8f, 1.2f, 0.15f), blue, root);
                }
                AccessRail("Window access rail " + side, new Vector3(side * 2.8f, 1.45f, 6.45f), new Vector3(1.8f, 0.1f, 0.1f), root);
            }
            Label("OBSERVATION", new Vector3(0, 3.7f, 6.72f), Quaternion.identity, 0.06f, root);
        }

        private static void Rails()
        {
            for (int side = -1; side <= 1; side += 2)
                for (int segment = 0; segment < 10; segment++)
                {
                    float z = -5.75f + segment * 1.25f;
                    if (!(side == -1 && segment == 1))
                        AccessRail($"Transit rail {side}:{segment}", new Vector3(side * 1.25f, 1.45f, z), new Vector3(0.1f, 0.1f, 1.2f), layout);
                    Box("Rail stanchion", new Vector3(side * 1.25f, 0.7f, z - 0.5f), new Vector3(0.07f, 1.4f, 0.07f), trim, layout);
                }
        }

        private static void Lighting()
        {
            var oldLight = GameObject.Find("Interior light").GetComponent<Light>();
            oldLight.intensity = 1;
            foreach (float z in new[] { -4.2f, 1f, 5f })
            {
                var fixture = Box("Ceiling light", new Vector3(0, 3.95f, z), new Vector3(1.5f, 0.08f, 0.65f), shell, layout);
                var lamp = new GameObject("Warm cabin light").AddComponent<Light>();
                lamp.transform.SetParent(layout, false);
                lamp.transform.position = fixture.transform.position - Vector3.up * 0.25f;
                lamp.type = LightType.Point;
                lamp.range = 9;
                lamp.intensity = 3;
                lamp.color = new Color(1, 0.86f, 0.7f);
            }
        }

        private static Transform Area(string name)
        {
            var root = new GameObject(name).transform;
            root.SetParent(layout, false);
            return root;
        }

        private static void AccessRail(string name, Vector3 position, Vector3 scale, Transform parent)
        {
            var rail = Box(name, position, scale, amber, parent).AddComponent<Grabbable>();
            var data = new SerializedObject(rail);
            data.FindProperty("handrail").boolValue = true;
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject Box(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.position = position;
            box.transform.localScale = scale;
            box.GetComponent<Renderer>().sharedMaterial = material;
            return box;
        }

        private static void Label(string value, Vector3 position, Quaternion rotation, float size, Transform parent)
        {
            var text = new GameObject(value + " sign").AddComponent<TextMesh>();
            text.transform.SetParent(parent, false);
            text.transform.SetPositionAndRotation(position, rotation);
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.GetComponent<Renderer>().sharedMaterial = SignMaterial();
            text.fontSize = 64;
            text.characterSize = size;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = new Color(0.85f, 0.92f, 0.97f);
        }

        private static Material SignMaterial()
        {
            string path = Root + "Art/Materials/CabinSign.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find("LongWayHome/CabinSign"));
            material.mainTexture = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf").material.mainTexture;
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        // Targeted visual correction; preserves the generated layout and all physics components.
        public static void RepairSignMaterials()
        {
            var scene = EditorSceneManager.OpenScene(Root + "Scenes/ShipInterior.unity");
            foreach (var text in UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsSortMode.None))
                text.GetComponent<Renderer>().sharedMaterial = SignMaterial();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            ShipBlockoutPreview.Capture();
        }

        private static Material MakeMaterial(string name, Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name, color = color };
            AssetDatabase.CreateAsset(material, Root + "Art/Materials/" + name + ".mat");
            return material;
        }

        private static void Remove(string name)
        {
            var target = GameObject.Find(name);
            if (target != null) UnityEngine.Object.DestroyImmediate(target);
        }
    }
}
