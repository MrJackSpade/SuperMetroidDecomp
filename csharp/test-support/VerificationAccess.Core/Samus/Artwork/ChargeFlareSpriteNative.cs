using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Verification's native view of the charge-flare compositions: the bank-$93 address of each, and the original selector
/// table's fifty-four entries as the component, facing and picture each stands for.
/// </summary>
internal static class ChargeFlareSpriteNative
{
    /// <summary>The original selector table's entries.</summary>
    internal const int NativeSelectorCount = 54;

    /// <summary>Native views of the flare compositions.</summary>
    extension(ChargeFlareSpriteDefinitions)
    {
        /// <summary>The bank-$93 address of each flare composition, in family order.</summary>
        internal static IReadOnlyList<ushort> NativePointers => ProjectileSpriteNative.FlareAddresses;

        /// <summary>
        /// The composition the port shows for the component, facing and picture the original selector table's entry
        /// <paramref name="selector"/> stands for: thirty main-flare pictures, then six pictures each of the right slow,
        /// right fast, left slow and left fast sparks.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The selector is outside the table.</exception>
        internal static ProjectileSpriteId NativeSelector(int selector)
        {
            (ChargeFlareComponent component, bool facingLeft, int phase) = selector switch
            {
                >= 0 and < 30 => (ChargeFlareComponent.MainFlare, false, selector),
                >= 30 and < 36 => (ChargeFlareComponent.SlowSparks, false, selector - 30),
                >= 36 and < 42 => (ChargeFlareComponent.FastSparks, false, selector - 36),
                >= 42 and < 48 => (ChargeFlareComponent.SlowSparks, true, selector - 42),
                >= 48 and < NativeSelectorCount => (ChargeFlareComponent.FastSparks, true, selector - 48),
                _ => throw new ArgumentOutOfRangeException(nameof(selector), selector, "The flare's selector table has 54 entries."),
            };
            return ChargeFlarePictures.Of(component, facingLeft, phase)
                ?? throw new InvalidOperationException($"Flare selector {selector} shows no picture.");
        }
    }
}
