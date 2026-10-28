using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// Verification's bank-$8D view of palette-FX colours: every record colour's address, resolved through the import
/// pipeline's record sources, so the ROM oracles read, compare and look up colours by the words the cartridge stores.
/// </summary>
internal static class PaletteFxColorAddressAccess
{
    /// <summary>Every record colour, by its bank-$8D address.</summary>
    private static readonly Dictionary<ushort, PaletteFxColorKey> Keys = BuildKeys();

    /// <summary>Walks every record's written colours.</summary>
    private static Dictionary<ushort, PaletteFxColorKey> BuildKeys()
    {
        var keys = new Dictionary<ushort, PaletteFxColorKey>();
        for (int record = 0; record < PaletteFxRecordSourcesConstants.RecordCount; record++)
        {
            var frame = new PaletteFxFrame(record);
            int colors = PaletteFxRecordDefinitions.Layout(frame).Where(part => !part.Skip).Sum(part => part.Count);
            for (int color = 0; color < colors; color++)
            {
                var key = new PaletteFxColorKey(frame, color);
                keys.Add(PaletteFxRecordSources.ColorAddress(key), key);
            }
        }
        return keys;
    }

    /// <summary>The bank-$8D address of <paramref name="key"/>'s colour word.</summary>
    internal static ushort Address(PaletteFxColorKey key) => PaletteFxRecordSources.ColorAddress(key);

    /// <summary>The record colour at bank-$8D <paramref name="pointer"/>, when the word is one.</summary>
    internal static bool TryKeyAt(int pointer, out PaletteFxColorKey key) => Keys.TryGetValue(unchecked((ushort)pointer), out key);

    /// <summary>The record colour at bank-$8D <paramref name="pointer"/>.</summary>
    internal static PaletteFxColorKey KeyAt(int pointer) => TryKeyAt(pointer, out PaletteFxColorKey key)
        ? key
        : throw new InvalidDataException($"$8D:{pointer:X4} is no palette-FX record colour.");

    /// <summary><paramref name="colors"/> keyed by record colour.</summary>
    internal static Dictionary<PaletteFxColorKey, RgbColor> Keyed(IReadOnlyDictionary<ushort, RgbColor> colors) =>
        colors.ToDictionary(pair => KeyAt(pair.Key), pair => pair.Value);

    /// <summary>Colour sources by address.</summary>
    extension(IPaletteFxColorSource source)
    {
        /// <summary>The colour at bank-$8D <paramref name="pointer"/>; false when the word is no record colour.</summary>
        internal bool TryReadColor(int pointer, out RgbColor color)
        {
            color = RgbColor.Black;
            return TryKeyAt(pointer, out PaletteFxColorKey key) && source.TryReadColor(key, out color);
        }
    }

    /// <summary>Keyed colour tables by address.</summary>
    extension(IReadOnlyDictionary<PaletteFxColorKey, RgbColor> colors)
    {
        /// <summary>Whether the table holds the colour at bank-$8D <paramref name="pointer"/>.</summary>
        internal bool ContainsKey(ushort pointer) => TryKeyAt(pointer, out PaletteFxColorKey key) && colors.ContainsKey(key);

        /// <summary>The table's colour at bank-$8D <paramref name="pointer"/>.</summary>
        internal bool TryGetValue(ushort pointer, out RgbColor value)
        {
            value = RgbColor.Black;
            return TryKeyAt(pointer, out PaletteFxColorKey key) && colors.TryGetValue(key, out value);
        }

        /// <summary>The bank-$8D addresses of the table's colours.</summary>
        internal IEnumerable<ushort> Pointers => colors.Keys.Select(Address);
    }
}

/// <summary>Address views of <see cref="HeatPaletteColorDefinitions"/>.</summary>
internal static class HeatPaletteColorDefinitionsAddressAccess
{
    /// <summary>The keyed members by bank-$8D address.</summary>
    extension(HeatPaletteColorDefinitions)
    {
        /// <summary>TryCalculatedColor for the colour at bank-$8D <paramref name="pointer"/>, with colours keyed by address.</summary>
        internal static bool TryCalculatedColor(int pointer, IReadOnlyDictionary<ushort, RgbColor> colors, out RgbColor value)
        {
            value = RgbColor.Black;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) &&
                HeatPaletteColorDefinitions.TryCalculatedColor(key, PaletteFxColorAddressAccess.Keyed(colors), out value);
        }

