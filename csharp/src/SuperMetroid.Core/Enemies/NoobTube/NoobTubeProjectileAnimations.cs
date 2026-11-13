using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Rooms.RoomObjects.NoobTubeProjectileCueKind;
using P = SuperMetroid.Core.Rooms.RoomObjects.NoobTubeProjectileSegment;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.NoobTube;

/// <summary>
/// The n00b tube's crack, glass shard and released-air bubble programs (#641). Each
/// frame shows a <see cref="NoobTubeProjectileFrame"/> pose and each cue is a <see cref="NoobTubeProjectileCue"/> with its typed operands.
/// </summary>
internal static class NoobTubeProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<NoobTubeProjectileSegment, NoobTubeProjectileFrame, NoobTubeProjectileCue> Segments { get; } = new(
        new Dictionary<NoobTubeProjectileSegment, AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>[]>
    {
        [P.Crack] =
        [
            Show(0, 12),
            Show(1, 10),
            Show(2, 8),
            Show(3, 6),
            Show(4, 6),
            Show(5, 6),
            Cue(new(K.SetFrameBehavior, FrameBehavior: EnemyProjectileFrameBehavior.NoobTubeCrackFlickering)),
            Show(6, 1),
            Show(7, 1),
            Show(8, 1),
            Show(9, 1),
            Show(10, 1),
            Show(11, 2),
            Show(12, 3),
            Show(13, 6),
            Show(14, 9),
            Show(15, 8),
            Cue(new(K.SetFrameBehavior, FrameBehavior: EnemyProjectileFrameBehavior.NoobTubeCrackFalling)),
            Show(16, 7),
            Show(17, 7),
            Show(18, 7),
            Show(19, 7),
            Show(20, 7),
            Show(21, 7),
            Show(22, 7),
            Show(23, 7),
            Show(24, 7),
            Show(25, 7),
            Show(26, 7),
            Show(27, 7),
            Show(28, 7),
            Show(29, 7),
            Show(30, 7),
            Show(31, 7),
            Show(32, 7),
            Show(33, 7),
            Show(34, 16),
        ],
        [P.CrackLoop] =
        [
            Show(35, 16),
            Show(36, 16),
            Cue(new(K.Delete)),
        ],
        [P.Shard0] =
        [
        ],
        [P.Shard0Loop] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(37, 38),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.SetFrameBehavior, FrameBehavior: EnemyProjectileFrameBehavior.NoobTubeShardFalling)),
        ],
        [P.Shard0Loop2] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(39, 40),
            Cue(new(K.Delete)),
        ],
        [P.Shard1] =
        [
        ],
        [P.Shard1Loop] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(41, 42),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.SetFrameBehavior, FrameBehavior: EnemyProjectileFrameBehavior.NoobTubeShardFalling)),
        ],
        [P.Shard1Loop2] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(43, 44),
            Cue(new(K.Delete)),
        ],
        [P.Shard2] =
        [
        ],
        [P.Shard2Loop] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(45, 46),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.SetFrameBehavior, FrameBehavior: EnemyProjectileFrameBehavior.NoobTubeShardFalling)),
        ],
        [P.Shard2Loop2] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(47, 48),
            Cue(new(K.Delete)),
        ],
        [P.Shard3] =
        [
        ],
        [P.Shard3Loop] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(49, 50),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.SetFrameBehavior, FrameBehavior: EnemyProjectileFrameBehavior.NoobTubeShardFalling)),
        ],
        [P.Shard3Loop2] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(51, 52),
            Cue(new(K.Delete)),
        ],
        [P.Shard4] =
        [
        ],
        [P.Shard4Loop] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(53, 54),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.SetFrameBehavior, FrameBehavior: EnemyProjectileFrameBehavior.NoobTubeShardFalling)),
        ],
        [P.Shard4Loop2] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(55, 56),
            Cue(new(K.Delete)),
        ],
        [P.Shard5] =
        [
        ],
        [P.Shard5Loop] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(57, 58),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.SetFrameBehavior, FrameBehavior: EnemyProjectileFrameBehavior.NoobTubeShardFalling)),
        ],
        [P.Shard5Loop2] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(59, 60),
            Cue(new(K.Delete)),
        ],
        [P.Shard6] =
        [
        ],
        [P.Shard6Loop] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(61, 62),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.SetFrameBehavior, FrameBehavior: EnemyProjectileFrameBehavior.NoobTubeShardFalling)),
        ],
        [P.Shard6Loop2] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(63, 64),
            Cue(new(K.Delete)),
        ],
        [P.Shard7] =
        [
        ],
        [P.Shard7Loop] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(65, 66),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.SetFrameBehavior, FrameBehavior: EnemyProjectileFrameBehavior.NoobTubeShardFalling)),
        ],
        [P.Shard7Loop2] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(67, 68),
            Cue(new(K.Delete)),
        ],
        [P.Shard8] =
        [
        ],
        [P.Shard8Loop] =
        [
            Cue(new(K.FlickerHidden)),
            Flicker(69),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.SetFrameBehavior, FrameBehavior: EnemyProjectileFrameBehavior.NoobTubeShardFalling)),
        ],
        [P.Shard8Loop2] =
        [
            Cue(new(K.FlickerHidden)),
            Flicker(70),
            Cue(new(K.Delete)),
        ],
        [P.Shard9] =
        [
        ],
        [P.Shard9Loop] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(71, 72),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.SetFrameBehavior, FrameBehavior: EnemyProjectileFrameBehavior.NoobTubeShardFalling)),
        ],
        [P.Shard9Loop2] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(73, 74),
            Cue(new(K.Delete)),
        ],
        [P.ReleasedAirBubble] =
        [
            Cue(new(K.SetFrameBehavior, FrameBehavior: EnemyProjectileFrameBehavior.NoobTubeBubbleFlying)),
            Show(75, 2),
            Show(76, 2),
            Show(77, 2),
            Show(78, 2),
            Cue(new(K.AssignBubbleFallingAngle)),
            Cue(new(K.SetFrameBehavior, FrameBehavior: EnemyProjectileFrameBehavior.NoobTubeBubbleFalling)),
            Show(79, 2),
            Show(80, 2),
            Show(81, 2),
            Show(82, 2),
            Show(83, 2),
            Show(84, 2),
            Show(85, 4),
            Show(86, 4),
            Show(87, 4),
            Show(88, 4),
            Show(89, 4),
            Cue(new(K.Delete)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<NoobTubeProjectileSegment, Animation<NoobTubeProjectileSegment>> StartingAt { get; } = new Dictionary<NoobTubeProjectileSegment, Animation<NoobTubeProjectileSegment>>
    {
        [P.Crack] = new([new(P.Crack, 0, 37, 1), new(P.CrackLoop, 0, 2, 6), new(P.CrackLoop, 2, 1, 1)], AnimationEnd.Holds),
        [P.Shard0] = new([new(P.Shard0Loop, 0, 2, 32), new(P.Shard0Loop, 2, 2, 1), new(P.Shard0Loop2, 0, 2, 272), new(P.Shard0Loop2, 2, 1, 1)], AnimationEnd.Holds),
        [P.Shard1] = new([new(P.Shard1Loop, 0, 2, 32), new(P.Shard1Loop, 2, 2, 1), new(P.Shard1Loop2, 0, 2, 272), new(P.Shard1Loop2, 2, 1, 1)], AnimationEnd.Holds),
        [P.Shard2] = new([new(P.Shard2Loop, 0, 2, 32), new(P.Shard2Loop, 2, 2, 1), new(P.Shard2Loop2, 0, 2, 272), new(P.Shard2Loop2, 2, 1, 1)], AnimationEnd.Holds),
        [P.Shard3] = new([new(P.Shard3Loop, 0, 2, 32), new(P.Shard3Loop, 2, 2, 1), new(P.Shard3Loop2, 0, 2, 272), new(P.Shard3Loop2, 2, 1, 1)], AnimationEnd.Holds),
        [P.Shard4] = new([new(P.Shard4Loop, 0, 2, 32), new(P.Shard4Loop, 2, 2, 1), new(P.Shard4Loop2, 0, 2, 272), new(P.Shard4Loop2, 2, 1, 1)], AnimationEnd.Holds),
        [P.Shard5] = new([new(P.Shard5Loop, 0, 2, 32), new(P.Shard5Loop, 2, 2, 1), new(P.Shard5Loop2, 0, 2, 272), new(P.Shard5Loop2, 2, 1, 1)], AnimationEnd.Holds),
        [P.Shard6] = new([new(P.Shard6Loop, 0, 2, 32), new(P.Shard6Loop, 2, 2, 1), new(P.Shard6Loop2, 0, 2, 272), new(P.Shard6Loop2, 2, 1, 1)], AnimationEnd.Holds),
        [P.Shard7] = new([new(P.Shard7Loop, 0, 2, 32), new(P.Shard7Loop, 2, 2, 1), new(P.Shard7Loop2, 0, 2, 272), new(P.Shard7Loop2, 2, 1, 1)], AnimationEnd.Holds),
        [P.Shard8] = new([new(P.Shard8Loop, 0, 2, 32), new(P.Shard8Loop, 2, 2, 1), new(P.Shard8Loop2, 0, 2, 272), new(P.Shard8Loop2, 2, 1, 1)], AnimationEnd.Holds),
        [P.Shard9] = new([new(P.Shard9Loop, 0, 2, 32), new(P.Shard9Loop, 2, 2, 1), new(P.Shard9Loop2, 0, 2, 272), new(P.Shard9Loop2, 2, 1, 1)], AnimationEnd.Holds),
        [P.ReleasedAirBubble] = new([new(P.ReleasedAirBubble, 0, 19, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<NoobTubeProjectileSegment> StartingWith(NoobTubeProjectileSegment segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue> Show(int pose, int duration) => new AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue> Cue(NoobTubeProjectileCue cue) => new AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>.Event(cue);

    /// <summary>Shows <paramref name="pose"/>, or on even frames <paramref name="reflectedPose"/>, for the one update a flicker lasts.</summary>
    private static AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue> Flicker(int pose, int reflectedPose) => new AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>.Frame(new(pose, reflectedPose), 1);

    /// <summary>Shows <paramref name="pose"/> for the one update a flicker lasts.</summary>
    private static AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue> Flicker(int pose) => new AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>.Frame(new(pose), 1);
}
