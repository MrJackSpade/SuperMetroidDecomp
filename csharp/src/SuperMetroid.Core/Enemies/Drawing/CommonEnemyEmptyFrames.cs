using SuperMetroid.Core.Enemies.Artwork;
using static SuperMetroid.Core.Enemies.Drawing.CommonEnemyEmptyFrameDefinitions;

namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>
/// What the empty frame means for an enemy showing it: it draws nothing, and an enemy that collides by its frames'
/// hitboxes still collides at the single point at its origin while it shows it.
/// </summary>
internal static class CommonEnemyEmptyFrames
{
    /// <summary>Whether an actor showing <paramref name="frame"/> draws nothing: no frame yet, or the empty frame.</summary>
    internal static bool DrawsNothing(EnemyFrameId? frame) => frame is not { } shown || EmptyFrames.Contains(shown);

    /// <summary>
    /// Whether <paramref name="enemy"/> collides at the single point at its origin: it collides by its frames' hitboxes and
    /// shows the empty frame, whose one hitbox is that point. Samus and her bombs meet the point; her other shots pass it.
    /// </summary>
    internal static bool CollidesAtOrigin(RoomEnemySlot enemy) =>
        enemy.CollidesByFrameHitboxes && enemy.Frame == EnemyFrameId.Empty;

    /// <summary>
    /// Whether Samus's shots pass <paramref name="enemy"/> before any hitbox is tested: it shows no frame, or the empty
    /// frame while it collides by its radius.
    /// </summary>
    internal static bool ShotsPass(RoomEnemySlot enemy) => DrawsNothing(enemy.Frame) && !CollidesAtOrigin(enemy);
}
