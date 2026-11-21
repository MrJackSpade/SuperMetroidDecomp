using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>The cartridge's spritemap table entries of Samus's suit explosion, which only verification reads.</summary>
internal static class SamusDeathSheetComposerConstants
{
    /// <summary>The spritemap table entry of the right-facing explosion's first frame; its later frames follow it.</summary>
    internal const int RightExplosionFirstEntry = 2076;

    /// <summary>The spritemap table entry of the left-facing explosion's first frame; its later frames follow it.</summary>
    internal const int LeftExplosionFirstEntry = 2085;

    /// <summary>The cartridge's spritemap table entry explosion picture <paramref name="frame"/> shows facing <paramref name="facing"/>.</summary>
    internal static int ExplosionSpritemap(HorizontalDirection facing, SamusDeathExplosionFrame frame) =>
        (facing == HorizontalDirection.Left ? LeftExplosionFirstEntry : RightExplosionFirstEntry) +
        SamusDeathSheetDefinitions.Explosion.ToList().IndexOf(frame);
}
