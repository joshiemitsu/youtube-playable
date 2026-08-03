using UnityEngine;

public class GameBoardBootsrapper : MonoBehaviour
{
    [SerializeField] private GameBoardController[] _controllers;

    private void Awake()
    {
        var model = new GameBoardModel();
        model.Init();

        foreach(var controller in _controllers)
        {
            controller.SetModel(model);
        }
    }
}
