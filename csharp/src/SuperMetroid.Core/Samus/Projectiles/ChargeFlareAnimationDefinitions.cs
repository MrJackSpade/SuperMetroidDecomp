using SuperMetroid.Core.Animation;
using C = SuperMetroid.Core.Samus.Projectiles.ChargeFlareComponent;
using P = SuperMetroid.Core.Samus.Projectiles.ChargeFlarePicture;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The charge flare's animations (#1624, #1747): each component's pictures with the updates each shows for, and how each
/// animation goes on past its last picture. The main flare plays its thirty beats once and then loops from its seventeenth:
/// its first two pictures alternate for eight beats, its third and second for eight more, and its third and fourth for the
/// last fourteen, which its loop repeats. Each spark ring plays its six pictures and restarts from its first.
/// </summary>
internal static class ChargeFlareAnimationDefinitions
{
    /// <summary>The main flare's first beat of its loop: beat 16, after which it repeats beats 16 through 29.</summary>
    internal const int MainFlareLoopStart = 16;

    /// <summary>The updates each picture of the Hyper Beam's flare shows for as it plays the components' pictures backward.</summary>
    internal const int HyperBeamPictureUpdates = 3;

    /// <summary>Every component's pictures, each with the updates it shows for.</summary>
    internal static AnimationSegments<C, ChargeFlarePicture, NoAnimationCue> Segments { get; } = new(
        new Dictionary<C, AnimationBeat<ChargeFlarePicture, NoAnimationCue>[]>
    {
        [C.MainFlare] =
        [
            Show(P.MainFirst, 4), Show(P.MainSecond, 4), Show(P.MainFirst, 4), Show(P.MainSecond, 4), Show(P.MainFirst, 4), Show(P.MainSecond, 4), Show(P.MainFirst, 4), Show(P.MainSecond, 4),
            Show(P.MainThird, 4), Show(P.MainSecond, 4), Show(P.MainThird, 4), Show(P.MainSecond, 4), Show(P.MainThird, 4), Show(P.MainSecond, 4), Show(P.MainThird, 4), Show(P.MainSecond, 4),
            Show(P.MainThird, 4), Show(P.MainFourth, 4), Show(P.MainThird, 4), Show(P.MainFourth, 4), Show(P.MainThird, 4), Show(P.MainFourth, 4), Show(P.MainThird, 4), Show(P.MainFourth, 4),
            Show(P.MainThird, 4), Show(P.MainFourth, 4), Show(P.MainThird, 4), Show(P.MainFourth, 4), Show(P.MainThird, 4), Show(P.MainFourth, 4),
        ],
        [C.SlowSparks] = [Show(P.SparksFirst, 6), Show(P.SparksSecond, 5), Show(P.SparksThird, 4), Show(P.SparksFourth, 4), Show(P.SparksFifth, 4), Show(P.SparksSixth, 4)],
        [C.FastSparks] = [Show(P.SparksFirst, 5), Show(P.SparksSecond, 4), Show(P.SparksThird, 3), Show(P.SparksFourth, 3), Show(P.SparksFifth, 3), Show(P.SparksSixth, 3)],
    });

    /// <summary>Each component's animation: the main flare's opening and loop, each spark ring's restarting cycle.</summary>
    internal static IReadOnlyDictionary<C, Animation<C>> Animations { get; } = new Dictionary<C, Animation<C>>
    {
        [C.MainFlare] = new([new(C.MainFlare, 0, 30, 1), new(C.MainFlare, MainFlareLoopStart, 30 - MainFlareLoopStart, 1)], AnimationEnd.LoopsFrom(1)),
        [C.SlowSparks] = new([new(C.SlowSparks, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [C.FastSparks] = new([new(C.FastSparks, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>Shows <paramref name="picture"/> for <paramref name="updates"/> updates.</summary>
    private static AnimationBeat<ChargeFlarePicture, NoAnimationCue> Show(ChargeFlarePicture picture, int updates) =>
        new AnimationBeat<ChargeFlarePicture, NoAnimationCue>.Frame(picture, updates);
}
