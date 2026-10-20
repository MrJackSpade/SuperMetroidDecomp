using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// Editable colors for room and cinematic palette animations. Native timing, destinations,
/// instructions, and side effects remain compiled engine mechanics.
/// </summary>
public sealed class RoomPaletteFxPresentation : IPaletteFxColorSource
{
    /// <summary>Explicit RGB5 samples by the record color each one is.</summary>
    internal readonly Dictionary<PaletteFxColorKey, Bgr555> colors;

    /// <summary>Canonical and derived heat-palette samples shared across Samus suits.</summary>
    internal readonly HeatPaletteInputView heatInputs;
    /// <summary>Editable and derived Samus loading-palette samples.</summary>
    internal readonly LoadingPaletteInputView loadingInputs;
    /// <summary>Endpoint samples used to reconstruct the ending gunship fade.</summary>
    private readonly EndingGunshipPaletteInputView gunshipInputs;

    /// <summary>Builds calculated-color views over validated, explicitly authored samples.</summary>
    private RoomPaletteFxPresentation(Dictionary<PaletteFxColorKey, Bgr555> colors)
    {
        this.colors = colors;
        heatInputs = new HeatPaletteInputView(colors);
        loadingInputs = new LoadingPaletteInputView(colors);
        gunshipInputs = new EndingGunshipPaletteInputView(colors);
    }

