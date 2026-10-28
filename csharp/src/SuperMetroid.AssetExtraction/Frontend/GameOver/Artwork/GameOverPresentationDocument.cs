using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.GameOver.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;

/// <summary>Authored game-over JSON: the layout the game binds and the actor compositions the game-over sheet is composed from (#1463).</summary>
/// <remarks>Init-only properties retain caller-owned mutable arrays and dictionaries; the stream loader parses its own independent document.</remarks>
public sealed record GameOverPresentationDocument : GameOverLayoutDocument
{
    /// <summary>Exactly eight named sprite compositions: three Baby frames, the egg, and four missile-cursor frames.</summary>
    [JsonPropertyOrder(3)]
    public required Dictionary<string, SpriteVisualPart[]> Sprites { get; init; }
}
