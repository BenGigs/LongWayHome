using System;
using System.Collections.Generic;
using System.IO;
using LongWayHome.Player;
using LongWayHome.ShipPhysics;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LongWayHome.Editor
{
    public static class FiveDeckGameplaySetup
    {
        public const string ScenePath = "Assets/_Game/Scenes/FiveDeckGameplay.unity";
        private const string Root = "Assets/_Game/";
        private static readonly Vector3 Shaft = new Vector3(0.9f, 0, -0.85f);

        [MenuItem("The Long Way Home/Create Five Deck Gameplay (once)")]
        public static void Create()
        {
            if (File.Exists(ScenePath)) throw new InvalidOperationException("Gameplay scene exists; edit it without regeneration.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var floor = Load("ShipShell"); var dark = Load("ShipTrim"); var amber = Load("WorkshopAmber");
            var green = Load("HabitatGreen"); var blue = Load("CockpitBlue");
            var source = new GameObject("Ship acceleration").AddComponent<ShipGravityController>();
            source.AccelerationLocal = Vector3.up * 9.81f;
            var player = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Prefabs/Player/FoundationPlayer.prefab"));
            player.name = "Astronaut";
            player.transform.position = Shaft + new Vector3(0, 1.05f, -1.55f);
            player.AddComponent<LedgeClimber>();
            player.AddComponent<GameplayReset>();
            Set(player.GetComponent<ShipGravityBody>(), "source", source);
            Set(player.GetComponent<FoundationPlayer>(), "gravitySource", source);
            PrefabUtility.RecordPrefabInstancePropertyModifications(player.GetComponent<ShipGravityBody>());
            PrefabUtility.RecordPrefabInstancePropertyModifications(player.GetComponent<FoundationPlayer>());
            var camera = player.GetComponentInChildren<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.025f, .045f, .065f);
            PrefabUtility.RecordPrefabInstancePropertyModifications(camera);

            var bottom = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bottom.name = "Sealed bottom floor"; bottom.transform.position = new Vector3(0, -.10f, 0);
            bottom.transform.localScale = new Vector3(8, .1f, 8); bottom.GetComponent<Renderer>().sharedMaterial = floor;
            UnityEngine.Object.DestroyImmediate(bottom.GetComponent<Collider>());
            bottom.AddComponent<MeshCollider>().sharedMesh = bottom.GetComponent<MeshFilter>().sharedMesh;
            var mesh = Annulus();
            AssetDatabase.CreateAsset(mesh, Root + "Art/Models/GameplayDeck.asset");
            string[] names = { "05 FUEL / STORAGE", "04 HABITAT / WATER", "03 WORKSHOP / ROBOT", "02 NAVIGATION / SLEEP", "01 COCKPIT" };
            for (int deck = 0; deck < 5; deck++)
            {
                float y = deck * 3.2f;
                var room = new GameObject(names[deck]);
                if (deck > 0)
                {
                    var obj = new GameObject("Deck " + deck + " floor", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
                    obj.transform.SetParent(room.transform); obj.transform.position = Vector3.up * y;
                    obj.GetComponent<MeshFilter>().sharedMesh = mesh;
                    obj.GetComponent<MeshRenderer>().sharedMaterial = floor;
                    obj.GetComponent<MeshCollider>().sharedMesh = mesh;
                }
                var ring = new GameObject("Deck " + deck + " grab ring", typeof(LedgeRing), typeof(Grabbable));
                ring.transform.SetParent(room.transform); ring.transform.position = Shaft + Vector3.up * y;
                var data = new SerializedObject(ring.GetComponent<Grabbable>());
                data.FindProperty("handrail").boolValue = true; data.ApplyModifiedPropertiesWithoutUndo();
                for (int segment = 0; segment < 24; segment++)
                {
                    float a = segment * Mathf.PI * 2 / 24;
                    Vector3 point = Shaft + new Vector3(Mathf.Sin(a) * 1.03f, y - .18f, Mathf.Cos(a) * 1.03f);
                    var grip = Box("Orange ring grip", point, new Vector3(.285f, .14f, .14f), amber);
                    grip.transform.rotation = Quaternion.Euler(0, a * Mathf.Rad2Deg, 0);
                    grip.transform.SetParent(ring.transform, true);
                }
                Label(names[deck], new Vector3(0, y + 2.3f, 3.65f));
                var light = new GameObject("Deck light").AddComponent<Light>();
                light.transform.position = new Vector3(0, y + 2.6f, 0); light.type = LightType.Point;
                light.range = 6; light.intensity = 3; light.color = new Color(1, .87f, .72f);
                light.transform.SetParent(room.transform, true);
                // Keep fixtures outside the central movement route.
                Box("Storage", new Vector3(-2.65f, y + .6f, .3f), new Vector3(1, 1.2f, 1.5f), deck == 1 ? green : dark).transform.SetParent(room.transform);
                if (deck == 3)
                {
                    Box("Open sleeping nook", new Vector3(-2.5f, y + .4f, -1.7f), new Vector3(1.2f, .8f, 2), green);
                    Box("Navigation table", new Vector3(0, y + .55f, 2.3f), new Vector3(1.7f, 1.1f, 1.1f), blue);
                }
                else Box(deck == 4 ? "Pilot console" : "Room work surface", new Vector3(0, y + .5f, 2.8f), new Vector3(2.2f, 1, .8f), deck == 1 ? green : blue);
            }
            for (int i = 0; i < 32; i++)
            {
                float a = i * Mathf.PI * 2 / 32;
                var wall = Box("Cylindrical hull", new Vector3(Mathf.Sin(a)*4, 8, Mathf.Cos(a)*4), new Vector3(.80f, 16, .20f), dark);
                wall.transform.rotation = Quaternion.Euler(0, a * Mathf.Rad2Deg, 0);
            }
            Box("Ceiling", new Vector3(0, 16, 0), new Vector3(8.2f, .2f, 8.2f), dark);
            var sample = Box("Carry test sample", new Vector3(-2.6f, 1.5f, .3f), Vector3.one * .2f, amber);
            sample.AddComponent<Rigidbody>().mass = .5f;
            sample.AddComponent<ShipGravityBody>().Source = source;
            sample.AddComponent<Grabbable>();
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.36f, .38f, .4f);
            EditorSceneManager.SaveScene(scene, ScenePath);
            var scenes = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(ScenePath, true) };
            foreach (var item in EditorBuildSettings.scenes) if (item.path != ScenePath) scenes.Add(item);
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("FIVE_DECK_GAMEPLAY_SETUP_OK");
        }

        public static void RepairDeckNormals()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(Root + "Art/Models/GameplayDeck.asset");
            var corrected = Annulus();
            EditorUtility.CopySerialized(corrected, existing);
            UnityEngine.Object.DestroyImmediate(corrected);
            EditorUtility.SetDirty(existing);
            AssetDatabase.SaveAssets();
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var bottom = GameObject.Find("Sealed bottom floor");
            if (!(bottom.GetComponent<Collider>() is MeshCollider))
            {
                UnityEngine.Object.DestroyImmediate(bottom.GetComponent<Collider>());
                bottom.AddComponent<MeshCollider>().sharedMesh = bottom.GetComponent<MeshFilter>().sharedMesh;
            }
            foreach (var ring in UnityEngine.Object.FindObjectsByType<LedgeRing>(FindObjectsSortMode.None))
                foreach (Transform segment in ring.transform)
                {
                    Vector3 position = segment.position;
                    position.y = ring.transform.position.y - .18f;
                    segment.position = position;
                }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("GAMEPLAY_DECK_NORMALS_OK");
        }

        public static void Preview()
        {
            EditorSceneManager.OpenScene(ScenePath);
            Directory.CreateDirectory("TestResults/BlockoutPreview");
            var camera = Camera.main;
            ShipBlockoutPreview.Save(camera, "gameplay-spawn", Shaft + new Vector3(0, 1.7f, -1.55f), Shaft + new Vector3(0, 3.165f, -1.03f));
            ShipBlockoutPreview.Save(camera, "gameplay-cockpit", new Vector3(0, 14.45f, -2.8f), new Vector3(0, 14, 2.7f));
            Debug.Log("GAMEPLAY_PREVIEW_OK");
        }

        public static void PreviewAndBuild()
        {
            Preview();
            // Discard capture-only camera poses before building saved scene data.
            EditorSceneManager.OpenScene(ScenePath);
            FoundationSetup.BuildWindows();
        }

        private static Mesh Annulus()
        {
            const int segments = 96;
            int n = segments + 1;
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            for (int layer = 0; layer < 2; layer++)
                for (int outer = 0; outer < 2; outer++)
                    for (int i = 0; i <= segments; i++)
                    {
                        float a = i * Mathf.PI * 2 / segments;
                        Vector3 ray = new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a));
                        float b = Vector3.Dot(Shaft, ray);
                        float r = outer == 0 ? 1.05f : -b + Mathf.Sqrt(b*b + 3.95f*3.95f - Shaft.sqrMagnitude);
                        vertices.Add(Shaft + ray*r + Vector3.down*(layer == 0 ? .18f : 0));
                    }
            void Quad(int a, int b, int c, int d) { triangles.AddRange(new[] { a,b,c,a,c,d }); }
            for (int i = 0; i < segments; i++)
            {
                Quad(i, n+i, n+i+1, i+1);
                Quad(2*n+i, 2*n+i+1, 3*n+i+1, 3*n+i);
                Quad(i, i+1, 2*n+i+1, 2*n+i);
                Quad(n+i, 3*n+i, 3*n+i+1, n+i+1);
            }
            // Unity's clockwise front faces: top surfaces face +Y.
            for (int i = 0; i < triangles.Count; i += 3)
                (triangles[i + 1], triangles[i + 2]) = (triangles[i + 2], triangles[i + 1]);
            var mesh = new Mesh { name = "Open circular deck" };
            mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
        }
        private static Material Load(string name) => AssetDatabase.LoadAssetAtPath<Material>(Root + "Art/Materials/" + name + ".mat");
        private static void Set(UnityEngine.Object obj, string field, UnityEngine.Object value)
        { var data = new SerializedObject(obj); data.FindProperty(field).objectReferenceValue = value; data.ApplyModifiedPropertiesWithoutUndo(); }
        private static GameObject Box(string name, Vector3 pos, Vector3 size, Material material)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube); obj.name = name;
            obj.transform.position = pos; obj.transform.localScale = size; obj.GetComponent<Renderer>().sharedMaterial = material; return obj;
        }
        private static void Label(string value, Vector3 pos)
        {
            var text = new GameObject(value).AddComponent<TextMesh>(); text.transform.position = pos;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.text = value; text.fontSize = 64;
            text.characterSize = .045f; text.anchor = TextAnchor.MiddleCenter;
            text.GetComponent<Renderer>().sharedMaterial = Load("CabinSign");
        }
    }
}
