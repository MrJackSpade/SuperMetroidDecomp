using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>Alcoon fireball's typed animation.</summary>
internal sealed partial class AlcoonFireballAnimation
    : EnemyProjectileFrameAnimation<AlcoonFireballSegment, EnemyProjectileCommonCue>
{
    /// <summary>Creates an <see cref="AlcoonFireballAnimation"/>.</summary>
    /// <param name="segment">segment</param>
    public AlcoonFireballAnimation(AlcoonFireballSegment segment)
        : base(segment, AlcoonFireballAnimations.StartingAt[segment], EnemyProjectileCueRunners.Common<AlcoonFireballSegment>())
    {
    }

    /// <inheritdoc/>
    private protected override AnimationSegments<AlcoonFireballSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => AlcoonFireballAnimations.Segments;
}
