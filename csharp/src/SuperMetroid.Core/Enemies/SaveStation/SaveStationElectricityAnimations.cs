using SuperMetroid.Core.Animation;
using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Rooms.RoomObjects.SaveStationElectricitySegment;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.SaveStation;

/// <summary>
/// The save-station electricity's twenty sweeps (#1296). Each frame is where the electricity's one band picture
/// (<see cref="EnemyProjectileFrameId.SaveStationElectricityBand"/>) is drawn from its origin (#2557), and each cue is an
/// <see cref="EnemyProjectileCommonCue"/>.
/// </summary>
internal static class SaveStationElectricityAnimations
{
    /// <summary>Every segment's beats: the cycle sweeps the band down the station, one place an update.</summary>
    internal static UnpausedAnimationSegments<SaveStationElectricitySegment, Offset16<RoomPixels>, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<SaveStationElectricitySegment, AnimationBeat<Offset16<RoomPixels>, EnemyProjectileCommonCue>.Unpaused[]>
    {
        [P.Cycle] =
        [
            .. Enumerable.Range(0, SaveStationElectricityDefinitions.SweepPlaces).Select(place => Show(Offset16<RoomPixels>.Of(0,
                SaveStationElectricityDefinitions.SweepStartPixelsBelow + (place * SaveStationElectricityDefinitions.SweepStepPixels)), 1)),
        ],
        [P.Deletion] =
        [
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
    });

    /// <summary>No animation starts at a segment: the electricity's one animation is started by name (#2061).</summary>
    internal static IReadOnlyDictionary<SaveStationElectricitySegment, Animation<SaveStationElectricitySegment>> StartingAt { get; } = new Dictionary<SaveStationElectricitySegment, Animation<SaveStationElectricitySegment>>();

    /// <summary>The electricity's discharge (#2061): twenty sweeps, then the deletion.</summary>
    internal static Animation<SaveStationElectricitySegment> Discharge { get; } = new([Segments.Repeated(P.Cycle, 20), Segments.Whole(P.Deletion)], AnimationEnd.Holds);

    /// <summary>Shows the band at <paramref name="placement"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<Offset16<RoomPixels>, EnemyProjectileCommonCue>.Unpaused Show(Offset16<RoomPixels> placement, int duration) => new AnimationBeat<Offset16<RoomPixels>, EnemyProjectileCommonCue>.Frame(placement, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<Offset16<RoomPixels>, EnemyProjectileCommonCue>.Unpaused Cue(EnemyProjectileCommonCue cue) => new AnimationBeat<Offset16<RoomPixels>, EnemyProjectileCommonCue>.Event(cue);
}
