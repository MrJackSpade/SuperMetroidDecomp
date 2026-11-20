using SuperMetroid.AssetExtraction.Fx.PaletteFx;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// One colour program of the installed room palette-effect document (#2557): the key its colours are stored under, how a
/// failure names it, and the record colours it holds, each record run validated against the document's frames. The
/// Brinstar blue spores' ordinary and Spore Spawn programs share one entry.
/// </summary>
/// <param name="Key">The key the program's frames are stored under in the document.</param>
/// <param name="Description">How a failure names the program.</param>
/// <param name="Frames">The record runs the stored frames give colours to; extraction reads the first.</param>
internal sealed record RoomPaletteFxProgram(string Key, string Description, IReadOnlyList<PaletteFxColorFrames> Frames);

/// <summary>Every colour program of the room palette-effect document, each listed once, in the document's order.</summary>
internal static class RoomPaletteFxPrograms
{
    /// <summary>The programs, in the order the document stores them.</summary>
    internal static IReadOnlyList<RoomPaletteFxProgram> All { get; } =
    [
        // Sixteen native phases of Samus's Power Suit palette in heat.
        new("samusHeatPowerSuit", "Samus Power suit in heat", [PaletteFxHeatPulseColorDefinitions.All.Single(item => item.Suit == ColourEffectHeatSuit.Power).ColorFrames]),
        // Sixteen native phases of Samus's Varia Suit palette in heat.
        new("samusHeatVariaSuit", "Samus Varia suit in heat", [PaletteFxHeatPulseColorDefinitions.All.Single(item => item.Suit == ColourEffectHeatSuit.Varia).ColorFrames]),
        // Sixteen native phases of Samus's Gravity Suit palette in heat.
        new("samusHeatGravitySuit", "Samus Gravity suit in heat", [PaletteFxHeatPulseColorDefinitions.All.Single(item => item.Suit == ColourEffectHeatSuit.Gravity).ColorFrames]),
        // Sixteen five-color phases for foreground palette three, synchronized with the compiled phase publication driving Samus's heat palette.
        new("norfairForegroundAndHeatPhase", "Norfair foreground and heat phase", [NorfairEnvironmentalPaletteFxColorDefinitions.All.Single(item => item.Owner == NorfairEnvironmentalPaletteOwner.ForegroundAndHeatPhase).ColorFrames]),
        // Sixteen five-color phases for the heat glow on Norfair's green and red foreground; native skip commands retain the intervening CGRAM entries.
        new("norfairGreenForeground", "Norfair green foreground", [NorfairEnvironmentalPaletteFxColorDefinitions.All.Single(item => item.Owner == NorfairEnvironmentalPaletteOwner.GreenForeground).ColorFrames]),
        // Sixteen five-color phases for the heat glow on Norfair's brown and purple foreground, synchronized with the environmental heat cycle.
        new("norfairPurpleForeground", "Norfair purple foreground", [NorfairEnvironmentalPaletteFxColorDefinitions.All.Single(item => item.Owner == NorfairEnvironmentalPaletteOwner.PurpleForeground).ColorFrames]),
        // Sixteen five-color phases for the heat glow on Norfair's dark brown foreground, synchronized with the environmental heat cycle.
        new("norfairDarkForeground", "Norfair dark foreground", [NorfairEnvironmentalPaletteFxColorDefinitions.All.Single(item => item.Owner == NorfairEnvironmentalPaletteOwner.DarkForeground).ColorFrames]),
        // Four eight-color sand-pit rotation records, containing two four-color sand bands per phase.
        new("maridiaSandPits", "Maridia sand pits", [MaridiaEnvironmentalPaletteFxColorDefinitions.All.Single(item => item.Owner == MaridiaEnvironmentalPaletteOwner.SandPits).ColorFrames]),
        // Four four-color rotation phases for Maridia's falling sand.
        new("maridiaSandFalls", "Maridia sand falls", [MaridiaEnvironmentalPaletteFxColorDefinitions.All.Single(item => item.Owner == MaridiaEnvironmentalPaletteOwner.SandFalls).ColorFrames]),
        // Eight eight-color rotation phases for Maridia's background waterfalls.
        new("maridiaBackgroundWaterfalls", "Maridia background waterfalls", [MaridiaEnvironmentalPaletteFxColorDefinitions.All.Single(item => item.Owner == MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls).ColorFrames]),
        // Eight two-color phases for the Wrecked Ship green-light cycle.
        new("wreckedShipGreenLights", "Wrecked Ship green lights", [WreckedShipGreenLightPaletteFxColorDefinitions.ColorFrames]),
        // Fourteen eight-color records for Red Brinstar's background glow.
        new("redBrinstarBackgroundGlow", "Red Brinstar background glow", [RedBrinstarGlowPaletteFxColorDefinitions.ColorFrames]),
        // Eleven eight-color records shared by the live and cloned Tourian glow programs.
        new("tourianGlow", "Tourian glow", [TourianGlowPaletteFxColorDefinitions.ColorFrames]),
        // Fourteen three-color phases shared by ordinary Brinstar rooms and the Spore Spawn room's separately controlled program.
        new("brinstarBlueSpores", "Brinstar blue spores", [.. BrinstarBlueSporePaletteFxColorDefinitions.All.Select(definition => definition.ColorFrames)]),
        // Six three-color belly phases for Bomb Torizo, with boss-dependent lifetime retained in mechanics.
        new("bombTorizoBelly", "Bomb Torizo belly", [TorizoBellyPaletteFxColorDefinitions.All.Single(item => item.Owner == TorizoBellyPaletteOwner.BombTorizo).ColorFrames]),
        // Six three-color belly phases for Golden Torizo, with its own native program identity.
        new("goldenTorizoBelly", "Golden Torizo belly", [TorizoBellyPaletteFxColorDefinitions.All.Single(item => item.Owner == TorizoBellyPaletteOwner.GoldenTorizo).ColorFrames]),
        // Eight eight-color grey-out records for the Tourian statues.
        new("tourianStatueGrey", "Tourian statue grey-out", [TourianStatueGreyPaletteFxColorDefinitions.ColorFrames]),
        // Thirteen eight-color records for surface lightning, including the long neutral holds and brief flashes defined by the native program.
        new("crateriaSurfaceLightning", "Crateria surface lightning", [CrateriaLightningPaletteFxColorDefinitions.Surface.ColorFrames]),
        // Two single-color records for the Ceres gunship engine-light loop.
        new("ceresGunshipEngineLights", "Ceres gunship-engine lights", [CeresCinematicLightPaletteFxColorDefinitions.GunshipEngineFrames]),
        // Fourteen two-color navigation-light records shared by the sprite and background destination programs.
        new("ceresNavigationLights", "Ceres navigation lights", [CeresCinematicLightPaletteFxColorDefinitions.NavigationLightsFrames]),
        // Eight three-color PLANET ZEBES text fade-in records, with matching samples reconstructed from the editable endpoint colors.
        new("planetZebesTextFadeIn", "PLANET ZEBES fade-in", [PlanetZebesTextPaletteFxColorDefinitions.All.Single(item => item.Owner == PlanetZebesTextPaletteFxProgramOwner.FadeIn).ColorFrames]),
        // Eight three-color PLANET ZEBES text fade-out records using the separate native fade-out program.
        new("planetZebesTextFadeOut", "PLANET ZEBES fade-out", [PlanetZebesTextPaletteFxColorDefinitions.All.Single(item => item.Owner == PlanetZebesTextPaletteFxProgramOwner.FadeOut).ColorFrames]),
        // Fourteen three-color glow records for the old Mother Brain cinematic background lights, held for six updates per record.
        new("oldMotherBrainBackgroundLights", "cinematic old Mother Brain background lights", [CinematicGlowPaletteFxColorDefinitions.All.Single(item => item.Owner == CinematicGlowPaletteFxProgramOwner.OldMotherBrainBackgroundLights).ColorFrames]),
        // Fourteen single-color cinematic gunship glow records, held for five updates per record.
        new("cinematicGunshipGlow", "cinematic gunship glow", [CinematicGlowPaletteFxColorDefinitions.All.Single(item => item.Owner == CinematicGlowPaletteFxProgramOwner.GunshipGlow).ColorFrames]),
        // Seven eight-color records for the exploding-Zebes cinematic fade.
        new("explodingZebesFade", "exploding Zebes fade", [ExplodingZebesFadePaletteFxColorDefinitions.ColorFrames]),
        // Eight fifteen-color records for the title-logo fade.
        new("titleLogoFade", "title-logo fade", [TitleLogoFadePaletteFxColorDefinitions.ColorFrames]),
        // Eight two-color records for the shared Nintendo-logo fade.
        new("nintendoSharedFade", "Nintendo shared fade", [NintendoLogoFadePaletteFxColorDefinitions.ColorFrames]),
        // Sixteen fifteen-color records for the expanding Zebes explosion foreground.
        new("zebesExplosionForeground", "Zebes explosion foreground", [ZebesExplosionForegroundPaletteFxColorDefinitions.ColorFrames]),
        // Forty-five fifteen-color finale records, preserving the mechanics' distinct fast and slow phases.
        new("zebesExplosionFinale", "Zebes explosion finale", [ZebesExplosionFinalePaletteFxColorDefinitions.ColorFrames]),
        // Fifteen single-color records shared by the wide-explosion background and space-whiteout programs.
        new("zebesExplosionWhiteout", "Zebes explosion shared whiteout", [ZebesExplosionWhiteoutPaletteFxColorDefinitions.ColorFrames]),
        // Six eight-color planet-afterglow records played on the native ninety-six-update cycle.
        new("zebesExplosionAfterglow", "Zebes explosion planet afterglow", [ZebesExplosionAmbientPaletteFxColorDefinitions.All.Single(item => item.Owner == ZebesExplosionAmbientPaletteFxProgramOwner.PlanetAfterglow).ColorFrames]),
        // Ten single-color lava records played on the native seventy-update cycle.
        new("zebesExplosionLava", "Zebes explosion lava", [ZebesExplosionAmbientPaletteFxColorDefinitions.All.Single(item => item.Owner == ZebesExplosionAmbientPaletteFxProgramOwner.Lava).ColorFrames]),
        // Eight fifteen-color fade records for the planet's crust layer.
        new("zebesExplosionCrust", "Zebes explosion crust", [ZebesExplosionLayerFadePaletteFxColorDefinitions.All.Single(item => item.Owner == ZebesExplosionLayerFadePaletteFxProgramOwner.Crust).ColorFrames]),
        // Eight fifteen-color fade records for the explosion's grey-cloud layer.
        new("zebesExplosionGreyClouds", "Zebes explosion grey clouds", [ZebesExplosionLayerFadePaletteFxColorDefinitions.All.Single(item => item.Owner == ZebesExplosionLayerFadePaletteFxProgramOwner.GreyClouds).ColorFrames]),
        // Sixteen sixteen-color gunship fade records.
        new("zebesExplosionGunship", "Zebes explosion gunship", [ZebesExplosionGunshipPaletteFxColorDefinitions.ColorFrames]),
        // Nine sixteen-color Power Suit loading records: four two-record groups followed by the terminal palette row.
        new("samusLoadingPowerSuit", "Samus loading Power suit", [SamusLoadingSuitPaletteFxColorDefinitions.All.Single(item => item.Owner == SamusLoadingSuitPaletteFxProgramOwner.PowerSuit).ColorFrames]),
        // Nine sixteen-color Varia Suit loading records with the same compiled group timing as the other suits.
        new("samusLoadingVariaSuit", "Samus loading Varia suit", [SamusLoadingSuitPaletteFxColorDefinitions.All.Single(item => item.Owner == SamusLoadingSuitPaletteFxProgramOwner.VariaSuit).ColorFrames]),
        // Nine sixteen-color Gravity Suit loading records with the same compiled group timing as the other suits.
        new("samusLoadingGravitySuit", "Samus loading Gravity suit", [SamusLoadingSuitPaletteFxColorDefinitions.All.Single(item => item.Owner == SamusLoadingSuitPaletteFxProgramOwner.GravitySuit).ColorFrames]),
        // Fourteen sixteen-color post-credits icon glare records.
        new("postCreditsIconGlare", "post-credits icon glare", [PostCreditsIconGlarePaletteFxColorDefinitions.ColorFrames]),
        // Fourteen six-colour red-flash records for the Tourian escape shutter, each held for two updates (<see cref="SuperMetroid.AssetExtraction.Fx.PaletteFx.TourianEscapeRedFlashPaletteFxColorDefinitions"/>).
        new("tourianEscapeShutter", "Tourian escape shutter", [TourianEscapeRedFlashPaletteFxColorDefinitions.All.Single(item => item.Owner == TourianEscapeRedFlashPaletteOwner.Shutter).ColorFrames]),
        // Fourteen four-color red-flash records for the Tourian escape background, held for four updates per record.
        new("tourianEscapeBackground", "Tourian escape background", [TourianEscapeRedFlashPaletteFxColorDefinitions.All.Single(item => item.Owner == TourianEscapeRedFlashPaletteOwner.Background).ColorFrames]),
        // Fourteen seven-color red-flash records shared by the general level and Arkanoid-block/red-orb destination programs.
        new("tourianEscapeSharedRedFlash", "Tourian escape shared red flash", [TourianEscapeSharedRedFlashPaletteFxColorDefinitions.ColorFrames]),
        // Fourteen eight-color records for the old Tourian escape's red-flash program.
        new("oldTourianEscapeRedFlash", "old Tourian escape red flash", [OldTourianEscapeRedFlashPaletteFxColorDefinitions.ColorFrames]),
        // Fifteen three-color accent records for the old Tourian escape's orange railings.
        new("oldTourianEscapeOrangeRailings", "old Tourian escape orange railings", [OldTourianEscapeAccentPaletteFxColorDefinitions.All.Single(item => item.Owner == OldTourianEscapeAccentPaletteOwner.OrangeRailings).ColorFrames]),
        // Fifteen three-color accent records for the old Tourian escape's yellow panels.
        new("oldTourianEscapeYellowPanels", "old Tourian escape yellow panels", [OldTourianEscapeAccentPaletteFxColorDefinitions.All.Single(item => item.Owner == OldTourianEscapeAccentPaletteOwner.YellowPanels).ColorFrames]),
        // Fourteen seven-color upper-Crateria escape red-flash records with the native variable hold durations.
        new("upperCrateriaEscapeRedFlash", "upper Crateria escape red flash", [UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.ColorFrames]),
        // Eleven eleven-color yellow-lightning records for Crateria's escape palette.
        new("crateriaEscapeYellowLightning", "Crateria escape yellow lightning", [CrateriaEscapeLightningPaletteFxColorDefinitions.All.Single(item => item.Owner == CrateriaEscapeLightningPaletteOwner.YellowLightning).ColorFrames]),
        // Eleven five-color records for the Parlor's dark sky, the yellow lightning's dark tail flickering in its cadence during the escape.
        new("parlorEscapeSky", "Crateria escape Parlor sky", [CrateriaEscapeLightningPaletteFxColorDefinitions.All.Single(item => item.Owner == CrateriaEscapeLightningPaletteOwner.ParlorSky).ColorFrames]),
        // Ten four-color beacon-flash records shared by Crateria and Brinstar, retaining the native sound instruction between record groups.
        new("beaconFlashing", "Crateria and Brinstar beacon flash", [BeaconPaletteFxColorDefinitions.ColorFrames]),
    ];
}
