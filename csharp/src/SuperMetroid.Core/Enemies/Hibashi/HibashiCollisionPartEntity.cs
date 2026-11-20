namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>
/// A Hibashi's invisible collision part (#2342): it takes no action of its own and holds its one-frame hitbox animation;
/// the flame listed before it moves and sizes it.
/// </summary>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the collision part occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class HibashiCollisionPartEntity(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => HibashiBehavior.InitializeHibashiCollisionPart(Place);

    /// <summary>The collision part takes no action of its own; the flame moves it.</summary>
    /// <param name="frame">The enemy frame.</param>
    public void RunMainAi(IEnemyAiFrame frame)
    {
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => HibashiBehavior.AdvanceHibashiCollisionPartTypedAnimation(Place);
}
