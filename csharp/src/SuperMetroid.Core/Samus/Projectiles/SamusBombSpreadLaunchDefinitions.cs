using System.Diagnostics.CodeAnalysis;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Fixed Bomb Spread mechanics, not editable explosion artwork or animation.</summary>
internal static class SamusBombSpreadLaunchDefinitions
{
    /// <summary>The bit of a launch's horizontal speed word that sends it left.</summary>
    [SuppressMessage("Design", "SME6447", Justification =
        "The direction bit of a bomb's horizontal speed word, not a cartridge address.")]
    internal const int LeftwardBit = 0x8000;
    /// <summary>The 16.16 fraction of half a pixel, the centre bomb's extra rise.</summary>
    [SuppressMessage("Design", "SME6447", Justification =
        "The fraction of half a pixel in a 16.16 speed, not a cartridge address.")]
    internal const int HalfPixelFraction = 0x8000;
}