    /// <inheritdoc />
    public bool TryReadColor(PaletteFxColorKey key, out Bgr555 color) =>
        gunshipInputs.TryGetValue(key, out color) ||
        PlanetZebesTextColorDefinitions.TryCalculate(key, colors, out color) ||
        MaridiaEnvironmentalColorDefinitions.TryReadColor(key, colors, out color) ||
        CrateriaLightningColorDefinitions.TryCalculatedColor(key, out color) ||
        CrateriaLightningColorDefinitions.TryCalculatedDarkColor(key, colors, out color) ||
        TourianStatueGreyColorDefinitions.TryCalculatedColor(key, colors, out color) ||
        LoadingPaletteColorDefinitions.TryReadColor(key, loadingInputs, out color) ||
        LogoGlarePaletteColorDefinitions.TryCalculatedColor(key, colors, out color) ||
        EndingGunshipPaletteColorDefinitions.TryCalculatedColor(key, gunshipInputs, out color) ||
        (HeatPaletteColorDefinitions.TryCanonical(key, out PaletteFxColorKey canonical) &&
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

        var colors = new Dictionary<PaletteFxColorKey, Bgr555>();
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
            if (inheritsHeatRows)
            {
                // A v17 player override predates these three color families. Keep
                // all its existing edits and inherit only the newly extracted
                // Samus-in-heat rows from the verified current stock catalog.
                for (int frame = 0; frame < definition.Rows.FrameCount; frame++)
                for (int index = 0; index < definition.Rows.ColorsPerFrame; index++)
                {
                    PaletteFxColorKey key = definition.Rows.Color(frame, index);
                    if (!(previousVersionFallback ?? throw new InvalidDataException("A previous-version room palette-FX override requires the current stock presentation.")).TryReadColor(key, out Bgr555 stockColor))
                        throw new InvalidDataException(
                            $"Current stock is missing {definition.Suit} heat color {key}.");
                    colors.Add(key, stockColor);
                }
            }
            else
                ValidateAndCompile($"Samus {definition.Suit} suit in heat", frames, definition.Rows, colors);
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
            ValidateAndCompile($"Norfair {definition.Owner}", frames, definition.Rows, colors);
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
            ValidateAndCompile($"Maridia {definition.Owner}", frames, definition.Rows, colors);
        }
        ValidateAndCompile("Wrecked Ship green lights", document.WreckedShipGreenLights, WreckedShipGreenLightPaletteFxColorDefinitions.Rows, colors);
        ValidateAndCompile("Red Brinstar background glow", document.RedBrinstarBackgroundGlow, RedBrinstarGlowPaletteFxColorDefinitions.Rows, colors);
        ValidateAndCompile("Tourian glow", document.TourianGlow, TourianGlowPaletteFxColorDefinitions.Rows, colors);
        foreach (BrinstarBlueSporePaletteFxColorDefinition definition in
                 BrinstarBlueSporePaletteFxColorDefinitions.All)
        {
            ValidateAndCompile($"Brinstar blue spores ({definition.Owner})", document.BrinstarBlueSpores, definition.Rows, colors);
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
            ValidateAndCompile($"{definition.Owner} belly", frames, definition.Rows, colors);
        }
        ValidateAndCompile("Tourian statue grey-out", document.TourianStatueGrey, TourianStatueGreyPaletteFxColorDefinitions.Rows, colors);
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
            ValidateAndCompile($"Crateria {definition.Owner}", frames, definition.Rows, colors);
        }
        ValidateAndCompile("Ceres gunship-engine lights", document.CeresGunshipEngineLights, CeresCinematicLightPaletteFxColorDefinitions.GunshipEngineRows, colors);
        ValidateAndCompile("Ceres navigation lights", document.CeresNavigationLights, CeresCinematicLightPaletteFxColorDefinitions.NavigationLightsRows, colors);
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
            ValidateAndCompile($"PLANET ZEBES {definition.Owner}", frames, definition.Rows, colors);
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
            ValidateAndCompile($"Cinematic glow {definition.Owner}", frames, definition.Rows, colors);
        }
        ValidateAndCompile("exploding Zebes fade", document.ExplodingZebesFade, ExplodingZebesFadePaletteFxColorDefinitions.Rows, colors);
        ValidateAndCompile("unused cinematic fade", document.UnusedCinematicFade, UnusedCinematicFadePaletteFxColorDefinitions.Rows, colors);
        ValidateAndCompile("title-logo fade", document.TitleLogoFade, TitleLogoFadePaletteFxColorDefinitions.Rows, colors);
        ValidateAndCompile("Nintendo shared fade", document.NintendoSharedFade, NintendoLogoFadePaletteFxColorDefinitions.Rows, colors);
        ValidateAndCompile("Zebes explosion foreground", document.ZebesExplosionForeground, ZebesExplosionForegroundPaletteFxColorDefinitions.Rows, colors);
        ValidateAndCompile("Zebes explosion finale", document.ZebesExplosionFinale, ZebesExplosionFinalePaletteFxColorDefinitions.Rows, colors);
        ValidateAndCompile("Zebes explosion shared whiteout", document.ZebesExplosionWhiteout, ZebesExplosionWhiteoutPaletteFxColorDefinitions.Rows, colors);
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
            ValidateAndCompile($"Zebes explosion {definition.Owner}", frames, definition.Rows, colors);
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
            ValidateAndCompile($"Zebes explosion {definition.Owner}", frames, definition.Rows, colors);
        }
        ValidateAndCompile("Zebes explosion gunship", document.ZebesExplosionGunship, ZebesExplosionGunshipPaletteFxColorDefinitions.Rows, colors);
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
            ValidateAndCompile($"Samus loading {definition.Owner}", frames, definition.Rows, colors);
        }
        ValidateAndCompile("post-credits icon glare", document.PostCreditsIconGlare, PostCreditsIconGlarePaletteFxColorDefinitions.Rows, colors);
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
            ValidateAndCompile($"Tourian escape {definition.Owner}", frames, definition.Rows, colors);
        }
        ValidateAndCompile("Tourian escape shared red flash", document.TourianEscapeSharedRedFlash, TourianEscapeSharedRedFlashPaletteFxColorDefinitions.Rows, colors);
        ValidateAndCompile("old Tourian escape red flash", document.OldTourianEscapeRedFlash, OldTourianEscapeRedFlashPaletteFxColorDefinitions.Rows, colors);
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
            ValidateAndCompile($"old Tourian escape {definition.Owner}", frames, definition.Rows, colors);
        }
        ValidateAndCompile("upper Crateria escape red flash", document.UpperCrateriaEscapeRedFlash, UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.Rows, colors);
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
            ValidateAndCompile($"Crateria escape {definition.Owner}", frames, definition.Rows, colors);
        }
        ValidateAndCompile("Crateria and Brinstar beacon flash", document.BeaconFlashing, BeaconPaletteFxColorDefinitions.Rows, colors);

        // Collapse only values equal to the calculated row alias. Differing player
        // edits keep their original per-frame identity, including edits to base rows.
        foreach (var program in PaletteFxHeatProgramMechanicsDefinitions.All)
        foreach (PaletteFxColorKey key in Keys(program.Rows))
        {
            if (HeatPaletteColorDefinitions.TryCanonical(key, out PaletteFxColorKey canonical) &&
                canonical != key && colors[key] == colors[canonical]) colors.Remove(key);
        }
        foreach (var program in PaletteFxHeatProgramMechanicsDefinitions.All)
        foreach (PaletteFxColorKey key in Keys(program.Rows))
        {
            if (colors.TryGetValue(key, out Bgr555 supplied) &&
                HeatPaletteColorDefinitions.TryCalculatedColor(key, colors, out Bgr555 calculated) && supplied == calculated)
                colors.Remove(key);
        }
        foreach (var program in SamusLoadingSuitPaletteFxColorDefinitions.All)
        foreach (PaletteFxColorKey key in Keys(program.Rows))
        {
            if (LoadingPaletteColorDefinitions.TryCanonical(key, out PaletteFxColorKey canonical) &&
                canonical != key && colors[key] == colors[canonical]) colors.Remove(key);
        }
        foreach (var program in SamusLoadingSuitPaletteFxColorDefinitions.All)
        foreach (PaletteFxColorKey key in Keys(program.Rows))
        {
            if (colors.TryGetValue(key, out Bgr555 supplied) &&
                LoadingPaletteColorDefinitions.TryCalculatedColor(key, colors, out Bgr555 calculated) && supplied == calculated)
                colors.Remove(key);
        }
        // Only matching samples are discarded. If the player edits a base color,
        // unchanged intermediate samples that no longer fit remain explicit values.
        foreach (PaletteFxColorKey key in Keys(PostCreditsIconGlarePaletteFxColorDefinitions.Rows, lastFrame: PostCreditsIconGlarePaletteFxColorDefinitions.FrameCount - 1))
        {
            if (LogoGlarePaletteColorDefinitions.TryCalculatedColor(key, colors, out Bgr555 calculated) &&
                colors[key] == calculated) colors.Remove(key);
        }
        foreach (PaletteFxColorKey key in Keys(ZebesExplosionGunshipPaletteFxColorDefinitions.Rows, lastFrame: ZebesExplosionGunshipPaletteFxColorDefinitions.FrameCount - 1))
        {
            if (EndingGunshipPaletteColorDefinitions.TryCalculatedColor(key, colors, out Bgr555 calculated) &&
                colors[key] == calculated) colors.Remove(key);
        }
        // Keep both endpoints and independent sample edits. Only values matching
        // their supplied endpoints can be reconstructed on demand.
        foreach (PaletteFxColorKey key in Keys(TourianStatueGreyPaletteFxColorDefinitions.Rows, firstFrame: 1, lastFrame: TourianStatueGreyPaletteFxColorDefinitions.FrameCount - 1))
        {
            if (TourianStatueGreyColorDefinitions.TryCalculatedColor(key, colors, out Bgr555 calculated) &&
                colors[key] == calculated) colors.Remove(key);
        }
        foreach (PaletteFxColorKey key in Keys(CrateriaLightningColorDefinitions.SurfaceProgram.Rows))
        {
            if (CrateriaLightningColorDefinitions.TryCalculatedColor(key, out Bgr555 calculated) &&
                colors[key] == calculated) colors.Remove(key);
        }
        foreach (PaletteFxColorKey key in Keys(CrateriaLightningColorDefinitions.DarkProgram.Rows, firstFrame: 1))
        {
            if (CrateriaLightningColorDefinitions.TryCalculatedDarkColor(key, colors, out Bgr555 calculated) &&
                colors[key] == calculated) colors.Remove(key);
        }
        foreach (var program in PlanetZebesTextPaletteFxColorDefinitions.All)
        for (int frame = 0; frame < PlanetZebesTextPaletteFxColorDefinitions.FrameCount; frame++)
        for (int index = 0; index < PlanetZebesTextPaletteFxColorDefinitions.ColorsPerFrame; index++)
        {
            PaletteFxColorKey key = program.Color(frame, index);
            if (key != PlanetZebesTextColorDefinitions.Endpoint(index)
                && PlanetZebesTextColorDefinitions.TryCalculate(key, colors, out Bgr555 calculated)
                && colors[key] == calculated) colors.Remove(key);
        }
        foreach (var program in MaridiaEnvironmentalPaletteFxColorDefinitions.All)
        foreach (PaletteFxColorKey key in Keys(program.Rows))
        {
            if (MaridiaEnvironmentalColorDefinitions.TrySource(key, out PaletteFxColorKey source)
                && source != key && colors[key] == colors[source]) colors.Remove(key);
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

    /// <summary>Every color of <paramref name="rows"/> from <paramref name="firstFrame"/> up to <paramref name="lastFrame"/>, by row then color.</summary>
    private static IEnumerable<PaletteFxColorKey> Keys(PaletteFxColorRows rows, int firstFrame = 0, int? lastFrame = null)
    {
        for (int frame = firstFrame; frame < (lastFrame ?? rows.FrameCount); frame++)
        for (int color = 0; color < rows.ColorsPerFrame; color++)
            yield return rows.Color(frame, color);
    }

    /// <summary>Validates one color family's frame geometry and stores each RGB5 sample by its record color.</summary>
    private static void ValidateAndCompile(
        string owner,
        PaletteRgb5[][]? frames,
        PaletteFxColorRows rows,
        Dictionary<PaletteFxColorKey, Bgr555> destination)
    {
        int frameCount = rows.FrameCount;
        int colorCount = rows.ColorsPerFrame;
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

                destination.Add(
                    rows.Color(frame, index),
                    color.ToBgr555());
            }
        }
    }
}
