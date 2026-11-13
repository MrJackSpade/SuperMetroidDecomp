namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Samus's breath bubbles and lava spray wait for the splashes, dust and spray that the game kept in the same
/// places as them (#1556). A breath bubble forms only once the effect in its place has run out: the previous bubble, the
/// right-hand particle of a landing's dust or splash, or the third particle of a lava spray; a landing on a surface that
/// shows no landing effect ends that wait at once. Lava spray starts again only once the effect in its leading place has
/// run out while it holds the spray up: the previous spray's first particle or a boosted run's leading footstep dust; a
/// water splash or a wet footstep taking that place ends the wait at once. The bubble's random draw and its sound, and the
/// spray's sound, happen only when the wait is over, so the waits decide when the shared random number advances and which
/// sounds are requested.
/// </summary>
/// <remarks>
/// <para>
/// Consulted when Samus's liquid handling tries a bubble (every 128th frame while her mouth is under water) and a spray
/// (every update below the lava or acid surface with her top above it); started and ended by the effects her liquid
/// handling, running footsteps and landings produce; advanced once on every update on which her effects animate, which is
/// her ordinary drawing pass (an elevator ride, a shinespark crash and her death leave her effects, and these waits,
/// standing). Each wait lasts as many such updates as the effect that starts it is shown, its delayed start included.
/// Lifetime: Samus's liquid state's; a dry room's setup and a new Samus clear both.
/// </para>
/// <para>
/// Evidence: the original game keeps Samus's atmospheric effects in four fixed places, each producer writing its own
/// places; the bubble producer tests that its place is empty, and the spray producer tests one bit of its leading
/// place's effect number, which the spray, the bubbles and both dusts set. The effects are cleared from their places only
/// by running out, by a landing that shows nothing (which clears the landing's two places), and by room setup.
/// </para>
/// </remarks>
[CompatibilityService("Samus's breath bubbles and lava spray wait for the splashes, dust and spray that shared their places to run out")]
public sealed class BreathBubbleAndLavaSprayWaits
{
    /// <summary>Updates until the effect holding up the next breath bubble runs out; zero when none does.</summary>
    public int BubbleWaitUpdates { get; private set => field = Ensure.AtLeastZero(value); }

    /// <summary>Updates until the effect holding up the next lava spray runs out; zero when none does.</summary>
    public int SprayWaitUpdates { get; private set => field = Ensure.AtLeastZero(value); }

    /// <summary>Whether a breath bubble may form.</summary>
    public bool BubbleCanForm => BubbleWaitUpdates == 0;

    /// <summary>Whether a lava spray may start.</summary>
    public bool SprayCanStart => SprayWaitUpdates == 0;

    /// <summary>Holds up breath bubbles while an effect taking the bubble's place is shown.</summary>
    /// <param name="updates">The updates the effect is shown, one or more.</param>
    public void HoldBubbles(int updates) => BubbleWaitUpdates = Positive(updates);

    /// <summary>Ends the bubbles' wait, as a landing that shows no effect does.</summary>
    public void ReleaseBubbles() => BubbleWaitUpdates = 0;

    /// <summary>Holds up lava spray while an effect that holds it up takes the spray's leading place.</summary>
    /// <param name="updates">The updates the effect is shown, one or more.</param>
    public void HoldSpray(int updates) => SprayWaitUpdates = Positive(updates);

    /// <summary>Ends the spray's wait, as a water splash or a wet footstep taking its leading place does.</summary>
    public void ReleaseSpray() => SprayWaitUpdates = 0;

    /// <summary>Counts one update of Samus's effects toward the end of each wait.</summary>
    public void Advance()
    {
        if (BubbleWaitUpdates > 0)
            BubbleWaitUpdates--;
        if (SprayWaitUpdates > 0)
            SprayWaitUpdates--;
    }

    /// <summary>Ends both waits, as room setup does.</summary>
    public void Clear()
    {
        BubbleWaitUpdates = 0;
        SprayWaitUpdates = 0;
    }

    /// <summary>A wait's length, which an effect that is shown at all makes one update or more.</summary>
    private static int Positive(int updates) =>
        updates > 0 ? updates : throw new ArgumentOutOfRangeException(nameof(updates), updates, "An effect is shown for one update or more.");
}
