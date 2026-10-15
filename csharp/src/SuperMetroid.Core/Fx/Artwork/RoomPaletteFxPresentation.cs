using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Areas.Escape.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// Editable colors for room and cinematic palette animations. Native timing, destinations,
/// instructions, and side effects remain compiled engine mechanics.
/// </summary>
public sealed class RoomPaletteFxPresentation : IPaletteFxColorSource
{
    /// <summary>Explicit RGB5 samples indexed by their native bank-$8D color-word pointer.</summary>
    internal readonly Dictionary<ushort, Bgr555> colors;

    /// <summary>Canonical and derived heat-palette samples shared across Samus suits.</summary>
    internal readonly HeatPaletteInputView heatInputs;
    /// <summary>Editable and derived Samus loading-palette samples.</summary>
    internal readonly LoadingPaletteInputView loadingInputs;
    /// <summary>Endpoint samples used to reconstruct the ending gunship fade.</summary>
    private readonly EndingGunshipPaletteInputView gunshipInputs;

    /// <summary>Builds calculated-color views over validated, explicitly authored samples.</summary>
    private RoomPaletteFxPresentation(Dictionary<ushort, Bgr555> colors)
    {
        this.colors = colors;
        heatInputs = new HeatPaletteInputView(colors);
        loadingInputs = new LoadingPaletteInputView(colors);
        gunshipInputs = new EndingGunshipPaletteInputView(colors);
    }

    /// <inheritdoc />
    public bool TryReadColor(ushort pointer, out Bgr555 color) =>
        gunshipInputs.TryGetValue(pointer, out color) ||
        PlanetZebesTextColorDefinitions.TryCalculate(pointer, colors, out color) ||
        MaridiaEnvironmentalColorDefinitions.TryReadColor(pointer, colors, out color) ||
        CrateriaLightningColorDefinitions.TryCalculatedColor(pointer, out color) ||
        CrateriaLightningColorDefinitions.TryCalculatedDarkColor(pointer, colors, out color) ||
        TourianStatueGreyColorDefinitions.TryCalculatedColor(pointer, colors, out color) ||
        LoadingPaletteColorDefinitions.TryReadColor(pointer, loadingInputs, out color) ||
        LogoGlarePaletteColorDefinitions.TryCalculatedColor(pointer, colors, out color) ||
        EndingGunshipPaletteColorDefinitions.TryCalculatedColor(pointer, gunshipInputs, out color) ||
        (HeatPaletteColorDefinitions.TryCanonicalPointer(pointer, out ushort canonical) &&
         (heatInputs.TryGetValue(canonical, out color) ||
          HeatPaletteColorDefinitions.TryCalculatedColor(canonical, heatInputs, out color)));

    /// <summary>Distinguishes revision 17's inherited Samus heat rows from revision 18's required rows.</summary>
    private static bool InheritsHeatRows(RoomPaletteFxPresentationRevision revision) => revision switch
    {
        RoomPaletteFxPresentationRevision.Previous => true,
        RoomPaletteFxPresentationRevision.Current => false,
        _ => throw new InvalidOperationException(
            $"Undefined {nameof(RoomPaletteFxPresentationRevision)} {(int)revision}."),
    };