        /// <summary>TryCalculatedColor for the colour at bank-$8D <paramref name="pointer"/>.</summary>
        internal static bool TryCalculatedColor(int pointer, IReadOnlyDictionary<PaletteFxColorKey, RgbColor> colors, out RgbColor value)
        {
            value = RgbColor.Black;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) && HeatPaletteColorDefinitions.TryCalculatedColor(key, colors, out value);
        }

        /// <summary>TryRedRamp for the colour at bank-$8D <paramref name="pointer"/>, with colours keyed by address.</summary>
        internal static bool TryRedRamp(int pointer, IReadOnlyDictionary<ushort, RgbColor> colors, out RgbColor value)
        {
            value = RgbColor.Black;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) &&
                HeatPaletteColorDefinitions.TryRedRamp(key, PaletteFxColorAddressAccess.Keyed(colors), out value);
        }

        /// <summary>TryRedRamp for the colour at bank-$8D <paramref name="pointer"/>.</summary>
        internal static bool TryRedRamp(int pointer, IReadOnlyDictionary<PaletteFxColorKey, RgbColor> colors, out RgbColor value)
        {
            value = RgbColor.Black;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) && HeatPaletteColorDefinitions.TryRedRamp(key, colors, out value);
        }

        /// <summary>TrySecondaryRedRamp for the colour at bank-$8D <paramref name="pointer"/>, with colours keyed by address.</summary>
        internal static bool TrySecondaryRedRamp(int pointer, IReadOnlyDictionary<ushort, RgbColor> colors, out RgbColor value)
        {
            value = RgbColor.Black;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) &&
                HeatPaletteColorDefinitions.TrySecondaryRedRamp(key, PaletteFxColorAddressAccess.Keyed(colors), out value);
        }

        /// <summary>TrySecondaryRedRamp for the colour at bank-$8D <paramref name="pointer"/>.</summary>
        internal static bool TrySecondaryRedRamp(int pointer, IReadOnlyDictionary<PaletteFxColorKey, RgbColor> colors, out RgbColor value)
        {
            value = RgbColor.Black;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) && HeatPaletteColorDefinitions.TrySecondaryRedRamp(key, colors, out value);
        }

        /// <summary>TryHighlightEndpoint for the colour at bank-$8D <paramref name="pointer"/>, with colours keyed by address.</summary>
        internal static bool TryHighlightEndpoint(int pointer, IReadOnlyDictionary<ushort, RgbColor> colors, out RgbColor value)
        {
            value = RgbColor.Black;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) &&
                HeatPaletteColorDefinitions.TryHighlightEndpoint(key, PaletteFxColorAddressAccess.Keyed(colors), out value);
        }

        /// <summary>TryHighlightEndpoint for the colour at bank-$8D <paramref name="pointer"/>.</summary>
        internal static bool TryHighlightEndpoint(int pointer, IReadOnlyDictionary<PaletteFxColorKey, RgbColor> colors, out RgbColor value)
        {
            value = RgbColor.Black;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) && HeatPaletteColorDefinitions.TryHighlightEndpoint(key, colors, out value);
        }

        /// <summary>TryMixedRamp for the colour at bank-$8D <paramref name="pointer"/>, with colours keyed by address.</summary>
        internal static bool TryMixedRamp(int pointer, IReadOnlyDictionary<ushort, RgbColor> colors, out RgbColor value)
        {
            value = RgbColor.Black;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) &&
                HeatPaletteColorDefinitions.TryMixedRamp(key, PaletteFxColorAddressAccess.Keyed(colors), out value);
        }

        /// <summary>TryMixedRamp for the colour at bank-$8D <paramref name="pointer"/>.</summary>
        internal static bool TryMixedRamp(int pointer, IReadOnlyDictionary<PaletteFxColorKey, RgbColor> colors, out RgbColor value)
        {
            value = RgbColor.Black;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) && HeatPaletteColorDefinitions.TryMixedRamp(key, colors, out value);
        }

        /// <summary>TrySharedRed for the colour at bank-$8D <paramref name="pointer"/>, with colours keyed by address.</summary>
        internal static bool TrySharedRed(int pointer, IReadOnlyDictionary<ushort, RgbColor> colors, out RgbColor value)
        {
            value = RgbColor.Black;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) &&
                HeatPaletteColorDefinitions.TrySharedRed(key, PaletteFxColorAddressAccess.Keyed(colors), out value);
        }

        /// <summary>TrySharedRed for the colour at bank-$8D <paramref name="pointer"/>.</summary>
        internal static bool TrySharedRed(int pointer, IReadOnlyDictionary<PaletteFxColorKey, RgbColor> colors, out RgbColor value)
        {
            value = RgbColor.Black;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) && HeatPaletteColorDefinitions.TrySharedRed(key, colors, out value);
        }

        /// <summary>TryCanonical for the colour at bank-$8D <paramref name="pointer"/>, as an address.</summary>
        internal static bool TryCanonicalPointer(int pointer, out ushort mapped)
        {
            mapped = 0;
            if (!PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) || !HeatPaletteColorDefinitions.TryCanonical(key, out PaletteFxColorKey target))
                return false;
            mapped = PaletteFxColorAddressAccess.Address(target);
            return true;
        }

        /// <summary>TryBasePalette for the colour at bank-$8D <paramref name="pointer"/>, as an address.</summary>
        internal static bool TryBasePalettePointer(int pointer, out ushort mapped)
        {
            mapped = 0;
            if (!PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) || !HeatPaletteColorDefinitions.TryBasePalette(key, out PaletteFxColorKey target))
                return false;
            mapped = PaletteFxColorAddressAccess.Address(target);
            return true;
        }
    }
}

