using UnityEngine;

[ExecuteAlways]
public class LevelMakerPreview : MonoBehaviour
{
    [Header("Shape")]
    [SerializeField] private Texture2D _shapeTexture;
    [SerializeField, Min(1)] private int _width = 11;
    [SerializeField, Min(1)] private int _height = 11;

    [Header("Preview")]
    [SerializeField] private float _boardSize = 10f;

    private ShapeMask _mask;

    // Runs in the editor every time a value changes in the Inspector
    private void OnValidate()
    {
        if (_shapeTexture == null) { _mask = null; return; }

        ShapeMask loaded = ShapeLoader.FromTexture(_shapeTexture);
        _mask = ShapeResizer.Resize(loaded, _width, _height);
    }

    // Draws in the Scene view, no Play needed
    private void OnDrawGizmos()
    {
        if (_mask == null) return;

        float cellSize = _boardSize / Mathf.Max(_mask.Width, _mask.Height);
        float offsetX = (_mask.Width - 1) * cellSize * 0.5f;
        float offsetY = (_mask.Height - 1) * cellSize * 0.5f;

        for (int x = 0; x < _mask.Width; x++)
        {
            for (int y = 0; y < _mask.Height; y++)
            {
                Gizmos.color = _mask.Get(x, y) == SlotType.FREE ? Color.white : Color.black;
                Vector3 pos = transform.position + new Vector3(x * cellSize - offsetX, y * cellSize - offsetY);
                Gizmos.DrawCube(pos, Vector3.one * cellSize * 0.9f);
            }
        }
    }
}