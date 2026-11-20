using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Enemies.EffectActors;

/// <summary>
/// A picture a room effect actor shows (#1817): a frame of its enemy sheet, or, for the burst Samus's hit shows at an
/// enemy, one of her hit-burst sprites, which only her projectile sheets picture (#1713).
/// </summary>
internal readonly record struct RoomEffectActorPicture
{
    /// <summary>A picture of one kind or the other, never both.</summary>
    private RoomEffectActorPicture(EnemyFrameId? enemyFrame, ProjectileSpriteId? hitBurst)
    {
        EnemyFrame = enemyFrame;
        HitBurst = hitBurst;
    }

    /// <summary>The enemy-sheet frame the picture is, or null for a hit burst's.</summary>
    internal EnemyFrameId? EnemyFrame { get; }

    /// <summary>The hit-burst sprite of Samus's the picture is, or null for an enemy-sheet frame.</summary>
    internal ProjectileSpriteId? HitBurst { get; }

    /// <summary>The picture of enemy-sheet frame <paramref name="frame"/>.</summary>
    internal static RoomEffectActorPicture OfEnemyFrame(EnemyFrameId frame) => new(frame, null);

    /// <summary>The picture of Samus's hit-burst sprite <paramref name="sprite"/>.</summary>
    internal static RoomEffectActorPicture OfHitBurst(ProjectileSpriteId sprite) => new(null, sprite);

    /// <summary>The enemy-sheet frame the picture is.</summary>
    /// <exception cref="InvalidOperationException">The picture is one of Samus's hit-burst sprites, on no enemy sheet.</exception>
    internal EnemyFrameId RequireEnemyFrame() => EnemyFrame
        ?? throw new InvalidOperationException($"Room effect-actor picture {HitBurst} is Samus's hit burst, on no enemy sheet.");
}
