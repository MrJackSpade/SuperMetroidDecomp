using SuperMetroid.Core;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The shootable reactions a Wave shot's horizontal scan sets off in a room where it reads halfway through a block (#1650):
/// for each such read whose straddled terrain collides as shootable, the collision kind it reads and the reaction of the
/// block the read starts in, whose behaviour the reaction takes and at which its room object spawns. Only a shot whose top
/// edge lies above the room's ceiling makes such a read (<see cref="WaveScanRead"/>), and only in a room whose rows the
/// scan's walk carries past the end of its cycle; there the read can set off a shot block over terrain installed as solid,
/// as Crocomire's arena does at blocks (99, 1) and (100, 1). The import's private knowledge: nothing installs it.
/// </summary>
/// <remarks>
/// <para>
/// The game's scan (<c>$94:A352</c>) rejects a shot whose position lies above the room's top or past its last screen, or
/// whose rows span sixteen or more; it takes the row of the shot's top edge within the cycle of 256 and the column of its
/// leading edge, and steps down one row for each row the shot spans. A shot above the room's top whose top edge lies
/// above the ceiling has a position of 0 through 254 and a radius larger than its position; the radius is one byte of the
/// shot's animation, whatever its list (the Murder Beam's lists read other data as their radii), so every such radius is
/// taken. Each read is the one <see cref="WaveScanRead.At"/> gives, and the straddled terrain the one the game's
/// <see cref="StraddledTerrain{TTerrain}"/> gives for the blocks the level holds.
/// </para>
/// <para>
/// A shot whose top edge lies in the room starts its walk in the room's own blocks, and no retail room is large enough
/// for that walk to reach the end of the cycle within sixteen rows, so such a shot reads only whole blocks, whose own
/// reactions the room's terrain already holds; a level large enough to break that is rejected.
/// </para>
/// <para>
/// Evidence: <c>$94:A352</c> multiplies the top edge's row by the room's width as eight-bit operands, doubles the sum
/// with the column into a sixteen-bit offset two bytes a block, and steps it by adding the room's width twice, the
/// second addition taking the first's carry; <c>$94:A1B5</c> reads the level word at that offset, which is unaligned
/// after such a carry, and takes the block index from the offset halved; <c>$94:9E55</c> and <c>$94:9E73</c> spawn the
/// shootable reaction the behaviour at that block index selects, at that block.
/// </para>
/// </remarks>
internal static class StraddledShotReactions
{
    /// <summary>The largest radius a shot's animation can give it: the radius is one byte.</summary>
    private const int LargestRadius = 255;

    /// <summary>The rows a shot may span for the scan to react to it: fewer than sixteen.</summary>
    private const int ScannedRowLimit = 16;

    /// <summary>The bytes a level holds for each block, which the scan counts its place in.</summary>
    private const int BytesPerBlock = 2;

    /// <summary>The bytes the scan's place counts before it continues from zero.</summary>
    private const int PlaceCycle = 65536;

    /// <summary>
    /// For each row a shot's top edge above the room's ceiling can lie in, as the scan numbers it, the most steps after
    /// the first its scan takes; the scan's reads for fewer steps are the first of these.
    /// </summary>
    private static readonly Dictionary<int, int> LongestScanByTopRow = ScansAboveCeiling();

    /// <summary>
    /// The shootable collision kinds and reactions a Wave shot's horizontal scan sets off in <paramref name="level"/> where
    /// it reads halfway through a block, the straddled terrain read through <paramref name="straddled"/>.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The level is large enough for a scan from inside it to reach the end of the scan's cycle, or a straddled read runs
    /// past the level's last block.
    /// </exception>
    internal static IReadOnlySet<RoomTerrainReaction> Of(InstalledRoomLevel level, StraddledTerrain<RoomTerrainBlock> straddled)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(straddled);
        int width = level.Size.X;
        int blocks = level.Size.Area;
        // The leading edge's column lies on a screen the room holds, so it may lie past the width of a room whose width is
        // not whole screens.
        int columns = (width + RoomGridConversions.BlocksPerScreen - 1) / RoomGridConversions.BlocksPerScreen * RoomGridConversions.BlocksPerScreen;
        if ((blocks + columns + (ScannedRowLimit * width)) * BytesPerBlock >= PlaceCycle)
        {
            throw new InvalidDataException(
                $"A {level.Size.X}x{level.Size.Y} level is large enough for a Wave shot's scan from inside it to reach the end of its cycle.");
        }

        var reactions = new HashSet<RoomTerrainReaction>();
        foreach ((int row, int longest) in LongestScanByTopRow)
        {
            for (int column = 0; column < columns; column++)
            {
                for (int step = 0; step <= longest; step++)
                {
                    WaveScanRead read = WaveScanRead.At(Position16<RoomBlocks>.Of(column, row), width, step);
                    // The scan reads nothing at a place past the level, and a whole block's own reaction is the terrain's.
                    if (!read.StraddlesNext || read.Block >= blocks)
                        continue;
                    if (read.Block + 1 >= blocks)
                        throw new InvalidDataException($"A Wave shot's straddled read at block {read.Block} runs past the {blocks}-block level.");
                    RoomCollisionType collision = straddled.Straddling(level.Foreground[read.Block], level.Foreground[read.Block + 1]).Collision;
                    if (collision is RoomCollisionType.ShootableAir or RoomCollisionType.ShootableBlock)
                        reactions.Add(new(collision, level.Behaviors[read.Block]));
                }
            }
        }
        return reactions;
    }

    /// <summary>
    /// Every row a shot's top edge above the room's ceiling lies in, as the scan numbers it, with the most steps after the
    /// first the scan takes for a shot whose top edge lies there: every position at or below the room's top whose radius
    /// is larger, among the shots the scan does not reject for spanning too many rows.
    /// </summary>
    private static Dictionary<int, int> ScansAboveCeiling()
    {
        var longest = new Dictionary<int, int>();
        for (int radius = 1; radius <= LargestRadius; radius++)
        {
            for (int position = 0; position < radius; position++)
            {
                int top = CoordinateAxis.Wrap(position - radius);
                int bottom = CoordinateAxis.Wrap(position + radius - 1);
                int span = CoordinateAxis.Wrap(bottom - (top - (top % RoomLevelPlane.BlockPixels))) / RoomLevelPlane.BlockPixels;
                if (span >= ScannedRowLimit)
                    continue;
                int row = top / RoomLevelPlane.BlockPixels;
                longest[row] = longest.TryGetValue(row, out int known) ? Math.Max(known, span) : span;
            }
        }
        return longest;
    }
}
