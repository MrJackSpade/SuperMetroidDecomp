namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Cartridge definitions for PLM $84:BB30, which clears the Crateria mainstreet escape
/// passage when the critters escaped. Room setup $8F:9194 spawns it during the escape.
/// </summary>
internal static class CrateriaMainstreetEscapePassagePlmDefinitions
{
    /// <summary>$8F:9198: the hardcoded spawn's block X.</summary>
    internal const int BlockX = 0x3d;
    /// <summary>$8F:9199: the hardcoded spawn's block Y.</summary>
    internal const int BlockY = 0x0b;

    /// <summary>$84:9253, <c>DrawInst_CrateriaMainStreetEscape</c>: two $00FF air blocks in a row.</summary>
    internal const ushort ClearPairDraw = 0x9253;

    /// <summary>Recognizes the escape passage draw pointer and supplies its two adjacent air blocks.</summary>
    internal static bool TryGetDraw(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        if (pointer != ClearPairDraw)
        {
            draw = default;
            return false;
        }
        draw = new(pointer, new RoomPlmShotBlockDrawDefinitions.Run[]
        {
            new(0x0002, new ushort[] { 0x00ff, 0x00ff }, new(0, 0)),
        });
        return true;
    }
}
