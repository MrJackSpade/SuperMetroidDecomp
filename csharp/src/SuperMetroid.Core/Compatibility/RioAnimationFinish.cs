namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The finish of a Rio's swoop-opening, takeoff, ascent-opening, landing or cooldown animation, which the animation's
/// finishing cue raises and its main AI takes as it moves on (#2450, #2562). It is carried beside the Rio's action, not as a
/// phase of it: a wall or floor can turn the dive into its rebound before the opening the dive waits for has finished, or on
/// the update after, and the opening's finish then carries through the rebound and, as the Rio lands, ends its wait for its
/// next animation at once. A Rio waiting for Samus forgets a finish it does not use.
/// </summary>
/// <remarks>
/// Evidence: a Rio's instruction list's finishing cue sets the Rio's animation-finished flag, which its main AI
/// reads and clears only as it moves on; a wall or floor rebound does not clear it, so the finish of the opening
/// carries through the rebound (#2450).
/// </remarks>
[CompatibilityService("a Rio's animation finish waits for its main AI to move on")]
public sealed class RioAnimationFinish
{
    /// <summary>Whether a finish waits for the Rio's main AI.</summary>
    public bool IsRaised { get; private set; }

    /// <summary>The animation's finishing cue raises the finish.</summary>
    internal void Raise() => IsRaised = true;

    /// <summary>The Rio's main AI takes the finish as it moves on: whether one was raised.</summary>
    internal bool Take()
    {
        bool raised = IsRaised;
        IsRaised = false;
        return raised;
    }

    /// <summary>The Rio's main AI forgets a finish it does not use.</summary>
    internal void Forget() => IsRaised = false;
}
