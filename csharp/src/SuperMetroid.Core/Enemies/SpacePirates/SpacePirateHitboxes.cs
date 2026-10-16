namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The engine-owned collision of all three ordinary Pirate families. Editable component offsets in the installed visual
/// asset cannot move these rectangles or replace their native touch/shot callbacks.
/// </summary>
internal static class SpacePirateHitboxes
{
    /// <summary>The callback of the first Pirate hitbox the probe overlaps.</summary>
    internal static bool Find(RoomEnemySlot enemy, EnemyHitboxProbe probe, out ushort callback)
    {
        callback = 0;
        foreach (SpacePirateCollisionComponent component in
                 SpacePirateCollisionDefinitions.ComponentsAt(
                     enemy.SpritemapPointer))
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            foreach (SpacePirateCollisionHitbox hitbox in
                     SpacePirateCollisionDefinitions.HitboxesAt(
                         component.HitboxPointer))
            {
                ushort left = unchecked((ushort)(componentOrigin.X + hitbox.Left));
                ushort top = unchecked((ushort)(componentOrigin.Y + hitbox.Top));
                ushort right = unchecked((ushort)(componentOrigin.X + hitbox.Right));
                ushort bottom = unchecked((ushort)(componentOrigin.Y + hitbox.Bottom));
                if (!probe.Overlaps(left, top, right, bottom))
                    continue;
                callback = probe.SelectShotCallback ? hitbox.ShotAi : hitbox.TouchAi;
                return true;
            }
        }
        return false;
    
    }
}
