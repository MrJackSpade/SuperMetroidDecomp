namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// The step back a mouth hit asks of Crocomire's fight (#2562). A hit that moves him asks for it at once, while it holds
/// his frame; his fight takes it the next time its cue runs in a phase that steps back, which may be many updates later,
/// and his sinking and his skeleton forget it. While it waits, his steps that wait out a hit stand still.
/// </summary>
public sealed class CrocomireAwaitedStepBack
{
    /// <summary>Whether a hit's step back waits for his fight.</summary>
    public bool IsAwaited { get; private set; }

    /// <summary>A hit asks for the step back; returns whether it is the first hit to ask since the fight last took one.</summary>
    internal bool Ask()
    {
        bool first = !IsAwaited;
        IsAwaited = true;
        return first;
    }

    /// <summary>The fight takes the step back: returns whether a hit asked for one, which no longer waits.</summary>
    internal bool Take()
    {
        bool awaited = IsAwaited;
        IsAwaited = false;
        return awaited;
    }

    /// <summary>His sinking or his skeleton forgets the step back a hit asked for; no fight takes it.</summary>
    internal void Forget() => IsAwaited = false;
}
