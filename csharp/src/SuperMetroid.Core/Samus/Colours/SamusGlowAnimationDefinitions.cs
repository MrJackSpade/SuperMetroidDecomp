using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;
using F = SuperMetroid.Core.Samus.Artwork.SamusFullBodyCycleFamily;
using G = SuperMetroid.Core.Samus.Colours.SamusGlowSegment;
using S = SuperMetroid.Core.Samus.Colours.SamusSuit;

namespace SuperMetroid.Core.Samus.Colours;

/// <summary>
/// Samus's named shade sequences (#2351, #2387), each a run of the shades it shows with the updates each shows for, played
/// as an animation: the charge pulse shows its six shades one update each and starts again; a charged shot's flash paints
/// the yellow glow on each of three updates; a Hyper Beam shot's glow shows the Hyper Beam's full-body glow from its last frame to
/// its first, two updates each; the Hyper Beam's colour cycle shows its ten beam colourings two updates each and starts
/// again. The Crystal Flash pulses its body through ten shades ten updates each and turns its bubble through six five
/// updates each. The drained Samus's rainbow walks the Hyper Beam's full-body glow, each shade held as long as the Baby's
/// hold says. The visor widens with the scope's beam, three shades five updates each, and rests on the widest, and the
/// steady visor cycle shows its three five updates each. The stored shine and the Screw Attack rise through four shades
/// and fall back through two, one update each; the shinespark climbs four; the Speed Booster rises through four, four
/// updates each, and rests on its brightest. A shot's glow restores the suit's own shade on the update after its last.
/// </summary>
internal static class SamusGlowAnimationDefinitions
{
    /// <summary>
    /// Every sequence's shades, each with the updates it shows for. A charged shot's flash paints its yellow anew on each
    /// of its three updates, over any colouring laid on the suit after it on the update before, so it is three beats of one
    /// update rather than one beat of three.
    /// </summary>
    internal static AnimationSegments<G, PictureShade, NoAnimationCue> Segments { get; } = new(
        new Dictionary<G, AnimationBeat<PictureShade, NoAnimationCue>[]>
    {
        [G.ChargedBeamPowerPulse] =
        [
            Pulse(SamusChargeKind.ChargedBeam, S.Power, 0), Pulse(SamusChargeKind.ChargedBeam, S.Power, 1),
            Pulse(SamusChargeKind.ChargedBeam, S.Power, 2), Pulse(SamusChargeKind.ChargedBeam, S.Power, 3),
            Pulse(SamusChargeKind.ChargedBeam, S.Power, 4), Pulse(SamusChargeKind.ChargedBeam, S.Power, 5),
        ],
        [G.ChargedBeamVariaPulse] =
        [
            Pulse(SamusChargeKind.ChargedBeam, S.Varia, 0), Pulse(SamusChargeKind.ChargedBeam, S.Varia, 1),
            Pulse(SamusChargeKind.ChargedBeam, S.Varia, 2), Pulse(SamusChargeKind.ChargedBeam, S.Varia, 3),
            Pulse(SamusChargeKind.ChargedBeam, S.Varia, 4), Pulse(SamusChargeKind.ChargedBeam, S.Varia, 5),
        ],
        [G.ChargedBeamGravityPulse] =
        [
            Pulse(SamusChargeKind.ChargedBeam, S.Gravity, 0), Pulse(SamusChargeKind.ChargedBeam, S.Gravity, 1),
            Pulse(SamusChargeKind.ChargedBeam, S.Gravity, 2), Pulse(SamusChargeKind.ChargedBeam, S.Gravity, 3),
            Pulse(SamusChargeKind.ChargedBeam, S.Gravity, 4), Pulse(SamusChargeKind.ChargedBeam, S.Gravity, 5),
        ],
        [G.PseudoScrewPowerPulse] =
        [
            Pulse(SamusChargeKind.PseudoScrew, S.Power, 0), Pulse(SamusChargeKind.PseudoScrew, S.Power, 1),
            Pulse(SamusChargeKind.PseudoScrew, S.Power, 2), Pulse(SamusChargeKind.PseudoScrew, S.Power, 3),
            Pulse(SamusChargeKind.PseudoScrew, S.Power, 4), Pulse(SamusChargeKind.PseudoScrew, S.Power, 5),
        ],
        [G.PseudoScrewVariaPulse] =
        [
            Pulse(SamusChargeKind.PseudoScrew, S.Varia, 0), Pulse(SamusChargeKind.PseudoScrew, S.Varia, 1),
            Pulse(SamusChargeKind.PseudoScrew, S.Varia, 2), Pulse(SamusChargeKind.PseudoScrew, S.Varia, 3),
            Pulse(SamusChargeKind.PseudoScrew, S.Varia, 4), Pulse(SamusChargeKind.PseudoScrew, S.Varia, 5),
        ],
        [G.PseudoScrewGravityPulse] =
        [
            Pulse(SamusChargeKind.PseudoScrew, S.Gravity, 0), Pulse(SamusChargeKind.PseudoScrew, S.Gravity, 1),
            Pulse(SamusChargeKind.PseudoScrew, S.Gravity, 2), Pulse(SamusChargeKind.PseudoScrew, S.Gravity, 3),
            Pulse(SamusChargeKind.PseudoScrew, S.Gravity, 4), Pulse(SamusChargeKind.PseudoScrew, S.Gravity, 5),
        ],
        [G.ChargedShotFlash] =
        [
            Show(SamusShades.ChargedShotGlow, 1), Show(SamusShades.ChargedShotGlow, 1), Show(SamusShades.ChargedShotGlow, 1),
        ],
        [G.HyperShotGlow] =
        [
            Show(SamusShades.HyperShot(9), 2), Show(SamusShades.HyperShot(8), 2), Show(SamusShades.HyperShot(7), 2),
            Show(SamusShades.HyperShot(6), 2), Show(SamusShades.HyperShot(5), 2), Show(SamusShades.HyperShot(4), 2),
            Show(SamusShades.HyperShot(3), 2), Show(SamusShades.HyperShot(2), 2), Show(SamusShades.HyperShot(1), 2),
            Show(SamusShades.HyperShot(0), 2),
        ],
        [G.HyperBeamCycle] =
        [
            Show(SamusShades.HyperBeamCycle(0), 2), Show(SamusShades.HyperBeamCycle(1), 2), Show(SamusShades.HyperBeamCycle(2), 2),
            Show(SamusShades.HyperBeamCycle(3), 2), Show(SamusShades.HyperBeamCycle(4), 2), Show(SamusShades.HyperBeamCycle(5), 2),
            Show(SamusShades.HyperBeamCycle(6), 2), Show(SamusShades.HyperBeamCycle(7), 2), Show(SamusShades.HyperBeamCycle(8), 2),
            Show(SamusShades.HyperBeamCycle(9), 2),
        ],
        [G.CrystalFlashBody] =
        [
            Show(SamusShades.CrystalFlashBody[0], 10), Show(SamusShades.CrystalFlashBody[1], 10),
            Show(SamusShades.CrystalFlashBody[2], 10), Show(SamusShades.CrystalFlashBody[3], 10),
            Show(SamusShades.CrystalFlashBody[4], 10), Show(SamusShades.CrystalFlashBody[5], 10),
            Show(SamusShades.CrystalFlashBody[6], 10), Show(SamusShades.CrystalFlashBody[7], 10),
            Show(SamusShades.CrystalFlashBody[8], 10), Show(SamusShades.CrystalFlashBody[9], 10),
        ],
        [G.CrystalFlashBubble] =
        [
            Show(SamusShades.CrystalFlashBubble[0], 5), Show(SamusShades.CrystalFlashBubble[1], 5),
            Show(SamusShades.CrystalFlashBubble[2], 5), Show(SamusShades.CrystalFlashBubble[3], 5),
            Show(SamusShades.CrystalFlashBubble[4], 5), Show(SamusShades.CrystalFlashBubble[5], 5),
        ],
        [G.HyperBeamRainbow] =
        [
            Show(SamusShades.HyperBeamSuit[0], 1), Show(SamusShades.HyperBeamSuit[1], 1), Show(SamusShades.HyperBeamSuit[2], 1),
            Show(SamusShades.HyperBeamSuit[3], 1), Show(SamusShades.HyperBeamSuit[4], 1), Show(SamusShades.HyperBeamSuit[5], 1),
            Show(SamusShades.HyperBeamSuit[6], 1), Show(SamusShades.HyperBeamSuit[7], 1), Show(SamusShades.HyperBeamSuit[8], 1),
            Show(SamusShades.HyperBeamSuit[9], 1),
        ],
        [G.XrayVisorWidening] = [Show(SamusShades.XrayWideningVisor[0], 5), Show(SamusShades.XrayWideningVisor[1], 5)],
        [G.XrayVisorWidest] = [Show(SamusShades.XrayWideningVisor[2], 5)],
        [G.VisorCycle] = [Show(SamusShades.VisorCycle[0], 5), Show(SamusShades.VisorCycle[1], 5), Show(SamusShades.VisorCycle[2], 5)],
        [G.StoredShinePower] = RiseAndFall(F.StoredShine, S.Power),
        [G.StoredShineVaria] = RiseAndFall(F.StoredShine, S.Varia),
        [G.StoredShineGravity] = RiseAndFall(F.StoredShine, S.Gravity),
        [G.ActiveShinesparkPower] =
        [
            Cycle(F.ActiveShinespark, S.Power, 0, 1), Cycle(F.ActiveShinespark, S.Power, 1, 1),
            Cycle(F.ActiveShinespark, S.Power, 2, 1), Cycle(F.ActiveShinespark, S.Power, 3, 1),
        ],
        [G.ActiveShinesparkVaria] =
        [
            Cycle(F.ActiveShinespark, S.Varia, 0, 1), Cycle(F.ActiveShinespark, S.Varia, 1, 1),
            Cycle(F.ActiveShinespark, S.Varia, 2, 1), Cycle(F.ActiveShinespark, S.Varia, 3, 1),
        ],
        [G.ActiveShinesparkGravity] =
        [
            Cycle(F.ActiveShinespark, S.Gravity, 0, 1), Cycle(F.ActiveShinespark, S.Gravity, 1, 1),
            Cycle(F.ActiveShinespark, S.Gravity, 2, 1), Cycle(F.ActiveShinespark, S.Gravity, 3, 1),
        ],
        [G.ScrewAttackPower] = RiseAndFall(F.ScrewAttack, S.Power),
        [G.ScrewAttackVaria] = RiseAndFall(F.ScrewAttack, S.Varia),
        [G.ScrewAttackGravity] = RiseAndFall(F.ScrewAttack, S.Gravity),
        [G.SpeedBoosterRisingPower] =
        [
            Cycle(F.SpeedBooster, S.Power, 0, 4), Cycle(F.SpeedBooster, S.Power, 1, 4), Cycle(F.SpeedBooster, S.Power, 2, 4),
        ],
        [G.SpeedBoosterRisingVaria] =
        [
            Cycle(F.SpeedBooster, S.Varia, 0, 4), Cycle(F.SpeedBooster, S.Varia, 1, 4), Cycle(F.SpeedBooster, S.Varia, 2, 4),
        ],
        [G.SpeedBoosterRisingGravity] =
        [
            Cycle(F.SpeedBooster, S.Gravity, 0, 4), Cycle(F.SpeedBooster, S.Gravity, 1, 4), Cycle(F.SpeedBooster, S.Gravity, 2, 4),
        ],
        [G.SpeedBoosterBrightestPower] = [Cycle(F.SpeedBooster, S.Power, 3, 4)],
        [G.SpeedBoosterBrightestVaria] = [Cycle(F.SpeedBooster, S.Varia, 3, 4)],
        [G.SpeedBoosterBrightestGravity] = [Cycle(F.SpeedBooster, S.Gravity, 3, 4)],
    });

