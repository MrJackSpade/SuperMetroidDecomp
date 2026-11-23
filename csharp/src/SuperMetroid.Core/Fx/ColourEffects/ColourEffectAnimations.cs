using System.Collections.Frozen;
using SuperMetroid.Core.Animation;
using A = SuperMetroid.Core.Fx.ColourEffects.ColourEffectId;
using S = SuperMetroid.Core.Fx.ColourEffects.ColourEffectSegment;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// The colour effects (#1298, #1554, #1598, #1600, #1739, #1982, #2366): the runs of frames they play
/// (<see cref="ColourEffectSegment"/>), each frame numbered by its place among the frames of the effect playing it (a
/// <see cref="ColourEffectFrame"/> of that effect) with its duration, and the <see cref="ColourEffectCue"/> events the
/// running effect reacts to between them; and each effect's definition: how it starts, the colours its frames show in, the
/// condition it watches, the phases it plays, each a whole run played a number of times, and whether it then stops or
/// loops. Effects that show alike play one run, each in its own colours.
/// </summary>
internal static partial class ColourEffectAnimations
{

    /// <summary>The updates the Power Suit's heat pulse shows its last heat phase's look for, cut short from the pulse's.</summary>
    private const int PowerSuitLastHeatLookUpdates = 3;

    /// <summary>Each heat phase's look shown for its updates, in phase order: the heat pulse.</summary>
    private static readonly AnimationBeat<int, ColourEffectCue>[] HeatPulseLooks = [.. HeatPhase.InOrder.Select(phase => Heat(phase, phase.LookUpdates))];

    /// <summary>
    /// The runs of frames the effects play; each suit's heat pulse plays its own (#2054), and Samus in heat, which plays her
    /// suit's pulse (<c>Shimmer</c>), shows none of its own.
    /// </summary>
    internal static AnimationSegments<ColourEffectSegment, int, ColourEffectCue> Segments { get; } = new(
        new Dictionary<ColourEffectSegment, AnimationBeat<int, ColourEffectCue>[]>
    {
        [S.TextFade] = Run(0, 8, 3),
        [S.Flicker] =
        [
            F(0, 1),
            F(1, 1),
        ],
        [S.SlowGlow] = Run(0, 8, 10),
        [S.SteadyBlink] = Run(0, 14, 4),
        [S.OldMotherBrainBackgroundLights] = Run(0, 14, 6),
        [S.GunshipGlow] = Run(0, 14, 5),
        [S.ZoomedOutZebesFade] = Run(0, 7, 8),
        [S.WideZebesExplosionForeground] =
        [
            .. Run(0, 3, 4),
            F(3, 60),
            .. Run(4, 12, 6),
        ],
        [S.ZebesExplosionFinale] =
        [
            .. Run(0, 15, 2),
            .. Run(15, 30, 9),
        ],
        [S.ZebesExplosionWhiteOut] = Run(0, 15, 14),
        [S.ZebesAfterglow] = Run(0, 6, 16),
        [S.ExplodingZebesLava] =
        [
            F(0, 9),
            F(1, 8),
            F(2, 7),
            F(3, 6),
            F(4, 5),
            F(5, 5),
            F(6, 6),
            F(7, 7),
            F(8, 8),
            F(9, 9),
        ],
        [S.ExplodingZebesCrustFade] = Run(0, 8, 20),
        [S.ExplodingZebesGreyCloudsFade] = Run(0, 8, 14),
        [S.GunshipEmerging] = Run(0, 16, 24),
        [S.HyperBeam] = Run(0, 10, 2),
        [S.SuitLoadingOpeningFlicker] =
        [
            F(0, 3),
            F(1, 3),
        ],
        [S.SuitLoadingSecondFlicker] =
        [
            F(2, 3),
            F(3, 3),
        ],
        [S.SuitLoadingThirdFlicker] =
        [
            F(4, 3),
            F(5, 3),
        ],
        [S.SuitLoadingLastFlicker] =
        [
            F(6, 3),
            F(7, 3),
        ],
        [S.SuitLoaded] =
        [
            F(8, 1),
        ],
        [S.PostCreditsIcon] = Run(0, 14, 1),
        [S.StatueGreyOut] = Run(0, 8, 8),
        [S.TorizoBelly] =
        [
            F(0, 10),
            F(1, 8),
            F(2, 8),
            F(3, 10),
            F(4, 8),
            F(5, 8),
        ],
        [S.HeatPulse] = HeatPulseLooks,
        [S.PowerSuitHeatPulse] = [.. HeatPulseLooks.SkipLast(1), Heat(HeatPhase.InOrder[^1], PowerSuitLastHeatLookUpdates)],
        [S.LandingSiteCalm] =
        [
            F(0, 240),
        ],
        [S.LandingSiteDoubleStrike] =
        [
            F(1, 2),
            .. Run(2, 5, 1),
            F(7, 2),
        ],
        [S.LandingSiteSecondCalm] =
        [
            F(8, 240),
        ],
        [S.LandingSiteSingleStrike] =
        [
            .. Run(9, 3, 1),
            F(12, 2),
        ],
        [S.SporeDrift] = Run(0, 14, 10),
        [S.BeaconFlashing] =
        [
            .. Run(0, 6, 10),
            E(new ColourEffectCue.QueueSound(Sound: EnemySound.Beacon)),
            .. Run(6, 4, 10),
        ],
        [S.NorfairHeatPhases] = [.. HeatPhase.InOrder.SelectMany(phase => new[] { E(new ColourEffectCue.PublishHeatPhase(phase)), Heat(phase, phase.LookUpdates) })],
        [S.MaridiaSand] = Run(0, 4, 10),
        [S.MaridiaBackgroundWaterfalls] = Run(0, 8, 2),
        [S.TourianArkanoidBlocksGlow] = Run(0, 11, 10),
        [S.TourianEscapeRedFlashing] = Run(0, 14, 2),
        [S.OldTourianRedFlashing] = Run(0, 14, 3),
        [S.OldTourianYellowFlash] =
        [
            F(0, 16),
            F(1, 1),
            F(2, 1),
            F(3, 2),
            F(4, 1),
            F(5, 2),
            .. Run(6, 4, 1),
            F(10, 32),
            F(11, 2),
            .. Run(12, 3, 1),
        ],
        [S.UpperCrateriaRedFlashing] =
        [
            F(0, 8),
            F(1, 7),
            F(2, 6),
            F(3, 5),
            F(4, 4),
            F(5, 3),
            F(6, 2),
            F(7, 1),
            F(8, 2),
            F(9, 3),
            F(10, 4),
            F(11, 5),
            F(12, 6),
            F(13, 7),
        ],
        [S.EscapeLightning] =
        [
            F(0, 49),
            .. Run(1, 4, 1),
            F(5, 17),
            F(6, 1),
            F(7, 24),
            .. Run(8, 3, 1),
        ],
    });

