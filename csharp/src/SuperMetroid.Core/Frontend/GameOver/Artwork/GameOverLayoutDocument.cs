using System.Text.Json.Serialization;
using SuperMetroid.Core.Map.Artwork;

namespace SuperMetroid.Core.Frontend.GameOver.Artwork;

/// <summary>
/// The game-over JSON's background cells, Baby palette phases and screen layout the game binds. The document the import
/// pipeline reads also holds the actor compositions the game-over sheet is composed from (#1463); each member keeps its
/// place in the document.
/// </summary>
/// <remarks>Init-only properties retain caller-owned mutable arrays and dictionaries; the stream loader parses its own independent document.</remarks>
public record GameOverLayoutDocument
{
    /// <summary>Schema version that must equal <see cref="GameOverPresentationDefinitions.Version"/>.</summary>
    [JsonPropertyOrder(1)]
    public required int Version { get; init; }
    /// <summary>1024 row-major cells composing the complete 32-by-32 game-over background tilemap.</summary>
    [JsonPropertyOrder(2)]
    public required MapPresentationCell[] Tilemap { get; init; }
    /// <summary>Four named palette phases, each holding sixteen packed SNES BGR555 words in the range $0000-$7FFF.</summary>
    [JsonPropertyOrder(4)]
    public required Dictionary<string, ushort[]> BabyPalettes { get; init; }
    /// <summary>Shared Baby and egg drawing origin in screen pixels, X=0-255 and Y=0-255.</summary>
    [JsonPropertyOrder(5)]
    public required Position16<SpritePixels> BabyAnchor { get; init; }
    /// <summary>Common horizontal missile-cursor position in screen pixels, 0-255.</summary>
    [JsonPropertyOrder(6)]
    public required int CursorX { get; init; }
    /// <summary>Vertical missile-cursor position for the Yes answer in screen pixels, 0-255.</summary>
    [JsonPropertyOrder(7)]
    public required int YesCursorY { get; init; }
    /// <summary>Vertical missile-cursor position for the No answer in screen pixels, 0-255.</summary>
    [JsonPropertyOrder(8)]
    public required int NoCursorY { get; init; }
    /// <summary>OBJ palette selector 0-7 for Baby sprites; does not change the fixed destination of Baby palette animation writes.</summary>
    [JsonPropertyOrder(9)]
    public required int BabyPalette { get; init; }
    /// <summary>OBJ palette selector 0-7 for the egg composition.</summary>
    [JsonPropertyOrder(10)]
    public required int EggPalette { get; init; }
    /// <summary>OBJ palette selector 0-7 for missile-cursor compositions.</summary>
    [JsonPropertyOrder(11)]
    public required int CursorPalette { get; init; }
    /// <summary>Duration 1-65535 in menu updates for each looping missile-cursor frame.</summary>
    [JsonPropertyOrder(12)]
    public required int CursorFrameDuration { get; init; }
}
