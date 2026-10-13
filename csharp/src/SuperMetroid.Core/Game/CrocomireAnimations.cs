using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>Crocomire's tongue programs.</summary>
public enum CrocomireTongueProgram
{
    /// <summary><c>InstList_CrocomireTongue_Fight</c> ($A4:BE56): four five-update frames, looping.</summary>
    Fight,
    /// <summary><c>InstList_Crocomire_Sleep</c> ($A4:BF62): a bare sleep for the hidden tongue after the bridge collapses.</summary>
    Sleep,
    /// <summary><c>InstList_CrocomireTongue_Melting</c> ($A4:BF98): five five-update frames, looping.</summary>
    Melting,
}

/// <summary>What a Crocomire projectile program shows.</summary>
public enum CrocomireProjectileFrameKind
{
    /// <summary>The mouth projectile in flight.</summary>
    Mouth,
    /// <summary>A crumbling bridge fragment.</summary>
    BridgeFragment,
    /// <summary>A spike-wall piece.</summary>
    SpikeWallPiece,
    /// <summary>The mouth projectile's explosion after a Samus shot.</summary>
    Shot,
}

/// <summary>One Crocomire projectile frame.</summary>
/// <param name="Kind">The program the frame belongs to.</param>
/// <param name="Phase">The frame's position in that program.</param>
public readonly record struct CrocomireProjectileFrame(CrocomireProjectileFrameKind Kind, int Phase);

/// <summary>Gameplay cues of Crocomire's projectile programs.</summary>
internal enum CrocomireProjectileCue
{
    /// <summary>Spawns an enemy drop with Crocomire's drop chances where the shot projectile exploded.</summary>
    SpawnDrops,
    /// <summary>Deletes the projectile, ending its animation.</summary>
    Delete,
}

/// <summary>Crocomire's typed tongue and projectile animations (#641), in place of their native lists.</summary>
internal static class CrocomireAnimations
{
    /// <summary>Frames in the mouth projectile's looping flight.</summary>
    internal const int MouthPhaseCount = 6;
    /// <summary>Frames in the shot projectile's explosion.</summary>
    internal const int ShotPhaseCount = 5;
    /// <summary>The bridge and spike frames last the native maximum, so their loops never advance in play.</summary>
    private const int StillFrameDuration = 32767;

    /// <summary>The tongue's fight loop.</summary>
    private static readonly AnimationTimeline<CrocomireTongueFrame, NoAnimationCue> TongueFight = new(
    [
        .. new[] { CrocomireTongueFrame.Fight0, CrocomireTongueFrame.Fight1, CrocomireTongueFrame.Fight2, CrocomireTongueFrame.Fight3 }
            .Select(frame => AnimationStep<CrocomireTongueFrame, NoAnimationCue>.Show(frame, 5)),
        AnimationStep<CrocomireTongueFrame, NoAnimationCue>.Loop,
    ]);

    /// <summary>The hidden tongue's bare sleep.</summary>
    private static readonly AnimationTimeline<CrocomireTongueFrame, NoAnimationCue> TongueSleep =
        new(AnimationStep<CrocomireTongueFrame, NoAnimationCue>.Hold);

    /// <summary>The tongue's melting loop.</summary>
    private static readonly AnimationTimeline<CrocomireTongueFrame, NoAnimationCue> TongueMelting = new(
    [
        .. new[]
        {
            CrocomireTongueFrame.MeltingPose0, CrocomireTongueFrame.MeltingPose1, CrocomireTongueFrame.MeltingPose2,
            CrocomireTongueFrame.MeltingPose3, CrocomireTongueFrame.MeltingPose4,
        }.Select(frame => AnimationStep<CrocomireTongueFrame, NoAnimationCue>.Show(frame, 5)),
        AnimationStep<CrocomireTongueFrame, NoAnimationCue>.Loop,
    ]);

    /// <summary>The timeline of <paramref name="program"/>.</summary>
    internal static AnimationTimeline<CrocomireTongueFrame, NoAnimationCue> Tongue(CrocomireTongueProgram program) => program switch
    {
        CrocomireTongueProgram.Fight => TongueFight,
        CrocomireTongueProgram.Sleep => TongueSleep,
        CrocomireTongueProgram.Melting => TongueMelting,
        _ => throw new ArgumentOutOfRangeException(nameof(program), program, "Undefined Crocomire tongue program."),
    };

