using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The block shape of each of the twenty-two frames Mother Brain's fight and fake death draw into her room (#1608): the
/// wall fill, the escape door, the twelve rows of the mural behind her tank, the two solid rows nothing draws and the six
/// tube clears, and how each cell collides. What each frame shows is its installed picture (#1557).
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

    /// <summary>The mural's top or bottom row: thirteen slope blocks rightward from the origin.</summary>
    private static readonly RoomBlockShape MuralBorderRow = new(
    [
        new(new(0, 0), RoomCollisionType.Slope), new(new(1, 0), RoomCollisionType.Slope), new(new(2, 0), RoomCollisionType.Slope),
        new(new(3, 0), RoomCollisionType.Slope), new(new(4, 0), RoomCollisionType.Slope), new(new(5, 0), RoomCollisionType.Slope),
        new(new(6, 0), RoomCollisionType.Slope), new(new(7, 0), RoomCollisionType.Slope), new(new(8, 0), RoomCollisionType.Slope),
        new(new(9, 0), RoomCollisionType.Slope), new(new(10, 0), RoomCollisionType.Slope), new(new(11, 0), RoomCollisionType.Slope),
        new(new(12, 0), RoomCollisionType.Slope),
    ]);

    /// <summary>A row inside the mural: thirteen empty blocks rightward from the origin.</summary>
    private static readonly RoomBlockShape MuralInteriorRow = new(
    [
        new(new(0, 0), RoomCollisionType.Air), new(new(1, 0), RoomCollisionType.Air), new(new(2, 0), RoomCollisionType.Air),
        new(new(3, 0), RoomCollisionType.Air), new(new(4, 0), RoomCollisionType.Air), new(new(5, 0), RoomCollisionType.Air),
        new(new(6, 0), RoomCollisionType.Air), new(new(7, 0), RoomCollisionType.Air), new(new(8, 0), RoomCollisionType.Air),
        new(new(9, 0), RoomCollisionType.Air), new(new(10, 0), RoomCollisionType.Air), new(new(11, 0), RoomCollisionType.Air),
        new(new(12, 0), RoomCollisionType.Air),
    ]);

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

    /// <summary>The block shape each of the twenty-two frames draws.</summary>
    internal static readonly Dictionary<MotherBrainFakeDeathDraw, RoomBlockShape> Shapes = new()
    {
        [MotherBrainFakeDeathDraw.FillWall] = FillWall,
        [MotherBrainFakeDeathDraw.EscapeDoor] = EscapeDoor,
        [MotherBrainFakeDeathDraw.MuralRow1] = MuralBorderRow,
        [MotherBrainFakeDeathDraw.MuralRow2] = MuralInteriorRow,
        [MotherBrainFakeDeathDraw.MuralRow3] = MuralInteriorRow,
        [MotherBrainFakeDeathDraw.MuralRow4] = MuralInteriorRow,
        [MotherBrainFakeDeathDraw.MuralRow5] = MuralInteriorRow,
        [MotherBrainFakeDeathDraw.MuralRow6] = MuralInteriorRow,
        [MotherBrainFakeDeathDraw.MuralRow7] = MuralInteriorRow,
        [MotherBrainFakeDeathDraw.MuralRow8] = MuralInteriorRow,
        [MotherBrainFakeDeathDraw.MuralRow9] = MuralInteriorRow,
        [MotherBrainFakeDeathDraw.MuralRow10] = MuralInteriorRow,
        [MotherBrainFakeDeathDraw.MuralRow11] = MuralInteriorRow,
        [MotherBrainFakeDeathDraw.MuralRow12] = MuralBorderRow,
        [MotherBrainFakeDeathDraw.ClearCeilingBlock] = ClearedCeilingBlock,
        [MotherBrainFakeDeathDraw.ClearCeilingTube] = ClearedCeilingTube,
        [MotherBrainFakeDeathDraw.ClearBottomMiddleSideTube] = ClearedBottomMiddleSideTube,
        [MotherBrainFakeDeathDraw.ClearBottomMiddleTubes] = ClearedBottomMiddleTubes,
        [MotherBrainFakeDeathDraw.ClearBottomLeftTube] = ClearedBottomLeftTube,
        [MotherBrainFakeDeathDraw.ClearBottomRightTube] = ClearedBottomRightTube,
    };
}
