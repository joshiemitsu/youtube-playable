using UnityEngine;

[System.Serializable]
public class GameBoardModel : BaseModel
{
    [SerializeField] private int WIDTH = 10;
    [SerializeField] private int HEIGHT = 10;

    private const int FREE_SLOT = 0;

    private int[,] _boardSlots;

    private Vector2 _currentPos;

    public override void Init()
    {
        _boardSlots = new int[WIDTH, HEIGHT];
        for (int w = 0; w < WIDTH; w++)
        {
            for(int h  = 0; h < HEIGHT; h++)
            {
                _boardSlots[w, h] = FREE_SLOT;
            }
        }
    }

    public void DisplaySlots()
    {
        for (int w = 0; w < WIDTH; w++)
        {
            for (int h = 0; h < HEIGHT; h++)
            {
                Debug.Log("[" + w + "][" + h + "] = " + _boardSlots[w, h]);
            }
        }
    }

    public int GetSlot(Vector2 p_slot)
    {
        return _boardSlots[(int)p_slot.x, (int)p_slot.y];
    }

    public void SetSlot(Vector2 p_slot, int p_value)
    {
        _boardSlots[(int)p_slot.x, (int)p_slot.y] = p_value;
    }

    public bool CanMove(Vector2 p_startPos, Vector2 p_direction)
    {
        _currentPos = p_startPos;

        return (CheckDirection(p_startPos, p_direction));
    }

    public bool CheckDirection(Vector2 p_startPos, Vector2 p_direction)
    {
        if (IsOutOfBounds())
        {
            Debug.Log("Cannot move, out of bounds: " + _currentPos);
            return false;
        }

        if (_boardSlots[(int)_currentPos.x, (int)_currentPos.y] != FREE_SLOT)
        {
            Debug.Log("Stopping [" + _currentPos.x + "][" + _currentPos.y + "], cannot move! ");
            return false;
        }

        _currentPos = p_startPos + p_direction;
        Debug.Log("CurrentPos [" + _currentPos.x + "][" + _currentPos.y + "]");

        return CheckDirection(_currentPos, p_direction);
    }

    private bool IsOutOfBounds()
    {
        bool isOutOfBoundsX = (_currentPos.x < 0 || _currentPos.x >= WIDTH);
        bool isOutOfBoundsY = (_currentPos.y < 0 || _currentPos.y >= HEIGHT);

        return (isOutOfBoundsX || isOutOfBoundsY);
    }
}
