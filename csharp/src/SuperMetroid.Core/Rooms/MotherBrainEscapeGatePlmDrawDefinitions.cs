using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>The three bank-$84 Mother Brain escape-gate draw lists.</summary>
internal enum MotherBrainEscapeGateDraw : ushort
{
    /// <summary>Fully open vertical gate draw at $84:9473.</summary>
    Open = 0x9473,
    /// <summary>Half-closed vertical gate draw at $84:947F.</summary>
    HalfClosed = 0x947f,
    /// <summary>Fully closed vertical gate draw at $84:948B.</summary>
    Closed = 0x948b,
}

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
            if ((uint)row >= 4) throw new IndexOutOfRangeException();
            int tile = Pointer == MotherBrainEscapeGateDraw.Open ? 0xff : row is 0 or 3 ? 0x30f :
                Pointer == MotherBrainEscapeGateDraw.HalfClosed ? 0xff : 0x2e8 | (row == 1 ? 0x800 : 0);
            return (ushort)(0x8000 | tile);
        }
    }

    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        // Pointers outside the three gate images belong to other draw families.
        bool owned = Enum.IsDefined((MotherBrainEscapeGateDraw)pointer);
        draw = owned ? new((MotherBrainEscapeGateDraw)pointer) : default;
        return owned;
    }

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

    // Temporary artwork DTOs; gameplay computes each cell directly.
    internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        list = default;
        if (!TryDescribe(pointer, out var draw)) return false;
        var words = new ushort[4];
        for (int row = 0; row < words.Length; row++) words[row] = draw.WordAt(row);
        list = new(pointer, new RoomPlmShotBlockDrawDefinitions.Run[] { new(0x8004, words, 0, 0) });
        return true;
    }

    internal static string VisualId(ushort pointer) =>
        ClosedNativeWords.Decode<MotherBrainEscapeGateDraw>(pointer, "Mother Brain escape-gate draw") switch
        {
            MotherBrainEscapeGateDraw.Open => "open",
            MotherBrainEscapeGateDraw.HalfClosed => "half-closed",
            MotherBrainEscapeGateDraw.Closed => "closed",
            _ => throw new InvalidOperationException(
                $"Undefined Mother Brain escape-gate draw ${pointer:X4}."),
        };

    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList candidate in All)
        {
            if (string.Equals(id, VisualId(candidate.Pointer), StringComparison.Ordinal))
            {
                list = candidate;
                return true;
            }
        }
        list = default;
        return false;
    }

}
