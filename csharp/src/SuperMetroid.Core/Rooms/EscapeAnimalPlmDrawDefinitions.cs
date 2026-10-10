namespace SuperMetroid.Core.Rooms;

/// <summary>Three-block vertical rescue-wall mutations, independent of editable art.</summary>
internal static class EscapeAnimalPlmDrawDefinitions
{
    /// <summary>$84:925B, DrawInst_CrittersEscapeBlock_0: three $8053 solid break frames.</summary>
    internal const ushort Frame0 = 0x925b;
    /// <summary>$84:9265, DrawInst_CrittersEscapeBlock_1: three $8054 solid break frames.</summary>
    internal const ushort Frame1 = 0x9265;
    /// <summary>$84:926F, DrawInst_CrittersEscapeBlock_2: three $8055 solid break frames.</summary>
    internal const ushort Frame2 = 0x926f;
    /// <summary>$84:9279, DrawInst_CrittersEscapeBlock_3: three $80FF blank solid words.</summary>
    internal const ushort Blank = 0x9279;

    /// <summary>Compiles a rescue-wall frame pointer into its three-block vertical terrain draw.</summary>
    /// <param name="pointer">Native draw-instruction address for one of the four wall break frames.</param>
    /// <param name="draw">Receives the vertical run of three solid or blank terrain words when recognized; otherwise receives the default value.</param>
    /// <returns><see langword="true"/> when <paramref name="pointer"/> identifies a compiled rescue-wall frame.</returns>
    internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        ushort word = pointer switch { Frame0 => 0x8053, Frame1 => 0x8054, Frame2 => 0x8055, Blank => 0x80ff, _ => 0 };
        if (word != 0)
        {
            draw = new(pointer, new RoomPlmShotBlockDrawDefinitions.Run[]
            {
                new(0x8003, new ushort[] { word, word, word }, 0, 0),
            });
            return true;
        }
        draw = default;
        return false;
    }
}
