using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LongWayHome.Core
{
    /// <summary>Owns scene composition. Bootstrap remains loaded for future session services.</summary>
    public sealed class SceneBootstrap : MonoBehaviour
    {
        [SerializeField] private string interiorScene = "ShipInterior";
        [SerializeField] private string exteriorScene = "SpaceExterior";

        private IEnumerator Start()
        {
            if (!Application.CanStreamedLevelBeLoaded(interiorScene) ||
                !Application.CanStreamedLevelBeLoaded(exteriorScene))
            {
                Debug.LogError("Foundation scenes are missing from the build scene list.", this);
                yield break;
            }

            if (!SceneManager.GetSceneByName(interiorScene).isLoaded)
                yield return SceneManager.LoadSceneAsync(interiorScene, LoadSceneMode.Additive);
            if (!SceneManager.GetSceneByName(exteriorScene).isLoaded)
                yield return SceneManager.LoadSceneAsync(exteriorScene, LoadSceneMode.Additive);

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(interiorScene));
        }
    }
}
