namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's arm's state: the arm keeps nothing of its own beside the encounter, animation and phase wait every part
/// holds (<see cref="KraidPartState"/>). It runs no phase: it follows his body every update and rests otherwise (#2208).
/// </summary>
[ArchitectureReview("back-reference", "A part of Kraid's encounter (#2218): the encounter owns this part's state for the room and the part reads the encounter it joined at setup, so the two share one lifetime.")]
public sealed class KraidArmState : KraidAnimatedPartState
{
    /// <summary>Makes the arm of <paramref name="kraid"/>'s encounter.</summary>
    /// <param name="kraid">The encounter the arm is part of.</param>
    /// <param name="reference">Kraid's reference to the arm, which it joins as it sets up.</param>
    internal KraidArmState(KraidEnemyState kraid, KraidPartReference reference)
        : base(kraid, reference, KraidPartSegment.ArmRisingOrSinking)
    {
    }

    /// <summary>
    /// Whether the arm swings normally (#1886, #2301): its normal swing or that swing's pause started, the animations from
    /// which it slows once Kraid is below half health. An arm rising or sinking, already slow, dying or preparing a lunge
    /// does not; the arm starts rising.
    /// </summary>
    internal bool SwingsNormally { get; private set; }

    /// <summary>
    /// Whether the arm has yet to reach its pause (#1886, #2301): its normal swing has started, as an animation or as the
    /// pause plays on into it, and has not shown its last frame, which the swing marks. While that last frame shows, the
    /// arm already counts as paused, as it does through the pause and any later animation. Kraid's small-form lunge waits
    /// on it.
    /// </summary>
    internal bool YetToPause { get; private set; }

    /// <summary>The arm's normal swing begins, as its animation's cue marks: the arm has yet to reach its pause.</summary>
    internal void BeginSwing() => YetToPause = true;

    /// <summary>The arm's normal swing shows its last frame, as its animation's cue marks: the arm counts as paused.</summary>
    internal void ReachPause() => YetToPause = false;

    /// <inheritdoc/>
    private protected override void Started(KraidPartSegment segment)
    {
        SwingsNormally = KraidArmSwingDefinitions.NormalSwing.Contains(segment);
        // An animation starting with the normal swing has yet to reach the pause before its first frame shows; every other
        // one starts at the pause or away from the swing.
        YetToPause = KraidArmSwingDefinitions.BeforePause.Contains(segment);
    }
}
