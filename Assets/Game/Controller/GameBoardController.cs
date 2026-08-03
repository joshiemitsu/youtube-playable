using UnityEngine;

public class GameBoardController : BaseController<GameBoardModel>
{
    protected override void OnModelSet()
    {
        Model.SetSlot(new Vector2(0, 9), 1);

        if (Model.CanMove(new Vector2(0, 0), new Vector2(1, 0)))
        {
            Debug.Log("Arrow can move");
        }
        else
        {
            Debug.Log("Arrow cannot move");
        }
    }
}
