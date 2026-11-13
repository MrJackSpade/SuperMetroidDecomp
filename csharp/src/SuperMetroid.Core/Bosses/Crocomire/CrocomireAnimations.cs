using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's typed tongue and projectile animations (#641, #1600).</summary>
internal static class CrocomireAnimations
{
    /// <summary>Frames in the mouth projectile's looping flight.</summary>
    internal const int MouthPhaseCount = 6;
    /// <summary>Frames in the shot projectile's explosion.</summary>
    internal const int ShotPhaseCount = 5;

    /// <summary>The fight tongue's frames, five updates each.</summary>
    private static readonly CrocomireTongueFrame[] FightTongueFrames =
        [CrocomireTongueFrame.Fight0, CrocomireTongueFrame.Fight1, CrocomireTongueFrame.Fight2, CrocomireTongueFrame.Fight3];

    /// <summary>The melting tongue's frames, five updates each.</summary>
    private static readonly CrocomireTongueFrame[] MeltingTongueFrames =
    [
        CrocomireTongueFrame.MeltingPose0, CrocomireTongueFrame.MeltingPose1, CrocomireTongueFrame.MeltingPose2,
        CrocomireTongueFrame.MeltingPose3, CrocomireTongueFrame.MeltingPose4,
    ];

    /// <summary>The tongue's segments: the fight loop's frames, the hidden tongue's bare sleep, and the melting loop's frames.</summary>
    internal static AnimationSegments<CrocomireTongueProgram, CrocomireTongueFrame, NoAnimationCue> TongueSegments { get; } = new(
        new Dictionary<CrocomireTongueProgram, AnimationBeat<CrocomireTongueFrame, NoAnimationCue>[]>
        {
            [CrocomireTongueProgram.Fight] = [.. FightTongueFrames.Select(frame => TongueFrame(frame, 5))],
            [CrocomireTongueProgram.Sleep] = [],
            [CrocomireTongueProgram.Melting] = [.. MeltingTongueFrames.Select(frame => TongueFrame(frame, 5))],
        });

    /// <summary>The tongue's animations: the fight and melting frames loop, and the hidden tongue rests.</summary>
    internal static IReadOnlyDictionary<CrocomireTongueProgram, Animation<CrocomireTongueProgram>> TongueStartingAt { get; } =
        new Dictionary<CrocomireTongueProgram, Animation<CrocomireTongueProgram>>
        {
            [CrocomireTongueProgram.Fight] = new([new(CrocomireTongueProgram.Fight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
            [CrocomireTongueProgram.Sleep] = new([], AnimationEnd.Holds),
            [CrocomireTongueProgram.Melting] = new([new(CrocomireTongueProgram.Melting, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        };

    /// <summary>The tongue animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CrocomireTongueProgram> TongueStartingWith(CrocomireTongueProgram segment) => TongueStartingAt[segment];

    /// <summary>
    /// The projectile segments, one per projectile kind: the fight projectile's six three-update frames; the bridge
    /// fragment's and spike-wall piece's one still frame, held for good (#1627: it shows for the
    /// longest duration a frame can have, after which it still shows that frame); the shot fight projectile's five four-update explosion frames,
    /// then a drop with Crocomire's chances and deletion.
    /// </summary>
    internal static AnimationSegments<CrocomireProjectileFrameKind, CrocomireProjectileFrame, CrocomireProjectileCue> ProjectileSegments { get; } = new(
        new Dictionary<CrocomireProjectileFrameKind, AnimationBeat<CrocomireProjectileFrame, CrocomireProjectileCue>[]>
        {
            [CrocomireProjectileFrameKind.Mouth] =
                [.. Enumerable.Range(0, MouthPhaseCount).Select(phase => ProjectileFrame(new(CrocomireProjectileFrameKind.Mouth, phase), 3))],
            [CrocomireProjectileFrameKind.BridgeFragment] =
                [new AnimationBeat<CrocomireProjectileFrame, CrocomireProjectileCue>.HeldFrame(new(CrocomireProjectileFrameKind.BridgeFragment, 0))],
            [CrocomireProjectileFrameKind.SpikeWallPiece] =
                [new AnimationBeat<CrocomireProjectileFrame, CrocomireProjectileCue>.HeldFrame(new(CrocomireProjectileFrameKind.SpikeWallPiece, 0))],
            [CrocomireProjectileFrameKind.Shot] =
            [
                .. Enumerable.Range(0, ShotPhaseCount).Select(phase => ProjectileFrame(new(CrocomireProjectileFrameKind.Shot, phase), 4)),
                new AnimationBeat<CrocomireProjectileFrame, CrocomireProjectileCue>.Event(CrocomireProjectileCue.SpawnDrops),
                new AnimationBeat<CrocomireProjectileFrame, CrocomireProjectileCue>.Event(CrocomireProjectileCue.Delete),
            ],
        });

    /// <summary>
    /// The projectile animations: the mouth projectile loops its frames, the bridge fragment and spike-wall piece rest on
    /// their held frame, and the shot holds after its deletion.
    /// </summary>
    internal static IReadOnlyDictionary<CrocomireProjectileFrameKind, Animation<CrocomireProjectileFrameKind>> ProjectileStartingAt { get; } =
        new Dictionary<CrocomireProjectileFrameKind, Animation<CrocomireProjectileFrameKind>>
        {
            [CrocomireProjectileFrameKind.Mouth] = new([new(CrocomireProjectileFrameKind.Mouth, 0, MouthPhaseCount, 1)], AnimationEnd.LoopsFrom(0)),
            [CrocomireProjectileFrameKind.BridgeFragment] = new([new(CrocomireProjectileFrameKind.BridgeFragment, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
            [CrocomireProjectileFrameKind.SpikeWallPiece] = new([new(CrocomireProjectileFrameKind.SpikeWallPiece, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
            [CrocomireProjectileFrameKind.Shot] = new([new(CrocomireProjectileFrameKind.Shot, 0, ShotPhaseCount + 2, 1)], AnimationEnd.Holds),
        };

    /// <summary>The projectile animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CrocomireProjectileFrameKind> ProjectileStartingWith(CrocomireProjectileFrameKind segment) => ProjectileStartingAt[segment];

    /// <summary>Shows tongue frame <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<CrocomireTongueFrame, NoAnimationCue> TongueFrame(CrocomireTongueFrame frame, int duration) =>
        new AnimationBeat<CrocomireTongueFrame, NoAnimationCue>.Frame(frame, duration);

    /// <summary>Shows projectile frame <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<CrocomireProjectileFrame, CrocomireProjectileCue> ProjectileFrame(CrocomireProjectileFrame frame, int duration) =>
        new AnimationBeat<CrocomireProjectileFrame, CrocomireProjectileCue>.Frame(frame, duration);
}
