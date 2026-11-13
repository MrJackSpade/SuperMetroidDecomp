namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's foot's own state beside the phase every part runs: its second-phase think timer.</summary>
public sealed class KraidFootState : KraidPartState
{
    /// <summary>Gets the second-phase foot's think timer, which expires when its decrement reaches zero.</summary>
    public int ThinkTimer { get; internal set; }
}
