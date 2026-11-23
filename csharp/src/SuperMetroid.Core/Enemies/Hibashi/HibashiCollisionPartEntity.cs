namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>
/// A Hibashi's invisible collision part (#2342): it takes no action of its own and holds its one-frame hitbox animation;
/// the flame listed before it moves and sizes it.
/// </summary>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the collision part occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="state">What the enemy's setup built (#2554).</param>
internal sealed partial class HibashiCollisionPartEntity(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    HibashiCollisionPartState state)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyTypedAnimation
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the collision part occupies.</param>
    /// <param name="common">The shared participation.</param>
    internal HibashiCollisionPartEntity(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
        : this(occupant, place, common, HibashiBehavior.InitializeHibashiCollisionPart(place))
    {
    }

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly HibashiCollisionPartState state = state;

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => HibashiBehavior.AdvanceHibashiCollisionPartTypedAnimation(Place, state);
}
