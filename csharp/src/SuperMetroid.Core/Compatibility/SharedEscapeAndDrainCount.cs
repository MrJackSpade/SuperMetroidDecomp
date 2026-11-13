namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// One count shared by three of Samus's scripted sequences (#1552): Crystal Flash counts down the updates until it next
/// drains a unit of ammunition on it, Draygon's grip counts the directional presses toward Samus's escape on it, and the
/// suit pickup leaves its stage's place in the stage order on it at its setup and after every stage call. Crystal Flash
/// starts its countdown at ten and a Draygon grab clears the count, but each then continues from whatever the count holds:
/// a Crystal Flash whose drain the suit pickup suspended counts down from the stage the pickup left, not from its own
/// countdown, and the escape test reads a count another sequence left (one wrapped to 32768 or more reads as below the
/// target).
/// </summary>
/// <remarks>
/// <para>
/// Consulted by Crystal Flash's ammunition drain on every update of its main phase, by the escape handler on every counted
/// press while Draygon holds Samus, and written by the suit pickup's stage calls. Lifetime: Samus's; a new Samus starts at
/// zero.
/// </para>
/// <para>
/// Evidence: the original game keeps Crystal Flash's ammunition decrement timer, Draygon's escape press counter and the
/// suit pickup's stage in one working word, which none of the three reloads from its own state; the suit pickup carryover
/// check reads the count after every stage call.
/// </para>
/// </remarks>
[CompatibilityService("Crystal Flash's ammunition countdown, Draygon's escape presses and the suit pickup's stage are one count")]
public sealed class SharedEscapeAndDrainCount
{
    /// <summary>The count, 0 through 65535, a wrapped countdown kept exactly.</summary>
    private int Count { get; set; }

    /// <summary>The count as Crystal Flash's ammunition countdown reads it.</summary>
    public WrappingCountdown AmmoCountdown => WrappingCountdown.Start(Count);

    /// <summary>The count as Draygon's escape handler reads it: the presses counted toward the escape, 0 through 65535.</summary>
    public int EscapePresses => Count;

    /// <summary>Restarts Crystal Flash's ammunition countdown.</summary>
    /// <param name="updates">The updates until the next unit is drained.</param>
    public void StartAmmoCountdown(int updates) => Count = WrappingCountdown.Start(updates).RemainingFrames;

    /// <summary>Counts Crystal Flash's ammunition countdown down by one update, wrapping past zero, and returns the step.</summary>
    public CountdownStep CountDownAmmo()
    {
        CountdownStep step = AmmoCountdown.Decrement();
        Count = step.Remaining.RemainingFrames;
        return step;
    }

    /// <summary>Clears the escape presses, as Draygon's grab does.</summary>
    public void ClearEscapePresses() => Count = 0;

    /// <summary>Counts one escape press, continuing from zero past 65535.</summary>
    public void CountEscapePress() => Count = WrappingCounter.IncrementCount(Count);

    /// <summary>The suit pickup leaves its stage on the count.</summary>
    /// <param name="stage">The stage's place in the stage order, from zero.</param>
    public void LeaveSuitPickupStage(int stage) => Count = Ensure.BetweenInclusive(0, ushort.MaxValue, stage);
}
