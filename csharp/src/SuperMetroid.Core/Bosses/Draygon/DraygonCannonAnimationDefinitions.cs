using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.RoomObjects;
using D = SuperMetroid.Core.Bosses.Draygon.DraygonCannonDraw;
using S = SuperMetroid.Core.Bosses.Draygon.DraygonCannonSegment;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// The authored pictures and timing of Draygon's wall cannons, by facing (#1554, #2401): a whole cannon shows its shield
/// and rests on it; a hit that does not destroy it flashes the shield white (<see cref="Flashes"/>); a destroyed cannon sparks for
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
            [S.RightShield] = Shield(D.RightShield),
            [S.RightHitFlash] = HitFlash(D.RightShield, D.RightShieldFlash),
            [S.RightSparks] = Sparks(D.RightSparksFlaring, D.RightSparksSpreading, D.RightSparksReceding, D.RightSparksFlat),
            [S.LeftShield] = Shield(D.LeftShield),
            [S.LeftHitFlash] = HitFlash(D.LeftShield, D.LeftShieldFlash),
            [S.LeftSparks] = Sparks(D.LeftSparksFlaring, D.LeftSparksSpreading, D.LeftSparksReceding, D.LeftSparksFlat),
        });

    /// <summary>What each facing's cannon shows.</summary>
    internal static IReadOnlyDictionary<DraygonCannonOrientation, DraygonCannonLook> Looks { get; } =
        new Dictionary<DraygonCannonOrientation, DraygonCannonLook>
        {
            [DraygonCannonOrientation.Right] = Look(S.RightShield, S.RightHitFlash, S.RightSparks),
            [DraygonCannonOrientation.Left] = Look(S.LeftShield, S.LeftHitFlash, S.LeftSparks),
        };

    /// <summary>A facing's shield, held (#2557).</summary>
    private static AnimationBeat<DraygonCannonDraw, NoAnimationCue>[] Shield(DraygonCannonDraw shield) => [Show(shield, ShieldDuration)];

    /// <summary>One flash of a facing's shield as a hit lands: the shield, then the shield flashing white (#2557).</summary>
    private static AnimationBeat<DraygonCannonDraw, NoAnimationCue>[] HitFlash(DraygonCannonDraw shield, DraygonCannonDraw flash) =>
        [Show(shield, FlashShieldDuration), Show(flash, FlashWhiteDuration)];

    /// <summary>A facing's destroyed cannon's sparks, each stage for <see cref="SparkDuration"/> (#2557).</summary>
    private static AnimationBeat<DraygonCannonDraw, NoAnimationCue>[] Sparks(
        DraygonCannonDraw flaring, DraygonCannonDraw spreading, DraygonCannonDraw receding, DraygonCannonDraw flat) =>
        [Show(flaring, SparkDuration), Show(spreading, SparkDuration), Show(receding, SparkDuration), Show(flat, SparkDuration)];

    /// <summary>
    /// What a facing's cannon shows (#2557): its shield held, the hit flash played <see cref="Flashes"/> times, and its sparks
    /// looping once destroyed.
    /// </summary>
    private static DraygonCannonLook Look(S shield, S hitFlash, S sparks) => new(
        new([Segments.Whole(shield)], AnimationEnd.Holds),
        new([Segments.Repeated(hitFlash, Flashes)], AnimationEnd.Stops),
        Animation<S>.Looping([Segments.Whole(sparks)]));

    /// <summary>Shows <paramref name="draw"/> for <paramref name="updates"/> updates.</summary>
    private static AnimationBeat<DraygonCannonDraw, NoAnimationCue> Show(DraygonCannonDraw draw, int updates) =>
        new AnimationBeat<DraygonCannonDraw, NoAnimationCue>.Frame(draw, updates);
}
