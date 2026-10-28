using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Pages of one installed Mother Brain special sprite sheet (legs, Baby, attack, exploded door).</summary>
/// <param name="Sheet">The sheet's index in <see cref="MotherBrainSpecialSpriteArtworkDefinitions.All"/>.</param>
/// <param name="Page">The first page uploaded.</param>
/// <param name="ByteCount">Bytes uploaded.</param>
internal sealed record MotherBrainSpecialSpriteCharacters(int Sheet, int Page, int ByteCount) : SpriteArtworkSource
{
    /// <summary>Names the source for diagnostics.</summary>
    public override string ToString() => $"{MotherBrainSpecialSpriteArtworkDefinitions.All[Sheet].FileName} page {Page}, {ByteCount} bytes";
}