    /// <summary>
    /// Each sequence's animation: a pulse, the Hyper Beam's cycle, the Crystal Flash's pulse and bubble, the rainbow, the
    /// steady visor cycle, the shines and the Screw Attack start again after their last shade; the X-ray widening and the
    /// Speed Booster rest on their last, showing it again each time it runs out; a shot's glow ends after its last.
    /// </summary>
    internal static IReadOnlyDictionary<G, Animation<G>> Animations { get; } = new Dictionary<G, Animation<G>>
    {
        [G.ChargedBeamPowerPulse] = Animation<G>.Looping([Segments.Whole(G.ChargedBeamPowerPulse)]),
        [G.ChargedBeamVariaPulse] = Animation<G>.Looping([Segments.Whole(G.ChargedBeamVariaPulse)]),
        [G.ChargedBeamGravityPulse] = Animation<G>.Looping([Segments.Whole(G.ChargedBeamGravityPulse)]),
        [G.PseudoScrewPowerPulse] = Animation<G>.Looping([Segments.Whole(G.PseudoScrewPowerPulse)]),
        [G.PseudoScrewVariaPulse] = Animation<G>.Looping([Segments.Whole(G.PseudoScrewVariaPulse)]),
        [G.PseudoScrewGravityPulse] = Animation<G>.Looping([Segments.Whole(G.PseudoScrewGravityPulse)]),
        [G.ChargedShotFlash] = new([Segments.Whole(G.ChargedShotFlash)], AnimationEnd.Stops),
        [G.HyperShotGlow] = new([Segments.Whole(G.HyperShotGlow)], AnimationEnd.Stops),
        [G.HyperBeamCycle] = Animation<G>.Looping([Segments.Whole(G.HyperBeamCycle)]),
        [G.CrystalFlashBody] = Animation<G>.Looping([Segments.Whole(G.CrystalFlashBody)]),
        [G.CrystalFlashBubble] = Animation<G>.Looping([Segments.Whole(G.CrystalFlashBubble)]),
        [G.HyperBeamRainbow] = Animation<G>.Looping([Segments.Whole(G.HyperBeamRainbow)]),
        [G.XrayVisorWidening] = Animation<G>.LeadingInto([Segments.Whole(G.XrayVisorWidening)], [Segments.Whole(G.XrayVisorWidest)]),
        [G.VisorCycle] = Animation<G>.Looping([Segments.Whole(G.VisorCycle)]),
        [G.StoredShinePower] = Animation<G>.Looping([Segments.Whole(G.StoredShinePower)]),
        [G.StoredShineVaria] = Animation<G>.Looping([Segments.Whole(G.StoredShineVaria)]),
        [G.StoredShineGravity] = Animation<G>.Looping([Segments.Whole(G.StoredShineGravity)]),
        [G.ActiveShinesparkPower] = Animation<G>.Looping([Segments.Whole(G.ActiveShinesparkPower)]),
        [G.ActiveShinesparkVaria] = Animation<G>.Looping([Segments.Whole(G.ActiveShinesparkVaria)]),
        [G.ActiveShinesparkGravity] = Animation<G>.Looping([Segments.Whole(G.ActiveShinesparkGravity)]),
        [G.ScrewAttackPower] = Animation<G>.Looping([Segments.Whole(G.ScrewAttackPower)]),
        [G.ScrewAttackVaria] = Animation<G>.Looping([Segments.Whole(G.ScrewAttackVaria)]),
        [G.ScrewAttackGravity] = Animation<G>.Looping([Segments.Whole(G.ScrewAttackGravity)]),
        [G.SpeedBoosterRisingPower] = Animation<G>.LeadingInto(
            [Segments.Whole(G.SpeedBoosterRisingPower)], [Segments.Whole(G.SpeedBoosterBrightestPower)]),
        [G.SpeedBoosterRisingVaria] = Animation<G>.LeadingInto(
            [Segments.Whole(G.SpeedBoosterRisingVaria)], [Segments.Whole(G.SpeedBoosterBrightestVaria)]),
        [G.SpeedBoosterRisingGravity] = Animation<G>.LeadingInto(
            [Segments.Whole(G.SpeedBoosterRisingGravity)], [Segments.Whole(G.SpeedBoosterBrightestGravity)]),
    };

