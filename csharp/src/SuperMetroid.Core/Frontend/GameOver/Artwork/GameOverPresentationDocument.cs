using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.Core.Frontend.GameOver.Artwork;

/// <summary>Authored game-over JSON with background cells, actor compositions, Baby palette phases, and screen layout.</summary>
/// <remarks>Init-only properties retain caller-owned mutable arrays and dictionaries; the stream loader parses its own independent document.</remarks>
public sealed record GameOverPresentationDocument
{
    /// <summary>Schema version that must equal <see cref="GameOverPresentationDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>1024 row-major cells composing the complete 32-by-32 game-over background tilemap.</summary>
    public required MapPresentationCell[] Tilemap { get; init; }
    /// <summary>Exactly eight named sprite compositions: three Baby frames, the egg, and four missile-cursor frames.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Sprites { get; init; }
    /// <summary>Four named palette phases, each holding sixteen packed SNES BGR555 words in the range $0000-$7FFF.</summary>
    public required Dictionary<string, ushort[]> BabyPalettes { get; init; }
    /// <summary>Shared Baby and egg drawing origin in screen pixels, X=0-255 and Y=0-255.</summary>
    public required Position16<OamPixels> BabyAnchor { get; init; }
    /// <summary>Common horizontal missile-cursor position in screen pixels, 0-255.</summary>
    public required int CursorX { get; init; }
    /// <summary>Vertical missile-cursor position for the Yes answer in screen pixels, 0-255.</summary>
    public required int YesCursorY { get; init; }
    /// <summary>Vertical missile-cursor position for the No answer in screen pixels, 0-255.</summary>
    public required int NoCursorY { get; init; }
    /// <summary>OBJ palette selector 0-7 for Baby sprites; does not change the fixed destination of Baby palette animation writes.</summary>
    public required int BabyPalette { get; init; }
    /// <summary>OBJ palette selector 0-7 for the egg composition.</summary>
    public required int EggPalette { get; init; }
    /// <summary>OBJ palette selector 0-7 for missile-cursor compositions.</summary>
    public required int CursorPalette { get; init; }
    /// <summary>Duration 1-65535 in menu updates for each looping missile-cursor frame.</summary>
    public required int CursorFrameDuration { get; init; }
}
