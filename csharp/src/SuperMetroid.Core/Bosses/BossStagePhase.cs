namespace SuperMetroid.Core.Bosses;

/// <summary>
/// The stage a boss's body is in with the phase of it the body runs (#2560): one value, so no phase of another stage is
/// held beside the stage.
/// </summary>
/// <typeparam name="TStage">The boss's stages.</typeparam>
/// <param name="Stage">The stage, which names the type of its phase.</param>
internal abstract record BossStagePhase<TStage>(TStage Stage)
    where TStage : struct, Enum
{
    /// <summary>The body in <paramref name="Stage"/>, a stage that runs no phase.</summary>
    /// <param name="Stage">The stage.</param>
    internal sealed record Phaseless(TStage Stage) : BossStagePhase<TStage>(Stage);

    /// <summary>The body in <paramref name="Stage"/>, running <paramref name="Phase"/> of it.</summary>
    /// <typeparam name="TPhase">The stage's phase type.</typeparam>
    /// <param name="Stage">The stage.</param>
    /// <param name="Phase">The phase of the stage the body runs.</param>
    internal sealed record Of<TPhase>(TStage Stage, TPhase Phase) : BossStagePhase<TStage>(Stage)
        where TPhase : struct, Enum;
}
