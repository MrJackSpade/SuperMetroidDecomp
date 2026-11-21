namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>One Skultera occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Skultera species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class SkulteraEntity(SkulteraBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, SkulteraSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal SkulteraSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly SkulteraEnemyState state = SkulteraBehavior.InitializeSkultera(settings);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunSkulteraMainAi(Settings, Place, state, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => SkulteraBehavior.AdvanceSkulteraTypedAnimation(Place, state);
}