/// <summary>Address views of <see cref="LoadingPaletteColorDefinitions"/>.</summary>
internal static class LoadingPaletteColorDefinitionsAddressAccess
{
    /// <summary>The keyed members by bank-$8D address.</summary>
    extension(LoadingPaletteColorDefinitions)
    {
        /// <summary>TryCalculatedColor for the colour at bank-$8D <paramref name="pointer"/>, with colours keyed by address.</summary>
        internal static bool TryCalculatedColor(int pointer, IReadOnlyDictionary<ushort, RgbColor> colors, out RgbColor value)
        {
            value = RgbColor.Black;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) &&
                LoadingPaletteColorDefinitions.TryCalculatedColor(key, PaletteFxColorAddressAccess.Keyed(colors), out value);
        }

        /// <summary>TryCalculatedColor for the colour at bank-$8D <paramref name="pointer"/>.</summary>
        internal static bool TryCalculatedColor(int pointer, IReadOnlyDictionary<PaletteFxColorKey, RgbColor> colors, out RgbColor value)
        {
            value = RgbColor.Black;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) && LoadingPaletteColorDefinitions.TryCalculatedColor(key, colors, out value);
        }

        /// <summary>TryCanonical for the colour at bank-$8D <paramref name="pointer"/>, as an address.</summary>
        internal static bool TryCanonicalPointer(int pointer, out ushort mapped)
        {
            mapped = 0;
            if (!PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) || !LoadingPaletteColorDefinitions.TryCanonical(key, out PaletteFxColorKey target))
                return false;
            mapped = PaletteFxColorAddressAccess.Address(target);
            return true;
        }
    }
}

/// <summary>Address views of <see cref="LogoGlarePaletteColorDefinitions"/>.</summary>
internal static class LogoGlarePaletteColorDefinitionsAddressAccess
{
    /// <summary>The keyed members by bank-$8D address.</summary>
    extension(LogoGlarePaletteColorDefinitions)
    {
        /// <summary>TryCalculatedColor for the colour at bank-$8D <paramref name="pointer"/>, with colours keyed by address.</summary>
        internal static bool TryCalculatedColor(int pointer, IReadOnlyDictionary<ushort, RgbColor> colors, out RgbColor value)
        {
            value = RgbColor.Black;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) &&
                LogoGlarePaletteColorDefinitions.TryCalculatedColor(key, PaletteFxColorAddressAccess.Keyed(colors), out value);
        }

