
namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Cartridge values of <see cref="SamusRenderingRomData.Body"/> that only verification reads.</summary>
internal static class SamusRenderingRomDataBodyConstants
{
    /// <summary><c>$92:945D</c>, base bottom-spritemap index for each pose.</summary>
    public const int BottomSpritemapBaseIndices = 0x92945d;
    /// <summary>Number of colors copied into Samus's OBJ palette.</summary>
    public const int SuitPaletteColorCount = 16;
    /// <summary><c>$92:9263</c>, base top-spritemap index for each pose.</summary>
    public const int TopSpritemapBaseIndices = 0x929263;

    /// <summary>Provides Samus body spritemap base indices and suit palette size for rendering checks.</summary>
    extension(SamusRenderingRomData.Body)
    {
        /// <inheritdoc cref="SamusRenderingRomDataBodyConstants.BottomSpritemapBaseIndices"/>
        internal static int BottomSpritemapBaseIndices => SamusRenderingRomDataBodyConstants.BottomSpritemapBaseIndices;
        /// <inheritdoc cref="SamusRenderingRomDataBodyConstants.SuitPaletteColorCount"/>
        internal static int SuitPaletteColorCount => SamusRenderingRomDataBodyConstants.SuitPaletteColorCount;
        /// <inheritdoc cref="SamusRenderingRomDataBodyConstants.TopSpritemapBaseIndices"/>
        internal static int TopSpritemapBaseIndices => SamusRenderingRomDataBodyConstants.TopSpritemapBaseIndices;
    }
}
