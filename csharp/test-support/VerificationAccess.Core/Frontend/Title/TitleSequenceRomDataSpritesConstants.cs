using SuperMetroid.Core.Input;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Frontend.Title;

/// <summary>Cartridge values and helpers of <see cref="TitleSequenceRomData.Sprites"/> that only verification reads.</summary>
internal static class TitleSequenceRomDataSpritesConstants
{
        /// <summary>$8C:879D, TitleSequenceSpritemaps_SuperMetroidTitleLogo: bank-relative pointer to the static logo composition.</summary>
        public const ushort SuperMetroidLogo = 0x879d;
}
