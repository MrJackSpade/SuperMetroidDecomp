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
    /// <summary>
    /// A waiting mouth: four blocks along its row from two left of the origin, its outer halves solid and its inner halves the
    /// special-air parent and its horizontal extension.
    /// </summary>
    private static readonly RoomBlockShape MouthWaiting = RoomBlockShapes.Moved(RoomBlockShapes.Line(Axis.Horizontal,
        [RoomCollisionType.SolidBlock, RoomCollisionType.HorizontalExtension, RoomCollisionType.SpecialAir, RoomCollisionType.SolidBlock]), new(-2, 0));

    /// <summary>A chewing mouth: its outer halves solid and its inner halves open.</summary>
    private static readonly RoomBlockShape MouthChewing = RoomBlockShapes.Moved(RoomBlockShapes.Line(Axis.Horizontal,
        [RoomCollisionType.SolidBlock, RoomCollisionType.Air, RoomCollisionType.Air, RoomCollisionType.SolidBlock]), new(-2, 0));

    /// <summary>A floor plant's foliage: the four spiked-air blocks below its mouth.</summary>
    private static readonly RoomBlockShape FloorFoliage = RoomBlockShapes.Rectangle(new(-2, -1), new(4, 1), RoomCollisionType.SpikeAir);

    /// <summary>A ceiling plant's foliage: the four spiked-air blocks above its mouth.</summary>
    private static readonly RoomBlockShape CeilingFoliage = RoomBlockShapes.Rectangle(new(-2, 1), new(4, 1), RoomCollisionType.SpikeAir);

    /// <summary>An idle floor plant: its mouth waiting, the foliage below it.</summary>
    private static readonly RoomBlockShape FloorIdle = RoomBlockShapes.Joined(MouthWaiting, FloorFoliage);

    /// <summary>A chewing floor plant: its mouth's inner halves open.</summary>
    private static readonly RoomBlockShape FloorChewing = RoomBlockShapes.Joined(MouthChewing, FloorFoliage);

    /// <summary>An idle ceiling plant: its mouth waiting, the foliage above it.</summary>
    private static readonly RoomBlockShape CeilingIdle = RoomBlockShapes.Joined(MouthWaiting, CeilingFoliage);

    /// <summary>A chewing ceiling plant: its mouth's inner halves open.</summary>
    private static readonly RoomBlockShape CeilingChewing = RoomBlockShapes.Joined(MouthChewing, CeilingFoliage);

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
