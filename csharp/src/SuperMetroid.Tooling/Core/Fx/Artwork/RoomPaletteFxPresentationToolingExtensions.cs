using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Development-tool instance members of <see cref="RoomPaletteFxPresentation"/>.</summary>
internal static class RoomPaletteFxPresentationToolingExtensions
{
    /// <summary>Inspects installed palette identities for the development dependency audit.</summary>
    extension(RoomPaletteFxPresentation self)
    {
        /// <summary>
        /// The bank-$8D addresses of the installed color keys, for the development dependency auditor. This exposes
        /// identities only, not cartridge bytes or colors.
        /// </summary>
        internal IEnumerable<ushort> ColorPointers => self.ColorKeys.Select(PaletteFxRecordSources.ColorAddress);

        /// <summary>Every installed color identity.</summary>
        internal IEnumerable<PaletteFxColorKey> ColorKeys => self.colors.Keys;
    }
}
