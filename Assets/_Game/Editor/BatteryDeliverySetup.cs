using System;
using System.IO;
using LongWayHome.Player;
using LongWayHome.ShipPhysics;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LongWayHome.Editor
{
    public static class BatteryDeliverySetup
    {
        private const string Root = "Assets/_Game/";
        public static void Create()
        {
            var scene = EditorSceneManager.OpenScene(FiveDeckGameplaySetup.ScenePath);
            if (UnityEngine.Object.FindFirstObjectByType<BatterySocket>() != null)
                throw new InvalidOperationException("Battery delivery already exists. Preserve the scene edits.");
            var blue = Load("CockpitBlue"); var amber = Load("WorkshopAmber"); var dark = Load("ShipTrim");
            var green = new Material(Load("HabitatGreen"));
            green.name = "BatteryPowerOn";
            green.color = new Color(.15f, 1, .3f);
            green.EnableKeyword("_EMISSION"); green.SetColor("_EmissionColor", new Color(.1f, 2, .2f));
            AssetDatabase.CreateAsset(green, Root + "Art/Materials/BatteryPowerOn.mat");
            var group = new GameObject("Battery delivery playtest");
            Box("Battery supply pedestal", new Vector3(-1, .5f, -2.3f), new Vector3(.65f, 1, .65f), dark, group.transform);
            var pack = Box("Workshop battery", new Vector3(-1, 1.22f, -2.3f), new Vector3(.26f, .36f, .20f), amber, group.transform);
            // Keep the root scale at one so its decorative children use metres.
            pack.transform.localScale = Vector3.one;
            pack.GetComponent<BoxCollider>().size = new Vector3(.26f, .36f, .20f);
            UnityEngine.Object.DestroyImmediate(pack.GetComponent<MeshFilter>());
            UnityEngine.Object.DestroyImmediate(pack.GetComponent<MeshRenderer>());
            var casing = Box("Battery casing", pack.transform.position, new Vector3(.26f, .36f, .20f), amber, pack.transform);
            UnityEngine.Object.DestroyImmediate(casing.GetComponent<Collider>());
            var stripe = Box("Battery blue stripe", pack.transform.position + new Vector3(0, 0, -.103f), new Vector3(.09f, .28f, .012f), blue, pack.transform);
            UnityEngine.Object.DestroyImmediate(stripe.GetComponent<Collider>());
            var body = pack.AddComponent<Rigidbody>(); body.mass = 1; body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate; body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            pack.AddComponent<ShipGravityBody>().Source = UnityEngine.Object.FindFirstObjectByType<ShipGravityController>();
            var battery = pack.AddComponent<Grabbable>();

            var socket = new GameObject("Workshop battery socket").AddComponent<BatterySocket>();
            socket.transform.SetParent(group.transform);
            socket.transform.position = new Vector3(-.55f, 7.68f, 2.45f);
            var mount = new GameObject("Battery latch pose").transform;
            mount.SetParent(socket.transform, false);
            Box("Socket backplate", socket.transform.position + Vector3.forward * .18f, new Vector3(.75f, .65f, .12f), blue, socket.transform);
            Box("Socket left guide", socket.transform.position + Vector3.left * .32f, new Vector3(.10f, .6f, .42f), amber, socket.transform);
            Box("Socket right guide", socket.transform.position + Vector3.right * .32f, new Vector3(.10f, .6f, .42f), amber, socket.transform);
            Box("Socket shelf", socket.transform.position + Vector3.down * .24f, new Vector3(.7f, .08f, .45f), blue, socket.transform);
            var lamp = Box("Power indicator", socket.transform.position + new Vector3(0, .42f, .05f), new Vector3(.30f, .1f, .08f), dark, socket.transform);
            var light = new GameObject("Powered work light").AddComponent<Light>();
            light.transform.SetParent(socket.transform, false); light.transform.localPosition = new Vector3(0, .6f, -.3f);
            light.type = LightType.Point; light.range = 3; light.intensity = 2; light.color = new Color(.3f, 1, .5f); light.enabled = false;
            var fan = new GameObject("Workshop fan rotor").transform;
            fan.SetParent(socket.transform, false); fan.localPosition = new Vector3(1.15f, .1f, .05f);
            Box("Fan housing", fan.position + Vector3.forward * .12f, new Vector3(.8f, .8f, .12f), dark, socket.transform);
            for (int i = 0; i < 2; i++)
            {
                var blade = Box("Fan blade", fan.position, new Vector3(.63f, .10f, .05f), blue, fan);
                blade.transform.localRotation = Quaternion.Euler(0, 0, i * 90);
                UnityEngine.Object.DestroyImmediate(blade.GetComponent<Collider>());
            }
            var player = UnityEngine.Object.FindFirstObjectByType<FoundationPlayer>();
            var hands = player.GetComponentsInChildren<HandInteractor>();
            var data = new SerializedObject(socket);
            Set(data, "battery", battery); Set(data, "mount", mount); Set(data, "player", player);
            Set(data, "view", player.GetComponentInChildren<Camera>().transform);
            Set(data, "leftHand", hands[0]); Set(data, "rightHand", hands[1]);
            Set(data, "poweredLight", light); Set(data, "indicator", lamp.GetComponent<Renderer>());
            Set(data, "offMaterial", dark); Set(data, "onMaterial", green); Set(data, "fan", fan);
            data.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Preview();
            Debug.Log("BATTERY_DELIVERY_SETUP_OK");
        }

        public static void Preview()
        {
            EditorSceneManager.OpenScene(FiveDeckGameplaySetup.ScenePath);
            Directory.CreateDirectory("TestResults/BlockoutPreview");
            ShipBlockoutPreview.Save(Camera.main, "battery-supply", new Vector3(.4f, 1.7f, -3.2f), new Vector3(-1, 1.2f, -2.3f));
            ShipBlockoutPreview.Save(Camera.main, "battery-workshop", new Vector3(0, 8.1f, .6f), new Vector3(0, 7.7f, 2.5f));
        }
        private static void Set(SerializedObject data, string name, UnityEngine.Object value) => data.FindProperty(name).objectReferenceValue = value;
        private static Material Load(string name) => AssetDatabase.LoadAssetAtPath<Material>(Root + "Art/Materials/" + name + ".mat");
        private static GameObject Box(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube); obj.name = name;
            obj.transform.position = position; obj.transform.localScale = scale; obj.transform.SetParent(parent, true);
            obj.GetComponent<Renderer>().sharedMaterial = material; return obj;
        }
    }
}
