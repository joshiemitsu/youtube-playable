using R3;

/// <summary>
/// The player's hearts (tries left), read-only. The HUD in the GameUI scene shows these.
/// </summary>
public interface IHearts
{
    // Hearts left. Starts at Max and drops by one for every arrow that bumps into another
    ReadOnlyReactiveProperty<int> Current { get; }

    int Max { get; }
}
