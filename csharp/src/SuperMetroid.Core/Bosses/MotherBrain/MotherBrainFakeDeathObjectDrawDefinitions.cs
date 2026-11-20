using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The block shape of each frame Mother Brain's fight and fake death draw into her room (#1608): the
/// wall fill, the escape door, the rows of the mural behind her tank and the tube clears, and how each cell
/// collides. What each frame shows is its installed picture (#1557).
/// </summary>
internal static class MotherBrainFakeDeathObjectDrawDefinitions
{
    /// <summary>The wall fill: three solid blocks down a column, centred on the origin.</summary>
    private static readonly RoomBlockShape FillWall = new(
    [
        new(new(0, -1), RoomCollisionType.SolidBlock),
        new(new(0, 0), RoomCollisionType.SolidBlock),
        new(new(0, 1), RoomCollisionType.SolidBlock),
    ]);

    /// <summary>The escape door: the door block over three vertical extensions, beside a column of four empty blocks to its right.</summary>
    private static readonly RoomBlockShape EscapeDoor = new(
    [
        new(new(0, 0), RoomCollisionType.DoorBlock),
        new(new(0, 1), RoomCollisionType.VerticalExtension),
        new(new(0, 2), RoomCollisionType.VerticalExtension),
        new(new(0, 3), RoomCollisionType.VerticalExtension),
        new(new(1, 0), RoomCollisionType.Air),
        new(new(1, 1), RoomCollisionType.Air),
        new(new(1, 2), RoomCollisionType.Air),
        new(new(1, 3), RoomCollisionType.Air),
    ]);

    /// <summary>How many blocks wide the mural behind her tank is.</summary>
    private const int MuralWidth = 13;

    /// <summary>The mural's top or bottom row: <see cref="MuralWidth"/> slope blocks rightward from the origin.</summary>
    internal static readonly RoomBlockShape MuralBorderRow = RoomBlockShapes.Line(down: false, MuralWidth, RoomCollisionType.Slope);

    /// <summary>A row inside the mural: <see cref="MuralWidth"/> empty blocks rightward from the origin.</summary>
    internal static readonly RoomBlockShape MuralInteriorRow = RoomBlockShapes.Line(down: false, MuralWidth, RoomCollisionType.Air);

    /// <summary>A cleared ceiling block: the restored ceiling slope over one emptied block.</summary>
    private static readonly RoomBlockShape ClearedCeilingBlock = new(
    [
        new(new(0, 0), RoomCollisionType.Slope),
        new(new(0, 1), RoomCollisionType.Air),
    ]);

    /// <summary>A cleared ceiling tube: the restored ceiling slope over the four emptied blocks of the tube.</summary>
    private static readonly RoomBlockShape ClearedCeilingTube = new(
    [
        new(new(0, 0), RoomCollisionType.Slope),
        new(new(0, 1), RoomCollisionType.Air),
        new(new(0, 2), RoomCollisionType.Air),
        new(new(0, 3), RoomCollisionType.Air),
        new(new(0, 4), RoomCollisionType.Air),
    ]);

    /// <summary>A cleared lower middle side tube: three emptied blocks over the restored floor slope.</summary>
    private static readonly RoomBlockShape ClearedBottomMiddleSideTube = new(
    [
        new(new(0, 0), RoomCollisionType.Air),
        new(new(0, 1), RoomCollisionType.Air),
        new(new(0, 2), RoomCollisionType.Air),
        new(new(0, 3), RoomCollisionType.Slope),
    ]);

    /// <summary>The cleared lower middle tubes: two side-by-side columns of six emptied blocks, each over its restored floor slope.</summary>
    private static readonly RoomBlockShape ClearedBottomMiddleTubes = new(
    [
        new(new(0, 0), RoomCollisionType.Air),
        new(new(0, 1), RoomCollisionType.Air),
        new(new(0, 2), RoomCollisionType.Air),
        new(new(0, 3), RoomCollisionType.Air),
        new(new(0, 4), RoomCollisionType.Air),
        new(new(0, 5), RoomCollisionType.Air),
        new(new(0, 6), RoomCollisionType.Slope),
        new(new(1, 0), RoomCollisionType.Air),
        new(new(1, 1), RoomCollisionType.Air),
        new(new(1, 2), RoomCollisionType.Air),
        new(new(1, 3), RoomCollisionType.Air),
        new(new(1, 4), RoomCollisionType.Air),
        new(new(1, 5), RoomCollisionType.Air),
        new(new(1, 6), RoomCollisionType.Slope),
    ]);

    /// <summary>
    /// The cleared lower left tube: four emptied blocks over the restored floor slope, and one more emptied block to the
    /// right of the top, toward the room's centre.
    /// </summary>
    private static readonly RoomBlockShape ClearedBottomLeftTube = new(
    [
        new(new(0, 0), RoomCollisionType.Air),
        new(new(0, 1), RoomCollisionType.Air),
        new(new(0, 2), RoomCollisionType.Air),
        new(new(0, 3), RoomCollisionType.Air),
        new(new(0, 4), RoomCollisionType.Slope),
        new(new(1, 0), RoomCollisionType.Air),
    ]);

    /// <summary>
    /// The cleared lower right tube: four emptied blocks over the restored floor slope, and one more emptied block to the
    /// left of the top, toward the room's centre.
    /// </summary>
    private static readonly RoomBlockShape ClearedBottomRightTube = new(
    [
        new(new(0, 0), RoomCollisionType.Air),
        new(new(0, 1), RoomCollisionType.Air),
        new(new(0, 2), RoomCollisionType.Air),
        new(new(0, 3), RoomCollisionType.Air),
        new(new(0, 4), RoomCollisionType.Slope),
        new(new(-1, 0), RoomCollisionType.Air),
    ]);

    /// <summary>The block shape each frame other than the mural's rows draws.</summary>
    internal static readonly Dictionary<MotherBrainFakeDeathDraw, RoomBlockShape> Shapes = new()
    {
        [MotherBrainFakeDeathDraw.FillWall] = FillWall,
        [MotherBrainFakeDeathDraw.EscapeDoor] = EscapeDoor,
        [MotherBrainFakeDeathDraw.ClearCeilingBlock] = ClearedCeilingBlock,
        [MotherBrainFakeDeathDraw.ClearCeilingTube] = ClearedCeilingTube,
        [MotherBrainFakeDeathDraw.ClearBottomMiddleSideTube] = ClearedBottomMiddleSideTube,
        [MotherBrainFakeDeathDraw.ClearBottomMiddleTubes] = ClearedBottomMiddleTubes,
        [MotherBrainFakeDeathDraw.ClearBottomLeftTube] = ClearedBottomLeftTube,
        [MotherBrainFakeDeathDraw.ClearBottomRightTube] = ClearedBottomRightTube,
    };
}
