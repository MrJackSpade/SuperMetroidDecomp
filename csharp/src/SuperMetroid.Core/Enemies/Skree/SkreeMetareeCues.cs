namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>The cues Skree and Metaree share in their common animations.</summary>
internal static partial class SkreeMetareeCues
{
    /// <summary>What <c>Apply</c> does for each <see cref="SkreeMetareeCueKind"/> member, each member's own arm.</summary>
    private static IdentityTable<SkreeMetareeCueKind, Action<RoomEnemyPlace, Action>> ApplyForSkreeMetareeCueKind => field ??= new(new Dictionary<SkreeMetareeCueKind, Action<RoomEnemyPlace, Action>>
    {
        [SkreeMetareeCueKind.SetAttackReady] = static (place, markAttackReady) => markAttackReady(),
        [SkreeMetareeCueKind.ProcessOffScreen] = static (place, markAttackReady) => place.Processing = place.Processing.With(EnemyProcessing.OffScreen),
    });

    /// <summary>
    /// Carries out one cue. The wind-up rests immediately after readying the attack; main AI starts the dive on the
    /// following enemy frame, exactly matching the animation-to-AI hand-off.
    /// </summary>
    /// <param name="place">The enemy.</param>
    /// <param name="cue">The cue the animation published.</param>
    /// <param name="markAttackReady">Readies the species' attack.</param>
    internal static void Apply(RoomEnemyPlace place, SkreeMetareeCue cue, Action markAttackReady)
    {
        ApplyForSkreeMetareeCueKind[cue.Kind](place, markAttackReady);
    }
}
