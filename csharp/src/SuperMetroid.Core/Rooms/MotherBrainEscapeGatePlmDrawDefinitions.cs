namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The three physical four-block gate images chosen by the Mother Brain
/// escape-gate program. Level words remain immutable collision definitions;
/// visual block references can be separated for authoring independently.
/// </summary>
internal static class MotherBrainEscapeGatePlmDrawDefinitions
{
    /// <summary>Fully open vertical gate draw at $84:9473.</summary>
    internal const ushort Open = 0x9473;
    /// <summary>Half-closed vertical gate draw at $84:947F.</summary>
    internal const ushort HalfClosed = 0x947f;
    /// <summary>Fully closed vertical gate draw at $84:948B.</summary>
    internal const ushort Closed = 0x948b;

    /// <summary>Four vertical cells, always solid, selected by open/half/closed state.</summary>
    /// <param name="Pointer">Native draw-list address selecting the open, half-closed, or closed visual.</param>
    internal readonly record struct Draw(ushort Pointer)
    {
        /// <summary>
        /// $84:9473..9496: open uses blank FF; half adds end caps 30F; closed
        /// fills the middle with tile 2E8, vertically flipped for the upper half.
        /// Collision nibble 8 is unchanged even for the visually open frame.
        /// </summary>
        internal ushort WordAt(int row)
        {
            if ((uint)row >= 4) throw new IndexOutOfRangeException();
            int tile = Pointer == Open ? 0xff : row is 0 or 3 ? 0x30f :
                Pointer == HalfClosed ? 0xff : 0x2e8 | (row == 1 ? 0x800 : 0);
            return (ushort)(0x8000 | tile);
        }
    }

    /// <summary>Identifies one of the three native gate draws without materializing its cell words.</summary>
    /// <param name="pointer">Bank-$84 draw-list address to classify.</param>
    /// <param name="draw">Receives the gate visual descriptor when the address is supported.</param>
    /// <returns><see langword="true"/> when the pointer is the open, half-closed, or closed gate draw.</returns>
    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        bool owned = pointer is Open or HalfClosed or Closed;
        draw = owned ? new(pointer) : default;
        return owned;
    }

    /// <summary>Enumerates the open, half-closed, and closed gate drawings in native pointer order.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            for (int pointer = Open; pointer <= Closed; pointer += 12)
            {
                TryGet((ushort)pointer, out var draw);
                yield return draw;
            }
        }
    }

    // Temporary artwork DTOs; gameplay computes each cell directly.
    /// <summary>Builds the temporary four-cell artwork description for a supported native gate draw.</summary>
    /// <param name="pointer">Bank-$84 draw-list address to materialize.</param>
    /// <param name="list">Receives the draw rows and native pointer when the address is supported.</param>
    /// <returns><see langword="true"/> when the pointer identifies one of the three gate states.</returns>
    internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        list = default;
        if (!TryDescribe(pointer, out var draw)) return false;
        var words = new ushort[4];
        for (int row = 0; row < words.Length; row++) words[row] = draw.WordAt(row);
        list = new(pointer, new RoomPlmShotBlockDrawDefinitions.Run[] { new(0x8004, words, 0, 0) });
        return true;
    }

    /// <summary>Returns the stable authoring identifier for one of the three gate visual states.</summary>
    /// <param name="pointer">Native draw-list address whose visual identifier is requested.</param>
    /// <returns><c>open</c>, <c>half-closed</c>, or <c>closed</c>, according to the pointer.</returns>
    /// <exception cref="InvalidDataException">The pointer does not identify a supported gate draw.</exception>
    internal static string VisualId(ushort pointer) => pointer switch
    {
        Open => "open",
        HalfClosed => "half-closed",
        Closed => "closed",
        _ => throw new InvalidDataException(
            $"Mother Brain escape-gate draw ${pointer:X4} has no visual ID."),
    };

    /// <summary>Finds a gate draw by its stable, case-sensitive authoring identifier.</summary>
    /// <param name="id">Visual identifier to match using ordinal string comparison.</param>
    /// <param name="list">Receives the matching draw list when an identifier is found.</param>
    /// <returns><see langword="true"/> when <paramref name="id"/> names an available gate state.</returns>
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
