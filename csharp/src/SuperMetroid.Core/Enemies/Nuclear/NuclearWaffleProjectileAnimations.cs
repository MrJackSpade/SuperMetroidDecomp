using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Enemies.Nuclear.NuclearWaffleProjectileSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>
/// The Nuclear Waffle's body animation (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyProjectileCommonCue"/> with the values it acts on.
/// </summary>
internal static partial class NuclearWaffleProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static UnpausedAnimationSegments<NuclearWaffleProjectileSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<NuclearWaffleProjectileSegment, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Unpaused[]>
    {
        [P.Churning] =
        [
            Show(EnemyProjectileFrameId.NuclearWaffleFireballSwirl, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleFireballBlazing, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleFireballMottled, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleFireballSwirlTurned, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleFireballBlazingRinged, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleFireballMottledDense, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleFireballSwirl, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleFireballBlazing, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleFireballMottledTurned, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleFireballSwirlTurned, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleFireballBlazingRinged, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleFireballMottledSparse, 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<NuclearWaffleProjectileSegment, Animation<NuclearWaffleProjectileSegment>> StartingAt { get; } = new Dictionary<NuclearWaffleProjectileSegment, Animation<NuclearWaffleProjectileSegment>>
    {
        [P.Churning] = Animation<NuclearWaffleProjectileSegment>.Looping([Segments.Whole(P.Churning)]),
    };

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Unpaused Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);
}
