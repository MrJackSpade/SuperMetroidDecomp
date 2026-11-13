namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The engine-owned collision of all three ordinary Pirate families. Editable component offsets in the installed visual
/// asset cannot move these rectangles or replace their touch/shot reactions.
/// </summary>
internal static class SpacePirateHitboxes
{
    /// <summary>The reaction of the first Pirate hitbox the probe overlaps.</summary>
    internal static bool Find(RoomEnemySlot enemy, EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        foreach (EnemyCollisionComponent component in
                 SpacePirateCollisionDefinitions.Frames.Of(enemy.Frame))
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            IReadOnlyList<EnemyCollisionHitbox> hitboxes = component.Hitboxes;
            for (int index = 0; index < hitboxes.Count; index++)
            {
                EnemyCollisionHitbox hitbox = hitboxes[index];
                int left = CoordinateAxis.Wrap(componentOrigin.X + hitbox.Left);
                int top = CoordinateAxis.Wrap(componentOrigin.Y + hitbox.Top);
                int right = CoordinateAxis.Wrap(componentOrigin.X + hitbox.Right);
                int bottom = CoordinateAxis.Wrap(componentOrigin.Y + hitbox.Bottom);
                if (!probe.Overlaps(left, top, right, bottom))
                    continue;
                reaction = probe.SelectsShot ? hitbox.Shot : hitbox.Touch;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Whether <paramref name="reaction"/> is one of the Pirate shot reactions the hitboxes select, which normal bombs
    /// dispatch like the common one.
    /// </summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    internal static bool OwnsShotReaction(EnemyReaction? reaction) =>
        reaction is EnemyReaction.SpacePirateShot or
            EnemyReaction.GoldNinjaVulnerableHitboxShot or
            EnemyReaction.GoldNinjaInvincibleHitboxShot;
}
