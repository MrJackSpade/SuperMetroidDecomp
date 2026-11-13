using SuperMetroid.Core.Programs;
using Beat = SuperMetroid.Core.Programs.AnimationBeat<SuperMetroid.Core.Enemies.Projectiles.KraidRockFrame, SuperMetroid.Core.Enemies.Projectiles.KraidRockCue>;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Kraid's typed rock animations (#641, #1600).</summary>
internal static class KraidRockAnimations
{
    /// <summary>Frames in the spat rock's burst.</summary>
    internal const int ShotPhaseCount = 5;

    /// <summary>
    /// Each rock segment's beats: the shared and right rising rocks hold their still pose for good (#1627: the original's
    /// lists show it for the longest frame a list gives and then rest on it); the spat rock's burst takes the burst's frame
    /// behaviour, shows five four-update frames, then deletes itself.
    /// </summary>
    internal static AnimationSegments<KraidRockProgram, KraidRockFrame, KraidRockCue> Segments { get; } = new(
        new Dictionary<KraidRockProgram, Beat[]>
        {
            [KraidRockProgram.SharedPose] = [new Beat.HeldFrame(new(KraidRockProgram.SharedPose, 0))],
            [KraidRockProgram.RisingRightPose] = [new Beat.HeldFrame(new(KraidRockProgram.RisingRightPose, 0))],
            [KraidRockProgram.SpitRockShot] =
            [
                new Beat.Event(KraidRockCue.DrawFromSharedSheetFromNextPass),
                .. Enumerable.Range(0, ShotPhaseCount)
                    .Select(phase => new Beat.Frame(new(KraidRockProgram.SpitRockShot, phase), 4)),
                new Beat.Event(KraidRockCue.Delete),
            ],
        });

    /// <summary>The animation that starts at each segment: each plays its segment once and holds its last frame.</summary>
    internal static IReadOnlyDictionary<KraidRockProgram, Animation<KraidRockProgram>> StartingAt { get; } = new Dictionary<KraidRockProgram, Animation<KraidRockProgram>>
    {
        [KraidRockProgram.SharedPose] = new([new(KraidRockProgram.SharedPose, 0, 1, 1)], AnimationEnd.Holds),
        [KraidRockProgram.RisingRightPose] = new([new(KraidRockProgram.RisingRightPose, 0, 1, 1)], AnimationEnd.Holds),
        [KraidRockProgram.SpitRockShot] = new([new(KraidRockProgram.SpitRockShot, 0, ShotPhaseCount + 2, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<KraidRockProgram> StartingWith(KraidRockProgram segment) => StartingAt[segment];
}