    /// <summary>
    /// A suit's loading: its opening flicker played many times over, its second and third a few times each, its last
    /// fewer still, then its loaded look once. Each suit plays it in its own frames.
    /// </summary>
    private static readonly Animation<ColourEffectSegment> SuitLoading = new(
        [
            Segments.Repeated(S.SuitLoadingOpeningFlicker, 36),
            Segments.Repeated(S.SuitLoadingSecondFlicker, 3),
            Segments.Repeated(S.SuitLoadingThirdFlicker, 3),
            Segments.Repeated(S.SuitLoadingLastFlicker, 2),
            Segments.Whole(S.SuitLoaded),
        ],
        AnimationEnd.Stops);

    /// <summary>
    /// The Varia and Gravity Suits' heat shimmer and Norfair's heated foreground glows: the heat pulse, looped. Each plays it
    /// in its own frames.
    /// </summary>
    private static readonly Animation<ColourEffectSegment> HeatPulseLoop = Looping(S.HeatPulse);

    /// <summary>
    /// How each effect starts, the colours its frames show in, the condition it watches from the update after its first
    /// frame, and how it plays its runs: its phases, then whether it stops or loops. Samus in heat plays none of her own: her
    /// start chooses her suit's pulse (<c>Shimmer</c>), which plays as her shimmer does, in
    /// the same colours and watching the same heat.
    /// </summary>
    private static readonly FrozenDictionary<ColourEffectId, ColourEffectDefinition> Definitions =
        new Dictionary<ColourEffectId, ColourEffectDefinition>
    {
        [A.FadeInSuperMetroidTitleLogo] = new(ColourEffectStart.Plain, ColourEffectColours.TitleLogo, ColourEffectCondition.WatchesNothing.Instance, Once(S.TextFade)),
        [A.FadeInNintendoCopyright] = new(ColourEffectStart.Plain, ColourEffectColours.NintendoCopyright, ColourEffectCondition.WatchesNothing.Instance, Once(S.TextFade)),
        [A.TitleScreenBabyMetroidTubeLight] = new(ColourEffectStart.Plain, ColourEffectColours.BabyMetroidTubeLight, ColourEffectCondition.WatchesNothing.Instance, Looping(S.SlowGlow)),
        [A.TitleScreenFlickeringDisplays] = new(ColourEffectStart.Plain, ColourEffectColours.TitleScreenDisplays, ColourEffectCondition.WatchesNothing.Instance, Looping(S.Flicker)),
        [A.CutsceneGunshipEngineFlicker] = new(ColourEffectStart.Plain, ColourEffectColours.CutsceneGunshipEngine, ColourEffectCondition.WatchesNothing.Instance, Looping(S.Flicker)),
        [A.CutsceneCeresNavigationLightsSprite] = new(ColourEffectStart.Plain, ColourEffectColours.CeresNavigationLightsSprite, ColourEffectCondition.WatchesNothing.Instance, Looping(S.SteadyBlink)),
        [A.FadeInPlanetZebesText] = new(ColourEffectStart.Plain, ColourEffectColours.PlanetZebesText, ColourEffectCondition.WatchesNothing.Instance, Once(S.TextFade)),
        [A.FadeOutPlanetZebesText] = new(ColourEffectStart.Plain, ColourEffectColours.PlanetZebesText, ColourEffectCondition.WatchesNothing.Instance, Once(S.TextFade)),
        [A.CutsceneCeresNavigationLightsBackground] = new(ColourEffectStart.Plain, ColourEffectColours.CeresNavigationLightsBackground, ColourEffectCondition.WatchesNothing.Instance, Looping(S.SteadyBlink)),
        [A.OldMotherBrainFightBackgroundLights] = new(ColourEffectStart.Plain, ColourEffectColours.OldMotherBrainBackgroundLights, ColourEffectCondition.WatchesNothing.Instance, Looping(S.OldMotherBrainBackgroundLights)),
        [A.GunshipGlow] = new(ColourEffectStart.Plain, ColourEffectColours.GunshipGlow, ColourEffectCondition.WatchesNothing.Instance, Looping(S.GunshipGlow)),
        [A.FadeOutZoomedOutExplodingZebes] = new(ColourEffectStart.Plain, ColourEffectColours.ZoomedOutExplodingZebes, ColourEffectCondition.WatchesNothing.Instance, Once(S.ZoomedOutZebesFade)),
        [A.WidePartOfZebesExplosionForeground] = new(ColourEffectStart.Plain, ColourEffectColours.WideZebesExplosionForeground, ColourEffectCondition.WatchesNothing.Instance, Once(S.WideZebesExplosionForeground)),
        [A.ZebesExplosionFinale] = new(ColourEffectStart.Plain, ColourEffectColours.ZebesExplosionFinale, ColourEffectCondition.WatchesNothing.Instance, Once(S.ZebesExplosionFinale)),
        [A.WhiteOutSpaceDuringZebesExplosion] = new(ColourEffectStart.Plain, ColourEffectColours.SpaceDuringZebesExplosion, ColourEffectCondition.WatchesNothing.Instance, Once(S.ZebesExplosionWhiteOut)),
        [A.ZebesExplosionPlanetAfterglow] = new(ColourEffectStart.Plain, ColourEffectColours.ZebesPlanetAfterglow, ColourEffectCondition.WatchesNothing.Instance, Looping(S.ZebesAfterglow)),
        [A.ExplodingZebesLava] = new(ColourEffectStart.Plain, ColourEffectColours.ExplodingZebesLava, ColourEffectCondition.WatchesNothing.Instance, Looping(S.ExplodingZebesLava)),
        [A.FadeOutExplodingZebesCrust] = new(ColourEffectStart.Plain, ColourEffectColours.ExplodingZebesCrust, ColourEffectCondition.WatchesNothing.Instance, Once(S.ExplodingZebesCrustFade)),
        [A.FadeOutExplodingZebesGreyClouds] = new(ColourEffectStart.Plain, ColourEffectColours.ExplodingZebesGreyClouds, ColourEffectCondition.WatchesNothing.Instance, Once(S.ExplodingZebesGreyCloudsFade)),
        [A.GunshipEmergingFromZebesExplosion] = new(ColourEffectStart.Plain, ColourEffectColours.GunshipEmergingFromExplosion, ColourEffectCondition.WatchesNothing.Instance, Once(S.GunshipEmerging)),
        [A.WidePartOfZebesExplosionBackground] = new(ColourEffectStart.Plain, ColourEffectColours.WideZebesExplosionBackground, ColourEffectCondition.WatchesNothing.Instance, Once(S.ZebesExplosionWhiteOut)),
        [A.HyperBeam] = new(ColourEffectStart.Plain, ColourEffectColours.HyperBeam, ColourEffectCondition.WatchesNothing.Instance, Looping(S.HyperBeam)),
        [A.SamusLoadingPowerSuit] = new(ColourEffectStart.Plain, ColourEffectColours.SamusLoadingSuit, ColourEffectCondition.WatchesNothing.Instance, SuitLoading),
        [A.SamusLoadingVariaSuit] = new(ColourEffectStart.Plain, ColourEffectColours.SamusLoadingSuit, ColourEffectCondition.WatchesNothing.Instance, SuitLoading),
        [A.SamusLoadingGravitySuit] = new(ColourEffectStart.Plain, ColourEffectColours.SamusLoadingSuit, ColourEffectCondition.WatchesNothing.Instance, SuitLoading),
        [A.PostCreditsSuperMetroidIcon] = new(ColourEffectStart.Plain, ColourEffectColours.PostCreditsSuperMetroidIcon, ColourEffectCondition.WatchesNothing.Instance, Once(S.PostCreditsIcon)),
        [A.GreyOutTourianStatueDraygon] = new(ColourEffectStart.Plain, ColourEffectColours.DraygonStatue, ColourEffectCondition.WatchesNothing.Instance, Once(S.StatueGreyOut)),
        [A.GreyOutTourianStatueKraid] = new(ColourEffectStart.Plain, ColourEffectColours.KraidStatue, ColourEffectCondition.WatchesNothing.Instance, Once(S.StatueGreyOut)),
        [A.GreyOutTourianStatueRidley] = new(ColourEffectStart.Plain, ColourEffectColours.RidleyStatue, ColourEffectCondition.WatchesNothing.Instance, Once(S.StatueGreyOut)),
        [A.GreyOutTourianStatuePhantoon] = new(ColourEffectStart.Plain, ColourEffectColours.PhantoonStatue, ColourEffectCondition.WatchesNothing.Instance, Once(S.StatueGreyOut)),
        [A.BombTorizoBelly] = new(ColourEffectStart.Plain, ColourEffectColours.TorizoBelly, ColourEffectCondition.EndsWhenTorizoDies.Instance, Looping(S.TorizoBelly)),
        [A.GoldenTorizoBelly] = new(ColourEffectStart.Plain, ColourEffectColours.TorizoBelly, ColourEffectCondition.EndsWhenTorizoDies.Instance, Looping(S.TorizoBelly)),
        [A.CrateriaLightning] = new(ColourEffectStart.Plain, ColourEffectColours.LandingSiteLightning,
            new ColourEffectCondition.RestartsWhileSamusIsHigh(CrateriaLightningDefinitions.SamusHighOnSurfaceHeight),
            Animation<ColourEffectSegment>.Looping(
            [
                Segments.Whole(S.LandingSiteCalm),
                Segments.Repeated(S.LandingSiteDoubleStrike, 2),
                Segments.Whole(S.LandingSiteSecondCalm),
                Segments.Whole(S.LandingSiteSingleStrike),
            ])),
        [A.WreckedShipGreenLights] = new(ColourEffectStart.Plain, ColourEffectColours.WreckedShipGreenLights, ColourEffectCondition.WatchesNothing.Instance, Looping(S.SlowGlow)),
        [A.BrinstarBlueBackgroundSpores] = new(ColourEffectStart.Plain, ColourEffectColours.BrinstarBlueSpores, ColourEffectCondition.WatchesNothing.Instance, Looping(S.SporeDrift)),
        [A.SporeSpawnBlueBackgroundSpores] = new(ColourEffectStart.SkippedOnceAreaMiniBossIsDead, ColourEffectColours.BrinstarBlueSpores, ColourEffectCondition.EndsWhenAreaMiniBossDies.Instance, Looping(S.SporeDrift)),
        [A.BrinstarRedBackgroundGlow] = new(ColourEffectStart.Plain, ColourEffectColours.BrinstarRedGlow, ColourEffectCondition.WatchesNothing.Instance, Looping(S.SporeDrift)),
        [A.BeaconFlashing] = new(ColourEffectStart.Plain, ColourEffectColours.Beacon, ColourEffectCondition.WatchesNothing.Instance, Looping(S.BeaconFlashing)),
        [A.NorfairForegroundAndHeatPhase] = new(ColourEffectStart.Plain, ColourEffectColours.NorfairHeatForeground, ColourEffectCondition.WatchesNothing.Instance, Looping(S.NorfairHeatPhases)),
        [A.NorfairGreenForegroundHeatGlow] = new(ColourEffectStart.Plain, ColourEffectColours.NorfairGreenForeground, ColourEffectCondition.WatchesNothing.Instance, HeatPulseLoop),
        [A.NorfairPurpleForegroundHeatGlow] = new(ColourEffectStart.Plain, ColourEffectColours.NorfairPurpleForeground, ColourEffectCondition.WatchesNothing.Instance, HeatPulseLoop),
        [A.NorfairDarkForegroundHeatGlow] = new(ColourEffectStart.Plain, ColourEffectColours.NorfairDarkForeground, ColourEffectCondition.WatchesNothing.Instance, HeatPulseLoop),
        [A.MaridiaSandPits] = new(ColourEffectStart.Plain, ColourEffectColours.MaridiaSandPits, ColourEffectCondition.WatchesNothing.Instance, Looping(S.MaridiaSand)),
        [A.MaridiaSandFalls] = new(ColourEffectStart.Plain, ColourEffectColours.MaridiaSandFalls, ColourEffectCondition.WatchesNothing.Instance, Looping(S.MaridiaSand)),
        [A.MaridiaBackgroundWaterfalls] = new(ColourEffectStart.Plain, ColourEffectColours.MaridiaBackgroundWaterfalls, ColourEffectCondition.WatchesNothing.Instance, Looping(S.MaridiaBackgroundWaterfalls)),
        [A.TourianGlowingArkanoidBlocksAndRedOrbs] = new(ColourEffectStart.Plain, ColourEffectColours.TourianArkanoidBlocksAndOrbs, ColourEffectCondition.WatchesNothing.Instance, Looping(S.TourianArkanoidBlocksGlow)),
        [A.TourianShutterRedFlashing] = new(ColourEffectStart.Plain, ColourEffectColours.TourianShutters, ColourEffectCondition.WatchesNothing.Instance, Looping(S.TourianEscapeRedFlashing)),
        [A.TourianBackgroundRedFlashing] = new(ColourEffectStart.Plain, ColourEffectColours.TourianBackground, ColourEffectCondition.WatchesNothing.Instance, Looping(S.SteadyBlink)),
        [A.TourianGeneralLevelRedFlashing] = new(ColourEffectStart.Plain, ColourEffectColours.TourianLevel, ColourEffectCondition.WatchesNothing.Instance, Looping(S.TourianEscapeRedFlashing)),
        [A.TourianRedFlashingArkanoidBlocksAndRedOrbs] = new(ColourEffectStart.Plain, ColourEffectColours.TourianArkanoidBlocksAndOrbs, ColourEffectCondition.WatchesNothing.Instance, Looping(S.TourianEscapeRedFlashing)),
        [A.OldTourianRedFlashing] = new(ColourEffectStart.Plain, ColourEffectColours.OldTourianLevel, ColourEffectCondition.WatchesNothing.Instance, Looping(S.OldTourianRedFlashing)),
        [A.OldTourianBackgroundRailingsFlashYellow] = new(ColourEffectStart.Plain, ColourEffectColours.OldTourianRailings, ColourEffectCondition.WatchesNothing.Instance, Looping(S.OldTourianYellowFlash)),
        [A.OldTourianBackgroundPanelsFlashYellow] = new(ColourEffectStart.Plain, ColourEffectColours.OldTourianPanels, ColourEffectCondition.WatchesNothing.Instance, Looping(S.OldTourianYellowFlash)),
        [A.UpperCrateriaRedFlashing] = new(ColourEffectStart.Plain, ColourEffectColours.UpperCrateriaLevel, ColourEffectCondition.WatchesNothing.Instance, Looping(S.UpperCrateriaRedFlashing)),
        [A.CrateriaEscapeYellowLightning] = new(ColourEffectStart.Plain, ColourEffectColours.CrateriaEscapeLightning, ColourEffectCondition.WatchesNothing.Instance, Looping(S.EscapeLightning)),
        [A.ParlorEscapeSkyFlicker] = new(ColourEffectStart.Plain, ColourEffectColours.ParlorEscapeSky, ColourEffectCondition.WatchesNothing.Instance, Looping(S.EscapeLightning)),
        [A.SamusInHeat] = new(ColourEffectStart.ChoosesHeatAnimationForSuit, ColourEffectColours.HeatSuitGlow, ColourEffectCondition.BurnsSamusInHeat.Instance,
            ColourEffectFrames.SuitHeatPulse),
        [A.PowerSuitHeatPulse] = new(ColourEffectStart.Plain, ColourEffectColours.HeatSuitGlow, ColourEffectCondition.BurnsSamusInHeat.Instance,
            Looping(S.PowerSuitHeatPulse)),
        [A.VariaSuitHeatPulse] = new(ColourEffectStart.Plain, ColourEffectColours.HeatSuitGlow, ColourEffectCondition.BurnsSamusInHeat.Instance, HeatPulseLoop),
        [A.GravitySuitHeatPulse] = new(ColourEffectStart.Plain, ColourEffectColours.HeatSuitGlow, ColourEffectCondition.BurnsSamusInHeat.Instance, HeatPulseLoop),
    }.ToFrozenDictionary();

