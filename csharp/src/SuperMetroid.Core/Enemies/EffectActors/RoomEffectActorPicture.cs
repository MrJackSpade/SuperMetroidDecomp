using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Enemies.EffectActors;

/// <summary>
/// A picture a room effect actor shows (#1817): a frame of its enemy sheet, or, for the burst Samus's hit shows at an
/// enemy, one of her hit-burst sprites, which only her projectile sheets picture (#1713).
/// </summary>
internal readonly record struct RoomEffectActorPicture
{
    /// <summary>The picture of the enemy sheet's frame <paramref name="enemyFrame"/>.</summary>
    private RoomEffectActorPicture(EnemyFrameId enemyFrame) => this.enemyFrame = enemyFrame;

    /// <summary>The picture of Samus's hit-burst sprite <paramref name="hitBurst"/>.</summary>
    private RoomEffectActorPicture(ProjectileSpriteId hitBurst)
    {
        isHitBurst = true;
        this.hitBurst = hitBurst;
    }

    /// <summary>Whether the picture is one of Samus's hit-burst sprites rather than a frame of the enemy sheet.</summary>
    private readonly bool isHitBurst;

    /// <summary>The enemy-sheet frame the picture is, for a picture on the enemy sheet.</summary>
    private readonly EnemyFrameId enemyFrame;

    /// <summary>The hit-burst sprite of Samus's the picture is, for a hit burst's picture.</summary>
    private readonly ProjectileSpriteId hitBurst;

    /// <summary>The picture of enemy-sheet frame <paramref name="frame"/>.</summary>
    internal static RoomEffectActorPicture OfEnemyFrame(EnemyFrameId frame) => new(frame);

    /// <summary>The picture of Samus's hit-burst sprite <paramref name="sprite"/>.</summary>
    internal static RoomEffectActorPicture OfHitBurst(ProjectileSpriteId sprite) => new(sprite);

    /// <summary>
    /// What <paramref name="onEnemySheet"/> makes of the picture's enemy-sheet frame, or for a hit burst's picture what
    /// <paramref name="asHitBurst"/> makes of its sprite.
    /// </summary>
    internal TResult Select<TResult>(Func<EnemyFrameId, TResult> onEnemySheet, Func<ProjectileSpriteId, TResult> asHitBurst) =>
        isHitBurst ? Ensure.NotNull(asHitBurst)(hitBurst) : Ensure.NotNull(onEnemySheet)(enemyFrame);

    /// <inheritdoc/>
    public override string ToString() => isHitBurst ? $"hit burst {hitBurst}" : $"enemy frame {enemyFrame}";
}
