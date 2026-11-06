namespace SuperMetroid.Core.Fx;

/// <summary>
/// Where the effect overlay shows the room effect's page (#1514), in overlay pixels: a liquid shows the whole page below
/// a band of nothing and repeats down every two pages' height; an atmosphere shows the page from below its first row and
/// repeats down every page. Both repeat across every page's width.
/// </summary>
public static class FxOverlayPageDefinitions
{
    /// <summary>The overlay's width before it repeats across.</summary>
    public const int RepeatWidth = 256;

    /// <summary>The overlay row a liquid's page's top edge lies on; the rows above it show nothing.</summary>
    public const int LiquidPageTop = 248;

    /// <summary>A liquid's overlay height before it repeats down: the band of nothing and the page below it.</summary>
    public const int LiquidRepeatHeight = 512;

    /// <summary>The page row an atmosphere's overlay begins with; the page's rows above it never show.</summary>
    public const int AtmosphereFirstPageRow = 8;

    /// <summary>An atmosphere's overlay height before it repeats down.</summary>
    public const int AtmosphereRepeatHeight = 256;
}
