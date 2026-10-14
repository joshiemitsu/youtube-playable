using R3;
using RapidPro.MVC;

/// <summary>
/// Tries left in a level. The game mode changes it; the HUD only reads it through <see cref="IHearts"/>.
/// </summary>
public class HeartsModel : Model, IHearts
{
    private readonly ReactiveProperty<int> _current;

    public HeartsModel() : this(3) { }

    public HeartsModel(int p_max)
    {
        Max = p_max;
        _current = CreateProperty(p_max);
    }

    public ReadOnlyReactiveProperty<int> Current => _current;
    public int Max { get; private set; }

    public bool IsOut => _current.Value <= 0;

    // Removes one heart. Returns true when that was the last one
    public bool LoseOne()
    {
        if (_current.Value > 0)
        {
            _current.Value--;
        }
        return IsOut;
    }

    public void Reset(int p_max)
    {
        Max = p_max;
        _current.Value = p_max;
    }
}
