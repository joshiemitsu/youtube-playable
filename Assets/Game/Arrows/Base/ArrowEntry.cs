using ObservableCollections;
using R3;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ArrowEntry : MonoBehaviour
{
    private LineRenderer _lineRenderer;

    public readonly ObservableList<Vector2> Points = new();
    public readonly Subject<Vector2> OnMove = new();

    public void Initialize()
    {
        
    }
}
