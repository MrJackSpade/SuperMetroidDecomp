using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>One page of one installed Mother Brain special sprite sheet (legs, Baby, attack, exploded door) shown as a look.</summary>
/// <param name="Sheet">The sheet's index in <see cref="MotherBrainSpecialSpriteArtworkDefinitions.All"/>.</param>
/// <param name="Page">The page shown.</param>
internal sealed record MotherBrainSpecialSpriteCharacters(int Sheet, int Page) : SpriteArtworkSource
{
    /// <summary>Names the look for diagnostics.</summary>
    public override string ToString() => $"{MotherBrainSpecialSpriteArtworkDefinitions.All[Sheet].FileName} page {Page}";
}
