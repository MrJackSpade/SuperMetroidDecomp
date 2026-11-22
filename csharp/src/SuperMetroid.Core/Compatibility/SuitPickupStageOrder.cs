namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The suit pickup's light-beam stages in the order they run, whose place in that order, from zero, is the number the
/// pickup leaves where other sequences read it (#1928): in the shinespark's upward acceleration
/// (<see cref="SuitPickupSparkCarryover{TStage}"/>) and on the count Crystal Flash's drain and Draygon's escape share
/// (<see cref="SharedEscapeAndDrainCount{TStage}"/>).
/// </summary>
/// <remarks>
/// A rule with no state, created with each Samus and given to the two values the pickup overwrites. Evidence: the original
/// counted the pickup's stage up from zero in the working word those values share; the suit pickup carryover check reads
/// both values after every stage call of a Varia transformation.
/// </remarks>
/// <typeparam name="TStage">A suit pickup stage.</typeparam>
[CompatibilityService("the suit pickup leaves its stage's place in the stage order where the shinespark's acceleration and the shared escape and drain count read it")]
[EncodingOwner("the suit pickup stage's place in the stage order, which the values it overwrites hold")]
public sealed partial class SuitPickupStageOrder<TStage>
    where TStage : notnull
{
    /// <summary>The stages in the order they run.</summary>
    private readonly IReadOnlyList<TStage> _stages;

    /// <summary>Keeps the stages in the order they run.</summary>
    /// <param name="stages">Every stage once, in the order the pickup runs them.</param>
    /// <exception cref="ArgumentException">The order names no stage, or a stage twice.</exception>
    public SuitPickupStageOrder(IReadOnlyList<TStage> stages)
    {
        Ensure.NotNull(stages);
        if (stages.Count == 0 || stages.Distinct().Count() != stages.Count)
            throw new ArgumentException("The suit pickup's stage order names every stage once.", nameof(stages));
        _stages = stages;
    }

    /// <summary>The place of <paramref name="stage"/> in the stage order, from zero.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The order does not name the stage.</exception>
    public int Number(TStage stage)
    {
        for (int place = 0; place < _stages.Count; place++)
        {
            if (EqualityComparer<TStage>.Default.Equals(_stages[place], stage))
                return place;
        }
        throw new ArgumentOutOfRangeException(nameof(stage), stage, "The suit pickup's stage order does not name this stage.");
    }
}
