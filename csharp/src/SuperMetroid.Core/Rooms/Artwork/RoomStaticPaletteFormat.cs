namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>One editable RGB5 palette file per distinct graphics-set color source.</summary>
public static class RoomStaticPaletteFormat
{
    /// <summary>The supported room-palette JSON schema version.</summary>
    public const int Version = 1;

    /// <summary>The number of RGB5 colors in one room graphics-set palette source.</summary>
    public const int ColorCount = RoomAssetRomData.GraphicsLayout.BackgroundPaletteByteCount / Bgr555.ByteCount;

    /// <summary>Builds the editable file name for a palette's 24-bit native source address.</summary>
    /// <param name="sourceAddress">The palette's 24-bit native source address.</param>
    /// <returns>A file name in the form <c>room-palette-XXXXXX.json</c>.</returns>
    public static string SourceFileName(int sourceAddress) => $"room-palette-{sourceAddress:X6}.json";
}
