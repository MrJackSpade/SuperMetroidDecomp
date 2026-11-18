using SuperMetroid.Core.Animation;
using D = SuperMetroid.Core.Bosses.Draygon.DraygonCannonDraw;
using S = SuperMetroid.Core.Bosses.Draygon.DraygonCannonSegment;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// The authored pictures and timing of Draygon's wall cannons, by facing (#1554, #2401): a whole cannon shows its shield
/// and rests on it; a hit that does not destroy it flashes the shield white three times; a destroyed cannon sparks for
/// good.
/// </summary>
internal static class DraygonCannonAnimationDefinitions
{
    /// <summary>How long the shield shows before the cannon rests on it.</summary>
    internal const int ShieldDuration = 8;

    /// <summary>How long each flash of a hit shows the shield as it is.</summary>
    internal const int FlashShieldDuration = 3;

    /// <summary>How long each flash of a hit shows the shield white.</summary>
    internal const int FlashWhiteDuration = 4;

    /// <summary>How many times a hit flashes the shield white.</summary>
    internal const int Flashes = 3;

    /// <summary>How long each stage of a destroyed cannon's sparks shows.</summary>
    internal const int SparkDuration = 6;

    /// <summary>Every run's pictures, each with the updates it shows for.</summary>
    internal static AnimationSegments<S, DraygonCannonDraw, NoAnimationCue> Segments { get; } = new(
        new Dictionary<S, AnimationBeat<DraygonCannonDraw, NoAnimationCue>[]>
        {
            [S.RightShield] = [Show(D.RightShield, ShieldDuration)],
            [S.RightHitFlash] = [Show(D.RightShield, FlashShieldDuration), Show(D.RightShieldFlash, FlashWhiteDuration)],
            [S.RightSparks] =
            [
                Show(D.RightSparksFlaring, SparkDuration), Show(D.RightSparksSpreading, SparkDuration),
                Show(D.RightSparksReceding, SparkDuration), Show(D.RightSparksFlat, SparkDuration),
            ],
            [S.LeftShield] = [Show(D.LeftShield, ShieldDuration)],
            [S.LeftHitFlash] = [Show(D.LeftShield, FlashShieldDuration), Show(D.LeftShieldFlash, FlashWhiteDuration)],
            [S.LeftSparks] =
            [
                Show(D.LeftSparksFlaring, SparkDuration), Show(D.LeftSparksSpreading, SparkDuration),
                Show(D.LeftSparksReceding, SparkDuration), Show(D.LeftSparksFlat, SparkDuration),
            ],
        });

    /// <summary>What each facing's cannon shows.</summary>
    internal static IReadOnlyDictionary<DraygonCannonOrientation, DraygonCannonLook> Looks { get; } =
        new Dictionary<DraygonCannonOrientation, DraygonCannonLook>
        {
            [DraygonCannonOrientation.Right] = new(
                new([Segments.Whole(S.RightShield)], AnimationEnd.Holds),
                new([Segments.Repeated(S.RightHitFlash, Flashes)], AnimationEnd.Stops),
                Animation<S>.Looping([Segments.Whole(S.RightSparks)])),
            [DraygonCannonOrientation.Left] = new(
                new([Segments.Whole(S.LeftShield)], AnimationEnd.Holds),
                new([Segments.Repeated(S.LeftHitFlash, Flashes)], AnimationEnd.Stops),
                Animation<S>.Looping([Segments.Whole(S.LeftSparks)])),
        };

    /// <summary>Shows <paramref name="draw"/> for <paramref name="updates"/> updates.</summary>
    private static AnimationBeat<DraygonCannonDraw, NoAnimationCue> Show(DraygonCannonDraw draw, int updates) =>
        new AnimationBeat<DraygonCannonDraw, NoAnimationCue>.Frame(draw, updates);
}
