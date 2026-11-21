namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The sequence the game runs with the step of it the sequence is at (#2560): one value, so no sequence's step exists
/// while another sequence runs.
/// </summary>
/// <param name="Phase">The sequence's phase.</param>
internal abstract partial record SequenceStep(IGamePhase Phase)
{
    /// <summary>A sequence made of one step.</summary>
    /// <param name="Phase">The sequence's phase.</param>
    internal sealed partial record Stepless(IGamePhase Phase) : SequenceStep(Phase);

    /// <summary>A sequence at <paramref name="Step"/>.</summary>
    /// <typeparam name="TStep">The sequence's steps.</typeparam>
    /// <param name="Phase">The sequence's phase.</param>
    /// <param name="Step">The step it is at.</param>
    internal sealed partial record Of<TStep>(IGamePhase Phase, TStep Step) : SequenceStep(Phase)
        where TStep : struct, Enum;
}
