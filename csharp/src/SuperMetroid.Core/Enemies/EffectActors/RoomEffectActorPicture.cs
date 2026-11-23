using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Enemies.EffectActors;

/// <summary>
/// A picture a room effect actor shows (#1817): a frame of its enemy sheet, or, for the burst Samus's hit shows at an
/// enemy, one of her hit-burst sprites, which only her projectile sheets picture (#1713).
/// </summary>
internal readonly partial record struct RoomEffectActorPicture
{
    /// <summary>The one picture the actor shows (#2560).</summary>
    private readonly Picture picture;

    /// <summary>Holds the actor's picture.</summary>
    private RoomEffectActorPicture(Picture picture) => this.picture = picture;

    /// <summary>The enemy-sheet frame the picture is, or null for a hit burst's.</summary>
    internal EnemyFrameId? EnemyFrame => picture is EnemySheetPicture enemy ? enemy.Frame : null;

    /// <summary>The hit-burst sprite of Samus's the picture is, or null for an enemy-sheet frame.</summary>
    internal ProjectileSpriteId? HitBurst => picture is HitBurstPicture burst ? burst.Sprite : null;

    /// <summary>The picture of enemy-sheet frame <paramref name="frame"/>.</summary>
    internal static RoomEffectActorPicture OfEnemyFrame(EnemyFrameId frame) => new(new EnemySheetPicture(frame));

    /// <summary>The picture of Samus's hit-burst sprite <paramref name="sprite"/>.</summary>
    internal static RoomEffectActorPicture OfHitBurst(ProjectileSpriteId sprite) => new(new HitBurstPicture(sprite));

    /// <summary>An effect actor's picture (#2560): a closed choice of an enemy-sheet frame and Samus's hit-burst sprite.</summary>
    private abstract partial record Picture;

    /// <summary>A frame of an enemy sheet.</summary>
    /// <param name="Frame">The frame.</param>
    private sealed partial record EnemySheetPicture(EnemyFrameId Frame) : Picture;

    /// <summary>Samus's hit-burst sprite.</summary>
    /// <param name="Sprite">The sprite.</param>
    private sealed partial record HitBurstPicture(ProjectileSpriteId Sprite) : Picture;
}
