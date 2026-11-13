using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Gunship.Artwork;

/// <summary>Cartridge values and helpers of <see cref="GunshipAnimations"/> that only verification reads.</summary>
internal static class GunshipFrameDefinitionsConstants
{
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(EnemyFrameId frame) => EnemyFrameCartridgeLookupTooling.Word(frame);
}
