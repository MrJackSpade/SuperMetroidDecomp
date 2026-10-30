using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The codec of the cartridge's atmospheric effect slot word (#1470): the effect in the high byte, numbered in the
/// order of the cartridge's per-effect tables, and the animation frame in the low byte. The game holds the effect and
/// frame apart; this is the one place their packing is known, for imports and checks of native state.
/// </summary>
[EncodingOwner("Samus atmospheric effect slot word")]
internal static class SamusAtmosphericEffectWordCodec
{
    /// <summary>The bits of the word's animation frame, below its effect.</summary>
    private const int FrameBits = 8;

    /// <summary>The effects in the order the word's high byte numbers them.</summary>
    private static readonly SamusAtmosphericEffectType[] EffectOrder =
    [
        SamusAtmosphericEffectType.Inactive, SamusAtmosphericEffectType.FootstepSplash,
        SamusAtmosphericEffectType.AlternateFootstepSplash, SamusAtmosphericEffectType.DivingSplash,
        SamusAtmosphericEffectType.LavaSurfaceDamage, SamusAtmosphericEffectType.Bubbles, SamusAtmosphericEffectType.Dust,
        SamusAtmosphericEffectType.AlternateDust,
    ];

    /// <summary>The effect a slot word records.</summary>
    /// <param name="word">An atmospheric effect slot word.</param>
    /// <exception cref="InvalidDataException">The high byte names no effect.</exception>
    public static SamusAtmosphericEffectType DecodeType(int word) =>
        (word >> FrameBits) is var position and >= 0 && position < EffectOrder.Length
            ? EffectOrder[position]
            : throw new InvalidDataException($"Atmospheric effect word ${word:X4} names no effect.");

    /// <summary>The animation frame a slot word records.</summary>
    /// <param name="word">An atmospheric effect slot word.</param>
    public static int DecodeFrame(int word) => word & ((1 << FrameBits) - 1);
}
