using SuperMetroid.AssetExtraction.Fx.PaletteFx;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// One colour program of the installed room palette-effect document (#2557): the key its colours are stored under, how a
/// failure names it, and the record colours it holds, each record run validated against the document's frames. The
/// Brinstar blue spores' ordinary and Spore Spawn programs share one entry.
/// </summary>
/// <param name="Key">The key the program's frames are stored under in the document.</param>
/// <param name="Runs">The record runs the stored frames give colours to, each named as a failure names it; extraction reads the first.</param>
internal sealed record RoomPaletteFxProgram(string Key, IReadOnlyList<RoomPaletteFxRun> Runs);

/// <summary>One record run of a room palette-effect program and how a failure names it.</summary>
/// <param name="Owner">How a failure names the run.</param>
/// <param name="Frames">The records the run's colours go to.</param>
internal sealed record RoomPaletteFxRun(string Owner, PaletteFxColorFrames Frames);

/// <summary>Every colour program of the room palette-effect document, each listed once, in the document's order.</summary>
internal static class RoomPaletteFxPrograms
{
    /// <summary>The programs, in the order the document stores them.</summary>
    internal static IReadOnlyList<RoomPaletteFxProgram> All { get; } =
    [
        new("samusHeatPowerSuit", [new("Samus Power suit in heat", PaletteFxHeatPulseColorDefinitions.All.Single(item => item.Suit == ColourEffectHeatSuit.Power).ColorFrames)]),
        new("samusHeatVariaSuit", [new("Samus Varia suit in heat", PaletteFxHeatPulseColorDefinitions.All.Single(item => item.Suit == ColourEffectHeatSuit.Varia).ColorFrames)]),
        new("samusHeatGravitySuit", [new("Samus Gravity suit in heat", PaletteFxHeatPulseColorDefinitions.All.Single(item => item.Suit == ColourEffectHeatSuit.Gravity).ColorFrames)]),
        new("norfairForegroundAndHeatPhase", [new("Norfair foreground and heat phase", NorfairEnvironmentalPaletteFxColorDefinitions.NorfairForegroundAndHeatPhase.ColorFrames)]),
        new("norfairGreenForeground", [new("Norfair green foreground", NorfairEnvironmentalPaletteFxColorDefinitions.NorfairGreenForegroundHeatGlow.ColorFrames)]),
        new("norfairPurpleForeground", [new("Norfair purple foreground", NorfairEnvironmentalPaletteFxColorDefinitions.NorfairPurpleForegroundHeatGlow.ColorFrames)]),
        new("norfairDarkForeground", [new("Norfair dark foreground", NorfairEnvironmentalPaletteFxColorDefinitions.NorfairDarkForegroundHeatGlow.ColorFrames)]),
        new("maridiaSandPits", [new("Maridia sand pits", MaridiaEnvironmentalPaletteFxColorDefinitions.MaridiaSandPits.ColorFrames)]),
        new("maridiaSandFalls", [new("Maridia sand falls", MaridiaEnvironmentalPaletteFxColorDefinitions.MaridiaSandFalls.ColorFrames)]),
        new("maridiaBackgroundWaterfalls", [new("Maridia background waterfalls", MaridiaEnvironmentalPaletteFxColorDefinitions.MaridiaBackgroundWaterfalls.ColorFrames)]),
        new("wreckedShipGreenLights", [new("Wrecked Ship green lights", WreckedShipGreenLightPaletteFxColorDefinitions.ColorFrames)]),
        new("redBrinstarBackgroundGlow", [new("Red Brinstar background glow", RedBrinstarGlowPaletteFxColorDefinitions.ColorFrames)]),
        new("tourianGlow", [new("Tourian glow", TourianGlowPaletteFxColorDefinitions.ColorFrames)]),
        new("brinstarBlueSpores", [.. BrinstarBlueSporePaletteFxColorDefinitions.All.Select(definition => new RoomPaletteFxRun($"Brinstar blue spores ({definition.Effect})", definition.ColorFrames))]),
        new("bombTorizoBelly", [new("Bomb Torizo belly", TorizoBellyPaletteFxColorDefinitions.BombTorizoBelly.ColorFrames)]),
        new("goldenTorizoBelly", [new("Golden Torizo belly", TorizoBellyPaletteFxColorDefinitions.GoldenTorizoBelly.ColorFrames)]),
        new("tourianStatueGrey", [new("Tourian statue grey-out", TourianStatueGreyPaletteFxColorDefinitions.ColorFrames)]),
        new("crateriaSurfaceLightning", [new("Crateria surface lightning", CrateriaLightningPaletteFxColorDefinitions.Surface.ColorFrames)]),
        new("ceresGunshipEngineLights", [new("Ceres gunship-engine lights", CeresCinematicLightPaletteFxColorDefinitions.GunshipEngineFrames)]),
        new("ceresNavigationLights", [new("Ceres navigation lights", CeresCinematicLightPaletteFxColorDefinitions.NavigationLightsFrames)]),
        new("planetZebesTextFadeIn", [new("PLANET ZEBES fade-in", PlanetZebesTextPaletteFxColorDefinitions.FadeInPlanetZebesText.ColorFrames)]),
        new("planetZebesTextFadeOut", [new("PLANET ZEBES fade-out", PlanetZebesTextPaletteFxColorDefinitions.FadeOutPlanetZebesText.ColorFrames)]),
        new("oldMotherBrainBackgroundLights", [new("cinematic old Mother Brain background lights", CinematicGlowPaletteFxColorDefinitions.OldMotherBrainFightBackgroundLights.ColorFrames)]),
        new("cinematicGunshipGlow", [new("cinematic gunship glow", CinematicGlowPaletteFxColorDefinitions.GunshipGlow.ColorFrames)]),
        new("explodingZebesFade", [new("exploding Zebes fade", ExplodingZebesFadePaletteFxColorDefinitions.ColorFrames)]),
        new("titleLogoFade", [new("title-logo fade", TitleLogoFadePaletteFxColorDefinitions.ColorFrames)]),
        new("nintendoSharedFade", [new("Nintendo shared fade", NintendoLogoFadePaletteFxColorDefinitions.ColorFrames)]),
        new("zebesExplosionForeground", [new("Zebes explosion foreground", ZebesExplosionForegroundPaletteFxColorDefinitions.ColorFrames)]),
        new("zebesExplosionFinale", [new("Zebes explosion finale", ZebesExplosionFinalePaletteFxColorDefinitions.ColorFrames)]),
        new("zebesExplosionWhiteout", [new("Zebes explosion shared whiteout", ZebesExplosionWhiteoutPaletteFxColorDefinitions.ColorFrames)]),
        new("zebesExplosionAfterglow", [new("Zebes explosion planet afterglow", ZebesExplosionAmbientPaletteFxColorDefinitions.ZebesExplosionPlanetAfterglow.ColorFrames)]),
        new("zebesExplosionLava", [new("Zebes explosion lava", ZebesExplosionAmbientPaletteFxColorDefinitions.ExplodingZebesLava.ColorFrames)]),
        new("zebesExplosionCrust", [new("Zebes explosion crust", ZebesExplosionLayerFadePaletteFxColorDefinitions.FadeOutExplodingZebesCrust.ColorFrames)]),
        new("zebesExplosionGreyClouds", [new("Zebes explosion grey clouds", ZebesExplosionLayerFadePaletteFxColorDefinitions.FadeOutExplodingZebesGreyClouds.ColorFrames)]),
        new("zebesExplosionGunship", [new("Zebes explosion gunship", ZebesExplosionGunshipPaletteFxColorDefinitions.ColorFrames)]),
        new("samusLoadingPowerSuit", [new("Samus loading Power suit", SamusLoadingSuitPaletteFxColorDefinitions.SamusLoadingPowerSuit.ColorFrames)]),
        new("samusLoadingVariaSuit", [new("Samus loading Varia suit", SamusLoadingSuitPaletteFxColorDefinitions.SamusLoadingVariaSuit.ColorFrames)]),
        new("samusLoadingGravitySuit", [new("Samus loading Gravity suit", SamusLoadingSuitPaletteFxColorDefinitions.SamusLoadingGravitySuit.ColorFrames)]),
        new("postCreditsIconGlare", [new("post-credits icon glare", PostCreditsIconGlarePaletteFxColorDefinitions.ColorFrames)]),
        new("tourianEscapeShutter", [new("Tourian escape shutter", TourianEscapeRedFlashPaletteFxColorDefinitions.TourianShutterRedFlashing.ColorFrames)]),
        new("tourianEscapeBackground", [new("Tourian escape background", TourianEscapeRedFlashPaletteFxColorDefinitions.TourianBackgroundRedFlashing.ColorFrames)]),
        new("tourianEscapeSharedRedFlash", [new("Tourian escape shared red flash", TourianEscapeSharedRedFlashPaletteFxColorDefinitions.ColorFrames)]),
        new("oldTourianEscapeRedFlash", [new("old Tourian escape red flash", OldTourianEscapeRedFlashPaletteFxColorDefinitions.ColorFrames)]),
        new("oldTourianEscapeOrangeRailings", [new("old Tourian escape orange railings", OldTourianEscapeAccentPaletteFxColorDefinitions.OldTourianBackgroundRailingsFlashYellow.ColorFrames)]),
        new("oldTourianEscapeYellowPanels", [new("old Tourian escape yellow panels", OldTourianEscapeAccentPaletteFxColorDefinitions.OldTourianBackgroundPanelsFlashYellow.ColorFrames)]),
        new("upperCrateriaEscapeRedFlash", [new("upper Crateria escape red flash", UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.ColorFrames)]),
        new("crateriaEscapeYellowLightning", [new("Crateria escape yellow lightning", CrateriaEscapeLightningPaletteFxColorDefinitions.CrateriaEscapeYellowLightning.ColorFrames)]),
        new("parlorEscapeSky", [new("Crateria escape Parlor sky", CrateriaEscapeLightningPaletteFxColorDefinitions.ParlorEscapeSkyFlicker.ColorFrames)]),
        new("beaconFlashing", [new("Crateria and Brinstar beacon flash", BeaconPaletteFxColorDefinitions.ColorFrames)]),
    ];
}
