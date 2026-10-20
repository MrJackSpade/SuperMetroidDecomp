namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The engine-owned collision of all three ordinary Pirate families. Editable component offsets in the installed visual
/// asset cannot move these rectangles or replace their native touch/shot callbacks.
/// </summary>
internal static class SpacePirateHitboxes
{
    /// <summary>The callback of the first Pirate hitbox the probe overlaps.</summary>
    internal static bool Find(RoomEnemySlot enemy, EnemyHitboxProbe probe, out EnemyInteractionCallback? callback)
    {
        callback = null;
        foreach (SpacePirateCollisionComponent component in
                 SpacePirateCollisionDefinitions.Frames.Of(enemy.Frame))
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            IReadOnlyList<SpacePirateCollisionHitbox> hitboxes = component.Hitboxes;
            for (int index = 0; index < hitboxes.Count; index++)
            {
                SpacePirateCollisionHitbox hitbox = hitboxes[index];
                ushort left = unchecked((ushort)(componentOrigin.X + hitbox.Left));
                ushort top = unchecked((ushort)(componentOrigin.Y + hitbox.Top));
                ushort right = unchecked((ushort)(componentOrigin.X + hitbox.Right));
                ushort bottom = unchecked((ushort)(componentOrigin.Y + hitbox.Bottom));
                if (!probe.Overlaps(left, top, right, bottom))
                    continue;
                callback = probe.SelectShotCallback
                    ? SpacePirateHitboxCallbackCodec.ToCallback(hitbox.Shot)
                    : SpacePirateHitboxCallbackCodec.ToCallback(hitbox.Touch);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Whether <paramref name="callback"/> is one of the Pirate shot callbacks the hitboxes select, which normal bombs
    /// dispatch like the common one.
    /// </summary>
    /// <param name="callback">The named callback the selected pointer decodes to, or null.</param>
    internal static bool OwnsShotCallback(EnemyInteractionCallback? callback) =>
        callback is EnemyInteractionCallback.SpacePirateShot or
            EnemyInteractionCallback.GoldNinjaVulnerableHitboxShot or
            EnemyInteractionCallback.GoldNinjaInvincibleHitboxShot;
}
