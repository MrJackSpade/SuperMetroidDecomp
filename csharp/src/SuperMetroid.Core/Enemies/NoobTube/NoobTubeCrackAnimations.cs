using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Rooms.RoomObjects.NoobTubeProjectileSegment;
using F = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileFrameId;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.NoobTube;

/// <summary>
/// The n00b tube's crack animation (#641, #2554): it spreads, flickers, breaks into its falling pieces, which rest a while,
/// and is deleted. Each frame shows a <see cref="NoobTubeProjectileFrame"/>; each cue is a <see cref="NoobTubeCrackCue"/>.
/// </summary>
internal static class NoobTubeCrackAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<NoobTubeProjectileSegment, NoobTubeProjectileFrame, NoobTubeCrackCue> Segments { get; } = new(
        new Dictionary<NoobTubeProjectileSegment, AnimationBeat<NoobTubeProjectileFrame, NoobTubeCrackCue>[]>
    {
        [P.Crack] =
        [
            Show(F.NoobTubeCrackStarting, 12),
            Show(F.NoobTubeCrackSpreading, 10),
            Show(F.NoobTubeCrackWidening, 8),
            Show(F.NoobTubeCrackReaching, 6),
            Show(F.NoobTubeCrackSpanning, 6),
            Show(F.NoobTubeCrackBranching, 6),
            Cue(NoobTubeCrackCue.StartFlicker),
            Show(F.NoobTubeShardsBursting, 1),
            Show(F.NoobTubeShardsFlyingOut, 1),
            Show(F.NoobTubeShardsFlyingApart, 1),
            Show(F.NoobTubeShardsSpreading, 1),
            Show(F.NoobTubeShardsSlowing, 1),
            Show(F.NoobTubeShardsPeaking, 2),
            Show(F.NoobTubeShardsTurning, 3),
            Show(F.NoobTubeShardsCoasting, 6),
            Show(F.NoobTubeShardsTipping, 9),
            Show(F.NoobTubeShardsAtWidest, 8),
            Cue(NoobTubeCrackCue.StartFall),
            Show(F.NoobTubeShardsDipping, 7),
            Show(F.NoobTubeShardsDropping, 7),
            Show(F.NoobTubeShardsSinking, 7),
            Show(F.NoobTubeShardsLowering, 7),
            Show(F.NoobTubeShardsDescending, 7),
            Show(F.NoobTubeShardsFalling, 7),
            Show(F.NoobTubeShardsDrifting, 7),
            Show(F.NoobTubeShardsFloating, 7),
            Show(F.NoobTubeShardsWafting, 7),
            Show(F.NoobTubeShardsGliding, 7),
            Show(F.NoobTubeShardsSailing, 7),
            Show(F.NoobTubeShardsFluttering, 7),
            Show(F.NoobTubeShardsSubsiding, 7),
            Show(F.NoobTubeShardsSlipping, 7),
            Show(F.NoobTubeShardsSifting, 7),
            Show(F.NoobTubeShardsSettling, 7),
            Show(F.NoobTubeShardsSettlingLower, 7),
            Show(F.NoobTubeShardsNearlySettled, 7),
            Show(F.NoobTubeShardsComingToRest, 16),
        ],
        [P.CrackFallen] =
        [
            Show(F.NoobTubeShardsLanded, 16),
            Show(F.NoobTubeShardsComingToRest, 16),
        ],
        [P.Deletion] =
        [
            Cue(NoobTubeCrackCue.Delete),
        ],
    });

    /// <summary>The crack's animation, from its spread through its fallen pieces' plays to its deletion.</summary>
    internal static Animation<NoobTubeProjectileSegment> Crack { get; } =
        new([Segments.Whole(P.Crack), Segments.Repeated(P.CrackFallen, CrackFallenPlays), Segments.Whole(P.Deletion)], AnimationEnd.Holds);

    /// <summary>The animation that starts at each segment an owner can start or restart at: the crack's.</summary>
    internal static IReadOnlyDictionary<NoobTubeProjectileSegment, Animation<NoobTubeProjectileSegment>> StartingAt { get; } =
        new Dictionary<NoobTubeProjectileSegment, Animation<NoobTubeProjectileSegment>> { [P.Crack] = Crack };

    /// <summary>The plays of the fallen crack's last pieces before it is deleted.</summary>
    private const int CrackFallenPlays = 6;

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<NoobTubeProjectileFrame, NoobTubeCrackCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<NoobTubeProjectileFrame, NoobTubeCrackCue>.Frame(new(frame), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<NoobTubeProjectileFrame, NoobTubeCrackCue> Cue(NoobTubeCrackCue cue) => new AnimationBeat<NoobTubeProjectileFrame, NoobTubeCrackCue>.Event(cue);
}
