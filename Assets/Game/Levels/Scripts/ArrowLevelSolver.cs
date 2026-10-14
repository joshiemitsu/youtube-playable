using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Checks that a level can be cleared, using the same rule as <see cref="GameBoardSystem"/>:
/// an arrow leaves when the cells in front of its head are free all the way to a restricted cell
/// (a board edge or a cell outside the shape).
/// </summary>
/// <remarks>
/// Removing an arrow never blocks another one, so trying arrows until none can move is enough:
/// if that empties the board, the level is solvable.
/// </remarks>
public static class ArrowLevelSolver
{
    public static bool IsSolvable(Func<Vector2Int, bool> p_isPlayable, IReadOnlyList<ArrowDefinition> p_arrows)
    {
        var occupied = new HashSet<Vector2Int>();
        foreach (var arrow in p_arrows)
            occupied.UnionWith(arrow.Cells);

        var remaining = new List<ArrowDefinition>(p_arrows);
        bool removedOne = true;

        while (remaining.Count > 0 && removedOne)
        {
            removedOne = false;
            for (int i = remaining.Count - 1; i >= 0; i--)
            {
                if (!CanExit(remaining[i], p_isPlayable, occupied))
                    continue;

                occupied.ExceptWith(remaining[i].Cells);
                remaining.RemoveAt(i);
                removedOne = true;
            }
        }

        return remaining.Count == 0;
    }

    /// <summary>True when every cell in front of the head is free until the shape ends.</summary>
    public static bool CanExit(ArrowDefinition p_arrow, Func<Vector2Int, bool> p_isPlayable, HashSet<Vector2Int> p_occupied)
    {
        var cell = p_arrow.Head + p_arrow.Direction;
        while (p_isPlayable(cell))
        {
            if (p_occupied.Contains(cell))
                return false;
            cell += p_arrow.Direction;
        }
        return true;
    }
}
