using SuperMetroid.Core.Programs;
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
    internal readonly record struct Draw(MotherBrainEscapeGateDraw Pointer)
    {
        /// <summary>
        /// $84:9473..9496: open uses blank FF; half adds end caps 30F; closed
        /// fills the middle with tile 2E8, vertically flipped for the upper half.
        /// Collision nibble 8 is unchanged even for the visually open frame.
        /// </summary>
        internal ushort WordAt(int row)
        {
            if ((uint)row >= 4) throw new ArgumentOutOfRangeException(nameof(row));
            int tile = Pointer == MotherBrainEscapeGateDraw.Open ? 0xff : row is 0 or 3 ? 0x30f :
                Pointer == MotherBrainEscapeGateDraw.HalfClosed ? 0xff : 0x2e8 | (row == 1 ? 0x800 : 0);
            return (ushort)(0x8000 | tile);
        }
    }

    /// <summary>Recognizes one of the three gate draw pointers; other PLM draw families are left unclaimed.</summary>
    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        // Pointers outside the three gate images belong to other draw families.
        bool owned = Enum.IsDefined((MotherBrainEscapeGateDraw)pointer);
        draw = owned ? new((MotherBrainEscapeGateDraw)pointer) : default;
        return owned;
    }

    /// <summary>Materializes the open, half-closed, and closed gate images in native pointer order.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (MotherBrainEscapeGateDraw pointer in Enum.GetValues<MotherBrainEscapeGateDraw>())
            {
                TryGet((ushort)pointer, out var draw);
                yield return draw;
            }
        }
    }

    /// <summary>Builds a four-cell artwork draw list for a known gate pointer; gameplay computes the cells directly.</summary>
    internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        list = default;
        if (!TryDescribe(pointer, out var draw)) return false;
        var words = new ushort[4];
        for (int row = 0; row < words.Length; row++) words[row] = draw.WordAt(row);
        list = new(pointer, new RoomPlmShotBlockDrawDefinitions.Run[] { new(0x8004, words, new(0, 0)) });
        return true;
    }

    /// <summary>Maps a native gate draw pointer to its stable authored visual identifier.</summary>
    internal static string VisualId(ushort pointer) =>
        ClosedNativeWords.Decode<MotherBrainEscapeGateDraw>(pointer, "Mother Brain escape-gate draw") switch
        {
            MotherBrainEscapeGateDraw.Open => "open",
            MotherBrainEscapeGateDraw.HalfClosed => "half-closed",
            MotherBrainEscapeGateDraw.Closed => "closed",
            _ => throw new InvalidOperationException(
                $"Undefined Mother Brain escape-gate draw ${pointer:X4}."),
        };

    /// <summary>Finds a gate artwork draw list by its exact, case-sensitive visual identifier.</summary>
    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmShotBlockDrawDefinitions.FindByVisualId(All, VisualId, id, out list);

}
