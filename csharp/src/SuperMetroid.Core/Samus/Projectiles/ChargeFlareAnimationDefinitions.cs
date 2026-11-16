using SuperMetroid.Core.Animation;
using C = SuperMetroid.Core.Samus.Projectiles.ChargeFlareComponent;
using P = SuperMetroid.Core.Samus.Projectiles.ChargeFlarePicture;
using S = SuperMetroid.Core.Samus.Projectiles.ChargeFlareSegment;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The charge flare's animations (#1624, #1747, #2285): each run's pictures with the updates each shows for, and how each
/// component's animation plays its runs. The main flare plays its opening once and then repeats its loop: the opening's
/// first two pictures alternate for eight beats and its third and second for eight more, and the loop alternates its
/// third and fourth for fourteen. Each spark ring plays its six pictures and restarts from its first.
/// </summary>
internal static class ChargeFlareAnimationDefinitions
{
    /// <summary>The flare count from which a charging beam's main flare is drawn.</summary>
    internal const int MainFlareShownFrom = 15;

    /// <summary>The flare count from which both spark rings are drawn around the main flare.</summary>
    internal const int SparksShownFrom = 30;

    /// <summary>The updates each picture of the Hyper Beam's flare shows for as it plays the components' pictures backward.</summary>
    internal const int HyperBeamPictureUpdates = 3;

    /// <summary>Every run's pictures, each with the updates it shows for.</summary>
    internal static AnimationSegments<S, ChargeFlarePicture, NoAnimationCue> Segments { get; } = new(
        new Dictionary<S, AnimationBeat<ChargeFlarePicture, NoAnimationCue>[]>
    {
        [S.MainFlareOpening] =
        [
            Show(P.MainFirst, 4), Show(P.MainSecond, 4), Show(P.MainFirst, 4), Show(P.MainSecond, 4), Show(P.MainFirst, 4), Show(P.MainSecond, 4), Show(P.MainFirst, 4), Show(P.MainSecond, 4),
            Show(P.MainThird, 4), Show(P.MainSecond, 4), Show(P.MainThird, 4), Show(P.MainSecond, 4), Show(P.MainThird, 4), Show(P.MainSecond, 4), Show(P.MainThird, 4), Show(P.MainSecond, 4),
        ],
        [S.MainFlareLoop] =
        [
            Show(P.MainThird, 4), Show(P.MainFourth, 4), Show(P.MainThird, 4), Show(P.MainFourth, 4), Show(P.MainThird, 4), Show(P.MainFourth, 4), Show(P.MainThird, 4), Show(P.MainFourth, 4),
            Show(P.MainThird, 4), Show(P.MainFourth, 4), Show(P.MainThird, 4), Show(P.MainFourth, 4), Show(P.MainThird, 4), Show(P.MainFourth, 4),
        ],
        [S.SlowSparks] = [Show(P.SparksFirst, 6), Show(P.SparksSecond, 5), Show(P.SparksThird, 4), Show(P.SparksFourth, 4), Show(P.SparksFifth, 4), Show(P.SparksSixth, 4)],
        [S.FastSparks] = [Show(P.SparksFirst, 5), Show(P.SparksSecond, 4), Show(P.SparksThird, 3), Show(P.SparksFourth, 3), Show(P.SparksFifth, 3), Show(P.SparksSixth, 3)],
    });

    /// <summary>Each component's runs, in the order its pictures stand: the main flare's opening and loop, each spark ring's cycle.</summary>
    internal static IReadOnlyDictionary<C, IReadOnlyList<S>> Runs { get; } = new Dictionary<C, IReadOnlyList<S>>
    {
        [C.MainFlare] = [S.MainFlareOpening, S.MainFlareLoop],
        [C.SlowSparks] = [S.SlowSparks],
        [C.FastSparks] = [S.FastSparks],
    };

    /// <summary>Each component's animation: the main flare's opening once and then its loop repeated, each spark ring's cycle repeated.</summary>
    internal static IReadOnlyDictionary<C, Animation<S>> Animations { get; } = new Dictionary<C, Animation<S>>
    {
        [C.MainFlare] = new([new(S.MainFlareOpening, 0, 16, 1), new(S.MainFlareLoop, 0, 14, 1)], AnimationEnd.LoopsFrom(1)),
        [C.SlowSparks] = new([new(S.SlowSparks, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [C.FastSparks] = new([new(S.FastSparks, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>Shows <paramref name="picture"/> for <paramref name="updates"/> updates.</summary>
    private static AnimationBeat<ChargeFlarePicture, NoAnimationCue> Show(ChargeFlarePicture picture, int updates) =>
        new AnimationBeat<ChargeFlarePicture, NoAnimationCue>.Frame(picture, updates);
}
