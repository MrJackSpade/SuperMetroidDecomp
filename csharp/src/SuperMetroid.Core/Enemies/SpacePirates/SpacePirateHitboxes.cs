namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The collision the game defines for all three ordinary Pirate families. Editable component offsets in the installed visual
/// asset cannot move these rectangles or replace their touch/shot reactions.
/// </summary>
internal static class SpacePirateHitboxes
{
    /// <summary>The reaction of the first Pirate hitbox the probe overlaps.</summary>
    internal static EnemyHitboxContact Find(RoomEnemyPlace enemy, EnemyHitboxProbe probe)
    {
        Position16<RoomPixels> origin = enemy.Position.Whole;
        foreach (EnemyCollisionHitbox hitbox in SpacePirateCollisionDefinitions.Frames.Of(enemy.Frame))
        {
            int left = origin.X + hitbox.Left;
            int top = origin.Y + hitbox.Top;
            int right = origin.X + hitbox.Right;
            int bottom = origin.Y + hitbox.Bottom;
            if (!probe.Overlaps(left, top, right, bottom))
                continue;
            return EnemyHitboxContact.Struck(probe.SelectsShot ? hitbox.Shot : hitbox.Touch);
        }
        return EnemyHitboxContact.Missed;
    }

    /// <summary>
    /// Whether <paramref name="reaction"/> is one of the Pirate shot reactions the hitboxes select, which a normal bomb
    /// triggers as it does the common one.
    /// </summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names.</param>
    internal static bool OwnsShotReaction(EnemyReaction reaction) => SpacePirateReactions.Own.Contains(reaction);
}
