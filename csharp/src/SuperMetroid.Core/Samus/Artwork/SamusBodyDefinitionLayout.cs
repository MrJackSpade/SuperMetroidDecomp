namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Where each stock body set's half pictures start among the body's numbered half pictures.</summary>
public static class SamusBodyDefinitionLayout
{
    /// <summary>The first half picture of SamusTopTiles_Set0 through _SetC, indexed by set.</summary>
    internal static readonly int[] TopSetStarts = [0, 32, 62, 94, 111, 128, 145, 162, 182, 192, 371, 392, 416];

    /// <summary>The first half picture of SamusBottomTiles_Set0 through _SetA, indexed by set.</summary>
    internal static readonly int[] BottomSetStarts = [208, 240, 272, 399, 296, 319, 343, 424, 366, 427, 369];

    /// <summary>Number of numbered body half pictures, top and bottom sets together.</summary>
    public const int RecordCount = 435;
}
