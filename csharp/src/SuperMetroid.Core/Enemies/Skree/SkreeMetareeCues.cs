namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>The cues Skree and Metaree share in their common animation programs.</summary>
internal static class SkreeMetareeCues
{
    /// <summary>
    /// Carries out one cue. The wind-up sleeps immediately after marking the attack ready; main AI consumes the flag on
    /// the following enemy frame and starts the dive, exactly matching the native instruction/AI hand-off.
    /// </summary>
    /// <param name="slot">The enemy.</param>
    /// <param name="cue">The cue the program published.</param>
    /// <param name="markAttackReady">Sets the species state's attack-ready flag.</param>
    /// <returns>Null: no cue continues at another program.</returns>
    internal static SkreeMetareeProgram? Apply(RoomEnemySlot slot, SkreeMetareeCue cue, Action markAttackReady)
    {
        switch (cue.Kind)
        {
            case SkreeMetareeCueKind.SetAttackReady:
                markAttackReady();
                break;
            case SkreeMetareeCueKind.ProcessOffScreen:
                slot.Properties = slot.Properties.With(EnemyProperties.ProcessOffScreen);
                break;
            case SkreeMetareeCueKind.StopProcessingOffScreen:
                slot.Properties = slot.Properties.Without(EnemyProperties.ProcessOffScreen);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(SkreeMetareeCueKind)} {cue.Kind}.");
        }
        return null;
    }
}