    /// <summary>
    /// The pulse Samus's suit shows while she is charged, for a charged beam or the pseudo Screw Attack in each suit. Every
    /// pulse runs its six shades alike, so the place one pulse stands at is where another goes on from.
    /// </summary>
    internal static IReadOnlyDictionary<SamusChargeKind, IReadOnlyDictionary<S, G>> ChargePulses { get; } =
        new Dictionary<SamusChargeKind, IReadOnlyDictionary<S, G>>
        {
            [SamusChargeKind.ChargedBeam] = new Dictionary<S, G>
            {
                [S.Power] = G.ChargedBeamPowerPulse,
                [S.Varia] = G.ChargedBeamVariaPulse,
                [S.Gravity] = G.ChargedBeamGravityPulse,
            },
            [SamusChargeKind.PseudoScrew] = new Dictionary<S, G>
            {
                [S.Power] = G.PseudoScrewPowerPulse,
                [S.Varia] = G.PseudoScrewVariaPulse,
                [S.Gravity] = G.PseudoScrewGravityPulse,
            },
        };

    /// <summary>
    /// The stored shine Samus's suit shows in each suit (#2387). Every suit's shine runs its six shades alike, so the place
    /// one stands at is where another goes on from.
    /// </summary>
    internal static IReadOnlyDictionary<S, G> StoredShine { get; } = new Dictionary<S, G>
    {
        [S.Power] = G.StoredShinePower,
        [S.Varia] = G.StoredShineVaria,
        [S.Gravity] = G.StoredShineGravity,
    };

