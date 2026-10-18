using SuperMetroid.AssetExtraction.Fx;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Verification's native view of the palette-FX colour families: each record's and colour's bank-$8D address, resolved
/// through the import pipeline's record sources, so the ROM oracles compare the words the cartridge stores.
/// </summary>
internal static class PaletteFxColorLayoutAccess
{
    /// <summary>The bank-$8D address of <paramref name="frame"/>'s timer word.</summary>
    internal static ushort RecordAddress(PaletteFxFrame frame) => PaletteFxRecordSources.Address(frame);

    /// <summary>The bank-$8D address of the colour word <paramref name="key"/> names.</summary>
    internal static ushort ColorAddress(PaletteFxColorKey key) => PaletteFxRecordSources.ColorAddress(key);

    /// <summary>Native view of a <see cref="CinematicGlowPaletteFxColorDefinition"/>.</summary>
    extension(CinematicGlowPaletteFxColorDefinition self)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal ushort FirstFramePointer => RecordAddress(self.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal ushort FramePointer(int frame) => RecordAddress(self.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal ushort ColorPointer(int frame, int color) => ColorAddress(self.Color(frame, color));
    }

    /// <summary>Native view of a <see cref="CrateriaEscapeLightningPaletteFxColorDefinition"/>.</summary>
    extension(CrateriaEscapeLightningPaletteFxColorDefinition self)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal ushort FirstFramePointer => RecordAddress(self.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal ushort FramePointer(int frame) => RecordAddress(self.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal ushort ColorPointer(int frame, int color) => ColorAddress(self.Color(frame, color));
    }

    /// <summary>Native view of a <see cref="OldTourianEscapeAccentPaletteFxColorDefinition"/>.</summary>
    extension(OldTourianEscapeAccentPaletteFxColorDefinition self)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal ushort FirstFramePointer => RecordAddress(self.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal ushort FramePointer(int frame) => RecordAddress(self.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal ushort ColorPointer(int frame, int color) => ColorAddress(self.Color(frame, color));
    }

    /// <summary>Native view of a <see cref="SamusLoadingSuitPaletteFxColorDefinition"/>.</summary>
    extension(SamusLoadingSuitPaletteFxColorDefinition self)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal ushort FirstFramePointer => RecordAddress(self.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal ushort FramePointer(int frame) => RecordAddress(self.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal ushort ColorPointer(int frame, int color) => ColorAddress(self.Color(frame, color));
    }

    /// <summary>Native view of a <see cref="TourianEscapeRedFlashPaletteFxColorDefinition"/>.</summary>
    extension(TourianEscapeRedFlashPaletteFxColorDefinition self)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal ushort FirstFramePointer => RecordAddress(self.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal ushort FramePointer(int frame) => RecordAddress(self.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal ushort ColorPointer(int frame, int color) => ColorAddress(self.Color(frame, color));
    }

    /// <summary>Native view of a <see cref="ZebesExplosionAmbientPaletteFxColorDefinition"/>.</summary>
    extension(ZebesExplosionAmbientPaletteFxColorDefinition self)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal ushort FirstFramePointer => RecordAddress(self.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal ushort FramePointer(int frame) => RecordAddress(self.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal ushort ColorPointer(int frame, int color) => ColorAddress(self.Color(frame, color));
    }

    /// <summary>Native view of a <see cref="ZebesExplosionLayerFadePaletteFxColorDefinition"/>.</summary>
    extension(ZebesExplosionLayerFadePaletteFxColorDefinition self)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal ushort FirstFramePointer => RecordAddress(self.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal ushort FramePointer(int frame) => RecordAddress(self.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal ushort ColorPointer(int frame, int color) => ColorAddress(self.Color(frame, color));
    }

    /// <summary>Native view of a <see cref="NorfairEnvironmentalPaletteFxColorDefinition"/>.</summary>
    extension(NorfairEnvironmentalPaletteFxColorDefinition self)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal ushort FirstFramePointer => RecordAddress(self.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal ushort FramePointer(int frame) => RecordAddress(self.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal ushort ColorPointer(int frame, int color) => ColorAddress(self.Color(frame, color));
    }

    /// <summary>Native view of a <see cref="TorizoBellyPaletteFxColorDefinition"/>.</summary>
    extension(TorizoBellyPaletteFxColorDefinition self)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal ushort FirstFramePointer => RecordAddress(self.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal ushort FramePointer(int frame) => RecordAddress(self.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal ushort ColorPointer(int frame, int color) => ColorAddress(self.Color(frame, color));
    }

    /// <summary>Native view of a <see cref="BrinstarBlueSporePaletteFxColorDefinition"/>.</summary>
    extension(BrinstarBlueSporePaletteFxColorDefinition self)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal ushort FirstFramePointer => RecordAddress(self.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal ushort FramePointer(int frame) => RecordAddress(self.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal ushort ColorPointer(int frame, int color) => ColorAddress(self.Color(frame, color));
    }

    /// <summary>Native view of a <see cref="MaridiaEnvironmentalPaletteFxColorDefinition"/>.</summary>
    extension(MaridiaEnvironmentalPaletteFxColorDefinition self)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal ushort FirstFramePointer => RecordAddress(self.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal ushort FramePointer(int frame) => RecordAddress(self.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal ushort ColorPointer(int frame, int color) => ColorAddress(self.Color(frame, color));
    }

    /// <summary>Native view of a <see cref="PlanetZebesTextPaletteFxColorDefinition"/>.</summary>
    extension(PlanetZebesTextPaletteFxColorDefinition self)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal ushort FirstFramePointer => RecordAddress(self.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal ushort FramePointer(int frame) => RecordAddress(self.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal ushort ColorPointer(int frame, int color) => ColorAddress(self.Color(frame, color));
    }

    /// <summary>Native view of a <see cref="TitleScreenAmbientPaletteFxColorDefinition"/>.</summary>
    extension(TitleScreenAmbientPaletteFxColorDefinition self)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal ushort FirstFramePointer => RecordAddress(self.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal ushort FramePointer(int frame) => RecordAddress(self.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal ushort ColorPointer(int frame, int color) => ColorAddress(self.Color(frame, color));
    }

    /// <summary>Native view of a <see cref="CrateriaLightningPaletteFxColorDefinition"/>.</summary>
    extension(CrateriaLightningPaletteFxColorDefinition self)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal ushort FirstFramePointer => RecordAddress(self.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal ushort FramePointer(int frame) => RecordAddress(self.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal ushort ColorPointer(int frame, int color) => ColorAddress(self.Color(frame, color));
    }
}

/// <summary>Native view of <see cref="TitleLogoFadePaletteFxColorDefinitions"/>.</summary>
internal static class TitleLogoFadePaletteFxColorDefinitionsLayoutAccess
{
    /// <summary>The family's record and colour addresses.</summary>
    extension(TitleLogoFadePaletteFxColorDefinitions)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal static ushort FirstFramePointer => PaletteFxColorLayoutAccess.RecordAddress(TitleLogoFadePaletteFxColorDefinitions.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal static ushort FramePointer(int frame) => PaletteFxColorLayoutAccess.RecordAddress(TitleLogoFadePaletteFxColorDefinitions.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal static ushort ColorPointer(int frame, int color) => PaletteFxColorLayoutAccess.ColorAddress(TitleLogoFadePaletteFxColorDefinitions.Color(frame, color));
    }
}

/// <summary>Native view of <see cref="NintendoLogoFadePaletteFxColorDefinitions"/>.</summary>
internal static class NintendoLogoFadePaletteFxColorDefinitionsLayoutAccess
{
    /// <summary>The family's record and colour addresses.</summary>
    extension(NintendoLogoFadePaletteFxColorDefinitions)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal static ushort FirstFramePointer => PaletteFxColorLayoutAccess.RecordAddress(NintendoLogoFadePaletteFxColorDefinitions.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal static ushort FramePointer(int frame) => PaletteFxColorLayoutAccess.RecordAddress(NintendoLogoFadePaletteFxColorDefinitions.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal static ushort ColorPointer(int frame, int color) => PaletteFxColorLayoutAccess.ColorAddress(NintendoLogoFadePaletteFxColorDefinitions.Color(frame, color));
    }
}

/// <summary>Native view of <see cref="ExplodingZebesFadePaletteFxColorDefinitions"/>.</summary>
internal static class ExplodingZebesFadePaletteFxColorDefinitionsLayoutAccess
{
    /// <summary>The family's record and colour addresses.</summary>
    extension(ExplodingZebesFadePaletteFxColorDefinitions)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal static ushort FirstFramePointer => PaletteFxColorLayoutAccess.RecordAddress(ExplodingZebesFadePaletteFxColorDefinitions.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal static ushort FramePointer(int frame) => PaletteFxColorLayoutAccess.RecordAddress(ExplodingZebesFadePaletteFxColorDefinitions.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal static ushort ColorPointer(int frame, int color) => PaletteFxColorLayoutAccess.ColorAddress(ExplodingZebesFadePaletteFxColorDefinitions.Color(frame, color));
    }
}

/// <summary>Native view of <see cref="ZebesExplosionForegroundPaletteFxColorDefinitions"/>.</summary>
internal static class ZebesExplosionForegroundPaletteFxColorDefinitionsLayoutAccess
{
    /// <summary>The family's record and colour addresses.</summary>
    extension(ZebesExplosionForegroundPaletteFxColorDefinitions)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal static ushort FirstFramePointer => PaletteFxColorLayoutAccess.RecordAddress(ZebesExplosionForegroundPaletteFxColorDefinitions.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal static ushort FramePointer(int frame) => PaletteFxColorLayoutAccess.RecordAddress(ZebesExplosionForegroundPaletteFxColorDefinitions.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal static ushort ColorPointer(int frame, int color) => PaletteFxColorLayoutAccess.ColorAddress(ZebesExplosionForegroundPaletteFxColorDefinitions.Color(frame, color));
    }
}

/// <summary>Native view of <see cref="ZebesExplosionFinalePaletteFxColorDefinitions"/>.</summary>
internal static class ZebesExplosionFinalePaletteFxColorDefinitionsLayoutAccess
{
    /// <summary>The family's record and colour addresses.</summary>
    extension(ZebesExplosionFinalePaletteFxColorDefinitions)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal static ushort FirstFramePointer => PaletteFxColorLayoutAccess.RecordAddress(ZebesExplosionFinalePaletteFxColorDefinitions.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal static ushort FramePointer(int frame) => PaletteFxColorLayoutAccess.RecordAddress(ZebesExplosionFinalePaletteFxColorDefinitions.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal static ushort ColorPointer(int frame, int color) => PaletteFxColorLayoutAccess.ColorAddress(ZebesExplosionFinalePaletteFxColorDefinitions.Color(frame, color));
    }
}

/// <summary>Native view of <see cref="ZebesExplosionWhiteoutPaletteFxColorDefinitions"/>.</summary>
internal static class ZebesExplosionWhiteoutPaletteFxColorDefinitionsLayoutAccess
{
    /// <summary>The family's record and colour addresses.</summary>
    extension(ZebesExplosionWhiteoutPaletteFxColorDefinitions)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal static ushort FirstFramePointer => PaletteFxColorLayoutAccess.RecordAddress(ZebesExplosionWhiteoutPaletteFxColorDefinitions.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal static ushort FramePointer(int frame) => PaletteFxColorLayoutAccess.RecordAddress(ZebesExplosionWhiteoutPaletteFxColorDefinitions.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal static ushort ColorPointer(int frame, int color) => PaletteFxColorLayoutAccess.ColorAddress(ZebesExplosionWhiteoutPaletteFxColorDefinitions.Color(frame, color));
    }
}

/// <summary>Native view of <see cref="ZebesExplosionGunshipPaletteFxColorDefinitions"/>.</summary>
internal static class ZebesExplosionGunshipPaletteFxColorDefinitionsLayoutAccess
{
    /// <summary>The family's record and colour addresses.</summary>
    extension(ZebesExplosionGunshipPaletteFxColorDefinitions)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal static ushort FirstFramePointer => PaletteFxColorLayoutAccess.RecordAddress(ZebesExplosionGunshipPaletteFxColorDefinitions.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal static ushort FramePointer(int frame) => PaletteFxColorLayoutAccess.RecordAddress(ZebesExplosionGunshipPaletteFxColorDefinitions.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal static ushort ColorPointer(int frame, int color) => PaletteFxColorLayoutAccess.ColorAddress(ZebesExplosionGunshipPaletteFxColorDefinitions.Color(frame, color));
    }
}

/// <summary>Native view of <see cref="UnusedCinematicFadePaletteFxColorDefinitions"/>.</summary>
internal static class UnusedCinematicFadePaletteFxColorDefinitionsLayoutAccess
{
    /// <summary>The family's record and colour addresses.</summary>
    extension(UnusedCinematicFadePaletteFxColorDefinitions)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal static ushort FirstFramePointer => PaletteFxColorLayoutAccess.RecordAddress(UnusedCinematicFadePaletteFxColorDefinitions.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal static ushort FramePointer(int frame) => PaletteFxColorLayoutAccess.RecordAddress(UnusedCinematicFadePaletteFxColorDefinitions.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal static ushort ColorPointer(int frame, int color) => PaletteFxColorLayoutAccess.ColorAddress(UnusedCinematicFadePaletteFxColorDefinitions.Color(frame, color));
    }
}

/// <summary>Native view of <see cref="PostCreditsIconGlarePaletteFxColorDefinitions"/>.</summary>
internal static class PostCreditsIconGlarePaletteFxColorDefinitionsLayoutAccess
{
    /// <summary>The family's record and colour addresses.</summary>
    extension(PostCreditsIconGlarePaletteFxColorDefinitions)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal static ushort FirstFramePointer => PaletteFxColorLayoutAccess.RecordAddress(PostCreditsIconGlarePaletteFxColorDefinitions.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal static ushort FramePointer(int frame) => PaletteFxColorLayoutAccess.RecordAddress(PostCreditsIconGlarePaletteFxColorDefinitions.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal static ushort ColorPointer(int frame, int color) => PaletteFxColorLayoutAccess.ColorAddress(PostCreditsIconGlarePaletteFxColorDefinitions.Color(frame, color));
    }
}

/// <summary>Native view of <see cref="TourianStatueGreyPaletteFxColorDefinitions"/>.</summary>
internal static class TourianStatueGreyPaletteFxColorDefinitionsLayoutAccess
{
    /// <summary>The family's record and colour addresses.</summary>
    extension(TourianStatueGreyPaletteFxColorDefinitions)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal static ushort FirstFramePointer => PaletteFxColorLayoutAccess.RecordAddress(TourianStatueGreyPaletteFxColorDefinitions.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal static ushort FramePointer(int frame) => PaletteFxColorLayoutAccess.RecordAddress(TourianStatueGreyPaletteFxColorDefinitions.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal static ushort ColorPointer(int frame, int color) => PaletteFxColorLayoutAccess.ColorAddress(TourianStatueGreyPaletteFxColorDefinitions.Color(frame, color));
    }
}

/// <summary>Native view of <see cref="WreckedShipGreenLightPaletteFxColorDefinitions"/>.</summary>
internal static class WreckedShipGreenLightPaletteFxColorDefinitionsLayoutAccess
{
    /// <summary>The family's record and colour addresses.</summary>
    extension(WreckedShipGreenLightPaletteFxColorDefinitions)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal static ushort FirstFramePointer => PaletteFxColorLayoutAccess.RecordAddress(WreckedShipGreenLightPaletteFxColorDefinitions.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal static ushort FramePointer(int frame) => PaletteFxColorLayoutAccess.RecordAddress(WreckedShipGreenLightPaletteFxColorDefinitions.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal static ushort ColorPointer(int frame, int color) => PaletteFxColorLayoutAccess.ColorAddress(WreckedShipGreenLightPaletteFxColorDefinitions.Color(frame, color));
    }
}

/// <summary>Native view of <see cref="RedBrinstarGlowPaletteFxColorDefinitions"/>.</summary>
internal static class RedBrinstarGlowPaletteFxColorDefinitionsLayoutAccess
{
    /// <summary>The family's record and colour addresses.</summary>
    extension(RedBrinstarGlowPaletteFxColorDefinitions)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal static ushort FirstFramePointer => PaletteFxColorLayoutAccess.RecordAddress(RedBrinstarGlowPaletteFxColorDefinitions.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal static ushort FramePointer(int frame) => PaletteFxColorLayoutAccess.RecordAddress(RedBrinstarGlowPaletteFxColorDefinitions.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal static ushort ColorPointer(int frame, int color) => PaletteFxColorLayoutAccess.ColorAddress(RedBrinstarGlowPaletteFxColorDefinitions.Color(frame, color));
    }
}

/// <summary>Native view of <see cref="BeaconPaletteFxColorDefinitions"/>.</summary>
internal static class BeaconPaletteFxColorDefinitionsLayoutAccess
{
    /// <summary>The family's record and colour addresses.</summary>
    extension(BeaconPaletteFxColorDefinitions)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal static ushort FirstFramePointer => PaletteFxColorLayoutAccess.RecordAddress(BeaconPaletteFxColorDefinitions.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal static ushort FramePointer(int frame) => PaletteFxColorLayoutAccess.RecordAddress(BeaconPaletteFxColorDefinitions.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal static ushort ColorPointer(int frame, int color) => PaletteFxColorLayoutAccess.ColorAddress(BeaconPaletteFxColorDefinitions.Color(frame, color));
    }
}

/// <summary>Native view of <see cref="TourianGlowPaletteFxColorDefinitions"/>.</summary>
internal static class TourianGlowPaletteFxColorDefinitionsLayoutAccess
{
    /// <summary>The family's record and colour addresses.</summary>
    extension(TourianGlowPaletteFxColorDefinitions)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal static ushort FirstFramePointer => PaletteFxColorLayoutAccess.RecordAddress(TourianGlowPaletteFxColorDefinitions.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal static ushort FramePointer(int frame) => PaletteFxColorLayoutAccess.RecordAddress(TourianGlowPaletteFxColorDefinitions.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal static ushort ColorPointer(int frame, int color) => PaletteFxColorLayoutAccess.ColorAddress(TourianGlowPaletteFxColorDefinitions.Color(frame, color));
    }
}

/// <summary>Native view of <see cref="TourianEscapeSharedRedFlashPaletteFxColorDefinitions"/>.</summary>
internal static class TourianEscapeSharedRedFlashPaletteFxColorDefinitionsLayoutAccess
{
    /// <summary>The family's record and colour addresses.</summary>
    extension(TourianEscapeSharedRedFlashPaletteFxColorDefinitions)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal static ushort FirstFramePointer => PaletteFxColorLayoutAccess.RecordAddress(TourianEscapeSharedRedFlashPaletteFxColorDefinitions.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal static ushort FramePointer(int frame) => PaletteFxColorLayoutAccess.RecordAddress(TourianEscapeSharedRedFlashPaletteFxColorDefinitions.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal static ushort ColorPointer(int frame, int color) => PaletteFxColorLayoutAccess.ColorAddress(TourianEscapeSharedRedFlashPaletteFxColorDefinitions.Color(frame, color));
    }
}

/// <summary>Native view of <see cref="OldTourianEscapeRedFlashPaletteFxColorDefinitions"/>.</summary>
internal static class OldTourianEscapeRedFlashPaletteFxColorDefinitionsLayoutAccess
{
    /// <summary>The family's record and colour addresses.</summary>
    extension(OldTourianEscapeRedFlashPaletteFxColorDefinitions)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal static ushort FirstFramePointer => PaletteFxColorLayoutAccess.RecordAddress(OldTourianEscapeRedFlashPaletteFxColorDefinitions.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal static ushort FramePointer(int frame) => PaletteFxColorLayoutAccess.RecordAddress(OldTourianEscapeRedFlashPaletteFxColorDefinitions.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal static ushort ColorPointer(int frame, int color) => PaletteFxColorLayoutAccess.ColorAddress(OldTourianEscapeRedFlashPaletteFxColorDefinitions.Color(frame, color));
    }
}

/// <summary>Native view of <see cref="UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions"/>.</summary>
internal static class UpperCrateriaEscapeRedFlashPaletteFxColorDefinitionsLayoutAccess
{
    /// <summary>The family's record and colour addresses.</summary>
    extension(UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions)
    {
        /// <summary>The bank-$8D address of the first record.</summary>
        internal static ushort FirstFramePointer => PaletteFxColorLayoutAccess.RecordAddress(UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.Rows.Frame(0));

        /// <summary>The bank-$8D address of record <paramref name="frame"/>.</summary>
        internal static ushort FramePointer(int frame) => PaletteFxColorLayoutAccess.RecordAddress(UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.Rows.Frame(frame));

        /// <summary>The bank-$8D address of colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        internal static ushort ColorPointer(int frame, int color) => PaletteFxColorLayoutAccess.ColorAddress(UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.Color(frame, color));
    }
}
