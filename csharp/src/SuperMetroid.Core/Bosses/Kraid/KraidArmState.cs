namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's arm's state: the arm keeps nothing of its own beside the encounter, animation and phase timer every part
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
}
