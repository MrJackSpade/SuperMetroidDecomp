namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>
/// A Hibashi's invisible collision part (#2342): it takes no action of its own and holds its one-frame hitbox animation;
/// the flame listed before it moves and sizes it.
/// </summary>
/// <param name="place">The place the collision part occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class HibashiCollisionPartEntity(RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => HibashiBehavior.InitializeHibashiCollisionPart(Place);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => HibashiBehavior.AdvanceHibashiCollisionPartTypedAnimation(Place);
}