    /// <summary><c>InstList_EnemyProjectile_CrocomiresProjectile</c> ($86:8FCF): six three-update frames, looping.</summary>
    internal static AnimationTimeline<CrocomireProjectileFrame, CrocomireProjectileCue> Mouth { get; } = new(
    [
        .. Enumerable.Range(0, MouthPhaseCount)
            .Select(phase => AnimationStep<CrocomireProjectileFrame, CrocomireProjectileCue>.Show(new(CrocomireProjectileFrameKind.Mouth, phase), 3)),
        AnimationStep<CrocomireProjectileFrame, CrocomireProjectileCue>.Loop,
    ]);

    /// <summary><c>InstList_EnemyProjectile_CrocomireBridgeCrumbling</c> ($86:8FEB): one still frame, looping.</summary>
    internal static AnimationTimeline<CrocomireProjectileFrame, CrocomireProjectileCue> BridgeFragment { get; } = new(
        AnimationStep<CrocomireProjectileFrame, CrocomireProjectileCue>.Show(new(CrocomireProjectileFrameKind.BridgeFragment, 0), StillFrameDuration),
        AnimationStep<CrocomireProjectileFrame, CrocomireProjectileCue>.Loop);

    /// <summary><c>InstList_EnemyProjectile_CrocomireSpikeWallPieces</c> ($86:8FF3): one still frame, looping.</summary>
    internal static AnimationTimeline<CrocomireProjectileFrame, CrocomireProjectileCue> SpikeWallPiece { get; } = new(
        AnimationStep<CrocomireProjectileFrame, CrocomireProjectileCue>.Show(new(CrocomireProjectileFrameKind.SpikeWallPiece, 0), StillFrameDuration),
        AnimationStep<CrocomireProjectileFrame, CrocomireProjectileCue>.Loop);

    /// <summary>
    /// <c>InstList_EnemyProjectile_Shot_CrocomiresProjectile</c> ($86:9007): five four-update explosion frames,
    /// then a drop with Crocomire's chances and deletion.
    /// </summary>
    internal static AnimationTimeline<CrocomireProjectileFrame, CrocomireProjectileCue> Shot { get; } = new(
    [
        .. Enumerable.Range(0, ShotPhaseCount)
            .Select(phase => AnimationStep<CrocomireProjectileFrame, CrocomireProjectileCue>.Show(new(CrocomireProjectileFrameKind.Shot, phase), 4)),
        AnimationStep<CrocomireProjectileFrame, CrocomireProjectileCue>.Publish(CrocomireProjectileCue.SpawnDrops),
        AnimationStep<CrocomireProjectileFrame, CrocomireProjectileCue>.Publish(CrocomireProjectileCue.Delete),
        AnimationStep<CrocomireProjectileFrame, CrocomireProjectileCue>.Hold,
    ]);
}

/// <summary>A Crocomire projectile's typed animation.</summary>
internal sealed class CrocomireProjectileAnimation : EnemyProjectileTimelineAnimation<CrocomireProjectileFrame, CrocomireProjectileCue>
{
    /// <summary>Plays <paramref name="kind"/>'s program.</summary>
    internal CrocomireProjectileAnimation(CrocomireProjectileFrameKind kind) => Kind = kind;

    /// <summary>The program played.</summary>
    internal CrocomireProjectileFrameKind Kind { get; }

    /// <inheritdoc/>
    private protected override AnimationTimeline<CrocomireProjectileFrame, CrocomireProjectileCue> Timeline => Kind switch
    {
        CrocomireProjectileFrameKind.Mouth => CrocomireAnimations.Mouth,
        CrocomireProjectileFrameKind.BridgeFragment => CrocomireAnimations.BridgeFragment,
        CrocomireProjectileFrameKind.SpikeWallPiece => CrocomireAnimations.SpikeWallPiece,
        CrocomireProjectileFrameKind.Shot => CrocomireAnimations.Shot,
        _ => throw new InvalidOperationException($"Undefined {nameof(CrocomireProjectileFrameKind)} {Kind}."),
    };

    /// <inheritdoc/>
    private protected override ushort PresentationKey(CrocomireProjectileFrame frame) =>
        CrocomireProjectileVisualDefinitions.PresentationKey(frame);
}
