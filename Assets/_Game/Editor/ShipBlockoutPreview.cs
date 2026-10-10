using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace LongWayHome.Editor
{
    public static class ShipBlockoutPreview
    {
        // Batch capture uses a separate Editor process. Never saves these temporary camera poses.
        public static void Capture()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/ShipInterior.unity");
            Camera camera = Camera.main;
            Directory.CreateDirectory("TestResults/BlockoutPreview");
            Save(camera, "habitat", new Vector3(0, 1.65f, -5.9f), new Vector3(0, 1.8f, 0));
            Save(camera, "workshop", new Vector3(-0.2f, 1.7f, 0), new Vector3(3.2f, 1.4f, 1.3f));
            Save(camera, "cockpit", new Vector3(0, 1.65f, 3.6f), new Vector3(0, 1.8f, 7));
            GameObject.Find("Ceiling").GetComponent<Renderer>().enabled = false;
            Save(camera, "overview", new Vector3(0, 16, -0.1f), Vector3.zero);
            foreach (var label in Object.FindObjectsByType<TextMesh>(FindObjectsSortMode.None))
                Debug.Log($"BLOCKOUT_LABEL {label.text}: {label.GetComponent<Renderer>().bounds.size}");
            Debug.Log("SHIP_BLOCKOUT_PREVIEW_OK");
        }

        internal static void Save(Camera camera, string name, Vector3 position, Vector3 target)
        {
            camera.transform.position = position;
            camera.transform.LookAt(target);
            camera.aspect = 16f / 9f;
            var texture = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32);
            texture.Create();
            var previous = RenderTexture.active;
            var pixels = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            try
            {
                var request = new RenderPipeline.StandardRequest { destination = texture };
                RenderPipeline.SubmitRenderRequest(camera, request);
                RenderPipeline.SubmitRenderRequest(camera, request);
                RenderTexture.active = texture;
                pixels.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                pixels.Apply();
                File.WriteAllBytes("TestResults/BlockoutPreview/" + name + ".png", pixels.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous;
                texture.Release();
                Object.DestroyImmediate(texture);
                Object.DestroyImmediate(pixels);
            }
        }
    }
}
