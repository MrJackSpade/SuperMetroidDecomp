using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Escape.Artwork;

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

        /// <summary>Installed color identities, including shared-color aliases.</summary>
        internal IEnumerable<PaletteFxColorKey> ColorKeys
        {
            get
            {
                foreach (PaletteFxColorKey key in self.colors.Keys)
                    if (!MaridiaEnvironmentalColorDefinitions.TrySource(key, out _) &&
                        !PlanetZebesTextColorDefinitions.TryCoordinates(key, out _, out _, out _) &&
                        !CrateriaLightningColorDefinitions.SurfaceProgram.Rows.TryLocate(key, out _, out _) &&
                        !CrateriaLightningColorDefinitions.DarkProgram.Rows.TryLocate(key, out _, out _) &&
                        !HeatPaletteColorDefinitions.TryCanonical(key, out _) &&
                        !LoadingPaletteColorDefinitions.TryCanonical(key, out _) &&
                        !PostCreditsIconGlarePaletteFxColorDefinitions.Rows.TryLocate(key, out _, out _) &&
                        !ZebesExplosionGunshipPaletteFxColorDefinitions.Rows.TryLocate(key, out _, out _) &&
                        !TourianStatueGreyPaletteFxColorDefinitions.Rows.TryLocate(key, out _, out _)) yield return key;
                foreach (var program in MaridiaEnvironmentalPaletteFxColorDefinitions.All)
                    foreach (PaletteFxColorKey key in Keys(program.Rows)) yield return key;
                foreach (var program in PlanetZebesTextPaletteFxColorDefinitions.All)
                    foreach (PaletteFxColorKey key in Keys(program.Rows)) yield return key;
                foreach (PaletteFxColorKey key in Keys(CrateriaLightningColorDefinitions.DarkProgram.Rows)) yield return key;
                foreach (PaletteFxColorKey key in Keys(CrateriaLightningColorDefinitions.SurfaceProgram.Rows)) yield return key;
                foreach (PaletteFxColorKey key in Keys(TourianStatueGreyPaletteFxColorDefinitions.Rows)) yield return key;
                foreach (PaletteFxColorKey key in Keys(ZebesExplosionGunshipPaletteFxColorDefinitions.Rows)) yield return key;
                foreach (PaletteFxColorKey key in Keys(PostCreditsIconGlarePaletteFxColorDefinitions.Rows)) yield return key;
                foreach (var program in SamusLoadingSuitPaletteFxColorDefinitions.All)
                    foreach (PaletteFxColorKey key in Keys(program.Rows)) yield return key;
                foreach (var program in PaletteFxHeatProgramMechanicsDefinitions.All)
                    foreach (PaletteFxColorKey key in Keys(program.Rows)) yield return key;
            }
        }
    }

    /// <summary>Every color of <paramref name="rows"/>, by row then color.</summary>
    private static IEnumerable<PaletteFxColorKey> Keys(PaletteFxColorRows rows)
    {
        for (int frame = 0; frame < rows.FrameCount; frame++)
            for (int index = 0; index < rows.ColorsPerFrame; index++)
                yield return rows.Color(frame, index);
    }
}
