using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Edit-mode tests for the level generator, solver and hearts. The heart shape is the same
/// 11 x 11 picture as Level-Heart.png, written out so the tests do not need the image.
/// </summary>
public class ArrowLevelTests
{
    // Top row first, like the image. '#' = outside the shape, '.' = playable.
    private static readonly string[] HeartRows =
    {
        "###########",
        "##..###..##",
        "#....#....#",
        "...........",
        "...........",
        "#.........#",
        "##.......##",
        "###.....###",
        "####...####",
        "#####.#####",
        "###########",
    };

    private static ShapeMask HeartMask()
    {
        int height = HeartRows.Length;
        int width = HeartRows[0].Length;
        var mask = new ShapeMask(width, height);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                mask.Set(x, y, HeartRows[height - 1 - y][x] == '#' ? SlotType.RESTRICTED : SlotType.FREE);
        return mask;
    }

    private static List<ArrowDefinition> GenerateHeart()
    {
        var mask = HeartMask();
        for (int seed = 1; seed <= 5000; seed++)
        {
            if (ArrowLevelGenerator.TryGenerate(mask, seed, 2, 5, out var arrows))
                return arrows;
        }
        Assert.Fail("No seed produced a full, solvable heart level.");
        return null;
    }

    [Test]
    public void Generator_FillsEveryPlayableCellExactlyOnce()
    {
        var mask = HeartMask();
        var arrows = GenerateHeart();

        var covered = new HashSet<Vector2Int>();
        foreach (var arrow in arrows)
            foreach (var cell in arrow.Cells)
                Assert.IsTrue(covered.Add(cell), $"Cell {cell} is used by two arrows.");

        int playable = 0;
        for (int x = 0; x < mask.Width; x++)
            for (int y = 0; y < mask.Height; y++)
                if (mask.Get(x, y) == SlotType.FREE) playable++;

        Assert.AreEqual(59, playable, "The heart has 59 playable cells.");
        Assert.AreEqual(playable, covered.Count);
    }

    [Test]
    public void Generator_ArrowsAreConnectedAndHeadsAreClearOfOwnBody()
    {
        foreach (var arrow in GenerateHeart())
        {
            for (int i = 1; i < arrow.Cells.Count; i++)
            {
                var step = arrow.Cells[i] - arrow.Cells[i - 1];
                Assert.AreEqual(1, Mathf.Abs(step.x) + Mathf.Abs(step.y), "Body cells must touch the one before.");
            }

            // The cell right in front of the head must not be the arrow's own body
            Assert.IsFalse(new List<Vector2Int>(arrow.Cells).Contains(arrow.Head + arrow.Direction));

            // The head faces away from the second cell, so that must be the way the arrow moves
            if (arrow.Cells.Count > 1)
                Assert.AreEqual(arrow.Direction, arrow.Cells[0] - arrow.Cells[1], "The head must continue straight from its second segment.");
        }
    }

    [Test]
    public void Generator_LevelIsSolvable()
    {
        var mask = HeartMask();
        var arrows = GenerateHeart();

        bool IsPlayable(Vector2Int c) => mask.IsInBounds(c.x, c.y) && mask.Get(c.x, c.y) == SlotType.FREE;
        Assert.IsTrue(ArrowLevelSolver.IsSolvable(IsPlayable, arrows));
    }

    [Test]
    public void Solver_TwoArrowsFacingEachOther_IsNotSolvable()
    {
        // A 3 x 1 strip: left arrow points right, right arrow points left. Each blocks the other.
        bool IsPlayable(Vector2Int c) => c.y == 0 && c.x >= 0 && c.x < 3;
        var arrows = new List<ArrowDefinition>
        {
            new ArrowDefinition(new List<Vector2Int> { new Vector2Int(0, 0) }, Vector2Int.right),
            new ArrowDefinition(new List<Vector2Int> { new Vector2Int(2, 0) }, Vector2Int.left),
        };

        // The middle cell is empty, but each head's path runs into the other arrow.
        Assert.IsFalse(ArrowLevelSolver.IsSolvable(IsPlayable, arrows));
    }

    [Test]
    public void Hearts_ThreeHits_RunOut()
    {
        var hearts = new HeartsModel(3);

        Assert.IsFalse(hearts.LoseOne());
        Assert.IsFalse(hearts.LoseOne());
        Assert.IsTrue(hearts.LoseOne(), "The third hit uses the last heart.");
        Assert.AreEqual(0, hearts.Current.CurrentValue);

        hearts.Reset(3);
        Assert.AreEqual(3, hearts.Current.CurrentValue);
        hearts.Dispose();
    }
}
