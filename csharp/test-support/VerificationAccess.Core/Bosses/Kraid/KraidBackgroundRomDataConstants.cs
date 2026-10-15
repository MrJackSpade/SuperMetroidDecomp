
namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Cartridge values of <see cref="KraidBackgroundRomData"/> that only verification reads.</summary>
internal static class KraidBackgroundRomDataConstants
{
    /// <summary>
    /// First untouched word left by <c>$A7:AB19-$AB2C</c> after the lower stream is
    /// decompressed directly into the working tilemap.
    /// </summary>
    public const int PreservedLowerTailFirstWord = 0x0700;

    /// <summary>Provides the preserved lower-tail word used when checking Kraid background import fidelity.</summary>
    extension(KraidBackgroundRomData)
    {
        /// <inheritdoc cref="KraidBackgroundRomDataConstants.PreservedLowerTailFirstWord"/>
        internal static int PreservedLowerTailFirstWord => KraidBackgroundRomDataConstants.PreservedLowerTailFirstWord;
    }
}