    /// <summary>The glow Samus's suit shows in each suit while she shinesparks (#2387), its four shades alike in every suit.</summary>
    internal static IReadOnlyDictionary<S, G> ActiveShinespark { get; } = new Dictionary<S, G>
    {
        [S.Power] = G.ActiveShinesparkPower,
        [S.Varia] = G.ActiveShinesparkVaria,
        [S.Gravity] = G.ActiveShinesparkGravity,
    };

    /// <summary>The glow Samus's suit shows in each suit as she spins with the Screw Attack (#2387).</summary>
    internal static IReadOnlyDictionary<S, G> ScrewAttack { get; } = new Dictionary<S, G>
    {
        [S.Power] = G.ScrewAttackPower,
        [S.Varia] = G.ScrewAttackVaria,
        [S.Gravity] = G.ScrewAttackGravity,
    };

    /// <summary>The glow Samus's suit shows in each suit while she runs boosted (#2387), named by the shades it rises through.</summary>
    internal static IReadOnlyDictionary<S, G> SpeedBooster { get; } = new Dictionary<S, G>
    {
        [S.Power] = G.SpeedBoosterRisingPower,
        [S.Varia] = G.SpeedBoosterRisingVaria,
        [S.Gravity] = G.SpeedBoosterRisingGravity,
    };

