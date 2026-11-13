using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.YappingMaws.YappingMawBodyProjectileProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// The Yapping Maw's body-link programs (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class YappingMawBodyProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<YappingMawBodyProjectileProgram, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<YappingMawBodyProjectileProgram, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
    {
        [P.FacingDown] =
        [
            Show(EnemyProjectileFrameId.YappingMawFacingDown, 1),
        ],
        [P.FacingUp] =
        [
            Show(EnemyProjectileFrameId.YappingMawFacingUp, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<YappingMawBodyProjectileProgram, Animation<YappingMawBodyProjectileProgram>> StartingAt { get; } = new Dictionary<YappingMawBodyProjectileProgram, Animation<YappingMawBodyProjectileProgram>>
    {
        [P.FacingDown] = new([new(P.FacingDown, 0, 1, 1)], AnimationEnd.Holds),
        [P.FacingUp] = new([new(P.FacingUp, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<YappingMawBodyProjectileProgram> StartingWith(YappingMawBodyProjectileProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);
}
