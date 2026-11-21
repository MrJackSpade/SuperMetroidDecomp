namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>The cues Skree and Metaree share in their common animations.</summary>
internal static partial class SkreeMetareeCues
{
    /// <summary>
    /// Carries out one cue. The wind-up rests immediately after readying the attack; main AI starts the dive on the
    /// following enemy frame, exactly matching the animation-to-AI hand-off.
    /// </summary>
    /// <param name="place">The enemy.</param>
    /// <param name="cue">The cue the animation published.</param>
    /// <param name="markAttackReady">Readies the species' attack.</param>
    internal static void Apply(RoomEnemyPlace place, SkreeMetareeCue cue, Action markAttackReady)
    {
        switch (cue.Kind)
        {
            case SkreeMetareeCueKind.SetAttackReady:
                markAttackReady();
                break;
            case SkreeMetareeCueKind.ProcessOffScreen:
                place.Processing = place.Processing.With(EnemyProcessing.OffScreen);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(SkreeMetareeCueKind)} {cue.Kind}.");
        }
    }
}
