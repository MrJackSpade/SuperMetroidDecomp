namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>What Kraid's death phases let through (<see cref="KraidBodyState"/>).</summary>
internal static partial class KraidDeathPhaseDefinitions
{
    /// <summary>
    /// The death phases Samus's shots still collide with him in: his death's start and fade, the update that shows his body
    /// and the one that starts his sinking.
    /// </summary>
    internal static readonly KraidDeathPhase[] ShotsCollide =
        [KraidDeathPhase.Initialize, KraidDeathPhase.FadeOut, KraidDeathPhase.ShowBody, KraidDeathPhase.StartSinking];
}
