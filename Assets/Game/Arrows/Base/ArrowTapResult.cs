/// <summary>What happens when an arrow is tapped.</summary>
public enum ArrowTapResult
{
    // Nothing happens: the arrow is already moving or has left the board
    Ignored,

    // The path is clear: the arrow slides off the board
    Exited,

    // Another arrow is in the way: the arrow bumps into it and goes back (costs a heart)
    Blocked
}
