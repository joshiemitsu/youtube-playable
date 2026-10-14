using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fills every playable cell of a shape with arrows so the level is always solvable.
/// </summary>
/// <remarks>
/// <b>Solvable by construction.</b> Arrows are placed in the reverse of the order the player could
/// remove them. When an arrow is placed, the cells in front of its head must not hold any arrow
/// placed before it. Those earlier arrows are removed <i>after</i> it, so its exit path is always
/// clear when its turn comes. Arrows placed later may sit in its path: they are removed first.
/// <para>
/// Every arrow's second cell sits straight behind its head, so the head (drawn facing away from the
/// second cell) always points the way the arrow moves.
/// </para>
/// <para>
/// The hardest cells (fewest empty neighbours) are filled first, so no cell gets trapped. A cell that
/// cannot start an arrow joins the tail of a neighbouring arrow when that keeps every path clear.
/// A seed that still gets stuck returns false; try the next seed.
/// </para>
/// </remarks>
public static class ArrowLevelGenerator
{
    private static readonly Vector2Int[] Directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    private class PlacedArrow
    {
        public readonly List<Vector2Int> Cells = new List<Vector2Int>();
        public Vector2Int Direction;
        public HashSet<Vector2Int> Path;
    }

    /// <summary>
    /// Tries to fill <paramref name="p_mask"/> with arrows of <paramref name="p_minLength"/> to
    /// <paramref name="p_maxLength"/> cells. Returns false when this seed gets stuck.
    /// </summary>
    public static bool TryGenerate(ShapeMask p_mask, int p_seed, int p_minLength, int p_maxLength, out List<ArrowDefinition> p_arrows)
    {
        var random = new System.Random(p_seed);
        var owner = new Dictionary<Vector2Int, PlacedArrow>();
        var placed = new List<PlacedArrow>();
        p_arrows = null;

        bool IsPlayable(Vector2Int c) => p_mask.IsInBounds(c.x, c.y) && p_mask.Get(c.x, c.y) == SlotType.FREE;
        bool IsEmpty(Vector2Int c) => IsPlayable(c) && !owner.ContainsKey(c);

        int EmptyNeighbours(Vector2Int c)
        {
            int count = 0;
            foreach (var d in Directions)
                if (IsEmpty(c + d)) count++;
            return count;
        }

        while (true)
        {
            var empties = new List<Vector2Int>();
            for (int x = 0; x < p_mask.Width; x++)
                for (int y = 0; y < p_mask.Height; y++)
                    if (IsEmpty(new Vector2Int(x, y))) empties.Add(new Vector2Int(x, y));

            if (empties.Count == 0)
                break;

            Shuffle(empties, random);
            empties.Sort((a, b) => EmptyNeighbours(a).CompareTo(EmptyNeighbours(b)));

            bool didPlace = false;
            foreach (var cell in empties)
            {
                if (TryPlaceArrow(cell) || TryJoinTail(cell))
                {
                    didPlace = true;
                    break;
                }
            }

            if (!didPlace)
                return false;
        }

        p_arrows = new List<ArrowDefinition>();
        foreach (var arrow in placed)
            p_arrows.Add(new ArrowDefinition(new List<Vector2Int>(arrow.Cells), arrow.Direction));

        return ArrowLevelSolver.IsSolvable(IsPlayable, p_arrows);

        // Starts a new arrow with its head on p_head, pointing a way whose path holds no placed arrow.
        bool TryPlaceArrow(Vector2Int p_head)
        {
            var directions = new List<Vector2Int>(Directions);
            Shuffle(directions, random);

            foreach (var direction in directions)
            {
                var path = GetPath(p_head, direction);
                if (path.Overlaps(owner.Keys))
                    continue;

                var arrow = new PlacedArrow { Direction = direction, Path = path };
                arrow.Cells.Add(p_head);

                // The head is drawn facing away from the second cell, so that cell must sit straight behind it
                var behindHead = p_head - direction;
                if (IsEmpty(behindHead))
                {
                    arrow.Cells.Add(behindHead);
                    GrowBody(arrow, random.Next(p_minLength, p_maxLength + 1));
                }

                // A lone head is only allowed when the cell has no empty neighbour at all
                bool isTooShort = arrow.Cells.Count < p_minLength && EmptyNeighbours(p_head) > 0;
                if (isTooShort)
                    continue;

                Place(arrow);
                return true;
            }

            return false;
        }

        // Extends the body from the tail into empty cells, never into the arrow's own path.
        void GrowBody(PlacedArrow p_arrow, int p_targetLength)
        {
            while (p_arrow.Cells.Count < p_targetLength)
            {
                var tail = p_arrow.Cells[p_arrow.Cells.Count - 1];
                var options = new List<Vector2Int>();
                foreach (var d in Directions)
                {
                    var next = tail + d;
                    if (IsEmpty(next) && !p_arrow.Path.Contains(next) && !p_arrow.Cells.Contains(next))
                        options.Add(next);
                }

                if (options.Count == 0)
                    return;

                Shuffle(options, random);
                options.Sort((a, b) => EmptyNeighbours(a).CompareTo(EmptyNeighbours(b)));
                p_arrow.Cells.Add(options[0]);
            }
        }

        // Adds a stuck cell to the tail of a neighbouring arrow, if no path would be blocked by it.
        bool TryJoinTail(Vector2Int p_cell)
        {
            for (int i = 0; i < placed.Count; i++)
            {
                var arrow = placed[i];
                var tail = arrow.Cells[arrow.Cells.Count - 1];
                bool touchesTail = Mathf.Abs(tail.x - p_cell.x) + Mathf.Abs(tail.y - p_cell.y) == 1;
                if (!touchesTail || arrow.Path.Contains(p_cell))
                    continue;

                // Arrows placed after this one are removed before it, so their paths must stay clear of its cells.
                bool blocksLaterArrow = false;
                for (int j = i + 1; j < placed.Count; j++)
                {
                    if (placed[j].Path.Contains(p_cell))
                    {
                        blocksLaterArrow = true;
                        break;
                    }
                }

                if (blocksLaterArrow)
                    continue;

                arrow.Cells.Add(p_cell);
                owner[p_cell] = arrow;
                return true;
            }

            return false;
        }

        void Place(PlacedArrow p_arrow)
        {
            placed.Add(p_arrow);
            foreach (var c in p_arrow.Cells)
                owner[c] = p_arrow;
        }

        HashSet<Vector2Int> GetPath(Vector2Int p_head, Vector2Int p_direction)
        {
            var path = new HashSet<Vector2Int>();
            var cell = p_head + p_direction;
            while (IsPlayable(cell))
            {
                path.Add(cell);
                cell += p_direction;
            }
            return path;
        }
    }

    /// <summary>Restricted cells of a mask, as stored in <see cref="ArrowLevelData"/>.</summary>
    public static List<Vector2Int> GetRestrictedCells(ShapeMask p_mask)
    {
        var cells = new List<Vector2Int>();
        for (int x = 0; x < p_mask.Width; x++)
            for (int y = 0; y < p_mask.Height; y++)
                if (p_mask.Get(x, y) == SlotType.RESTRICTED) cells.Add(new Vector2Int(x, y));
        return cells;
    }

    private static void Shuffle<T>(List<T> p_list, System.Random p_random)
    {
        for (int i = p_list.Count - 1; i > 0; i--)
        {
            int j = p_random.Next(i + 1);
            (p_list[i], p_list[j]) = (p_list[j], p_list[i]);
        }
    }
}
