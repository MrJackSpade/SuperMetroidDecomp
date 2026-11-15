using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The block shapes of the eight floor and ceiling plant draws (#1862): the plant's mouth, its object's block and the one
/// right of it and the two left of those, and the four-wide foliage one row toward the plant's body, up for a floor plant
/// and down for a ceiling one. The mouth's outer halves are solid; while idle its inner halves hold the special-air parent
/// and its horizontal extension, and chewing opens them. The foliage is spiked air. What each frame shows is its installed
/// picture (#1557).
/// </summary>
internal static class SamusEaterObjectDrawDefinitions
{
    /// <summary>An idle floor plant: its mouth waiting, the foliage below it.</summary>
    private static readonly RoomBlockShape FloorIdle = new(
    [
        new(new(0, 0), RoomCollisionType.SpecialAir), new(new(1, 0), RoomCollisionType.SolidBlock),
        new(new(-2, 0), RoomCollisionType.SolidBlock), new(new(-1, 0), RoomCollisionType.HorizontalExtension),
        new(new(-2, -1), RoomCollisionType.SpikeAir), new(new(-1, -1), RoomCollisionType.SpikeAir),
        new(new(0, -1), RoomCollisionType.SpikeAir), new(new(1, -1), RoomCollisionType.SpikeAir),
    ]);

    /// <summary>A chewing floor plant: its mouth's inner halves open.</summary>
    private static readonly RoomBlockShape FloorChewing = new(
    [
        new(new(0, 0), RoomCollisionType.Air), new(new(1, 0), RoomCollisionType.SolidBlock),
        new(new(-2, 0), RoomCollisionType.SolidBlock), new(new(-1, 0), RoomCollisionType.Air),
        new(new(-2, -1), RoomCollisionType.SpikeAir), new(new(-1, -1), RoomCollisionType.SpikeAir),
        new(new(0, -1), RoomCollisionType.SpikeAir), new(new(1, -1), RoomCollisionType.SpikeAir),
    ]);

    /// <summary>An idle ceiling plant: its mouth waiting, the foliage above it.</summary>
    private static readonly RoomBlockShape CeilingIdle = new(
    [
        new(new(0, 0), RoomCollisionType.SpecialAir), new(new(1, 0), RoomCollisionType.SolidBlock),
        new(new(-2, 0), RoomCollisionType.SolidBlock), new(new(-1, 0), RoomCollisionType.HorizontalExtension),
        new(new(-2, 1), RoomCollisionType.SpikeAir), new(new(-1, 1), RoomCollisionType.SpikeAir),
        new(new(0, 1), RoomCollisionType.SpikeAir), new(new(1, 1), RoomCollisionType.SpikeAir),
    ]);

    /// <summary>A chewing ceiling plant: its mouth's inner halves open.</summary>
    private static readonly RoomBlockShape CeilingChewing = new(
    [
        new(new(0, 0), RoomCollisionType.Air), new(new(1, 0), RoomCollisionType.SolidBlock),
        new(new(-2, 0), RoomCollisionType.SolidBlock), new(new(-1, 0), RoomCollisionType.Air),
        new(new(-2, 1), RoomCollisionType.SpikeAir), new(new(-1, 1), RoomCollisionType.SpikeAir),
        new(new(0, 1), RoomCollisionType.SpikeAir), new(new(1, 1), RoomCollisionType.SpikeAir),
    ]);

    /// <summary>The block shape each plant draw draws.</summary>
    internal static readonly IReadOnlyDictionary<SamusEaterDraw, RoomBlockShape> Shapes = new Dictionary<SamusEaterDraw, RoomBlockShape>
    {
        [SamusEaterDraw.FloorIdle] = FloorIdle,
        [SamusEaterDraw.FloorTeethPeeking] = FloorChewing,
        [SamusEaterDraw.FloorTeethBared] = FloorChewing,
        [SamusEaterDraw.FloorFangsOut] = FloorChewing,
        [SamusEaterDraw.CeilingIdle] = CeilingIdle,
        [SamusEaterDraw.CeilingTeethPeeking] = CeilingChewing,
        [SamusEaterDraw.CeilingTeethBared] = CeilingChewing,
        [SamusEaterDraw.CeilingFangsOut] = CeilingChewing,
    };
}
