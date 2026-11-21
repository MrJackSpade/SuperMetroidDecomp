namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>One Fune occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Fune/Namihe species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class FuneEntity(FuneNamiheBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, FuneSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal FuneSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal FuneEnemyState State { get; } = FuneNamiheBehavior.InitializeFune(settings, settings, place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => FuneNamiheBehavior.RunFuneMain(Settings, Settings, Place, State);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceFuneNamiheTypedAnimation<FuneEnemyState>(Occupant, Settings, Place, State);
}
