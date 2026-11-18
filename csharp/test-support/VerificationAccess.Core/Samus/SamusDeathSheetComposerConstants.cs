using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Cartridge values and helpers of <see cref="SamusDeathSheetComposer"/> that only verification reads.</summary>
internal static class SamusDeathSheetComposerConstants
{
    /// <summary>The Samus spritemap explosion picture <paramref name="frame"/> shows facing <paramref name="facing"/>.</summary>
    internal static int ExplosionSpritemap(HorizontalDirection facing, SamusDeathExplosionFrame frame) =>
        SamusDeathSheetComposer.ExplosionSpritemap(facing, SamusDeathSheetDefinitions.Explosion.ToList().IndexOf(frame));
}
