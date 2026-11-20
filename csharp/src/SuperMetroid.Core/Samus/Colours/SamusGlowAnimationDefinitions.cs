using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;
using F = SuperMetroid.Core.Samus.Artwork.SamusFullBodyCycleFamily;
using G = SuperMetroid.Core.Samus.Colours.SamusGlowSegment;
using S = SuperMetroid.Core.Samus.Colours.SamusSuit;
using static SuperMetroid.Core.DefinitionRun;

namespace SuperMetroid.Core.Samus.Colours;

/// <summary>
/// Samus's named shade sequences (#2351, #2387), each a run of the shades it shows with the updates each shows for, played
/// as an animation: the charge pulse shows its shades one update each and starts again; a charged shot's flash paints
/// the yellow glow on each of its updates; a Hyper Beam shot's glow shows the Hyper Beam's full-body glow from its last
/// frame to its first; the Hyper Beam's colour cycle shows its beam colourings and starts again. The Crystal Flash
/// pulses its body slowly through its shades and turns its bubble more quickly through its own. The drained Samus's
/// rainbow walks the Hyper Beam's full-body glow, each shade held as long as the Baby's hold says. The visor widens
/// with the scope's beam through its shades and rests on the widest, and the steady visor cycle shows its own. The
/// stored shine and the Screw Attack rise through their shades and fall partway back, one update each; the shinespark
/// climbs its own; the Speed Booster rises through its shades and rests on its brightest. A shot's glow restores the
/// suit's own shade on the update after its last.
/// </summary>
internal static class SamusGlowAnimationDefinitions
{
    /// <summary>
    /// Every sequence's shades, each with the updates it shows for. A charged shot's flash paints its yellow anew on each
    /// of its updates, over any colouring laid on the suit after it on the update before, so it is a beat of one update for
    /// each rather than one beat for them all.
    /// </summary>
    internal static AnimationSegments<G, PictureShade, NoAnimationCue> Segments { get; } = new(
        new Dictionary<G, AnimationBeat<PictureShade, NoAnimationCue>[]>
    {
        [G.ChargedBeamPowerPulse] = Each(SamusShades.ChargePulse(SamusChargeKind.ChargedBeam, S.Power), 1),
        [G.ChargedBeamVariaPulse] = Each(SamusShades.ChargePulse(SamusChargeKind.ChargedBeam, S.Varia), 1),
        [G.ChargedBeamGravityPulse] = Each(SamusShades.ChargePulse(SamusChargeKind.ChargedBeam, S.Gravity), 1),
        [G.PseudoScrewPowerPulse] = Each(SamusShades.ChargePulse(SamusChargeKind.PseudoScrew, S.Power), 1),
        [G.PseudoScrewVariaPulse] = Each(SamusShades.ChargePulse(SamusChargeKind.PseudoScrew, S.Varia), 1),
        [G.PseudoScrewGravityPulse] = Each(SamusShades.ChargePulse(SamusChargeKind.PseudoScrew, S.Gravity), 1),
        [G.ChargedShotFlash] = Each([.. Repeated(SamusShades.ChargedShotGlow, 3)], 1),
        [G.HyperShotGlow] = Each(SamusShades.HyperShot.Reverse(), 2),
        [G.HyperBeamCycle] = Each(SamusShades.HyperBeamCycle, 2),
        [G.CrystalFlashBody] = Each(SamusShades.CrystalFlashBody, 10),
        [G.CrystalFlashBubble] = Each(SamusShades.CrystalFlashBubble, 5),
        [G.HyperBeamRainbow] = Each(SamusShades.HyperBeamSuit, 1),
        [G.XrayVisorWidening] = Each(SamusShades.XrayWideningVisor.SkipLast(1), 5),
        [G.XrayVisorWidest] = [Show(SamusShades.XrayWideningVisor[^1], 5)],
        [G.VisorCycle] = Each(SamusShades.VisorCycle, 5),
        [G.StoredShinePower] = RiseAndFall(F.StoredShine, S.Power),
        [G.StoredShineVaria] = RiseAndFall(F.StoredShine, S.Varia),
        [G.StoredShineGravity] = RiseAndFall(F.StoredShine, S.Gravity),
        [G.ActiveShinesparkPower] = Each(SamusShades.FullBodyCycleShades(F.ActiveShinespark, S.Power), 1),
        [G.ActiveShinesparkVaria] = Each(SamusShades.FullBodyCycleShades(F.ActiveShinespark, S.Varia), 1),
        [G.ActiveShinesparkGravity] = Each(SamusShades.FullBodyCycleShades(F.ActiveShinespark, S.Gravity), 1),
        [G.ScrewAttackPower] = RiseAndFall(F.ScrewAttack, S.Power),
        [G.ScrewAttackVaria] = RiseAndFall(F.ScrewAttack, S.Varia),
        [G.ScrewAttackGravity] = RiseAndFall(F.ScrewAttack, S.Gravity),
        [G.SpeedBoosterRisingPower] = Each(SamusShades.FullBodyCycleShades(F.SpeedBooster, S.Power).SkipLast(1), 4),
        [G.SpeedBoosterRisingVaria] = Each(SamusShades.FullBodyCycleShades(F.SpeedBooster, S.Varia).SkipLast(1), 4),
        [G.SpeedBoosterRisingGravity] = Each(SamusShades.FullBodyCycleShades(F.SpeedBooster, S.Gravity).SkipLast(1), 4),
        [G.SpeedBoosterBrightestPower] = [Show(SamusShades.FullBodyCycleShades(F.SpeedBooster, S.Power)[^1], 4)],
        [G.SpeedBoosterBrightestVaria] = [Show(SamusShades.FullBodyCycleShades(F.SpeedBooster, S.Varia)[^1], 4)],
        [G.SpeedBoosterBrightestGravity] = [Show(SamusShades.FullBodyCycleShades(F.SpeedBooster, S.Gravity)[^1], 4)],
    });