        /// <summary>TryCalculatedColor for the colour at bank-$8D <paramref name="pointer"/>.</summary>
        internal static bool TryCalculatedColor(int pointer, IReadOnlyDictionary<PaletteFxColorKey, RgbColor> colors, out RgbColor value)
        {
            value = RgbColor.Black;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) && LogoGlarePaletteColorDefinitions.TryCalculatedColor(key, colors, out value);
        }

        /// <summary>The row and colour of the colour at bank-$8D <paramref name="pointer"/>.</summary>
        internal static bool TryCoordinates(int pointer, out int frame, out int color)
        {
            frame = color = 0;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) && PostCreditsIconGlarePaletteFxColorDefinitions.Rows.TryLocate(key, out frame, out color);
        }
    }
}

/// <summary>Address views of <see cref="EndingGunshipPaletteColorDefinitions"/>.</summary>
internal static class EndingGunshipPaletteColorDefinitionsAddressAccess
{
    /// <summary>The keyed members by bank-$8D address.</summary>
    extension(EndingGunshipPaletteColorDefinitions)
    {
        /// <summary>TryCalculatedColor for the colour at bank-$8D <paramref name="pointer"/>, with colours keyed by address.</summary>
        internal static bool TryCalculatedColor(int pointer, IReadOnlyDictionary<ushort, RgbColor> colors, out RgbColor value)
        {
            value = RgbColor.Black;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) &&
                EndingGunshipPaletteColorDefinitions.TryCalculatedColor(key, PaletteFxColorAddressAccess.Keyed(colors), out value);
        }

        /// <summary>TryCalculatedColor for the colour at bank-$8D <paramref name="pointer"/>.</summary>
        internal static bool TryCalculatedColor(int pointer, IReadOnlyDictionary<PaletteFxColorKey, RgbColor> colors, out RgbColor value)
        {
            value = RgbColor.Black;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) && EndingGunshipPaletteColorDefinitions.TryCalculatedColor(key, colors, out value);
        }

        /// <summary>The row and colour of the colour at bank-$8D <paramref name="pointer"/>.</summary>
        internal static bool TryCoordinates(int pointer, out int frame, out int color)
        {
            frame = color = 0;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) && ZebesExplosionGunshipPaletteFxColorDefinitions.Rows.TryLocate(key, out frame, out color);
        }
    }
}

/// <summary>Address views of <see cref="TourianStatueGreyColorDefinitions"/>.</summary>
internal static class TourianStatueGreyColorDefinitionsAddressAccess
{
    /// <summary>The keyed members by bank-$8D address.</summary>
    extension(TourianStatueGreyColorDefinitions)
    {
        /// <summary>TryCalculatedColor for the colour at bank-$8D <paramref name="pointer"/>, with colours keyed by address.</summary>
        internal static bool TryCalculatedColor(int pointer, IReadOnlyDictionary<ushort, RgbColor> colors, out RgbColor value)
        {
            value = RgbColor.Black;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) &&
                TourianStatueGreyColorDefinitions.TryCalculatedColor(key, PaletteFxColorAddressAccess.Keyed(colors), out value);
        }

        /// <summary>TryCalculatedColor for the colour at bank-$8D <paramref name="pointer"/>.</summary>
        internal static bool TryCalculatedColor(int pointer, IReadOnlyDictionary<PaletteFxColorKey, RgbColor> colors, out RgbColor value)
        {
            value = RgbColor.Black;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) && TourianStatueGreyColorDefinitions.TryCalculatedColor(key, colors, out value);
        }

        /// <summary>The row and colour of the colour at bank-$8D <paramref name="pointer"/>.</summary>
        internal static bool TryCoordinates(int pointer, out int frame, out int color)
        {
            frame = color = 0;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) && TourianStatueGreyPaletteFxColorDefinitions.Rows.TryLocate(key, out frame, out color);
        }
    }
}

/// <summary>Address views of <see cref="CrateriaLightningColorDefinitions"/>.</summary>
internal static class CrateriaLightningColorDefinitionsAddressAccess
{
    /// <summary>The keyed members by bank-$8D address.</summary>
    extension(CrateriaLightningColorDefinitions)
    {
        /// <summary>TryCalculatedColor for the colour at bank-$8D <paramref name="pointer"/>.</summary>
        internal static bool TryCalculatedColor(int pointer, out RgbColor value)
        {
            value = RgbColor.Black;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) && CrateriaLightningColorDefinitions.TryCalculatedColor(key, out value);
        }

