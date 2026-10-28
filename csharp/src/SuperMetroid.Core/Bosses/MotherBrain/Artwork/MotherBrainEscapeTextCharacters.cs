using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>One page of the installed Mother Brain escape text.</summary>
/// <param name="Page">The page.</param>
internal sealed record MotherBrainEscapeTextCharacters(int Page) : SpriteArtworkSource
{
    /// <summary>Names the source for diagnostics.</summary>
    public override string ToString() => $"Mother Brain escape text page {Page}";
}
