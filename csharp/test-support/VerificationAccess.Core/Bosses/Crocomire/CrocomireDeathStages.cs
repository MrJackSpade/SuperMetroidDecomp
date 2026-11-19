namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Verification's way to name a stage of Crocomire's course (#2540): every stage of
/// <see cref="CrocomireDeathStageDefinitions.Stages"/> in order, and the stage that is the given one of its kind.
/// </summary>
internal static class CrocomireDeathStages
{
    /// <summary>Every stage of his course, in order, each at its place.</summary>
    internal static IEnumerable<ListStep<CrocomireDeathStage>> All =>
        Enumerable.Range(0, CrocomireDeathStageDefinitions.Stages.Count)
            .Select(place => ListStep<CrocomireDeathStage>.At(CrocomireDeathStageDefinitions.Stages, place));

    /// <summary>
    /// The <paramref name="occurrence"/>th stage of kind <typeparamref name="TStage"/>, counting from one in the course's
    /// order: the first <see cref="CrocomireDeathStage.Sink"/> is his first hop's, the bridge's being its own stage.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">His course has fewer stages of that kind.</exception>
    internal static ListStep<CrocomireDeathStage> Nth<TStage>(int occurrence) where TStage : CrocomireDeathStage
    {
        ListStep<CrocomireDeathStage>[] matches = [.. All.Where(stage => stage.Entry is TStage)];
        return occurrence >= 1 && occurrence <= matches.Length
            ? matches[occurrence - 1]
            : throw new ArgumentOutOfRangeException(nameof(occurrence), occurrence,
                $"Crocomire's course has {matches.Length} {typeof(TStage).Name} stages.");
    }

    /// <summary>The one stage of kind <typeparamref name="TStage"/>.</summary>
    /// <exception cref="InvalidOperationException">His course has no stage of that kind, or more than one.</exception>
    internal static ListStep<CrocomireDeathStage> Only<TStage>() where TStage : CrocomireDeathStage =>
        All.Single(stage => stage.Entry is TStage);
}
