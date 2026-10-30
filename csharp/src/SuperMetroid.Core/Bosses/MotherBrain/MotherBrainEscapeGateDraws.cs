using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's escape-gate block draws: which PLM draws are the gate's, their cells from
/// <see cref="MotherBrainEscapeGatePlmDrawDefinitions"/>, and their editable artwork identities.
/// </summary>
internal static class MotherBrainEscapeGateDraws
{
    /// <summary>Four vertical cells, always solid, selected by open/half/closed state.</summary>
    internal readonly record struct Draw(MotherBrainEscapeGateDraw Frame)
    {
        /// <summary>The terrain block of the gate's cell at <paramref name="row"/>, top to bottom.</summary>
        internal RoomTerrainBlock BlockAt(int row)
        {
            if ((uint)row >= 4) throw new ArgumentOutOfRangeException(nameof(row));
            return MotherBrainEscapeGatePlmDrawDefinitions.Cells[Frame][row];
        }
    }

    /// <summary>Recognizes one of the three gate draw pointers; other PLM draw families are left unclaimed.</summary>
    internal static bool TryDescribe(PlmDraw request, out Draw draw)
    {
        // Pointers outside the three gate images belong to other draw families.
        bool owned = request is PlmFrameDraw<MotherBrainEscapeGateDraw>;
        draw = owned ? new(PlmDraw.FrameOf<MotherBrainEscapeGateDraw>(request, "MotherBrainEscapeGateDraw")) : default;
        return owned;
    }

    /// <summary>Materializes the open, half-closed, and closed gate images in native pointer order.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (MotherBrainEscapeGateDraw frame in Enum.GetValues<MotherBrainEscapeGateDraw>())
            {
                TryGet(PlmDraw.Of(frame), out var draw);
                yield return draw;
            }
        }
    }

    /// <summary>Builds a four-cell artwork draw list for a known gate pointer; gameplay reads the cells directly.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        list = default;
        if (!TryDescribe(request, out var draw)) return false;
        var blocks = new RoomTerrainBlock[4];
        for (int row = 0; row < blocks.Length; row++) blocks[row] = draw.BlockAt(row);
        list = new(request, new RoomPlmShotBlockDrawDefinitions.Run[] { new(PlmRunDirection.Vertical, blocks, new(0, 0)) });
        return true;
    }

    /// <summary>Maps a native gate draw pointer to its stable authored visual identifier.</summary>
    internal static string VisualId(PlmDraw request) =>
        MotherBrainEscapeGatePlmDrawDefinitions.VisualIds.TryGetValue(
            PlmDraw.FrameOf<MotherBrainEscapeGateDraw>(request, "Mother Brain escape-gate draw"), out string? id)
            ? id
            : throw new InvalidOperationException($"Undefined Mother Brain escape-gate draw {request}.");

    /// <summary>Finds a gate artwork draw list by its exact, case-sensitive visual identifier.</summary>
    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        ShotBlockPlmDraws.FindByVisualId(All, VisualId, id, out list);
}