    /// <summary>Shows <paramref name="shade"/> for <paramref name="updates"/> updates.</summary>
    private static AnimationBeat<PictureShade, NoAnimationCue> Show(PictureShade shade, int updates) =>
        new AnimationBeat<PictureShade, NoAnimationCue>.Frame(shade, updates);

    /// <summary>Shows step <paramref name="step"/> of <paramref name="kind"/>'s pulse in <paramref name="suit"/> for one update.</summary>
    private static AnimationBeat<PictureShade, NoAnimationCue> Pulse(SamusChargeKind kind, S suit, int step) =>
        Show(SamusShades.Charge(new SamusChargeShade(kind, suit, step)), 1);

    /// <summary>Shows <paramref name="family"/>'s shade <paramref name="shade"/> in <paramref name="suit"/> for <paramref name="updates"/> updates.</summary>
    private static AnimationBeat<PictureShade, NoAnimationCue> Cycle(F family, S suit, int shade, int updates) =>
        Show(SamusShades.FullBodyCycle(new SamusFullBodyCycleShade(family, suit, shade)), updates);

    /// <summary>
    /// <paramref name="family"/>'s rise through its four shades in <paramref name="suit"/> and its fall back through the
    /// middle two, one update each: the stored shine's and the Screw Attack's six.
    /// </summary>
    private static AnimationBeat<PictureShade, NoAnimationCue>[] RiseAndFall(F family, S suit) =>
    [
        Cycle(family, suit, 0, 1), Cycle(family, suit, 1, 1), Cycle(family, suit, 2, 1),
        Cycle(family, suit, 3, 1), Cycle(family, suit, 2, 1), Cycle(family, suit, 1, 1),
    ];
}
