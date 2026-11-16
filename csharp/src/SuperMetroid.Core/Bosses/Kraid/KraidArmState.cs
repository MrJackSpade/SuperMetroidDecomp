namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's arm's state: the arm keeps nothing of its own beside the encounter, animation and phase timer every part
/// holds (<see cref="KraidPartState"/>). It runs no phase: it follows his body every update and rests otherwise (#2208).
/// </summary>
public sealed class KraidArmState : KraidPartState
{
    /// <summary>Makes the arm of <paramref name="kraid"/>'s encounter.</summary>
    /// <param name="kraid">The encounter the arm is part of.</param>
    internal KraidArmState(KraidEnemyState kraid)
        : base(kraid)
    {
    }
}
