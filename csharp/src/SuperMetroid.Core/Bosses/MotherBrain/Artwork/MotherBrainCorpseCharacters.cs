using SuperMetroid.Core.Bosses.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>One page of the installed Mother Brain corpse tiles.</summary>
/// <param name="Page">The page.</param>
internal sealed record MotherBrainCorpseCharacters(int Page) : SpriteArtworkSource
{
    /// <summary>Names the source for diagnostics.</summary>
    public override string ToString() => $"Mother Brain corpse page {Page}";
}
