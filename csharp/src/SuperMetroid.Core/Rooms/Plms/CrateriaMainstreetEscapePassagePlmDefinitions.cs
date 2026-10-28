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

    /// <summary>The passage's one draw.</summary>
    internal enum Draw
    {
        /// <summary>Clears the two passage blocks.</summary>
        ClearPair,
    }

    /// <summary>The draw that clears the two passage blocks.</summary>
    internal static PlmFrameDraw<Draw> ClearPairDraw { get; } = PlmDraw.Of(Draw.ClearPair);

    /// <summary>Recognizes the escape passage draw pointer and supplies its two adjacent air blocks.</summary>
    internal static bool TryGetDraw(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        if (request != ClearPairDraw)
        {
            draw = default;
            return false;
        }
        draw = new(request, new RoomPlmShotBlockDrawDefinitions.Run[]
        {
            new(PlmRunDirection.Horizontal, new ushort[] { 0x00ff, 0x00ff }, new(0, 0)),
        });
        return true;
    }
}
