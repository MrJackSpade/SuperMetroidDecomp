using SuperMetroid.Core.Animation;
using M = SuperMetroid.Core.Samus.Projectiles.MainFlareSegment;
using P = SuperMetroid.Core.Samus.Projectiles.MainFlarePicture;
using R = SuperMetroid.Core.Samus.Projectiles.SparkSegment;
using S = SuperMetroid.Core.Samus.Projectiles.SparkPicture;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The charge flare's animations (#1624, #1747, #2285, #2396, #2458): each run's pictures with the updates each shows for,
/// and how each component's animation plays its runs, its phases standing in the order its pictures stand. The main flare
/// plays its opening once and then repeats its loop: the opening alternates the ember and the glow
/// <see cref="EmberGlowPlays"/> times and the white ball and the glow as often, and the loop alternates the white ball
/// and the starburst <see cref="StarburstPlays"/> times. Each spark ring draws in from its widest to its narrowest and
/// starts again.
/// </summary>
internal static partial class ChargeFlareAnimationDefinitions
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

    /// <summary>The main flare's runs of pictures, each with the updates it shows for.</summary>
    internal static AnimationSegments<M, MainFlarePicture, NoAnimationCue> MainFlareSegments { get; } = new(
        new Dictionary<M, AnimationBeat<MainFlarePicture, NoAnimationCue>[]>
    {
        [M.EmberAndGlow] = [Show(P.Ember, 4), Show(P.Glow, 4)],
        [M.WhiteBallAndGlow] = [Show(P.WhiteBall, 4), Show(P.Glow, 4)],
        [M.WhiteBallAndStarburst] = [Show(P.WhiteBall, 4), Show(P.Starburst, 4)],
    });

    /// <summary>The spark rings' runs of pictures, each with the updates it shows for.</summary>
    internal static AnimationSegments<R, SparkPicture, NoAnimationCue> SparkSegments { get; } = new(
        new Dictionary<R, AnimationBeat<SparkPicture, NoAnimationCue>[]>
    {
        [R.SlowSparks] = [Show(S.Widest, 6), Show(S.Wider, 5), Show(S.Wide, 4), Show(S.Narrow, 4), Show(S.Narrower, 4), Show(S.Narrowest, 4)],
        [R.FastSparks] = [Show(S.Widest, 5), Show(S.Wider, 4), Show(S.Wide, 3), Show(S.Narrow, 3), Show(S.Narrower, 3), Show(S.Narrowest, 3)],
    });

    /// <summary>The main flare's animation, its phases read from its runs (#2387): its opening once and then its loop repeated.</summary>
    internal static LoopingAnimation<M> MainFlareAnimation { get; } = LoopingAnimation<M>.LeadingInto(
        [MainFlareSegments.Repeated(M.EmberAndGlow, EmberGlowPlays), MainFlareSegments.Repeated(M.WhiteBallAndGlow, EmberGlowPlays)],
        [MainFlareSegments.Repeated(M.WhiteBallAndStarburst, StarburstPlays)]);

    /// <summary>The slow spark ring's animation, its cycle repeated.</summary>
    internal static LoopingAnimation<R> SlowSparksAnimation { get; } = LoopingAnimation<R>.Looping([SparkSegments.Whole(R.SlowSparks)]);

    /// <summary>The fast spark ring's animation, its cycle repeated.</summary>
    internal static LoopingAnimation<R> FastSparksAnimation { get; } = LoopingAnimation<R>.Looping([SparkSegments.Whole(R.FastSparks)]);

    /// <summary>Shows <paramref name="picture"/> for <paramref name="updates"/> updates.</summary>
    private static AnimationBeat<MainFlarePicture, NoAnimationCue> Show(MainFlarePicture picture, int updates) =>
        new AnimationBeat<MainFlarePicture, NoAnimationCue>.Frame(picture, updates);

    /// <summary>Shows <paramref name="picture"/> for <paramref name="updates"/> updates.</summary>
    private static AnimationBeat<SparkPicture, NoAnimationCue> Show(SparkPicture picture, int updates) =>
        new AnimationBeat<SparkPicture, NoAnimationCue>.Frame(picture, updates);
}
