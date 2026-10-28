namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Native body-art allocations and transfer-row geometry, independent of chosen pixel content.</summary>
internal static class SamusBodyTransferDefinitions
{
    /// <summary>Bytes in one 4bpp OBJ tile, used to calculate transfer row sizes.</summary>
    internal const int TileBytes = 32;
    /// <summary>The 8-pixel tiles across one large 16-pixel Samus sprite part.</summary>
    internal const int LargePartTilesAcross = 16 / 8;
}
