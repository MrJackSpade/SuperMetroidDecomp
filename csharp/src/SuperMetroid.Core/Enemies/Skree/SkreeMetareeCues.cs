namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>The cues Skree and Metaree share in their common animations.</summary>
internal static class SkreeMetareeCues
{
    /// <summary>
    /// Carries out one cue. The wind-up rests immediately after marking the attack ready; main AI consumes the flag on
    /// the following enemy frame and starts the dive, exactly matching the animation-to-AI hand-off.
    /// </summary>
    /// <param name="place">The enemy.</param>
    /// <param name="cue">The cue the animation published.</param>
    /// <param name="markAttackReady">Sets the species state's attack-ready flag.</param>
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