    /// <summary>Validates editable palette rows and compiles their RGB5 colors into the native bank-$8D color lookup.</summary>
    /// <param name="json">The UTF-8 JSON presentation stream with the required frame counts and RGB components from zero through thirty-one.</param>
    /// <param name="previousVersionFallback">Current stock colors used only to supply the missing Samus heat rows when loading the previous schema version.</param>
    /// <returns>A color source preserving independent edits while reconstructing matching shared or calculated samples on demand.</returns>
    /// <exception cref="InvalidDataException">The schema version, required frame geometry, or a color component is invalid, or the fallback lacks a required heat color.</exception>
    public static RoomPaletteFxPresentation Load(Stream json,
        RoomPaletteFxPresentation? previousVersionFallback = null)
    {
        RoomPaletteFxPresentationDocument document = JsonAssetDocument.Read<RoomPaletteFxPresentationDocument>(
            json, ArtworkJson.Options, "room palette-FX presentation");

        bool supported = Enum.IsDefined((RoomPaletteFxPresentationRevision)document.Version);
        bool inheritsHeatRows = supported && InheritsHeatRows((RoomPaletteFxPresentationRevision)document.Version);
        if (!supported || (inheritsHeatRows && previousVersionFallback is null))
        {
            throw new InvalidDataException(
                $"Room palette-FX presentation requires version " +
                $"{RoomPaletteFxPresentationFormat.Version}.");
        }

        var colors = new Dictionary<ushort, Bgr555>();
        foreach (PaletteFxHeatProgramDefinition definition in
                 PaletteFxHeatProgramMechanicsDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Suit switch
            {
                PaletteFxHeatSuit.Power => document.SamusHeatPowerSuit,
                PaletteFxHeatSuit.Varia => document.SamusHeatVariaSuit,
                PaletteFxHeatSuit.Gravity => document.SamusHeatGravitySuit,
                _ => throw new InvalidDataException($"Unsupported heat suit {definition.Suit}."),
            };
            ushort ColorPointer(int frame, int index) => unchecked((ushort)(
                definition.Frames[frame].FirstColorPointer + index * sizeof(ushort)));
            if (inheritsHeatRows)
            {
                // A v17 player override predates these three color families. Keep
                // all its existing edits and inherit only the newly extracted
                // Samus-in-heat rows from the verified current stock catalog.
                for (int frame = 0; frame < definition.Frames.Count; frame++)
                for (int index = 0; index < PaletteFxHeatProgramDefinition.ColorsPerFrame; index++)
                {
                    ushort pointer = ColorPointer(frame, index);
                    if (!(previousVersionFallback ?? throw new InvalidDataException("A previous-version room palette-FX override requires the current stock presentation.")).TryReadColor(pointer, out Bgr555 stockColor))
                        throw new InvalidDataException(
                            $"Current stock is missing {definition.Suit} heat color $8D:{pointer:X4}.");
                    colors.Add(pointer, stockColor);
                }
            }
            else
                ValidateAndCompile($"Samus {definition.Suit} suit in heat", frames,
                    definition.Frames.Count, PaletteFxHeatProgramDefinition.ColorsPerFrame,
                    ColorPointer, colors);
        }
        foreach (NorfairEnvironmentalPaletteFxColorDefinition definition in
                 NorfairEnvironmentalPaletteFxColorDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                NorfairEnvironmentalPaletteOwner.ForegroundAndHeatPhase =>
                    document.NorfairForegroundAndHeatPhase,
                NorfairEnvironmentalPaletteOwner.ForegroundPalette4 =>
                    document.NorfairForegroundPalette4,
                NorfairEnvironmentalPaletteOwner.ForegroundPalette5 =>
                    document.NorfairForegroundPalette5,
                NorfairEnvironmentalPaletteOwner.ForegroundPalette6 =>
                    document.NorfairForegroundPalette6,
                _ => throw new InvalidDataException(
                    $"Unsupported Norfair palette owner {definition.Owner}."),
            };
            ValidateAndCompile(
                $"Norfair {definition.Owner}",
                frames,
                NorfairEnvironmentalPaletteFxColorDefinitions.FrameCount,
                NorfairEnvironmentalPaletteFxColorDefinitions.ColorsPerFrame,
                definition.ColorPointer,
                colors);
        }
        foreach (MaridiaEnvironmentalPaletteFxColorDefinition definition in
                 MaridiaEnvironmentalPaletteFxColorDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                MaridiaEnvironmentalPaletteOwner.SandPits => document.MaridiaSandPits,
                MaridiaEnvironmentalPaletteOwner.SandFalls => document.MaridiaSandFalls,
                MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls =>
                    document.MaridiaBackgroundWaterfalls,
                _ => throw new InvalidDataException(
                    $"Unsupported Maridia palette owner {definition.Owner}."),
            };
            ValidateAndCompile(
                $"Maridia {definition.Owner}",
                frames,
                definition.FrameCount,
                definition.ColorsPerFrame,
                definition.ColorPointer,
                colors);
        }
        ValidateAndCompile(
            "Wrecked Ship green lights",
            document.WreckedShipGreenLights,
            WreckedShipGreenLightPaletteFxColorDefinitions.FrameCount,
            WreckedShipGreenLightPaletteFxColorDefinitions.ColorsPerFrame,
            WreckedShipGreenLightPaletteFxColorDefinitions.ColorPointer,
            colors);
        ValidateAndCompile(
            "Red Brinstar background glow",
            document.RedBrinstarBackgroundGlow,
            RedBrinstarGlowPaletteFxColorDefinitions.FrameCount,
            RedBrinstarGlowPaletteFxColorDefinitions.ColorsPerFrame,
            RedBrinstarGlowPaletteFxColorDefinitions.ColorPointer,
            colors);
        ValidateAndCompile(
            "Tourian glow",
            document.TourianGlow,
            TourianGlowPaletteFxColorDefinitions.FrameCount,
            TourianGlowPaletteFxColorDefinitions.ColorsPerFrame,
            TourianGlowPaletteFxColorDefinitions.ColorPointer,
            colors);
        foreach (BrinstarBlueSporePaletteFxColorDefinition definition in
                 BrinstarBlueSporePaletteFxColorDefinitions.All)
        {
            ValidateAndCompile(
                $"Brinstar blue spores ({definition.Owner})",
                document.BrinstarBlueSpores,
                BrinstarBlueSporePaletteFxColorDefinitions.FrameCount,
                BrinstarBlueSporePaletteFxColorDefinitions.ColorsPerFrame,
                definition.ColorPointer,
                colors);
        }
        foreach (TorizoBellyPaletteFxColorDefinition definition in
                 TorizoBellyPaletteFxColorDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                TorizoBellyPaletteOwner.BombTorizo => document.BombTorizoBelly,
                TorizoBellyPaletteOwner.GoldenTorizo => document.GoldenTorizoBelly,
                _ => throw new InvalidDataException(
                    $"Unsupported Torizo belly owner {definition.Owner}."),
            };
            ValidateAndCompile(
                $"{definition.Owner} belly",
                frames,
                TorizoBellyPaletteFxColorDefinitions.FrameCount,
                TorizoBellyPaletteFxColorDefinitions.ColorsPerFrame,
                definition.ColorPointer,
                colors);
        }
        ValidateAndCompile(
            "Tourian statue grey-out",
            document.TourianStatueGrey,
            TourianStatueGreyPaletteFxColorDefinitions.FrameCount,
            TourianStatueGreyPaletteFxColorDefinitions.ColorsPerFrame,
            TourianStatueGreyPaletteFxColorDefinitions.ColorPointer,
            colors);
        foreach (CrateriaLightningPaletteFxColorDefinition definition in
                 CrateriaLightningPaletteFxColorDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                CrateriaLightningPaletteOwner.SurfaceLightning =>
                    document.CrateriaSurfaceLightning,
                CrateriaLightningPaletteOwner.UnusedDarkLightning =>
                    document.CrateriaUnusedDarkLightning,
                _ => throw new InvalidDataException(
                    $"Unsupported Crateria lightning owner {definition.Owner}."),
            };
            ValidateAndCompile(
                $"Crateria {definition.Owner}",
                frames,
                definition.Frames.Count,
                definition.ColorsPerFrame,
                definition.ColorPointer,
                colors);
        }
        ValidateAndCompile(
            "Ceres gunship-engine lights",
            document.CeresGunshipEngineLights,
            CeresCinematicLightPaletteFxColorDefinitions
                .GunshipEngineFrameCount,
            CeresCinematicLightPaletteFxColorDefinitions
                .GunshipEngineColorsPerFrame,
            CeresCinematicLightPaletteFxColorDefinitions
                .GunshipEngineColorPointer,
            colors);
        ValidateAndCompile(
            "Ceres navigation lights",
            document.CeresNavigationLights,
            CeresCinematicLightPaletteFxColorDefinitions
                .NavigationLightsFrameCount,
            CeresCinematicLightPaletteFxColorDefinitions
                .NavigationLightsColorsPerFrame,
            CeresCinematicLightPaletteFxColorDefinitions
                .NavigationLightsColorPointer,
            colors);
        foreach (PlanetZebesTextPaletteFxColorDefinition definition in
                 PlanetZebesTextPaletteFxColorDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                PlanetZebesTextPaletteFxProgramOwner.FadeIn =>
                    document.PlanetZebesTextFadeIn,
                PlanetZebesTextPaletteFxProgramOwner.FadeOut =>
                    document.PlanetZebesTextFadeOut,
                _ => throw new InvalidDataException(
                    $"Unsupported PLANET ZEBES text-fade owner {definition.Owner}."),
            };
            ValidateAndCompile(
                $"PLANET ZEBES {definition.Owner}",
                frames,
                PlanetZebesTextPaletteFxColorDefinitions.FrameCount,
                PlanetZebesTextPaletteFxColorDefinitions.ColorsPerFrame,
                definition.ColorPointer,
                colors);
        }
        foreach (CinematicGlowPaletteFxColorDefinition definition in
                 CinematicGlowPaletteFxColorDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                CinematicGlowPaletteFxProgramOwner.OldMotherBrainBackgroundLights =>
                    document.OldMotherBrainBackgroundLights,
                CinematicGlowPaletteFxProgramOwner.GunshipGlow =>
                    document.CinematicGunshipGlow,
                _ => throw new InvalidDataException(
                    $"Unsupported cinematic glow owner {definition.Owner}."),
            };
            ValidateAndCompile(
                $"Cinematic glow {definition.Owner}",
                frames,
                CinematicGlowPaletteFxColorDefinitions.FrameCount,
                definition.ColorsPerFrame,
                definition.ColorPointer,
                colors);
        }
        ValidateAndCompile(
            "exploding Zebes fade",
            document.ExplodingZebesFade,
            ExplodingZebesFadePaletteFxColorDefinitions.FrameCount,
            ExplodingZebesFadePaletteFxColorDefinitions.ColorsPerFrame,
            ExplodingZebesFadePaletteFxColorDefinitions.ColorPointer,
            colors);
        ValidateAndCompile(
            "unused cinematic fade",
            document.UnusedCinematicFade,
            UnusedCinematicFadePaletteFxColorDefinitions.FrameCount,
            UnusedCinematicFadePaletteFxColorDefinitions.ColorsPerFrame,
            UnusedCinematicFadePaletteFxColorDefinitions.ColorPointer,
            colors);
        ValidateAndCompile(
            "title-logo fade",
            document.TitleLogoFade,
            TitleLogoFadePaletteFxColorDefinitions.FrameCount,
            TitleLogoFadePaletteFxColorDefinitions.ColorsPerFrame,
            TitleLogoFadePaletteFxColorDefinitions.ColorPointer,
            colors);
        ValidateAndCompile(
            "Nintendo shared fade",
            document.NintendoSharedFade,
            NintendoLogoFadePaletteFxColorDefinitions.FrameCount,
            NintendoLogoFadePaletteFxColorDefinitions.ColorsPerFrame,
            NintendoLogoFadePaletteFxColorDefinitions.ColorPointer,
            colors);
        ValidateAndCompile(
            "Zebes explosion foreground",
            document.ZebesExplosionForeground,
            ZebesExplosionForegroundPaletteFxColorDefinitions.FrameCount,
            ZebesExplosionForegroundPaletteFxColorDefinitions.ColorsPerFrame,
            ZebesExplosionForegroundPaletteFxColorDefinitions.ColorPointer,
            colors);
        ValidateAndCompile(
            "Zebes explosion finale",
            document.ZebesExplosionFinale,
            ZebesExplosionFinalePaletteFxColorDefinitions.FrameCount,
            ZebesExplosionFinalePaletteFxColorDefinitions.ColorsPerFrame,
            ZebesExplosionFinalePaletteFxColorDefinitions.ColorPointer,
            colors);
        ValidateAndCompile(
            "Zebes explosion shared whiteout",
            document.ZebesExplosionWhiteout,
            ZebesExplosionWhiteoutPaletteFxColorDefinitions.FrameCount,
            ZebesExplosionWhiteoutPaletteFxColorDefinitions.ColorsPerFrame,
            ZebesExplosionWhiteoutPaletteFxColorDefinitions.ColorPointer,
            colors);
        foreach (ZebesExplosionAmbientPaletteFxColorDefinition definition in
                 ZebesExplosionAmbientPaletteFxColorDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                ZebesExplosionAmbientPaletteFxProgramOwner.PlanetAfterglow =>
                    document.ZebesExplosionAfterglow,
                ZebesExplosionAmbientPaletteFxProgramOwner.Lava =>
                    document.ZebesExplosionLava,
                _ => throw new InvalidDataException(
                    $"Unsupported Zebes explosion ambient owner {definition.Owner}."),
            };
            ValidateAndCompile(
                $"Zebes explosion {definition.Owner}",
                frames,
                definition.FrameCount,
                definition.ColorsPerFrame,
                definition.ColorPointer,
                colors);
        }
        foreach (ZebesExplosionLayerFadePaletteFxColorDefinition definition in
                 ZebesExplosionLayerFadePaletteFxColorDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                ZebesExplosionLayerFadePaletteFxProgramOwner.Crust =>
                    document.ZebesExplosionCrust,
                ZebesExplosionLayerFadePaletteFxProgramOwner.GreyClouds =>
                    document.ZebesExplosionGreyClouds,
                _ => throw new InvalidDataException(
                    $"Unsupported Zebes explosion layer owner {definition.Owner}."),
            };
            ValidateAndCompile(
                $"Zebes explosion {definition.Owner}",
                frames,
                ZebesExplosionLayerFadePaletteFxColorDefinitions.FrameCount,
                ZebesExplosionLayerFadePaletteFxColorDefinitions.ColorsPerFrame,
                definition.ColorPointer,
                colors);
        }
        ValidateAndCompile(
            "Zebes explosion gunship",
            document.ZebesExplosionGunship,
            ZebesExplosionGunshipPaletteFxColorDefinitions.FrameCount,
            ZebesExplosionGunshipPaletteFxColorDefinitions.ColorsPerFrame,
            ZebesExplosionGunshipPaletteFxColorDefinitions.ColorPointer,
            colors);
        foreach (SamusLoadingSuitPaletteFxColorDefinition definition in
                 SamusLoadingSuitPaletteFxColorDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                SamusLoadingSuitPaletteFxProgramOwner.PowerSuit =>
                    document.SamusLoadingPowerSuit,
                SamusLoadingSuitPaletteFxProgramOwner.VariaSuit =>
                    document.SamusLoadingVariaSuit,
                SamusLoadingSuitPaletteFxProgramOwner.GravitySuit =>
                    document.SamusLoadingGravitySuit,
                _ => throw new InvalidDataException(
                    $"Unsupported Samus loading-suit owner {definition.Owner}."),
            };
            ValidateAndCompile(
                $"Samus loading {definition.Owner}",
                frames,
                SamusLoadingSuitPaletteFxColorDefinitions.FrameCount,
                SamusLoadingSuitPaletteFxColorDefinitions.ColorsPerFrame,
                definition.ColorPointer,
                colors);
        }
        ValidateAndCompile(
            "post-credits icon glare",
            document.PostCreditsIconGlare,
            PostCreditsIconGlarePaletteFxColorDefinitions.FrameCount,
            PostCreditsIconGlarePaletteFxColorDefinitions.ColorsPerFrame,
            PostCreditsIconGlarePaletteFxColorDefinitions.ColorPointer,
            colors);
        foreach (TourianEscapeRedFlashPaletteFxColorDefinition definition in
                 TourianEscapeRedFlashPaletteFxColorDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                TourianEscapeRedFlashPaletteOwner.Shutter => document.TourianEscapeShutter,
                TourianEscapeRedFlashPaletteOwner.Background =>
                    document.TourianEscapeBackground,
                _ => throw new InvalidDataException(
                    $"Unsupported Tourian escape red-flash owner {definition.Owner}."),
            };
            ValidateAndCompile($"Tourian escape {definition.Owner}", frames,
                TourianEscapeRedFlashPaletteFxColorDefinitions.FrameCount,
                definition.ColorsPerFrame, definition.ColorPointer, colors);
        }
        ValidateAndCompile(
            "Tourian escape shared red flash",
            document.TourianEscapeSharedRedFlash,
            TourianEscapeSharedRedFlashPaletteFxColorDefinitions.FrameCount,
            TourianEscapeSharedRedFlashPaletteFxColorDefinitions.ColorsPerFrame,
            TourianEscapeSharedRedFlashPaletteFxColorDefinitions.ColorPointer,
            colors);
        ValidateAndCompile(
            "old Tourian escape red flash",
            document.OldTourianEscapeRedFlash,
            OldTourianEscapeRedFlashPaletteFxColorDefinitions.FrameCount,
            OldTourianEscapeRedFlashPaletteFxColorDefinitions.ColorsPerFrame,
            OldTourianEscapeRedFlashPaletteFxColorDefinitions.ColorPointer,
            colors);
        foreach (OldTourianEscapeAccentPaletteFxColorDefinition definition in
                 OldTourianEscapeAccentPaletteFxColorDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                OldTourianEscapeAccentPaletteOwner.OrangeRailings =>
                    document.OldTourianEscapeOrangeRailings,
                OldTourianEscapeAccentPaletteOwner.YellowPanels =>
                    document.OldTourianEscapeYellowPanels,
                _ => throw new InvalidDataException(
                    $"Unsupported old Tourian escape accent owner {definition.Owner}."),
            };
            ValidateAndCompile($"old Tourian escape {definition.Owner}", frames,
                OldTourianEscapeAccentPaletteFxColorDefinitions.FrameCount,
                OldTourianEscapeAccentPaletteFxColorDefinitions.ColorsPerFrame,
                definition.ColorPointer, colors);
        }
        ValidateAndCompile(
            "upper Crateria escape red flash",
            document.UpperCrateriaEscapeRedFlash,
            UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.FrameCount,
            UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.ColorsPerFrame,
            UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.ColorPointer,
            colors);
        foreach (CrateriaEscapeLightningPaletteFxColorDefinition definition in
                 CrateriaEscapeLightningPaletteFxColorDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                CrateriaEscapeLightningPaletteOwner.YellowLightning =>
                    document.CrateriaEscapeYellowLightning,
                CrateriaEscapeLightningPaletteOwner.CreBlockPixel =>
                    document.CrateriaEscapeCreBlockPixel,
                _ => throw new InvalidDataException(
                    $"Unsupported Crateria escape lightning owner {definition.Owner}."),
            };
            ValidateAndCompile($"Crateria escape {definition.Owner}", frames,
                CrateriaEscapeLightningPaletteFxColorDefinitions.FrameCount,
                definition.ColorsPerFrame, definition.ColorPointer, colors);
        }
        ValidateAndCompile(
            "Crateria and Brinstar beacon flash",
            document.BeaconFlashing,
            BeaconPaletteFxColorDefinitions.FrameCount,
            BeaconPaletteFxColorDefinitions.ColorsPerFrame,
            BeaconPaletteFxColorDefinitions.ColorPointer,
            colors);

        // Collapse only values equal to the calculated row alias. Differing player
        // edits keep their original per-frame identity, including edits to base rows.
        foreach (var program in PaletteFxHeatProgramMechanicsDefinitions.All)
        foreach (var frame in program.Frames)
        for (int index = 0; index < 15; index++)
        {
            ushort pointer = (ushort)(frame.FirstColorPointer + 2 * index);
            if (HeatPaletteColorDefinitions.TryCanonicalPointer(pointer, out ushort canonical) &&
                canonical != pointer && colors[pointer] == colors[canonical]) colors.Remove(pointer);
        }
        foreach (var program in PaletteFxHeatProgramMechanicsDefinitions.All)
        foreach (var frame in program.Frames)
        for (int index = 0; index < 15; index++)
        {
            ushort pointer = (ushort)(frame.FirstColorPointer + 2 * index);
            if (colors.TryGetValue(pointer, out Bgr555 supplied) &&
                HeatPaletteColorDefinitions.TryCalculatedColor(pointer, colors, out Bgr555 calculated) && supplied == calculated)
                colors.Remove(pointer);
        }
        foreach (var program in SamusLoadingSuitPaletteFxColorDefinitions.All)
        for (int frame = 0; frame < 9; frame++)
        for (int index = 0; index < 16; index++)
        {
            ushort pointer = program.ColorPointer(frame, index);
            if (LoadingPaletteColorDefinitions.TryCanonicalPointer(pointer, out ushort canonical) &&
                canonical != pointer && colors[pointer] == colors[canonical]) colors.Remove(pointer);
        }
        foreach (var program in SamusLoadingSuitPaletteFxColorDefinitions.All)
        for (int frame = 0; frame < 9; frame++)
        for (int index = 0; index < 16; index++)
        {
            ushort pointer = program.ColorPointer(frame, index);
            if (colors.TryGetValue(pointer, out Bgr555 supplied) &&
                LoadingPaletteColorDefinitions.TryCalculatedColor(pointer, colors, out Bgr555 calculated) && supplied == calculated)
                colors.Remove(pointer);
        }
        // Only matching samples are discarded. If the player edits a base color,
        // unchanged intermediate samples that no longer fit remain explicit values.
        for (int frame = 0; frame < PostCreditsIconGlarePaletteFxColorDefinitions.FrameCount - 1; frame++)
        for (int index = 0; index < PostCreditsIconGlarePaletteFxColorDefinitions.ColorsPerFrame; index++)
        {
            ushort pointer = PostCreditsIconGlarePaletteFxColorDefinitions.ColorPointer(frame, index);
            if (LogoGlarePaletteColorDefinitions.TryCalculatedColor(pointer, colors, out Bgr555 calculated) &&
                colors[pointer] == calculated) colors.Remove(pointer);
        }
        for (int frame = 0; frame < ZebesExplosionGunshipPaletteFxColorDefinitions.FrameCount - 1; frame++)
        for (int index = 0; index < ZebesExplosionGunshipPaletteFxColorDefinitions.ColorsPerFrame; index++)
        {
            ushort pointer = ZebesExplosionGunshipPaletteFxColorDefinitions.ColorPointer(frame, index);
            if (EndingGunshipPaletteColorDefinitions.TryCalculatedColor(pointer, colors, out Bgr555 calculated) &&
                colors[pointer] == calculated) colors.Remove(pointer);
        }
        // Keep both endpoints and independent sample edits. Only values matching
        // their supplied endpoints can be reconstructed on demand.
        for (int frame = 1; frame < TourianStatueGreyPaletteFxColorDefinitions.FrameCount - 1; frame++)
        for (int index = 0; index < TourianStatueGreyPaletteFxColorDefinitions.ColorsPerFrame; index++)
        {
            ushort pointer = TourianStatueGreyPaletteFxColorDefinitions.ColorPointer(frame, index);
            if (TourianStatueGreyColorDefinitions.TryCalculatedColor(pointer, colors, out Bgr555 calculated) &&
                colors[pointer] == calculated) colors.Remove(pointer);
        }
        var surfaceLightning = CrateriaLightningColorDefinitions.SurfaceProgram;
        for (int frame = 0; frame < surfaceLightning.Frames.Count; frame++)
        for (int index = 0; index < surfaceLightning.ColorsPerFrame; index++)
        {
            ushort pointer = surfaceLightning.ColorPointer(frame, index);
            if (CrateriaLightningColorDefinitions.TryCalculatedColor(pointer, out Bgr555 calculated) &&
                colors[pointer] == calculated) colors.Remove(pointer);
        }
        var darkLightning = CrateriaLightningColorDefinitions.DarkProgram;
        for (int frame = 1; frame < darkLightning.Frames.Count; frame++)
        for (int index = 0; index < darkLightning.ColorsPerFrame; index++)
        {
            ushort pointer = darkLightning.ColorPointer(frame, index);
            if (CrateriaLightningColorDefinitions.TryCalculatedDarkColor(pointer, colors, out Bgr555 calculated) &&
                colors[pointer] == calculated) colors.Remove(pointer);
        }
        foreach (var program in PlanetZebesTextPaletteFxColorDefinitions.All)
        for (int frame = 0; frame < PlanetZebesTextPaletteFxColorDefinitions.FrameCount; frame++)
        for (int index = 0; index < PlanetZebesTextPaletteFxColorDefinitions.ColorsPerFrame; index++)
        {
            ushort pointer = program.ColorPointer(frame, index);
            if (pointer != PlanetZebesTextColorDefinitions.EndpointPointer(index)
                && PlanetZebesTextColorDefinitions.TryCalculate(pointer, colors, out Bgr555 calculated)
                && colors[pointer] == calculated) colors.Remove(pointer);
        }
        foreach (var program in MaridiaEnvironmentalPaletteFxColorDefinitions.All)
        for (int frame = 0; frame < program.FrameCount; frame++)
        for (int index = 0; index < program.ColorsPerFrame; index++)
        {
            ushort pointer = program.ColorPointer(frame, index);
            if (MaridiaEnvironmentalColorDefinitions.TrySourcePointer(pointer, out ushort source)
                && source != pointer && colors[pointer] == colors[source]) colors.Remove(pointer);
        }
        return new RoomPaletteFxPresentation(colors);
    }

    /// <summary>Serializes a current-version presentation document and validates it before writing any bytes to the destination.</summary>
    /// <param name="json">The stream that receives the validated UTF-8 JSON document.</param>
    /// <param name="document">The complete editable color document; animation timing and destinations remain defined by engine mechanics.</param>
    /// <exception cref="InvalidDataException">The document fails the current schema or palette-row validation.</exception>
    public static void Write(Stream json, RoomPaletteFxPresentationDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(
            document,
            ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }

    /// <summary>Validates one color family's frame geometry and stores each RGB5 sample at its native pointer.</summary>
    private static void ValidateAndCompile(
        string owner,
        PaletteRgb5[][]? frames,
        int frameCount,
        int colorCount,
        Func<int, int, ushort> colorPointer,
        Dictionary<ushort, Bgr555> destination)
    {
        if (frames is null || frames.Length != frameCount)
        {
            throw new InvalidDataException(
                $"Room palette-FX {owner} requires exactly {frameCount} frames.");
        }

        for (int frame = 0; frame < frames.Length; frame++)
        {
            PaletteRgb5[]? frameColors = frames[frame];
            if (frameColors is null || frameColors.Length != colorCount)
            {
                throw new InvalidDataException(
                    $"Room palette-FX {owner} frame {frame} requires exactly " +
                    $"{colorCount} colors.");
            }

            for (int index = 0; index < frameColors.Length; index++)
            {
                PaletteRgb5? color = frameColors[index];
                if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                    (uint)color.Blue > 31)
                {
                    throw new InvalidDataException(
                        $"Room palette-FX {owner} frame {frame} color {index} " +
                        "requires RGB components from 0 to 31.");
                }

                ushort pointer = colorPointer(frame, index);
                destination.Add(
                    pointer,
                    color.ToBgr555());
            }
        }
    }
}
