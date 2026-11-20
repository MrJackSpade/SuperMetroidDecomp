using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.SaveStation;

/// <summary>
/// The save-station electricity's typed animation: its discharge (#2061), each frame the place its one band picture is drawn at
/// (#2557).
/// </summary>
internal sealed class SaveStationElectricityAnimation()
    : EnemyProjectileTypedAnimation<SaveStationElectricitySegment, Offset16<RoomPixels>, EnemyProjectileCommonCue>(SaveStationElectricityAnimations.Discharge, EnemyProjectileCueRunners.Common<SaveStationElectricitySegment>())
{
    /// <summary>The electricity shows its band wherever its sweep places it.</summary>
    private protected override EnemyProjectileFrameId Present(Offset16<RoomPixels> frame) => EnemyProjectileFrameId.SaveStationElectricityBand;

    /// <summary>The band is drawn where the sweep's frame places it.</summary>
    private protected override Offset16<RoomPixels> Placement(Offset16<RoomPixels> frame) => frame;

    /// <summary>The segment the discharge starts at: its cycle.</summary>
    private protected override SaveStationElectricitySegment SegmentStartedAt(Animation<SaveStationElectricitySegment> started) =>
        Ensure.NotNull(started).Phases[0].Segment;

    /// <inheritdoc/>
    private protected override AnimationSegments<SaveStationElectricitySegment, Offset16<RoomPixels>, EnemyProjectileCommonCue> Segments => SaveStationElectricityAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<SaveStationElectricitySegment, Animation<SaveStationElectricitySegment>> StartingAt => SaveStationElectricityAnimations.StartingAt;
}
