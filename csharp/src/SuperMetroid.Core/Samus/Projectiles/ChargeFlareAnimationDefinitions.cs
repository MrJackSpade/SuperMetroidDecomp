using SuperMetroid.Core.Animation;
using C = SuperMetroid.Core.Samus.Projectiles.ChargeFlareComponent;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The charge flare's animations (#1624): each component's pictures with the updates each shows for, and how each animation
/// goes on past its last picture. The main flare plays its thirty pictures once and then loops from its seventeenth; each
/// spark ring plays its six pictures and restarts from its first.
/// </summary>
internal static class ChargeFlareAnimationDefinitions
{
    /// <summary>The main flare's first picture of its loop: picture 16, after which it repeats pictures 16 through 29.</summary>
    internal const int MainFlareLoopStart = 16;

    /// <summary>The updates each picture of the Hyper Beam's flare shows for as it plays the components' pictures backward.</summary>
    internal const int HyperBeamPictureUpdates = 3;

    /// <summary>Every component's pictures, each with the updates it shows for.</summary>
    internal static AnimationSegments<C, ChargeFlarePhase, NoAnimationCue> Segments { get; } = new(
        new Dictionary<C, AnimationBeat<ChargeFlarePhase, NoAnimationCue>[]>
    {
        [C.MainFlare] =
        [
            Show(0, 4), Show(1, 4), Show(2, 4), Show(3, 4), Show(4, 4), Show(5, 4), Show(6, 4), Show(7, 4),
            Show(8, 4), Show(9, 4), Show(10, 4), Show(11, 4), Show(12, 4), Show(13, 4), Show(14, 4), Show(15, 4),
            Show(16, 4), Show(17, 4), Show(18, 4), Show(19, 4), Show(20, 4), Show(21, 4), Show(22, 4), Show(23, 4),
            Show(24, 4), Show(25, 4), Show(26, 4), Show(27, 4), Show(28, 4), Show(29, 4),
        ],
        [C.SlowSparks] = [Show(0, 6), Show(1, 5), Show(2, 4), Show(3, 4), Show(4, 4), Show(5, 4)],
        [C.FastSparks] = [Show(0, 5), Show(1, 4), Show(2, 3), Show(3, 3), Show(4, 3), Show(5, 3)],
    });

    /// <summary>Each component's animation: the main flare's opening and loop, each spark ring's restarting cycle.</summary>
    internal static IReadOnlyDictionary<C, Animation<C>> Animations { get; } = new Dictionary<C, Animation<C>>
    {
        [C.MainFlare] = new([new(C.MainFlare, 0, 30, 1), new(C.MainFlare, MainFlareLoopStart, 30 - MainFlareLoopStart, 1)], AnimationEnd.LoopsFrom(1)),
        [C.SlowSparks] = new([new(C.SlowSparks, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [C.FastSparks] = new([new(C.FastSparks, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>Shows picture <paramref name="phase"/> for <paramref name="updates"/> updates.</summary>
    private static AnimationBeat<ChargeFlarePhase, NoAnimationCue> Show(int phase, int updates) =>
        new AnimationBeat<ChargeFlarePhase, NoAnimationCue>.Frame(new(phase), updates);
}