        /// <summary>TryCalculatedDarkColor for the colour at bank-$8D <paramref name="pointer"/>, with colours keyed by address.</summary>
        internal static bool TryCalculatedDarkColor(int pointer, IReadOnlyDictionary<ushort, RgbColor> colors, out RgbColor value)
        {
            value = RgbColor.Black;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) &&
                CrateriaLightningColorDefinitions.TryCalculatedDarkColor(key, PaletteFxColorAddressAccess.Keyed(colors), out value);
        }

        /// <summary>TryCalculatedDarkColor for the colour at bank-$8D <paramref name="pointer"/>.</summary>
        internal static bool TryCalculatedDarkColor(int pointer, IReadOnlyDictionary<PaletteFxColorKey, RgbColor> colors, out RgbColor value)
        {
            value = RgbColor.Black;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) && CrateriaLightningColorDefinitions.TryCalculatedDarkColor(key, colors, out value);
        }

        /// <summary>The row and colour of the colour at bank-$8D <paramref name="pointer"/>.</summary>
        internal static bool TrySurfaceCoordinates(int pointer, out int frame, out int color)
        {
            frame = color = 0;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) && CrateriaLightningColorDefinitions.SurfaceProgram.Rows.TryLocate(key, out frame, out color);
        }
    }
}

/// <summary>Address views of <see cref="MaridiaEnvironmentalColorDefinitions"/>.</summary>
internal static class MaridiaEnvironmentalColorDefinitionsAddressAccess
{
    /// <summary>The keyed members by bank-$8D address.</summary>
    extension(MaridiaEnvironmentalColorDefinitions)
    {
        /// <summary>TrySource for the colour at bank-$8D <paramref name="pointer"/>, as an address.</summary>
        internal static bool TrySourcePointer(int pointer, out ushort mapped)
        {
            mapped = 0;
            if (!PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) || !MaridiaEnvironmentalColorDefinitions.TrySource(key, out PaletteFxColorKey target))
                return false;
            mapped = PaletteFxColorAddressAccess.Address(target);
            return true;
        }
    }
}

/// <summary>Address views of <see cref="PlanetZebesTextColorDefinitions"/>.</summary>
internal static class PlanetZebesTextColorDefinitionsAddressAccess
{
    /// <summary>The keyed members by bank-$8D address.</summary>
    extension(PlanetZebesTextColorDefinitions)
    {
        /// <summary>TryCalculate for the colour at bank-$8D <paramref name="pointer"/>, with colours keyed by address.</summary>
        internal static bool TryCalculate(int pointer, IReadOnlyDictionary<ushort, RgbColor> colors, out RgbColor value)
        {
            value = RgbColor.Black;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) &&
                PlanetZebesTextColorDefinitions.TryCalculate(key, PaletteFxColorAddressAccess.Keyed(colors), out value);
        }

        /// <summary>TryCalculate for the colour at bank-$8D <paramref name="pointer"/>.</summary>
        internal static bool TryCalculate(int pointer, IReadOnlyDictionary<PaletteFxColorKey, RgbColor> colors, out RgbColor value)
        {
            value = RgbColor.Black;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) && PlanetZebesTextColorDefinitions.TryCalculate(key, colors, out value);
        }

        /// <summary>The fade owner, row and colour of the colour at bank-$8D <paramref name="pointer"/>.</summary>
        internal static bool TryCoordinates(int pointer, out PlanetZebesTextPaletteFxProgramOwner owner, out int frame, out int color)
        {
            owner = default;
            frame = color = 0;
            return PaletteFxColorAddressAccess.TryKeyAt(pointer, out PaletteFxColorKey key) &&
                PlanetZebesTextColorDefinitions.TryCoordinates(key, out owner, out frame, out color);
        }

        /// <summary>The bank-$8D address of endpoint <paramref name="color"/>.</summary>
        internal static ushort EndpointPointer(int color) => PaletteFxColorAddressAccess.Address(PlanetZebesTextColorDefinitions.Endpoint(color));
    }
}
