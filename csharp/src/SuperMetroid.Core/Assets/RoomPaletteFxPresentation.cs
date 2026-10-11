using SuperMetroid.Core.Hardware;
using System.Text.Json;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable colors for room and cinematic palette animations. Native timing, destinations,
/// instructions, and side effects remain compiled engine mechanics.
/// </summary>
public sealed class RoomPaletteFxPresentation : IPaletteFxColorSource
{
    internal readonly Dictionary<ushort, Bgr555> colors;

    private readonly HeatPaletteInputView heatInputs;
    private readonly LoadingPaletteInputView loadingInputs;
    private readonly EndingGunshipPaletteInputView gunshipInputs;

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
            json, MapPresentationFormat.JsonOptions, "room palette-FX presentation");

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
                    if (!previousVersionFallback!.TryReadColor(pointer, out Bgr555 stockColor))
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
        foreach (NorfairEnvironmentalPaletteFxProgramDefinition definition in
                 NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.All)
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
                NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.FrameCount,
                NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                definition.ColorPointer,
                colors);
        }
        foreach (MaridiaEnvironmentalPaletteFxProgramDefinition definition in
                 MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.All)
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
            WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.FrameCount,
            WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        ValidateAndCompile(
            "Red Brinstar background glow",
            document.RedBrinstarBackgroundGlow,
            RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.FrameCount,
            RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        ValidateAndCompile(
            "Tourian glow",
            document.TourianGlow,
            TourianGlowPaletteFxProgramMechanicsDefinitions.FrameCount,
            TourianGlowPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            TourianGlowPaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        foreach (BrinstarBlueSporePaletteFxProgramDefinition definition in
                 BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.All)
        {
            ValidateAndCompile(
                $"Brinstar blue spores ({definition.Owner})",
                document.BrinstarBlueSpores,
                BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.FrameCount,
                BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                definition.ColorPointer,
                colors);
        }
        foreach (TorizoBellyPaletteFxProgramDefinition definition in
                 TorizoBellyPaletteFxProgramMechanicsDefinitions.All)
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
                TorizoBellyPaletteFxProgramMechanicsDefinitions.FrameCount,
                TorizoBellyPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                definition.ColorPointer,
                colors);
        }
        ValidateAndCompile(
            "Tourian statue grey-out",
            document.TourianStatueGrey,
            TourianStatueGreyPaletteFxProgramMechanicsDefinitions.FrameCount,
            TourianStatueGreyPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            TourianStatueGreyPaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        foreach (CrateriaLightningPaletteFxProgramDefinition definition in
                 CrateriaLightningPaletteFxProgramMechanicsDefinitions.All)
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
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .GunshipEngineFrameCount,
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .GunshipEngineColorsPerFrame,
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .GunshipEngineColorPointer,
            colors);
        ValidateAndCompile(
            "Ceres navigation lights",
            document.CeresNavigationLights,
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .NavigationLightsFrameCount,
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .NavigationLightsColorsPerFrame,
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .NavigationLightsColorPointer,
            colors);
        foreach (PlanetZebesTextPaletteFxProgramDefinition definition in
                 PlanetZebesTextPaletteFxProgramMechanicsDefinitions.All)
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
                PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameCount,
                PlanetZebesTextPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                definition.ColorPointer,
                colors);
        }
        foreach (CinematicGlowPaletteFxProgramDefinition definition in
                 CinematicGlowPaletteFxProgramMechanicsDefinitions.All)
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
                CinematicGlowPaletteFxProgramMechanicsDefinitions.FrameCount,
                definition.ColorsPerFrame,
                definition.ColorPointer,
                colors);
        }
        ValidateAndCompile(
            "exploding Zebes fade",
            document.ExplodingZebesFade,
            ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.FrameCount,
            ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        ValidateAndCompile(
            "unused cinematic fade",
            document.UnusedCinematicFade,
            UnusedCinematicFadePaletteFxProgramMechanicsDefinitions.FrameCount,
            UnusedCinematicFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            UnusedCinematicFadePaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        ValidateAndCompile(
            "title-logo fade",
            document.TitleLogoFade,
            TitleLogoFadePaletteFxProgramMechanicsDefinitions.FrameCount,
            TitleLogoFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            TitleLogoFadePaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        ValidateAndCompile(
            "Nintendo shared fade",
            document.NintendoSharedFade,
            NintendoLogoFadePaletteFxProgramMechanicsDefinitions.FrameCount,
            NintendoLogoFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            NintendoLogoFadePaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        ValidateAndCompile(
            "Zebes explosion foreground",
            document.ZebesExplosionForeground,
            ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.FrameCount,
            ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        ValidateAndCompile(
            "Zebes explosion finale",
            document.ZebesExplosionFinale,
            ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions.FrameCount,
            ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        ValidateAndCompile(
            "Zebes explosion shared whiteout",
            document.ZebesExplosionWhiteout,
            ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions.FrameCount,
            ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        foreach (ZebesExplosionAmbientPaletteFxProgramDefinition definition in
                 ZebesExplosionAmbientPaletteFxProgramMechanicsDefinitions.All)
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
        foreach (ZebesExplosionLayerFadePaletteFxProgramDefinition definition in
                 ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.All)
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
                ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.FrameCount,
                ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                definition.ColorPointer,
                colors);
        }
        ValidateAndCompile(
            "Zebes explosion gunship",
            document.ZebesExplosionGunship,
            ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.FrameCount,
            ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        foreach (SamusLoadingSuitPaletteFxProgramDefinition definition in
                 SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.All)
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
                SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FrameCount,
                SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                definition.ColorPointer,
                colors);
        }
        ValidateAndCompile(
            "post-credits icon glare",
            document.PostCreditsIconGlare,
            PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.FrameCount,
            PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        foreach (TourianEscapeRedFlashPaletteFxProgramDefinition definition in
                 TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.All)
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
                TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount,
                definition.ColorsPerFrame, definition.ColorPointer, colors);
        }
        ValidateAndCompile(
            "Tourian escape shared red flash",
            document.TourianEscapeSharedRedFlash,
            TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount,
            TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        ValidateAndCompile(
            "old Tourian escape red flash",
            document.OldTourianEscapeRedFlash,
            OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount,
            OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        foreach (OldTourianEscapeAccentPaletteFxProgramDefinition definition in
                 OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.All)
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
                OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.FrameCount,
                OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                definition.ColorPointer, colors);
        }
        ValidateAndCompile(
            "upper Crateria escape red flash",
            document.UpperCrateriaEscapeRedFlash,
            UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount,
            UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        foreach (CrateriaEscapeLightningPaletteFxProgramDefinition definition in
                 CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.All)
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
                CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.FrameCount,
                definition.ColorsPerFrame, definition.ColorPointer, colors);
        }
        ValidateAndCompile(
            "Crateria and Brinstar beacon flash",
            document.BeaconFlashing,
            BeaconPaletteFxProgramMechanicsDefinitions.FrameCount,
            BeaconPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            BeaconPaletteFxProgramMechanicsDefinitions.ColorPointer,
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
        foreach (var program in SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.All)
        for (int frame = 0; frame < 9; frame++)
        for (int index = 0; index < 16; index++)
        {
            ushort pointer = program.ColorPointer(frame, index);
            if (LoadingPaletteColorDefinitions.TryCanonicalPointer(pointer, out ushort canonical) &&
                canonical != pointer && colors[pointer] == colors[canonical]) colors.Remove(pointer);
        }
        foreach (var program in SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.All)
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
        for (int frame = 0; frame < PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.FrameCount - 1; frame++)
        for (int index = 0; index < PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.ColorsPerFrame; index++)
        {
            ushort pointer = PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.ColorPointer(frame, index);
            if (LogoGlarePaletteColorDefinitions.TryCalculatedColor(pointer, colors, out Bgr555 calculated) &&
                colors[pointer] == calculated) colors.Remove(pointer);
        }
        for (int frame = 0; frame < ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.FrameCount - 1; frame++)
        for (int index = 0; index < ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.ColorsPerFrame; index++)
        {
            ushort pointer = ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.ColorPointer(frame, index);
            if (EndingGunshipPaletteColorDefinitions.TryCalculatedColor(pointer, colors, out Bgr555 calculated) &&
                colors[pointer] == calculated) colors.Remove(pointer);
        }
        // Keep both endpoints and independent sample edits. Only values matching
        // their supplied endpoints can be reconstructed on demand.
        for (int frame = 1; frame < TourianStatueGreyPaletteFxProgramMechanicsDefinitions.FrameCount - 1; frame++)
        for (int index = 0; index < TourianStatueGreyPaletteFxProgramMechanicsDefinitions.ColorsPerFrame; index++)
        {
            ushort pointer = TourianStatueGreyPaletteFxProgramMechanicsDefinitions.ColorPointer(frame, index);
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
        foreach (var program in PlanetZebesTextPaletteFxProgramMechanicsDefinitions.All)
        for (int frame = 0; frame < PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameCount; frame++)
        for (int index = 0; index < PlanetZebesTextPaletteFxProgramMechanicsDefinitions.ColorsPerFrame; index++)
        {
            ushort pointer = program.ColorPointer(frame, index);
            if (pointer != PlanetZebesTextColorDefinitions.EndpointPointer(index)
                && PlanetZebesTextColorDefinitions.TryCalculate(pointer, colors, out Bgr555 calculated)
                && colors[pointer] == calculated) colors.Remove(pointer);
        }
        foreach (var program in MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.All)
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
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }

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

/// <summary>The editable JSON schema for color samples used by room and cinematic palette-FX programs.</summary>
/// <remarks>Each jagged array is ordered by native timed record, then by that record's emitted colors. RGB channels range from zero through thirty-one; record durations, skipped palette entries, destinations, and control side effects are compiled mechanics.</remarks>
public sealed record RoomPaletteFxPresentationDocument
{
    /// <summary>Gets the schema version; current documents use eighteen, while seventeen requires a current-stock fallback for the absent heat rows.</summary>
    public required int Version { get; init; }
    /// <summary>Sixteen native phases of Samus's Power Suit palette in heat.</summary>
    public PaletteRgb5[][]? SamusHeatPowerSuit { get; init; }
    /// <summary>Sixteen native phases of Samus's Varia Suit palette in heat.</summary>
    public PaletteRgb5[][]? SamusHeatVariaSuit { get; init; }
    /// <summary>Sixteen native phases of Samus's Gravity Suit palette in heat.</summary>
    public PaletteRgb5[][]? SamusHeatGravitySuit { get; init; }
    /// <summary>Gets sixteen five-color phases for foreground palette three, synchronized with the compiled phase publication driving Samus's heat palette.</summary>
    public required PaletteRgb5[][] NorfairForegroundAndHeatPhase { get; init; }
    /// <summary>Gets sixteen five-color phases for Norfair foreground palette four; native skip commands retain the intervening CGRAM entries.</summary>
    public required PaletteRgb5[][] NorfairForegroundPalette4 { get; init; }
    /// <summary>Gets sixteen five-color phases for Norfair foreground palette five, synchronized with the environmental heat cycle.</summary>
    public required PaletteRgb5[][] NorfairForegroundPalette5 { get; init; }
    /// <summary>Gets sixteen five-color phases for Norfair foreground palette six, synchronized with the environmental heat cycle.</summary>
    public required PaletteRgb5[][] NorfairForegroundPalette6 { get; init; }
    /// <summary>Gets four eight-color sand-pit rotation records, containing two four-color sand bands per phase.</summary>
    public required PaletteRgb5[][] MaridiaSandPits { get; init; }
    /// <summary>Gets four four-color rotation phases for Maridia's falling sand.</summary>
    public required PaletteRgb5[][] MaridiaSandFalls { get; init; }
    /// <summary>Gets eight eight-color rotation phases for Maridia's background waterfalls.</summary>
    public required PaletteRgb5[][] MaridiaBackgroundWaterfalls { get; init; }
    /// <summary>Gets eight two-color phases for the Wrecked Ship green-light cycle.</summary>
    public required PaletteRgb5[][] WreckedShipGreenLights { get; init; }
    /// <summary>Gets fourteen eight-color records for Red Brinstar's background glow.</summary>
    public required PaletteRgb5[][] RedBrinstarBackgroundGlow { get; init; }
    /// <summary>Gets eleven eight-color records shared by the live and cloned Tourian glow programs.</summary>
    public required PaletteRgb5[][] TourianGlow { get; init; }
    /// <summary>Gets fourteen three-color phases shared by ordinary Brinstar rooms and the Spore Spawn room's separately controlled program.</summary>
    public required PaletteRgb5[][] BrinstarBlueSpores { get; init; }
    /// <summary>Gets six three-color belly phases for Bomb Torizo, with boss-dependent lifetime retained in mechanics.</summary>
    public required PaletteRgb5[][] BombTorizoBelly { get; init; }
    /// <summary>Gets six three-color belly phases for Golden Torizo, with its own native program identity.</summary>
    public required PaletteRgb5[][] GoldenTorizoBelly { get; init; }
    /// <summary>Gets eight eight-color grey-out records for the Tourian statues; matching intermediate samples can be derived from the supplied endpoints.</summary>
    public required PaletteRgb5[][] TourianStatueGrey { get; init; }
    /// <summary>Gets thirteen eight-color records for surface lightning, including the long neutral holds and brief flashes defined by the native program.</summary>
    public required PaletteRgb5[][] CrateriaSurfaceLightning { get; init; }
    /// <summary>Gets fourteen seven-color records for the distinct unused dark-lightning program; independent sample edits remain addressable.</summary>
    public required PaletteRgb5[][] CrateriaUnusedDarkLightning { get; init; }
    /// <summary>Gets two single-color records for the Ceres gunship engine-light loop.</summary>
    public required PaletteRgb5[][] CeresGunshipEngineLights { get; init; }
    /// <summary>Gets fourteen two-color navigation-light records shared by the sprite and background destination programs.</summary>
    public required PaletteRgb5[][] CeresNavigationLights { get; init; }
    /// <summary>Gets eight three-color PLANET ZEBES text fade-in records, with matching samples reconstructed from the editable endpoint colors.</summary>
    public required PaletteRgb5[][] PlanetZebesTextFadeIn { get; init; }
    /// <summary>Gets eight three-color PLANET ZEBES text fade-out records using the separate native fade-out program.</summary>
    public required PaletteRgb5[][] PlanetZebesTextFadeOut { get; init; }
    /// <summary>Gets fourteen three-color glow records for the old Mother Brain cinematic background lights, held for six updates per record.</summary>
    public required PaletteRgb5[][] OldMotherBrainBackgroundLights { get; init; }
    /// <summary>Gets fourteen single-color cinematic gunship glow records, held for five updates per record.</summary>
    public required PaletteRgb5[][] CinematicGunshipGlow { get; init; }
    /// <summary>Gets seven eight-color records for the exploding-Zebes cinematic fade.</summary>
    public required PaletteRgb5[][] ExplodingZebesFade { get; init; }
    /// <summary>Gets eleven sixteen-color records for the native unused cinematic fade program.</summary>
    public required PaletteRgb5[][] UnusedCinematicFade { get; init; }
    /// <summary>Gets eight fifteen-color records for the title-logo fade.</summary>
    public required PaletteRgb5[][] TitleLogoFade { get; init; }
    /// <summary>Gets eight two-color records for the shared Nintendo-logo fade.</summary>
    public required PaletteRgb5[][] NintendoSharedFade { get; init; }
    /// <summary>Gets sixteen fifteen-color records for the expanding Zebes explosion foreground.</summary>
    public required PaletteRgb5[][] ZebesExplosionForeground { get; init; }
    /// <summary>Gets forty-five fifteen-color finale records, preserving the mechanics' distinct fast and slow phases.</summary>
    public required PaletteRgb5[][] ZebesExplosionFinale { get; init; }
    /// <summary>Gets fifteen single-color records shared by the wide-explosion background and space-whiteout programs.</summary>
    public required PaletteRgb5[][] ZebesExplosionWhiteout { get; init; }
    /// <summary>Gets six eight-color planet-afterglow records played on the native ninety-six-update cycle.</summary>
    public required PaletteRgb5[][] ZebesExplosionAfterglow { get; init; }
    /// <summary>Gets ten single-color lava records played on the native seventy-update cycle.</summary>
    public required PaletteRgb5[][] ZebesExplosionLava { get; init; }
    /// <summary>Gets eight fifteen-color fade records for the planet's crust layer.</summary>
    public required PaletteRgb5[][] ZebesExplosionCrust { get; init; }
    /// <summary>Gets eight fifteen-color fade records for the explosion's grey-cloud layer.</summary>
    public required PaletteRgb5[][] ZebesExplosionGreyClouds { get; init; }
    /// <summary>Gets sixteen sixteen-color gunship fade records; matching intermediate colors can be calculated from the retained endpoint row.</summary>
    public required PaletteRgb5[][] ZebesExplosionGunship { get; init; }
    /// <summary>Gets nine sixteen-color Power Suit loading records: four two-record groups followed by the terminal palette row.</summary>
    public required PaletteRgb5[][] SamusLoadingPowerSuit { get; init; }
    /// <summary>Gets nine sixteen-color Varia Suit loading records with the same compiled group timing as the other suits.</summary>
    public required PaletteRgb5[][] SamusLoadingVariaSuit { get; init; }
    /// <summary>Gets nine sixteen-color Gravity Suit loading records with the same compiled group timing as the other suits.</summary>
    public required PaletteRgb5[][] SamusLoadingGravitySuit { get; init; }
    /// <summary>Gets fourteen sixteen-color post-credits icon glare records; matching samples can be derived from the retained base colors.</summary>
    public required PaletteRgb5[][] PostCreditsIconGlare { get; init; }
    /// <summary>Gets fourteen two-color red-flash records for the Tourian escape shutter, held for six updates per record.</summary>
    public required PaletteRgb5[][] TourianEscapeShutter { get; init; }
    /// <summary>Gets fourteen four-color red-flash records for the Tourian escape background, held for four updates per record.</summary>
    public required PaletteRgb5[][] TourianEscapeBackground { get; init; }
    /// <summary>Gets fourteen seven-color red-flash records shared by the general level and Arkanoid-block/red-orb destination programs.</summary>
    public required PaletteRgb5[][] TourianEscapeSharedRedFlash { get; init; }
    /// <summary>Gets fourteen eight-color records for the old Tourian escape's red-flash program.</summary>
    public required PaletteRgb5[][] OldTourianEscapeRedFlash { get; init; }
    /// <summary>Gets fifteen three-color accent records for the old Tourian escape's orange railings.</summary>
    public required PaletteRgb5[][] OldTourianEscapeOrangeRailings { get; init; }
    /// <summary>Gets fifteen three-color accent records for the old Tourian escape's yellow panels.</summary>
    public required PaletteRgb5[][] OldTourianEscapeYellowPanels { get; init; }
    /// <summary>Gets fourteen seven-color upper-Crateria escape red-flash records with the native variable hold durations.</summary>
    public required PaletteRgb5[][] UpperCrateriaEscapeRedFlash { get; init; }
    /// <summary>Gets eleven eleven-color yellow-lightning records for Crateria's escape palette.</summary>
    public required PaletteRgb5[][] CrateriaEscapeYellowLightning { get; init; }
    /// <summary>Gets eleven five-color records for the shared CRE-block pixel color tail during Crateria escape lightning.</summary>
    public required PaletteRgb5[][] CrateriaEscapeCreBlockPixel { get; init; }
    /// <summary>Gets ten four-color beacon-flash records shared by Crateria and Brinstar, retaining the native sound instruction between record groups.</summary>
    public required PaletteRgb5[][] BeaconFlashing { get; init; }
}

/// <summary>The supported schema revisions of the room palette-FX presentation document.</summary>
internal enum RoomPaletteFxPresentationRevision
{
    /// <summary>Revision 17: the Samus-in-heat rows are inherited from current stock.</summary>
    Previous = RoomPaletteFxPresentationFormat.PreviousVersion,
    /// <summary>Revision 18: every color family is supplied by the document.</summary>
    Current = RoomPaletteFxPresentationFormat.Version,
}

/// <summary>File identity and schema versions for editable room/cinematic palette-FX color assets.</summary>
public static class RoomPaletteFxPresentationFormat
{
    /// <summary>The presentation filename resolved for the room and cinematic palette-FX color catalog.</summary>
    public const string FileName = "room-palette-effects.json";
    /// <summary>Version seventeen, whose existing color edits can be retained while missing Samus heat rows come from current stock.</summary>
    public const int PreviousVersion = 17;
    /// <summary>Version eighteen, requiring all room/cinematic color families including the three Samus-in-heat suit palettes.</summary>
    public const int Version = 18;
}
