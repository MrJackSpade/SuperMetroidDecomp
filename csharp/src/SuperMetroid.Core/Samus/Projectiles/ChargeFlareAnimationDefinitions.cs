using SuperMetroid.Core.Animation;
using C = SuperMetroid.Core.Samus.Projectiles.ChargeFlareComponent;
using P = SuperMetroid.Core.Samus.Projectiles.ChargeFlarePicture;
using S = SuperMetroid.Core.Samus.Projectiles.ChargeFlareSegment;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The charge flare's animations (#1624, #1747, #2285, #2396, #2458): each run's pictures with the updates each shows for,
/// and how each component's animation plays its runs, its phases standing in the order its pictures stand. The main flare
/// plays its opening once and then repeats its loop: the opening alternates the ember and the glow
/// <see cref="EmberGlowPlays"/> times and the white ball and the glow as often, and the loop alternates the white ball
/// and the starburst <see cref="StarburstPlays"/> times. Each spark ring draws in from its widest to its narrowest and
/// starts again.
/// </summary>
internal static class ChargeFlareAnimationDefinitions
{
    /// <summary>The flare count from which a charging beam's main flare is drawn.</summary>
    internal const int MainFlareShownFrom = 15;

    /// <summary>The flare count from which both spark rings are drawn around the main flare.</summary>
    internal const int SparksShownFrom = 30;

    /// <summary>The updates each picture of the Hyper Beam's flare shows for as it plays the components' pictures backward.</summary>
    internal const int HyperBeamPictureUpdates = 3;

    /// <summary>
    /// The updates the Hyper Beam's flare shows a component's first picture for once its backward count reaches it, before
    /// the count goes back past it: the count is not renewed on that picture, so it runs out on the next update.
    /// </summary>
    internal const int HyperBeamFirstPictureUpdates = 1;

    /// <summary>The updates a cleared component shows its first picture for before its animation moves on: its wait is spent.</summary>
    internal const int ClearedPictureUpdates = 1;

    /// <summary>
    /// The times the main flare's opening plays each of its runs: the ember and the glow, then the white ball and the glow.
    /// </summary>
    internal const int EmberGlowPlays = 4;

    /// <summary>The times one round of the main flare's loop alternates the white ball and the starburst.</summary>
    internal const int StarburstPlays = 7;

    /// <summary>Every run's pictures, each with the updates it shows for.</summary>
    internal static AnimationSegments<S, ChargeFlarePicture, NoAnimationCue> Segments { get; } = new(
        new Dictionary<S, AnimationBeat<ChargeFlarePicture, NoAnimationCue>[]>
    {
        [S.EmberAndGlow] = [Show(P.Ember, 4), Show(P.Glow, 4)],
        [S.WhiteBallAndGlow] = [Show(P.WhiteBall, 4), Show(P.Glow, 4)],
        [S.WhiteBallAndStarburst] = [Show(P.WhiteBall, 4), Show(P.Starburst, 4)],
        [S.SlowSparks] = [Show(P.SparksWidest, 6), Show(P.SparksWider, 5), Show(P.SparksWide, 4), Show(P.SparksNarrow, 4), Show(P.SparksNarrower, 4), Show(P.SparksNarrowest, 4)],
        [S.FastSparks] = [Show(P.SparksWidest, 5), Show(P.SparksWider, 4), Show(P.SparksWide, 3), Show(P.SparksNarrow, 3), Show(P.SparksNarrower, 3), Show(P.SparksNarrowest, 3)],
    });

    /// <summary>
    /// Each component's animation, its phases read from its runs (#2387): the main flare's opening once and then its loop
    /// repeated, each spark ring's cycle repeated.
    /// </summary>
    internal static IReadOnlyDictionary<C, Animation<S>> Animations { get; } = new Dictionary<C, Animation<S>>
    {
        [C.MainFlare] = Animation<S>.LeadingInto(
            [Segments.Repeated(S.EmberAndGlow, EmberGlowPlays), Segments.Repeated(S.WhiteBallAndGlow, EmberGlowPlays)],
            [Segments.Repeated(S.WhiteBallAndStarburst, StarburstPlays)]),
        [C.SlowSparks] = Animation<S>.Looping([Segments.Whole(S.SlowSparks)]),
        [C.FastSparks] = Animation<S>.Looping([Segments.Whole(S.FastSparks)]),
    };

    /// <summary>Shows <paramref name="picture"/> for <paramref name="updates"/> updates.</summary>
    private static AnimationBeat<ChargeFlarePicture, NoAnimationCue> Show(ChargeFlarePicture picture, int updates) =>
        new AnimationBeat<ChargeFlarePicture, NoAnimationCue>.Frame(picture, updates);
}
