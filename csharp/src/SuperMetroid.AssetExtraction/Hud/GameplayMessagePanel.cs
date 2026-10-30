using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>One large-panel title and four-row tile template; button substitution and message timing are supplied by compiled gameplay behavior.</summary>
public sealed record GameplayMessagePanel
{
    /// <summary>Nonempty title of at most 26 characters, using uppercase A..Z, space, hyphen, period or question mark; inserted into the first content row.</summary>
    public required string Title { get; init; }
    /// <summary>BG palette selector 0..7 used for compiled title glyphs and their space-filled interior; template rows retain their own packed attributes.</summary>
    public required int Palette { get; init; }
    /// <summary>Absolute zero-based tile column where the title starts, at least 3; the complete title must end before column 29.</summary>
    public required int Column { get; init; }
    /// <summary>Exactly 128 tiles in four 32-column rows. First-row columns 0..2 and 29..31 must be the transparent tile; columns 3..28 are replaced by the title at build time.</summary>
    public required BackgroundTile[] Template { get; init; }
}
