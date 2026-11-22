namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The gunship's bottom hull with its entrance pad (#2146): set up beside the top hull, then moved and animated with the
/// ship, with the shared reactions.
/// </summary>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the bottom hull occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
/// <param name="state">The piece's animation, which its setup began as the creation built it (#2554).</param>
internal sealed partial class GunshipBottomEntity(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, GunshipSpawnSettings settings,
    GunshipPieceState state)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyTypedAnimation
{

    /// <summary>The piece's animation, which its setup began as the enemy was built (#2554).</summary>
    private readonly GunshipPieceState ownState = Ensure.NotNull(state);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => GunshipBehavior.AdvanceGunshipAnimation(Place, ownState);
}
