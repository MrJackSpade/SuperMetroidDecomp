using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// A wall of Ceres Ridley's getaway chamber, beside its turning floor (#1953). It only holds its place: the getaway scene
/// shows the chamber's walls (#1545), and its animation makes it ignore Samus.
/// </summary>
/// <param name="doors">The room's owner of the Ceres doors and walls.</param>
/// <param name="place">The place the wall occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="side">The animation the wall starts in, for the side of the chamber it stands on.</param>
internal sealed class CeresGetawayChamberWallEntity(CeresDoorBehavior doors, RoomEnemyPlace place, EnemyCommonParticipation common, CeresDoorSegment side)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The door's or wall's animation, which it starts as it is built (#2554).</summary>
    internal CeresDoorAnimationState State { get; } = doors.Start(place, side);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame)
    {
        // The wall only holds its place.
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => doors.AdvanceAnimation(Place, State, context.Samus);
}
