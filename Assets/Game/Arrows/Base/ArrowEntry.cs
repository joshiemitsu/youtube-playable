using System.Collections.Generic;
using ObservableCollections;
using R3;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ArrowEntry : MonoBehaviour
{
    [Header("System")]
    [SerializeField] private GameBoardSystem _boardSystem;

    [Header("Prefab")]
    [SerializeField] private GameObject _headObj;
    [SerializeField] private GameObject _bodyObj;

    [Header("Parent")]
    [SerializeField] private Transform _parent;

    private LineRenderer _lineRenderer;

    public readonly ObservableList<Vector2> Points = new();
    public readonly Subject<Vector2> OnMove = new();

    public void Initialize(GameBoardSystem p_boardSystem, List<Vector2> p_points)
    {
        int FIRST_IDX = 0;
        this.transform.position = p_points[FIRST_IDX];

        _boardSystem = p_boardSystem;
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = p_points.Count;

        for(int i = 0; i < p_points.Count; i++)
        {
            GameObject prefab = (i == 0) ? _headObj : _bodyObj;
            GameObject obj = Instantiate(prefab, _parent.transform);
            obj.transform.position = p_points[i];
            _lineRenderer.SetPosition(i, p_points[i]);
        }
    }
}