    /// <summary>Each suit's heat pulse, whose frames are the heat phases' looks in that suit.</summary>
    internal static IdentityTable<ColourEffectHeatSuit, ColourEffectId> HeatPulses { get; } = new(new Dictionary<ColourEffectHeatSuit, ColourEffectId>
    {
        [ColourEffectHeatSuit.Power] = A.PowerSuitHeatPulse,
        [ColourEffectHeatSuit.Varia] = A.VariaSuitHeatPulse,
        [ColourEffectHeatSuit.Gravity] = A.GravitySuitHeatPulse,
    });

    /// <summary>How <paramref name="effect"/> starts, the colours it shows its frames in, what it watches and how it plays.</summary>
    internal static ColourEffectDefinition Definition(ColourEffectId effect) => Definitions[effect];

    /// <summary>The frame of a heat pulse that shows <paramref name="phase"/>'s look.</summary>
    internal static int HeatLook(HeatPhase phase) => phase.Look;

    /// <summary>Shows frame <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<int, ColourEffectCue> F(int frame, int duration) => new AnimationBeat<int, ColourEffectCue>.Frame(frame, duration);

    /// <summary>The <paramref name="count"/> frames from <paramref name="first"/> on, in turn, each shown for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<int, ColourEffectCue>[] Run(int first, int count, int duration) =>
        AnimationBeats<int, ColourEffectCue>.Steady(duration, [.. Enumerable.Range(first, count)]);

    /// <summary>Shows <paramref name="phase"/>'s look of a heat pulse for <paramref name="updates"/> updates.</summary>
    private static AnimationBeat<int, ColourEffectCue> Heat(HeatPhase phase, int updates) => F(HeatLook(phase), updates);

    /// <summary>Raises <paramref name="cue"/> for the running effect.</summary>
    private static AnimationBeat<int, ColourEffectCue> E(ColourEffectCue cue) => new AnimationBeat<int, ColourEffectCue>.Event(cue);

    /// <summary>An animation that plays <paramref name="segment"/> once and is over.</summary>
    private static Animation<ColourEffectSegment> Once(ColourEffectSegment segment) => new([Segments.Whole(segment)], AnimationEnd.Stops);

    /// <summary>An animation that plays <paramref name="segment"/> over and over.</summary>
    private static Animation<ColourEffectSegment> Looping(ColourEffectSegment segment) => Animation<ColourEffectSegment>.Looping([Segments.Whole(segment)]);
}
