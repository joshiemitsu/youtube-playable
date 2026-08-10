using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    private async void Start()
    {
        await UniTask.WhenAll(
            SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive).ToUniTask(),
            SceneManager.LoadSceneAsync("GameUI", LoadSceneMode.Additive).ToUniTask()
        );

        await SceneManager.UnloadSceneAsync("Bootstrap").ToUniTask();
    }
}