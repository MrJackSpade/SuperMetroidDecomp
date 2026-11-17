using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;
using G = SuperMetroid.Core.Samus.Colours.SamusGlowSegment;

namespace SuperMetroid.Core.Samus.Colours;

/// <summary>
/// Samus's named shade sequences (#2351), each a run of the shades it shows with the updates each shows for, played as an
/// animation: the charge pulse shows its six shades one update each and starts again; a charged shot's flash shows the
/// yellow glow for three updates; a Hyper Beam shot's glow shows the Hyper Beam's full-body glow from its last frame to
/// its first, two updates each; and the Hyper Beam's colour cycle shows its ten beam colourings two updates each and
/// starts again. A shot's glow restores the suit's own shade on the update after its last.
/// </summary>
internal static class SamusGlowAnimationDefinitions
{
    /// <summary>Every sequence's shades, each with the updates it shows for.</summary>
    internal static AnimationSegments<G, PictureShade, NoAnimationCue> Segments { get; } = new(
        new Dictionary<G, AnimationBeat<PictureShade, NoAnimationCue>[]>
    {
        [G.ChargedBeamPowerPulse] =
        [
            Pulse(SamusChargeKind.ChargedBeam, SamusSuit.Power, 0), Pulse(SamusChargeKind.ChargedBeam, SamusSuit.Power, 1),
            Pulse(SamusChargeKind.ChargedBeam, SamusSuit.Power, 2), Pulse(SamusChargeKind.ChargedBeam, SamusSuit.Power, 3),
            Pulse(SamusChargeKind.ChargedBeam, SamusSuit.Power, 4), Pulse(SamusChargeKind.ChargedBeam, SamusSuit.Power, 5),
        ],
        [G.ChargedBeamVariaPulse] =
        [
            Pulse(SamusChargeKind.ChargedBeam, SamusSuit.Varia, 0), Pulse(SamusChargeKind.ChargedBeam, SamusSuit.Varia, 1),
            Pulse(SamusChargeKind.ChargedBeam, SamusSuit.Varia, 2), Pulse(SamusChargeKind.ChargedBeam, SamusSuit.Varia, 3),
            Pulse(SamusChargeKind.ChargedBeam, SamusSuit.Varia, 4), Pulse(SamusChargeKind.ChargedBeam, SamusSuit.Varia, 5),
        ],
        [G.ChargedBeamGravityPulse] =
        [
            Pulse(SamusChargeKind.ChargedBeam, SamusSuit.Gravity, 0), Pulse(SamusChargeKind.ChargedBeam, SamusSuit.Gravity, 1),
            Pulse(SamusChargeKind.ChargedBeam, SamusSuit.Gravity, 2), Pulse(SamusChargeKind.ChargedBeam, SamusSuit.Gravity, 3),
            Pulse(SamusChargeKind.ChargedBeam, SamusSuit.Gravity, 4), Pulse(SamusChargeKind.ChargedBeam, SamusSuit.Gravity, 5),
        ],
        [G.PseudoScrewPowerPulse] =
        [
            Pulse(SamusChargeKind.PseudoScrew, SamusSuit.Power, 0), Pulse(SamusChargeKind.PseudoScrew, SamusSuit.Power, 1),
            Pulse(SamusChargeKind.PseudoScrew, SamusSuit.Power, 2), Pulse(SamusChargeKind.PseudoScrew, SamusSuit.Power, 3),
            Pulse(SamusChargeKind.PseudoScrew, SamusSuit.Power, 4), Pulse(SamusChargeKind.PseudoScrew, SamusSuit.Power, 5),
        ],
        [G.PseudoScrewVariaPulse] =
        [
            Pulse(SamusChargeKind.PseudoScrew, SamusSuit.Varia, 0), Pulse(SamusChargeKind.PseudoScrew, SamusSuit.Varia, 1),
            Pulse(SamusChargeKind.PseudoScrew, SamusSuit.Varia, 2), Pulse(SamusChargeKind.PseudoScrew, SamusSuit.Varia, 3),
            Pulse(SamusChargeKind.PseudoScrew, SamusSuit.Varia, 4), Pulse(SamusChargeKind.PseudoScrew, SamusSuit.Varia, 5),
        ],
        [G.PseudoScrewGravityPulse] =
        [
            Pulse(SamusChargeKind.PseudoScrew, SamusSuit.Gravity, 0), Pulse(SamusChargeKind.PseudoScrew, SamusSuit.Gravity, 1),
            Pulse(SamusChargeKind.PseudoScrew, SamusSuit.Gravity, 2), Pulse(SamusChargeKind.PseudoScrew, SamusSuit.Gravity, 3),
            Pulse(SamusChargeKind.PseudoScrew, SamusSuit.Gravity, 4), Pulse(SamusChargeKind.PseudoScrew, SamusSuit.Gravity, 5),
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
    });

    /// <summary>
    /// Each sequence's animation: a pulse and the Hyper Beam's cycle start again after their last shade, and a shot's glow
    /// ends after its last.
    /// </summary>
    internal static IReadOnlyDictionary<G, Animation<G>> Animations { get; } = new Dictionary<G, Animation<G>>
    {
        [G.ChargedBeamPowerPulse] = new([new(G.ChargedBeamPowerPulse, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [G.ChargedBeamVariaPulse] = new([new(G.ChargedBeamVariaPulse, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [G.ChargedBeamGravityPulse] = new([new(G.ChargedBeamGravityPulse, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [G.PseudoScrewPowerPulse] = new([new(G.PseudoScrewPowerPulse, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [G.PseudoScrewVariaPulse] = new([new(G.PseudoScrewVariaPulse, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [G.PseudoScrewGravityPulse] = new([new(G.PseudoScrewGravityPulse, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [G.ChargedShotFlash] = new([new(G.ChargedShotFlash, 0, 3, 1)], AnimationEnd.Stops),
        [G.HyperShotGlow] = new([new(G.HyperShotGlow, 0, 10, 1)], AnimationEnd.Stops),
        [G.HyperBeamCycle] = new([new(G.HyperBeamCycle, 0, 10, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>
    /// The pulse Samus's suit shows while she is charged, for a charged beam or the pseudo Screw Attack in each suit. Every
    /// pulse runs its six shades alike, so the place one pulse stands at is where another goes on from.
    /// </summary>
    internal static IReadOnlyDictionary<SamusChargeKind, IReadOnlyDictionary<SamusSuit, G>> ChargePulses { get; } =
        new Dictionary<SamusChargeKind, IReadOnlyDictionary<SamusSuit, G>>
        {
            [SamusChargeKind.ChargedBeam] = new Dictionary<SamusSuit, G>
            {
                [SamusSuit.Power] = G.ChargedBeamPowerPulse,
                [SamusSuit.Varia] = G.ChargedBeamVariaPulse,
                [SamusSuit.Gravity] = G.ChargedBeamGravityPulse,
            },
            [SamusChargeKind.PseudoScrew] = new Dictionary<SamusSuit, G>
            {
                [SamusSuit.Power] = G.PseudoScrewPowerPulse,
                [SamusSuit.Varia] = G.PseudoScrewVariaPulse,
                [SamusSuit.Gravity] = G.PseudoScrewGravityPulse,
            },
        };

    /// <summary>Shows <paramref name="shade"/> for <paramref name="updates"/> updates.</summary>
    private static AnimationBeat<PictureShade, NoAnimationCue> Show(PictureShade shade, int updates) =>
        new AnimationBeat<PictureShade, NoAnimationCue>.Frame(shade, updates);

    /// <summary>Shows step <paramref name="step"/> of <paramref name="kind"/>'s pulse in <paramref name="suit"/> for one update.</summary>
    private static AnimationBeat<PictureShade, NoAnimationCue> Pulse(SamusChargeKind kind, SamusSuit suit, int step) =>
        Show(SamusShades.Charge(new SamusChargeShade(kind, suit, step)), 1);
}
