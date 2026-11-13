using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>Cartridge values and helpers of <see cref="CeresDoorAnimations"/> that only verification reads.</summary>
internal static class CeresDoorFrameDefinitionsConstants
{
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(EnemyFrameId frame) => EnemyFrameCartridgeLookupTooling.Word(frame);
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa6;
    /// <summary>The Ridley-room door overlay $A6:A329 that Ridley's scene draws while it holds the door.</summary>
    internal const ushort RidleyPrivateOverlaySpritemap = 0xa329;
}
