using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace LongWayHome.Player
{
    public sealed class GameplayReset : MonoBehaviour
    {
        private bool loading;
        private void Update()
        {
            if (loading || Time.timeScale == 0 || Keyboard.current == null || !Keyboard.current.rKey.wasPressedThisFrame) return;
            loading = true;
            SceneManager.LoadSceneAsync(gameObject.scene.name, LoadSceneMode.Single);
        }
    }
}
