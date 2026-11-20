namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The stage Kraid's body is in with the phase of it the body runs (#2560): one value for the stage and its phase.</summary>
/// <param name="Stage">The stage, which names the type of its phase.</param>
internal abstract record KraidBodyStep(KraidBodyStage Stage)
{
    /// <summary>The body in <paramref name="Stage"/>, running <paramref name="Phase"/> of it.</summary>
    /// <typeparam name="TPhase">The stage's phase type.</typeparam>
    /// <param name="Stage">The stage.</param>
    /// <param name="Phase">The phase of the stage the body runs.</param>
    internal sealed record InStage<TPhase>(KraidBodyStage Stage, TPhase Phase) : KraidBodyStep(Stage)
        where TPhase : struct, Enum;
}
