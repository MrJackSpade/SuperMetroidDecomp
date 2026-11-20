namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The gunship's bottom hull with its entrance pad (#2146): set up beside the top hull, then moved and animated with the
/// ship, with the shared reactions.
/// </summary>
/// <param name="species">The Gunship species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the bottom hull occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="surroundings">The room, Samus, controller and camera the population load sets the enemy up in.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class GunshipBottomEntity(GunshipBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, EnemyPopulationSurroundings surroundings, GunshipSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyTypedAnimation
{
    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal GunshipSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal GunshipPieceState State { get; } = species.InitializeGunshipBottom(occupant, settings, place, surroundings.Samus);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => GunshipBehavior.AdvanceGunshipAnimation(Place, State);
}
