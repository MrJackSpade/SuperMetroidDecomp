using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;
using F = SuperMetroid.Core.Samus.Artwork.SamusFullBodyCycleFamily;
using G = SuperMetroid.Core.Samus.Colours.SamusGlowSegment;
using W = SuperMetroid.Core.Samus.Colours.SamusGlow;
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
    /// <summary>The suits in the order their sequences are listed: Power, Varia, Gravity.</summary>
    private static readonly IReadOnlyList<S> Suits = Ensure.EveryMemberOnce<S>([S.Power, S.Varia, S.Gravity]);

    /// <summary>
    /// Every sequence's shades, each with the updates it shows for. A charged shot's flash paints its yellow anew on each
    /// of its updates, over any colouring laid on the suit after it on the update before, so it is a beat of one update for
    /// each rather than one beat for them all.
    /// </summary>
    internal static AnimationSegments<G, PictureShade, NoAnimationCue> Segments { get; } = new(
        new Dictionary<G, AnimationBeat<PictureShade, NoAnimationCue>[]>(
            new Dictionary<G, AnimationBeat<PictureShade, NoAnimationCue>[]>
            {
                [G.Of(W.ChargedShotFlash)] = Each([.. Repeated(SamusShades.ChargedShotGlow, 3)], 1),
                [G.Of(W.HyperShotGlow)] = Each(SamusShades.HyperShot.Reverse(), 2),
                [G.Of(W.HyperBeamCycle)] = Each(SamusShades.HyperBeamCycle, 2),
                [G.Of(W.CrystalFlashBody)] = Each(SamusShades.CrystalFlashBody, 10),
                [G.Of(W.CrystalFlashBubble)] = Each(SamusShades.CrystalFlashBubble, 5),
                [G.Of(W.HyperBeamRainbow)] = Each(SamusShades.HyperBeamSuit, 1),
                [G.Of(W.XrayVisorWidening)] = Each(SamusShades.XrayWideningVisor.SkipLast(1), 5),
                [G.Of(W.XrayVisorWidest)] = [Show(SamusShades.XrayWideningVisor[^1], 5)],
                [G.Of(W.VisorCycle)] = Each(SamusShades.VisorCycle, 5),
            }.Concat(Suits.SelectMany(InSuit))));

    /// <summary>
    /// Each sequence's animation: a pulse, the Hyper Beam's cycle, the Crystal Flash's pulse and bubble, the rainbow, the
    /// steady visor cycle, the shines and the Screw Attack start again after their last shade; the X-ray widening and the
    /// Speed Booster rest on their last, showing it again each time it runs out; a shot's glow ends after its last.
    /// </summary>
    internal static IReadOnlyDictionary<G, Animation<G>> Animations { get; } = Segments.Starts(
        new Dictionary<G, Animation<G>>(
            new Dictionary<G, Animation<G>>
            {
                [G.Of(W.ChargedShotFlash)] = new([Segments.Whole(G.Of(W.ChargedShotFlash))], AnimationEnd.Stops),
                [G.Of(W.HyperShotGlow)] = new([Segments.Whole(G.Of(W.HyperShotGlow))], AnimationEnd.Stops),
                [G.Of(W.XrayVisorWidening)] = Animation<G>.LeadingInto([Segments.Whole(G.Of(W.XrayVisorWidening))], [Segments.Whole(G.Of(W.XrayVisorWidest))]),
            }.Concat(Suits.Select(suit => KeyValuePair.Create(G.In(W.SpeedBoosterRising, suit), Animation<G>.LeadingInto(
                [Segments.Whole(G.In(W.SpeedBoosterRising, suit))], [Segments.Whole(G.In(W.SpeedBoosterBrightest, suit))]))))),
        [
            G.Of(W.HyperBeamCycle), G.Of(W.CrystalFlashBody), G.Of(W.CrystalFlashBubble), G.Of(W.HyperBeamRainbow), G.Of(W.VisorCycle),
            .. Suits.SelectMany(suit => SuitLooping.Select(glow => G.In(glow, suit))),
        ]);

    /// <summary>
    /// The pulse Samus's suit shows while she is charged, for a charged beam or the pseudo Screw Attack, in the suit she
    /// wears. Every pulse runs its six shades alike, so the place one pulse stands at is where another goes on from.
    /// </summary>
    internal static IReadOnlyDictionary<SamusChargeKind, W> ChargePulses { get; } =
        new Dictionary<SamusChargeKind, W>
        {
            [SamusChargeKind.ChargedBeam] = W.ChargedBeamPulse,
            [SamusChargeKind.PseudoScrew] = W.PseudoScrewPulse,
        };

    /// <summary>
    /// The glows that differ by suit and start again after their last shade in each suit: the charge pulses, the stored shine
    /// (whose every suit runs six shades alike, so the place one stands at is where another goes on from), the shinespark's
    /// four and the Screw Attack's.
    /// </summary>
    private static W[] SuitLooping => [W.ChargedBeamPulse, W.PseudoScrewPulse, W.StoredShine, W.ActiveShinespark, W.ScrewAttack];

    /// <summary>The sequences of every glow that differs by suit, in <paramref name="suit"/>.</summary>
    private static KeyValuePair<G, AnimationBeat<PictureShade, NoAnimationCue>[]>[] InSuit(S suit) =>
    [
        KeyValuePair.Create(G.In(W.ChargedBeamPulse, suit), Each(SamusShades.ChargePulse(SamusChargeKind.ChargedBeam, suit), 1)),
        KeyValuePair.Create(G.In(W.PseudoScrewPulse, suit), Each(SamusShades.ChargePulse(SamusChargeKind.PseudoScrew, suit), 1)),
        KeyValuePair.Create(G.In(W.StoredShine, suit), RiseAndFall(F.StoredShine, suit)),
        KeyValuePair.Create(G.In(W.ActiveShinespark, suit), Each(SamusShades.FullBodyCycleShades(F.ActiveShinespark, suit), 1)),
        KeyValuePair.Create(G.In(W.ScrewAttack, suit), RiseAndFall(F.ScrewAttack, suit)),
        KeyValuePair.Create(G.In(W.SpeedBoosterRising, suit), Each(SamusShades.FullBodyCycleShades(F.SpeedBooster, suit).SkipLast(1), 4)),
        KeyValuePair.Create(G.In(W.SpeedBoosterBrightest, suit), new[] { Show(SamusShades.FullBodyCycleShades(F.SpeedBooster, suit)[^1], 4) }),
    ];

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
