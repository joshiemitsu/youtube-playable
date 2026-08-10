using Cysharp.Threading.Tasks;
using R3;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BoardInitializer : MonoBehaviour
{
    [Header("Main System")]
    [SerializeField] private GameBoardSystem _boardSystem;

    [Header("Spacing")]
    [SerializeField] private float _spacing = 1;

    [Header("Prefab")]
    [SerializeField] private GameObject _dotsPrefab;
    [SerializeField] private GameObject _arrowPrefab;

    [Header("World Parent")]
    [SerializeField] private Transform _worldParent;

    [Header("World Positions")]
    [SerializeField] private Vector2[,] _worldPosition;

    [Header("GameObject Lists")]
    // create list of arrows for now
    private List<ArrowEntry> _activeArrows = new List<ArrowEntry>();
    private List<ArrowEntry> _inactiveArrow = new List<ArrowEntry>();

    private void Awake()
    {
        var d = Disposable.CreateBuilder();
        _boardSystem.SpawnGrid
            .Subscribe(Size => SpawnBoard(Size.width, Size.height))
            .AddTo(ref d);
        d.RegisterTo(destroyCancellationToken);
    }

    private void SpawnBoard(int p_width, int p_height)
    {
        _worldPosition = new Vector2[p_width, p_height]; 

        for (int w = 0; w < p_width; w++)
        {
            for (int h = 0; h < p_height; h++)
            {
                GameObject obj = Instantiate(_dotsPrefab, _worldParent.transform);

                float xOffset = ((float)(p_width - 1) * _spacing) * 0.5f;
                float yOffset = ((float)(p_height - 1) * _spacing) * 0.5f;

                float xPos = _worldParent.position.x + 
                            (float)(w * _spacing) - xOffset;
                float yPos = _worldParent.position.y + 
                            (float)(h * _spacing) - yOffset;

                obj.transform.position = new Vector3(xPos, yPos);

                _worldPosition[w, h] = obj.transform.position;
            }
        }

        SpawnArrows();
    }

    private void SpawnArrows()
    {
        GameObject obj = Instantiate(_arrowPrefab, _worldParent.transform);
        ArrowEntry arrowEntry = obj.GetComponent<ArrowEntry>();

        List<Vector2> posList = new List<Vector2>();
        posList.Add(_worldPosition[0, 0]);
        posList.Add(_worldPosition[1, 0]);
        posList.Add(_worldPosition[2, 0]);
        posList.Add(_worldPosition[2, 1]);
        posList.Add(_worldPosition[2, 2]);

        arrowEntry.Initialize(_boardSystem, posList);
    }
}
