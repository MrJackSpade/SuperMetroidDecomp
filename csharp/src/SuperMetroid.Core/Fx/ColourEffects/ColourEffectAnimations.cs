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
internal static class ColourEffectAnimations
{
    /// <summary>
    /// The updates the heat pulse shows each heat phase's look for, in phase order (<see cref="HeatPhase.InOrder"/>):
    /// longest at the coolest and quickest between.
    /// </summary>
    private static readonly int[] HeatLookUpdates = [16, 4, 4, 5, 6, 7, 8, 8, 8, 8, 7, 6, 5, 4, 4, 16];

    /// <summary>The updates the Power Suit's heat pulse shows its last heat phase's look for, cut short from the pulse's.</summary>
    private const int PowerSuitLastHeatLookUpdates = 3;

    /// <summary>Each heat phase's look shown for its updates, in phase order: the heat pulse.</summary>
    private static readonly AnimationBeat<int, ColourEffectCue>[] HeatPulseLooks = [.. HeatPhase.InOrder.Zip(HeatLookUpdates, Heat)];

    /// <summary>
    /// The runs of frames the effects play; each suit's heat pulse plays its own (#2054), and Samus in heat, which plays her
    /// suit's pulse (<see cref="ColourEffectHeatAnimations.Shimmer"/>), shows none of its own.
    /// </summary>
    internal static AnimationSegments<ColourEffectSegment, int, ColourEffectCue> Segments { get; } = new(
        new Dictionary<ColourEffectSegment, AnimationBeat<int, ColourEffectCue>[]>
    {
        [S.TextFade] =
        [
            F(0, 3),
            F(1, 3),
            F(2, 3),
            F(3, 3),
            F(4, 3),
            F(5, 3),
            F(6, 3),
            F(7, 3),
        ],
        [S.Flicker] =
        [
            F(0, 1),
            F(1, 1),
        ],
        [S.SlowGlow] =
        [
            F(0, 10),
            F(1, 10),
            F(2, 10),
            F(3, 10),
            F(4, 10),
            F(5, 10),
            F(6, 10),
            F(7, 10),
        ],
        [S.SteadyBlink] =
        [
            F(0, 4),
            F(1, 4),
            F(2, 4),
            F(3, 4),
            F(4, 4),
            F(5, 4),
            F(6, 4),
            F(7, 4),
            F(8, 4),
            F(9, 4),
            F(10, 4),
            F(11, 4),
            F(12, 4),
            F(13, 4),
        ],
        [S.OldMotherBrainBackgroundLights] =
        [
            F(0, 6),
            F(1, 6),
            F(2, 6),
            F(3, 6),
            F(4, 6),
            F(5, 6),
            F(6, 6),
            F(7, 6),
            F(8, 6),
            F(9, 6),
            F(10, 6),
            F(11, 6),
            F(12, 6),
            F(13, 6),
        ],
        [S.GunshipGlow] =
        [
            F(0, 5),
            F(1, 5),
            F(2, 5),
            F(3, 5),
            F(4, 5),
            F(5, 5),
            F(6, 5),
            F(7, 5),
            F(8, 5),
            F(9, 5),
            F(10, 5),
            F(11, 5),
            F(12, 5),
            F(13, 5),
        ],
        [S.ZoomedOutZebesFade] =
        [
            F(0, 8),
            F(1, 8),
            F(2, 8),
            F(3, 8),
            F(4, 8),
            F(5, 8),
            F(6, 8),
        ],
        [S.WideZebesExplosionForeground] =
        [
            F(0, 4),
            F(1, 4),
            F(2, 4),
            F(3, 60),
            F(4, 6),
            F(5, 6),
            F(6, 6),
            F(7, 6),
            F(8, 6),
            F(9, 6),
            F(10, 6),
            F(11, 6),
            F(12, 6),
            F(13, 6),
            F(14, 6),
            F(15, 6),
        ],
        [S.ZebesExplosionFinale] =
        [
            F(0, 2),
            F(1, 2),
            F(2, 2),
            F(3, 2),
            F(4, 2),
            F(5, 2),
            F(6, 2),
            F(7, 2),
            F(8, 2),
            F(9, 2),
            F(10, 2),
            F(11, 2),
            F(12, 2),
            F(13, 2),
            F(14, 2),
            F(15, 9),
            F(16, 9),
            F(17, 9),
            F(18, 9),
            F(19, 9),
            F(20, 9),
            F(21, 9),
            F(22, 9),
            F(23, 9),
            F(24, 9),
            F(25, 9),
            F(26, 9),
            F(27, 9),
            F(28, 9),
            F(29, 9),
            F(30, 9),
            F(31, 9),
            F(32, 9),
            F(33, 9),
            F(34, 9),
            F(35, 9),
            F(36, 9),
            F(37, 9),
            F(38, 9),
            F(39, 9),
            F(40, 9),
            F(41, 9),
            F(42, 9),
            F(43, 9),
            F(44, 9),
        ],
        [S.ZebesExplosionWhiteOut] =
        [
            F(0, 14),
            F(1, 14),
            F(2, 14),
            F(3, 14),
            F(4, 14),
            F(5, 14),
            F(6, 14),
            F(7, 14),
            F(8, 14),
            F(9, 14),
            F(10, 14),
            F(11, 14),
            F(12, 14),
            F(13, 14),
            F(14, 14),
        ],
        [S.ZebesAfterglow] =
        [
            F(0, 16),
            F(1, 16),
            F(2, 16),
            F(3, 16),
            F(4, 16),
            F(5, 16),
        ],
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
        [S.ExplodingZebesCrustFade] =
        [
            F(0, 20),
            F(1, 20),
            F(2, 20),
            F(3, 20),
            F(4, 20),
            F(5, 20),
            F(6, 20),
            F(7, 20),
        ],
        [S.ExplodingZebesGreyCloudsFade] =
        [
            F(0, 14),
            F(1, 14),
            F(2, 14),
            F(3, 14),
            F(4, 14),
            F(5, 14),
            F(6, 14),
            F(7, 14),
        ],
        [S.GunshipEmerging] =
        [
            F(0, 24),
            F(1, 24),
            F(2, 24),
            F(3, 24),
            F(4, 24),
            F(5, 24),
            F(6, 24),
            F(7, 24),
            F(8, 24),
            F(9, 24),
            F(10, 24),
            F(11, 24),
            F(12, 24),
            F(13, 24),
            F(14, 24),
            F(15, 24),
        ],
        [S.HyperBeam] =
        [
            F(0, 2),
            F(1, 2),
            F(2, 2),
            F(3, 2),
            F(4, 2),
            F(5, 2),
            F(6, 2),
            F(7, 2),
            F(8, 2),
            F(9, 2),
        ],
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
        [S.PostCreditsIcon] =
        [
            F(0, 1),
            F(1, 1),
            F(2, 1),
            F(3, 1),
            F(4, 1),
            F(5, 1),
            F(6, 1),
            F(7, 1),
            F(8, 1),
            F(9, 1),
            F(10, 1),
            F(11, 1),
            F(12, 1),
            F(13, 1),
        ],
        [S.StatueGreyOut] =
        [
            F(0, 8),
            F(1, 8),
            F(2, 8),
            F(3, 8),
            F(4, 8),
            F(5, 8),
            F(6, 8),
            F(7, 8),
        ],
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
            F(2, 1),
            F(3, 1),
            F(4, 1),
            F(5, 1),
            F(6, 1),
            F(7, 2),
        ],
        [S.LandingSiteSecondCalm] =
        [
            F(8, 240),
        ],
        [S.LandingSiteSingleStrike] =
        [
            F(9, 1),
            F(10, 1),
            F(11, 1),
            F(12, 2),
        ],
        [S.SporeDrift] =
        [
            F(0, 10),
            F(1, 10),
            F(2, 10),
            F(3, 10),
            F(4, 10),
            F(5, 10),
            F(6, 10),
            F(7, 10),
            F(8, 10),
            F(9, 10),
            F(10, 10),
            F(11, 10),
            F(12, 10),
            F(13, 10),
        ],
        [S.BeaconFlashing] =
        [
            F(0, 10),
            F(1, 10),
            F(2, 10),
            F(3, 10),
            F(4, 10),
            F(5, 10),
            E(new ColourEffectCue.QueueSound(Sound: EnemySound.Beacon)),
            F(6, 10),
            F(7, 10),
            F(8, 10),
            F(9, 10),
        ],
        [S.NorfairHeatPhases] = [.. HeatPhase.InOrder.Zip(HeatLookUpdates, (phase, updates) => new[] { E(new ColourEffectCue.PublishHeatPhase(phase)), Heat(phase, updates) })
            .SelectMany(look => look)],
        [S.MaridiaSand] =
        [
            F(0, 10),
            F(1, 10),
            F(2, 10),
            F(3, 10),
        ],
        [S.MaridiaBackgroundWaterfalls] =
        [
            F(0, 2),
            F(1, 2),
            F(2, 2),
            F(3, 2),
            F(4, 2),
            F(5, 2),
            F(6, 2),
            F(7, 2),
        ],
        [S.TourianArkanoidBlocksGlow] =
        [
            F(0, 10),
            F(1, 10),
            F(2, 10),
            F(3, 10),
            F(4, 10),
            F(5, 10),
            F(6, 10),
            F(7, 10),
            F(8, 10),
            F(9, 10),
            F(10, 10),
        ],
        [S.TourianEscapeRedFlashing] =
        [
            F(0, 2),
            F(1, 2),
            F(2, 2),
            F(3, 2),
            F(4, 2),
            F(5, 2),
            F(6, 2),
            F(7, 2),
            F(8, 2),
            F(9, 2),
            F(10, 2),
            F(11, 2),
            F(12, 2),
            F(13, 2),
        ],
        [S.OldTourianRedFlashing] =
        [
            F(0, 3),
            F(1, 3),
            F(2, 3),
            F(3, 3),
            F(4, 3),
            F(5, 3),
            F(6, 3),
            F(7, 3),
            F(8, 3),
            F(9, 3),
            F(10, 3),
            F(11, 3),
            F(12, 3),
            F(13, 3),
        ],
        [S.OldTourianYellowFlash] =
        [
            F(0, 16),
            F(1, 1),
            F(2, 1),
            F(3, 2),
            F(4, 1),
            F(5, 2),
            F(6, 1),
            F(7, 1),
            F(8, 1),
            F(9, 1),
            F(10, 32),
            F(11, 2),
            F(12, 1),
            F(13, 1),
            F(14, 1),
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
            F(1, 1),
            F(2, 1),
            F(3, 1),
            F(4, 1),
            F(5, 17),
            F(6, 1),
            F(7, 24),
            F(8, 1),
            F(9, 1),
            F(10, 1),
        ],
    });

    /// <summary>
    /// A suit's loading: its opening flicker played thirty-six times, its second and third three times each, its last twice,
    /// then its loaded look once. Each suit plays it in its own frames.
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
    /// start chooses her suit's pulse (<see cref="ColourEffectHeatAnimations.Shimmer"/>), which plays as her shimmer does, in
    /// the same colours and watching the same heat.
    /// </summary>
    private static readonly FrozenDictionary<ColourEffectId, ColourEffectDefinition> Definitions =
        new Dictionary<ColourEffectId, ColourEffectDefinition>
    {
        [A.FadeInSuperMetroidTitleLogo] = new(ColourEffectStart.Plain, ColourEffectColours.TitleLogo, null, Once(S.TextFade)),
        [A.FadeInNintendoCopyright] = new(ColourEffectStart.Plain, ColourEffectColours.NintendoCopyright, null, Once(S.TextFade)),
        [A.TitleScreenBabyMetroidTubeLight] = new(ColourEffectStart.Plain, ColourEffectColours.BabyMetroidTubeLight, null, Looping(S.SlowGlow)),
        [A.TitleScreenFlickeringDisplays] = new(ColourEffectStart.Plain, ColourEffectColours.TitleScreenDisplays, null, Looping(S.Flicker)),
        [A.CutsceneGunshipEngineFlicker] = new(ColourEffectStart.Plain, ColourEffectColours.CutsceneGunshipEngine, null, Looping(S.Flicker)),
        [A.CutsceneCeresNavigationLightsSprite] = new(ColourEffectStart.Plain, ColourEffectColours.CeresNavigationLightsSprite, null, Looping(S.SteadyBlink)),
        [A.FadeInPlanetZebesText] = new(ColourEffectStart.Plain, ColourEffectColours.PlanetZebesText, null, Once(S.TextFade)),
        [A.FadeOutPlanetZebesText] = new(ColourEffectStart.Plain, ColourEffectColours.PlanetZebesText, null, Once(S.TextFade)),
        [A.CutsceneCeresNavigationLightsBackground] = new(ColourEffectStart.Plain, ColourEffectColours.CeresNavigationLightsBackground, null, Looping(S.SteadyBlink)),
        [A.OldMotherBrainFightBackgroundLights] = new(ColourEffectStart.Plain, ColourEffectColours.OldMotherBrainBackgroundLights, null, Looping(S.OldMotherBrainBackgroundLights)),
        [A.GunshipGlow] = new(ColourEffectStart.Plain, ColourEffectColours.GunshipGlow, null, Looping(S.GunshipGlow)),
        [A.FadeOutZoomedOutExplodingZebes] = new(ColourEffectStart.Plain, ColourEffectColours.ZoomedOutExplodingZebes, null, Once(S.ZoomedOutZebesFade)),
        [A.WidePartOfZebesExplosionForeground] = new(ColourEffectStart.Plain, ColourEffectColours.WideZebesExplosionForeground, null, Once(S.WideZebesExplosionForeground)),
        [A.ZebesExplosionFinale] = new(ColourEffectStart.Plain, ColourEffectColours.ZebesExplosionFinale, null, Once(S.ZebesExplosionFinale)),
        [A.WhiteOutSpaceDuringZebesExplosion] = new(ColourEffectStart.Plain, ColourEffectColours.SpaceDuringZebesExplosion, null, Once(S.ZebesExplosionWhiteOut)),
        [A.ZebesExplosionPlanetAfterglow] = new(ColourEffectStart.Plain, ColourEffectColours.ZebesPlanetAfterglow, null, Looping(S.ZebesAfterglow)),
        [A.ExplodingZebesLava] = new(ColourEffectStart.Plain, ColourEffectColours.ExplodingZebesLava, null, Looping(S.ExplodingZebesLava)),
        [A.FadeOutExplodingZebesCrust] = new(ColourEffectStart.Plain, ColourEffectColours.ExplodingZebesCrust, null, Once(S.ExplodingZebesCrustFade)),
        [A.FadeOutExplodingZebesGreyClouds] = new(ColourEffectStart.Plain, ColourEffectColours.ExplodingZebesGreyClouds, null, Once(S.ExplodingZebesGreyCloudsFade)),
        [A.GunshipEmergingFromZebesExplosion] = new(ColourEffectStart.Plain, ColourEffectColours.GunshipEmergingFromExplosion, null, Once(S.GunshipEmerging)),
        [A.WidePartOfZebesExplosionBackground] = new(ColourEffectStart.Plain, ColourEffectColours.WideZebesExplosionBackground, null, Once(S.ZebesExplosionWhiteOut)),
        [A.HyperBeam] = new(ColourEffectStart.Plain, ColourEffectColours.HyperBeam, null, Looping(S.HyperBeam)),
        [A.SamusLoadingPowerSuit] = new(ColourEffectStart.Plain, ColourEffectColours.SamusLoadingSuit, null, SuitLoading),
        [A.SamusLoadingVariaSuit] = new(ColourEffectStart.Plain, ColourEffectColours.SamusLoadingSuit, null, SuitLoading),
        [A.SamusLoadingGravitySuit] = new(ColourEffectStart.Plain, ColourEffectColours.SamusLoadingSuit, null, SuitLoading),
        [A.PostCreditsSuperMetroidIcon] = new(ColourEffectStart.Plain, ColourEffectColours.PostCreditsSuperMetroidIcon, null, Once(S.PostCreditsIcon)),
        [A.GreyOutTourianStatueDraygon] = new(ColourEffectStart.Plain, ColourEffectColours.DraygonStatue, null, Once(S.StatueGreyOut)),
        [A.GreyOutTourianStatueKraid] = new(ColourEffectStart.Plain, ColourEffectColours.KraidStatue, null, Once(S.StatueGreyOut)),
        [A.GreyOutTourianStatueRidley] = new(ColourEffectStart.Plain, ColourEffectColours.RidleyStatue, null, Once(S.StatueGreyOut)),
        [A.GreyOutTourianStatuePhantoon] = new(ColourEffectStart.Plain, ColourEffectColours.PhantoonStatue, null, Once(S.StatueGreyOut)),
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
        [A.WreckedShipGreenLights] = new(ColourEffectStart.Plain, ColourEffectColours.WreckedShipGreenLights, null, Looping(S.SlowGlow)),
        [A.BrinstarBlueBackgroundSpores] = new(ColourEffectStart.Plain, ColourEffectColours.BrinstarBlueSpores, null, Looping(S.SporeDrift)),
        [A.SporeSpawnBlueBackgroundSpores] = new(ColourEffectStart.SkippedOnceAreaMiniBossIsDead, ColourEffectColours.BrinstarBlueSpores, ColourEffectCondition.EndsWhenAreaMiniBossDies.Instance, Looping(S.SporeDrift)),
        [A.BrinstarRedBackgroundGlow] = new(ColourEffectStart.Plain, ColourEffectColours.BrinstarRedGlow, null, Looping(S.SporeDrift)),
        [A.BeaconFlashing] = new(ColourEffectStart.Plain, ColourEffectColours.Beacon, null, Looping(S.BeaconFlashing)),
        [A.NorfairForegroundAndHeatPhase] = new(ColourEffectStart.Plain, ColourEffectColours.NorfairHeatForeground, null, Looping(S.NorfairHeatPhases)),
        [A.NorfairGreenForegroundHeatGlow] = new(ColourEffectStart.Plain, ColourEffectColours.NorfairGreenForeground, null, HeatPulseLoop),
        [A.NorfairPurpleForegroundHeatGlow] = new(ColourEffectStart.Plain, ColourEffectColours.NorfairPurpleForeground, null, HeatPulseLoop),
        [A.NorfairDarkForegroundHeatGlow] = new(ColourEffectStart.Plain, ColourEffectColours.NorfairDarkForeground, null, HeatPulseLoop),
        [A.MaridiaSandPits] = new(ColourEffectStart.Plain, ColourEffectColours.MaridiaSandPits, null, Looping(S.MaridiaSand)),
        [A.MaridiaSandFalls] = new(ColourEffectStart.Plain, ColourEffectColours.MaridiaSandFalls, null, Looping(S.MaridiaSand)),
        [A.MaridiaBackgroundWaterfalls] = new(ColourEffectStart.Plain, ColourEffectColours.MaridiaBackgroundWaterfalls, null, Looping(S.MaridiaBackgroundWaterfalls)),
        [A.TourianGlowingArkanoidBlocksAndRedOrbs] = new(ColourEffectStart.Plain, ColourEffectColours.TourianArkanoidBlocksAndOrbs, null, Looping(S.TourianArkanoidBlocksGlow)),
        [A.TourianShutterRedFlashing] = new(ColourEffectStart.Plain, ColourEffectColours.TourianShutters, null, Looping(S.TourianEscapeRedFlashing)),
        [A.TourianBackgroundRedFlashing] = new(ColourEffectStart.Plain, ColourEffectColours.TourianBackground, null, Looping(S.SteadyBlink)),
        [A.TourianGeneralLevelRedFlashing] = new(ColourEffectStart.Plain, ColourEffectColours.TourianLevel, null, Looping(S.TourianEscapeRedFlashing)),
        [A.TourianRedFlashingArkanoidBlocksAndRedOrbs] = new(ColourEffectStart.Plain, ColourEffectColours.TourianArkanoidBlocksAndOrbs, null, Looping(S.TourianEscapeRedFlashing)),
        [A.OldTourianRedFlashing] = new(ColourEffectStart.Plain, ColourEffectColours.OldTourianLevel, null, Looping(S.OldTourianRedFlashing)),
        [A.OldTourianBackgroundRailingsFlashYellow] = new(ColourEffectStart.Plain, ColourEffectColours.OldTourianRailings, null, Looping(S.OldTourianYellowFlash)),
        [A.OldTourianBackgroundPanelsFlashYellow] = new(ColourEffectStart.Plain, ColourEffectColours.OldTourianPanels, null, Looping(S.OldTourianYellowFlash)),
        [A.UpperCrateriaRedFlashing] = new(ColourEffectStart.Plain, ColourEffectColours.UpperCrateriaLevel, null, Looping(S.UpperCrateriaRedFlashing)),
        [A.CrateriaEscapeYellowLightning] = new(ColourEffectStart.Plain, ColourEffectColours.CrateriaEscapeLightning, null, Looping(S.EscapeLightning)),
        [A.ParlorEscapeSkyFlicker] = new(ColourEffectStart.Plain, ColourEffectColours.ParlorEscapeSky, null, Looping(S.EscapeLightning)),
        [A.SamusInHeat] = new(ColourEffectStart.ChoosesHeatAnimationForSuit, ColourEffectColours.HeatSuitGlow, ColourEffectCondition.BurnsSamusInHeat.Instance,
            new([], AnimationEnd.Stops)),
        [A.PowerSuitHeatPulse] = new(ColourEffectStart.Plain, ColourEffectColours.HeatSuitGlow, ColourEffectCondition.BurnsSamusInHeat.Instance,
            Looping(S.PowerSuitHeatPulse)),
        [A.VariaSuitHeatPulse] = new(ColourEffectStart.Plain, ColourEffectColours.HeatSuitGlow, ColourEffectCondition.BurnsSamusInHeat.Instance, HeatPulseLoop),
        [A.GravitySuitHeatPulse] = new(ColourEffectStart.Plain, ColourEffectColours.HeatSuitGlow, ColourEffectCondition.BurnsSamusInHeat.Instance, HeatPulseLoop),
    }.ToFrozenDictionary();

    /// <summary>Each suit's heat pulse, whose sixteen frames are the heat phases' looks in that suit.</summary>
    internal static FrozenDictionary<ColourEffectHeatSuit, ColourEffectId> HeatPulses { get; } =
        new Dictionary<ColourEffectHeatSuit, ColourEffectId>
    {
        [ColourEffectHeatSuit.Power] = A.PowerSuitHeatPulse,
        [ColourEffectHeatSuit.Varia] = A.VariaSuitHeatPulse,
        [ColourEffectHeatSuit.Gravity] = A.GravitySuitHeatPulse,
    }.ToFrozenDictionary();

    /// <summary>How <paramref name="effect"/> starts, the colours it shows its frames in, what it watches and how it plays.</summary>
    internal static ColourEffectDefinition Definition(ColourEffectId effect) => Definitions[effect];

    /// <summary>
    /// The frame of a heat pulse that shows <paramref name="phase"/>'s look: a pulse's frames are the heat phases' looks, in
    /// phase order.
    /// </summary>
    internal static int HeatLook(HeatPhase phase) => phase.Index;

    /// <summary>Shows frame <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<int, ColourEffectCue> F(int frame, int duration) => new AnimationBeat<int, ColourEffectCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="phase"/>'s look of a heat pulse for <paramref name="updates"/> updates.</summary>
    private static AnimationBeat<int, ColourEffectCue> Heat(HeatPhase phase, int updates) => F(HeatLook(phase), updates);

    /// <summary>Raises <paramref name="cue"/> for the running effect.</summary>
    private static AnimationBeat<int, ColourEffectCue> E(ColourEffectCue cue) => new AnimationBeat<int, ColourEffectCue>.Event(cue);

    /// <summary>An animation that plays <paramref name="segment"/> once and is over.</summary>
    private static Animation<ColourEffectSegment> Once(ColourEffectSegment segment) => new([Segments.Whole(segment)], AnimationEnd.Stops);

    /// <summary>An animation that plays <paramref name="segment"/> over and over.</summary>
    private static Animation<ColourEffectSegment> Looping(ColourEffectSegment segment) => Animation<ColourEffectSegment>.Looping([Segments.Whole(segment)]);
}
