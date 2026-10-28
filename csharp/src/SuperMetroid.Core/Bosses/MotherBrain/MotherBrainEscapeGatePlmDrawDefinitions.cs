using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The three physical four-block gate images chosen by the Mother Brain
/// escape-gate program. Level words remain immutable collision definitions;
/// visual block references can be separated for authoring independently.
/// </summary>
internal static class MotherBrainEscapeGatePlmDrawDefinitions
{
    /// <summary>Four vertical cells, always solid, selected by open/half/closed state.</summary>
    internal readonly record struct Draw(MotherBrainEscapeGateDraw Frame)
    {
        /// <summary>
        /// $84:9473..9496: open uses blank FF; half adds end caps 30F; closed
        /// fills the middle with tile 2E8, vertically flipped for the upper half.
        /// Collision nibble 8 is unchanged even for the visually open frame.
        /// </summary>
        internal ushort WordAt(int row)
        {
            if ((uint)row >= 4) throw new ArgumentOutOfRangeException(nameof(row));
            int tile = Frame == MotherBrainEscapeGateDraw.Open ? 0xff : row is 0 or 3 ? 0x30f :
                Frame == MotherBrainEscapeGateDraw.HalfClosed ? 0xff : 0x2e8 | (row == 1 ? 0x800 : 0);
            return (ushort)(0x8000 | tile);
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

    /// <summary>Builds a four-cell artwork draw list for a known gate pointer; gameplay computes the cells directly.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        list = default;
        if (!TryDescribe(request, out var draw)) return false;
        var words = new ushort[4];
        for (int row = 0; row < words.Length; row++) words[row] = draw.WordAt(row);
        list = new(request, new RoomPlmShotBlockDrawDefinitions.Run[] { new(PlmRunDirection.Vertical, words, new(0, 0)) });
        return true;
    }

    /// <summary>Maps a native gate draw pointer to its stable authored visual identifier.</summary>
    internal static string VisualId(PlmDraw request) =>
        PlmDraw.FrameOf<MotherBrainEscapeGateDraw>(request, "Mother Brain escape-gate draw") switch
        {
            MotherBrainEscapeGateDraw.Open => "open",
            MotherBrainEscapeGateDraw.HalfClosed => "half-closed",
            MotherBrainEscapeGateDraw.Closed => "closed",
            _ => throw new InvalidOperationException(
                $"Undefined Mother Brain escape-gate draw {request}."),
        };

    /// <summary>Finds a gate artwork draw list by its exact, case-sensitive visual identifier.</summary>
    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmShotBlockDrawDefinitions.FindByVisualId(All, VisualId, id, out list);

}
