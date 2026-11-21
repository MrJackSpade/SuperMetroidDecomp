namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>One Namihe occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Fune/Namihe species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class NamiheEntity(FuneNamiheBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, NamiheSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal NamiheSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly NamiheEnemyState state = FuneNamiheBehavior.InitializeNamihe(settings, place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) =>
        FuneNamiheBehavior.RunNamiheMain(Settings, Settings, Place, state, Ensure.NotNull(frame).Samus);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceFuneNamiheTypedAnimation<NamiheEnemyState>(Occupant, Settings, Place, state);
}
