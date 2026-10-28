namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Physical record geometry shared by Samus body import and installed admission.</summary>
public static class SamusBodyDefinitionLayout
{
    /// <summary>The first record of SamusTopTiles_Set0 through _SetC, indexed by set; pixel payload lengths and artwork remain REQUIRED under SamusBodyTileTransfer.planar.</summary>
    internal static readonly int[] TopSetStarts = [0, 32, 62, 94, 111, 128, 145, 162, 182, 192, 371, 392, 416];

    /// <summary>The first record of SamusBottomTiles_Set0 through _SetA, indexed by set; pixel payload lengths and artwork remain REQUIRED under SamusBodyTileTransfer.planar.</summary>
    internal static readonly int[] BottomSetStarts = [208, 240, 272, 399, 296, 319, 343, 424, 366, 427, 369];

    /// <summary>Number of seven-byte body DMA definition records, stored one after another.</summary>
    public const int RecordCount = 435;
}
