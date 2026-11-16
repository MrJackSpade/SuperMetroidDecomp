using SuperMetroid.Core.Earthquakes;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// An ordinary Ceres door (#1953): it opens as Samus comes near and closes behind her, and keeps the station's escape
/// quake going through the foreground, the background and the enemies.
/// </summary>
/// <param name="doors">The room's owner of the Ceres doors and walls.</param>
/// <param name="slot">The slot the door occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="opening">The animation the door starts in, for the way it faces.</param>
internal sealed class CeresDoorEntity(CeresDoorBehavior doors, RoomEnemySlot slot, EnemyCommonParticipation common, CeresDoorSegment opening)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => doors.Start(Slot, opening);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => doors.EscapeQuake.Keep(EarthquakeReach.ForegroundBackgroundAndEnemies);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => doors.AdvanceAnimation(Slot, context.Samus);
}
