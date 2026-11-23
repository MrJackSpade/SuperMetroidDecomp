namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// A run of a colour effect's frames with their durations (#2366): the beats an effect's phases play, each showing the
/// frame of that number among the frames of the effect playing it, in the effect's own colours. Effects that show alike
/// play one run; an effect whose frames repeat in parts plays each part as its own run, repeated.
/// </summary>
internal enum ColourEffectSegment
{
    /// <summary>A quick fade through its looks: the title logo, the copyright and the Planet Zebes caption fading.</summary>
    TextFade,

    /// <summary>A flicker between its looks on alternate updates: the title screen's displays and the gunship's engine flickering.</summary>
    Flicker,

    /// <summary>A slow glow through its looks: the Baby Metroid's tube light and Wrecked Ship's green lights glowing.</summary>
    SlowGlow,

    /// <summary>
    /// A steady blink through its looks: Ceres station's navigation lights blinking, and Tourian's background flashing
    /// red alike.
    /// </summary>
    SteadyBlink,

    /// <summary>The old Mother Brain fight's background lights, pulsing slowly.</summary>
    OldMotherBrainBackgroundLights,

    /// <summary>The gunship's glow, pulsing through its looks.</summary>
    GunshipGlow,

    /// <summary>The zoomed-out exploding Zebes fading out.</summary>
    ZoomedOutZebesFade,

    /// <summary>The wide part of Zebes's explosion in front: quick looks, a long hold, then a slower run.</summary>
    WideZebesExplosionForeground,

    /// <summary>Zebes's explosion's finale: a quick run of looks, then a long slow one.</summary>
    ZebesExplosionFinale,

    /// <summary>
    /// A long slow run of looks: space whiting out during Zebes's explosion, and the explosion's wide part
    /// behind alike.
    /// </summary>
    ZebesExplosionWhiteOut,

    /// <summary>The exploded planet's afterglow, fading slowly.</summary>
    ZebesAfterglow,

    /// <summary>Exploding Zebes's lava: its looks quickening and then slowing again.</summary>
    ExplodingZebesLava,

    /// <summary>Exploding Zebes's crust fading out slowly.</summary>
    ExplodingZebesCrustFade,

    /// <summary>Exploding Zebes's grey clouds fading out slowly.</summary>
    ExplodingZebesGreyCloudsFade,

    /// <summary>The gunship emerging from Zebes's explosion, brightening slowly.</summary>
    GunshipEmerging,

    /// <summary>The Hyper Beam's glow, cycling quickly.</summary>
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

    /// <summary>The post-credits Super Metroid icon, a new look each update.</summary>
    PostCreditsIcon,

    /// <summary>A Tourian statue greying out through its looks.</summary>
    StatueGreyOut,

    /// <summary>A Torizo's belly glowing, two of its looks held longest.</summary>
    TorizoBelly,

    /// <summary>
    /// The heat pulse: the heat phases' looks in phase order, longest at the coolest and quickest between, which
    /// the Varia and Gravity Suits shimmer in and Norfair's heated foregrounds glow in.
    /// </summary>
    HeatPulse,

    /// <summary>The Power Suit's heat pulse: the heat pulse with its last look cut short.</summary>
    PowerSuitHeatPulse,

    /// <summary>The landing site's calm sky before its double strike of lightning.</summary>
    LandingSiteCalm,

    /// <summary>The landing site's double strike of lightning: quick looks, struck twice.</summary>
    LandingSiteDoubleStrike,

    /// <summary>The landing site's calm sky before its single strike of lightning.</summary>
    LandingSiteSecondCalm,

    /// <summary>The landing site's single strike of lightning: quick looks.</summary>
    LandingSiteSingleStrike,

    /// <summary>
    /// A slow drift through its looks: Brinstar's blue background spores drifting, Spore Spawn's alike, and Brinstar's
    /// red background glowing alike.
    /// </summary>
    SporeDrift,

    /// <summary>The beacon flashing through its looks, its beep sounding partway through.</summary>
    BeaconFlashing,

    /// <summary>
    /// Norfair's heated foreground glowing in the heat pulse: before each heat phase's look it publishes that phase, which
    /// Samus's heat shimmer follows.
    /// </summary>
    NorfairHeatPhases,

    /// <summary>A slow cycle of looks: Maridia's sand pits and sand falls.</summary>
    MaridiaSand,

    /// <summary>Maridia's background waterfalls, flowing quickly.</summary>
    MaridiaBackgroundWaterfalls,

    /// <summary>Tourian's Arkanoid blocks and red orbs glowing slowly.</summary>
    TourianArkanoidBlocksGlow,

    /// <summary>
    /// A quick red flash through its looks: Tourian flashing red during the escape, its shutters, its level and its Arkanoid
    /// blocks and red orbs alike.
    /// </summary>
    TourianEscapeRedFlashing,

    /// <summary>Old Tourian flashing red, quickly.</summary>
    OldTourianRedFlashing,

    /// <summary>Old Tourian's background railings and panels flashing yellow alike, two of its looks held long.</summary>
    OldTourianYellowFlash,

    /// <summary>Upper Crateria flashing red: its looks quickening and then slowing again.</summary>
    UpperCrateriaRedFlashing,

    /// <summary>The escape's sky lightning: Crateria's yellow lightning and the Parlor's sky flicker alike.</summary>
    EscapeLightning,
}
