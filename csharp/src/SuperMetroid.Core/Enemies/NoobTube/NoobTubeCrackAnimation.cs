using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.NoobTube;

/// <summary>The n00b tube crack's typed animation (#2554), from its spread to its deletion.</summary>
/// <param name="cues">The crack's behaviour, which owns its animation's cues.</param>
internal sealed class NoobTubeCrackAnimation(IEnemyProjectileCues<NoobTubeCrackCue> cues)
    : EnemyProjectileTypedAnimation<NoobTubeProjectileSegment, NoobTubeProjectileFrame, NoobTubeCrackCue>(NoobTubeCrackAnimations.Crack, cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<NoobTubeProjectileSegment, NoobTubeProjectileFrame, NoobTubeCrackCue> Segments => NoobTubeCrackAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<NoobTubeProjectileSegment, Animation<NoobTubeProjectileSegment>> StartingAt => NoobTubeCrackAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(NoobTubeProjectileFrame frame) => frame.Shown;
}
