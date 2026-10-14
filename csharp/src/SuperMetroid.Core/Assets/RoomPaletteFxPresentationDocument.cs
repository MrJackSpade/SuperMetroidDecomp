namespace SuperMetroid.Core.Assets;

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