    /// <summary>
    /// Each sequence's animation: a pulse, the Hyper Beam's cycle, the Crystal Flash's pulse and bubble, the rainbow, the
    /// steady visor cycle, the shines and the Screw Attack start again after their last shade; the X-ray widening and the
    /// Speed Booster rest on their last, showing it again each time it runs out; a shot's glow ends after its last.
    /// </summary>
    internal static IReadOnlyDictionary<G, Animation<G>> Animations { get; } = Segments.Starts(
        new Dictionary<G, Animation<G>>
        {
            [G.ChargedShotFlash] = new([Segments.Whole(G.ChargedShotFlash)], AnimationEnd.Stops),
            [G.HyperShotGlow] = new([Segments.Whole(G.HyperShotGlow)], AnimationEnd.Stops),
            [G.XrayVisorWidening] = Animation<G>.LeadingInto([Segments.Whole(G.XrayVisorWidening)], [Segments.Whole(G.XrayVisorWidest)]),
            [G.SpeedBoosterRisingPower] = Animation<G>.LeadingInto(
                [Segments.Whole(G.SpeedBoosterRisingPower)], [Segments.Whole(G.SpeedBoosterBrightestPower)]),
            [G.SpeedBoosterRisingVaria] = Animation<G>.LeadingInto(
                [Segments.Whole(G.SpeedBoosterRisingVaria)], [Segments.Whole(G.SpeedBoosterBrightestVaria)]),
            [G.SpeedBoosterRisingGravity] = Animation<G>.LeadingInto(
                [Segments.Whole(G.SpeedBoosterRisingGravity)], [Segments.Whole(G.SpeedBoosterBrightestGravity)]),
        },
        G.ChargedBeamPowerPulse, G.ChargedBeamVariaPulse, G.ChargedBeamGravityPulse,
        G.PseudoScrewPowerPulse, G.PseudoScrewVariaPulse, G.PseudoScrewGravityPulse,
        G.HyperBeamCycle, G.CrystalFlashBody, G.CrystalFlashBubble, G.HyperBeamRainbow, G.VisorCycle,
        G.StoredShinePower, G.StoredShineVaria, G.StoredShineGravity,
        G.ActiveShinesparkPower, G.ActiveShinesparkVaria, G.ActiveShinesparkGravity,
        G.ScrewAttackPower, G.ScrewAttackVaria, G.ScrewAttackGravity);

    /// <summary>
    /// The pulse Samus's suit shows while she is charged, for a charged beam or the pseudo Screw Attack in each suit. Every
    /// pulse runs its six shades alike, so the place one pulse stands at is where another goes on from.
    /// </summary>
    internal static IReadOnlyDictionary<SamusChargeKind, SamusSuitGlows> ChargePulses { get; } =
        new Dictionary<SamusChargeKind, SamusSuitGlows>
        {
            [SamusChargeKind.ChargedBeam] = new(G.ChargedBeamPowerPulse, G.ChargedBeamVariaPulse, G.ChargedBeamGravityPulse),
            [SamusChargeKind.PseudoScrew] = new(G.PseudoScrewPowerPulse, G.PseudoScrewVariaPulse, G.PseudoScrewGravityPulse),
        };

    /// <summary>
    /// The stored shine Samus's suit shows in each suit (#2387). Every suit's shine runs its six shades alike, so the place
    /// one stands at is where another goes on from.
    /// </summary>
    internal static SamusSuitGlows StoredShine { get; } = new(G.StoredShinePower, G.StoredShineVaria, G.StoredShineGravity);

    /// <summary>The glow Samus's suit shows in each suit while she shinesparks (#2387), its four shades alike in every suit.</summary>
    internal static SamusSuitGlows ActiveShinespark { get; } = new(G.ActiveShinesparkPower, G.ActiveShinesparkVaria, G.ActiveShinesparkGravity);

    /// <summary>The glow Samus's suit shows in each suit as she spins with the Screw Attack (#2387).</summary>
    internal static SamusSuitGlows ScrewAttack { get; } = new(G.ScrewAttackPower, G.ScrewAttackVaria, G.ScrewAttackGravity);

    /// <summary>The glow Samus's suit shows in each suit while she runs boosted (#2387), named by the shades it rises through.</summary>
    internal static SamusSuitGlows SpeedBooster { get; } = new(G.SpeedBoosterRisingPower, G.SpeedBoosterRisingVaria, G.SpeedBoosterRisingGravity);

    /// <summary>Shows <paramref name="shade"/> for <paramref name="updates"/> updates.</summary>
    private static AnimationBeat<PictureShade, NoAnimationCue> Show(PictureShade shade, int updates) =>
        new AnimationBeat<PictureShade, NoAnimationCue>.Frame(shade, updates);

    /// <summary>Shows each of <paramref name="shades"/> in turn, each for <paramref name="updates"/> updates (#2447).</summary>
    private static AnimationBeat<PictureShade, NoAnimationCue>[] Each(IEnumerable<PictureShade> shades, int updates) =>
        [.. shades.Select(shade => Show(shade, updates))];

    /// <summary>
    /// <paramref name="family"/>'s rise through its four shades in <paramref name="suit"/> and its fall back through the
    /// middle two, one update each: the stored shine's and the Screw Attack's six.
    /// </summary>
    private static AnimationBeat<PictureShade, NoAnimationCue>[] RiseAndFall(F family, S suit) =>
        Each([.. SamusShades.FullBodyCycleShades(family, suit), .. SamusShades.FullBodyCycleShades(family, suit).Reverse().Skip(1).SkipLast(1)], 1);
}
