using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo.Artwork;

/// <summary>Cartridge values and helpers of <see cref="TorizoAnimations"/> that only verification reads.</summary>
internal static class TorizoFrameDefinitionsConstants
{
    /// <summary>The body's poses.</summary>
    internal const int PoseCount = 106;
    /// <summary>The bank holding the body's extended spritemaps.</summary>
    internal const byte Bank = 0xaa;
    /// <summary>The extended spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(EnemyFrameId frame) => EnemyFrameCartridgeLookupTooling.Word(frame);
}
