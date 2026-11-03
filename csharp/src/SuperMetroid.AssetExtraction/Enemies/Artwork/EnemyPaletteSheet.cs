using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>One enemy's editable sixteen-colour palette, which the import colours its sheets in (#1512).</summary>
public sealed class EnemyPaletteSheet
{
    /// <summary>Owned RGB5 palette entries in OBJ color-index order.</summary>
    internal readonly RgbColor[] colors;
    /// <summary>Retains the validated sixteen-color array after JSON decoding.</summary>
    internal EnemyPaletteSheet(RgbColor[] colors) => this.colors = colors;

    /// <summary>The sixteen colours, colour zero first, which the import colours the enemy's sheets in where its graphics set loads them.</summary>
    public ReadOnlySpan<RgbColor> Colours => colors;
}
