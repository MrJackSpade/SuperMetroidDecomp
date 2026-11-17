namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// A run of a colour effect's frames with their durations (#2366): the beats an effect's phases play, each showing the
/// frame of that number among the frames of the effect playing it, in the effect's own colours. Effects that show alike
/// play one run; an effect whose frames repeat in parts plays each part as its own run, repeated.
/// </summary>
internal enum ColourEffectSegment
{
    /// <summary>Eight looks of three updates each: the title logo, the copyright and the Planet Zebes caption fading.</summary>
    TextFade,

    /// <summary>Two looks of one update each: the title screen's displays and the gunship's engine flickering.</summary>
    Flicker,

    /// <summary>Eight looks of ten updates each: the Baby Metroid's tube light and Wrecked Ship's green lights glowing.</summary>
    SlowGlow,

    /// <summary>
    /// Fourteen looks of four updates each: Ceres station's navigation lights blinking, and Tourian's background flashing
    /// red alike.
    /// </summary>
    SteadyBlink,

    /// <summary>The old Mother Brain fight's background lights: fourteen looks of six updates each.</summary>
    OldMotherBrainBackgroundLights,

    /// <summary>The gunship's glow: fourteen looks of five updates each.</summary>
    GunshipGlow,

    /// <summary>The zoomed-out exploding Zebes fading out: seven looks of eight updates each.</summary>
    ZoomedOutZebesFade,

    /// <summary>The wide part of Zebes's explosion in front: four quick looks, a long fourth, then twelve of six updates.</summary>
    WideZebesExplosionForeground,

    /// <summary>Zebes's explosion's finale: fifteen looks of two updates each, then thirty of nine.</summary>
    ZebesExplosionFinale,

    /// <summary>
    /// Fifteen looks of fourteen updates each: space whiting out during Zebes's explosion, and the explosion's wide part
    /// behind alike.
    /// </summary>
    ZebesExplosionWhiteOut,

    /// <summary>The exploded planet's afterglow: six looks of sixteen updates each.</summary>
    ZebesAfterglow,

    /// <summary>Exploding Zebes's lava: ten looks easing from nine updates down to five and back.</summary>
    ExplodingZebesLava,

    /// <summary>Exploding Zebes's crust fading out: eight looks of twenty updates each.</summary>
    ExplodingZebesCrustFade,

    /// <summary>Exploding Zebes's grey clouds fading out: eight looks of fourteen updates each.</summary>
    ExplodingZebesGreyCloudsFade,

    /// <summary>The gunship emerging from Zebes's explosion: sixteen looks of twenty-four updates each.</summary>
    GunshipEmerging,

    /// <summary>The Hyper Beam's glow: ten looks of two updates each.</summary>
    HyperBeam,

    /// <summary>A suit's loading opening: its first two looks, which flicker for the longest stretch.</summary>
    SuitLoadingOpeningFlicker,

    /// <summary>A suit's loading: its third and fourth looks, flickering.</summary>
    SuitLoadingSecondFlicker,

    /// <summary>A suit's loading: its fifth and sixth looks, flickering.</summary>
    SuitLoadingThirdFlicker,

    /// <summary>A suit's loading: its seventh and eighth looks, the last to flicker.</summary>
    SuitLoadingLastFlicker,

    /// <summary>A suit's loading: its loaded look, shown for one update as the effect ends.</summary>
    SuitLoaded,

    /// <summary>The post-credits Super Metroid icon: fourteen looks of one update each.</summary>
    PostCreditsIcon,

    /// <summary>Eight looks of eight updates each: a Tourian statue greying out.</summary>
    StatueGreyOut,

    /// <summary>A Torizo's belly glowing: six looks, the first and fourth held longest.</summary>
    TorizoBelly,

    /// <summary>
    /// The heat pulse: the sixteen heat phases' looks in phase order, longest at the coolest and quickest between, which
    /// the Varia and Gravity Suits shimmer in and Norfair's heated foregrounds glow in.
    /// </summary>
    HeatPulse,

    /// <summary>The Power Suit's heat pulse: the heat pulse with its last look cut short.</summary>
    PowerSuitHeatPulse,

    /// <summary>The landing site's calm sky before its double strike of lightning.</summary>
    LandingSiteCalm,

    /// <summary>The landing site's double strike of lightning: seven quick looks, struck twice.</summary>
    LandingSiteDoubleStrike,

    /// <summary>The landing site's calm sky before its single strike of lightning.</summary>
    LandingSiteSecondCalm,

    /// <summary>The landing site's single strike of lightning: four quick looks.</summary>
    LandingSiteSingleStrike,

    /// <summary>
    /// Fourteen looks of ten updates each: Brinstar's blue background spores drifting, Spore Spawn's alike, and Brinstar's
    /// red background glowing alike.
    /// </summary>
    SporeDrift,

    /// <summary>The beacon flashing: ten looks of ten updates each, its beep sounding before the seventh.</summary>
    BeaconFlashing,

    /// <summary>
    /// Norfair's heated foreground glowing in the heat pulse: before each heat phase's look it publishes that phase, which
    /// Samus's heat shimmer follows.
    /// </summary>
    NorfairHeatPhases,

    /// <summary>Four looks of ten updates each: Maridia's sand pits and sand falls.</summary>
    MaridiaSand,

    /// <summary>Maridia's background waterfalls: eight looks of two updates each.</summary>
    MaridiaBackgroundWaterfalls,

    /// <summary>Tourian's Arkanoid blocks and red orbs glowing: eleven looks of ten updates each.</summary>
    TourianArkanoidBlocksGlow,

    /// <summary>
    /// Fourteen looks of two updates each: Tourian flashing red during the escape, its shutters, its level and its Arkanoid
    /// blocks and red orbs alike.
    /// </summary>
    TourianEscapeRedFlashing,

    /// <summary>Old Tourian flashing red: fourteen looks of three updates each.</summary>
    OldTourianRedFlashing,

    /// <summary>Old Tourian's background railings and panels flashing yellow alike: fifteen looks, two of them held long.</summary>
    OldTourianYellowFlash,

    /// <summary>Upper Crateria flashing red: fourteen looks easing from eight updates down to one and back.</summary>
    UpperCrateriaRedFlashing,

    /// <summary>The escape's sky lightning: Crateria's yellow lightning and the Parlor's sky flicker alike.</summary>
    EscapeLightning,
}
