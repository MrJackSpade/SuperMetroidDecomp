namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The engine-owned collision of all three ordinary Pirate families. Editable component offsets in the installed visual
/// asset cannot move these rectangles or replace their touch/shot reactions.
/// </summary>
internal static class SpacePirateHitboxes
{
    /// <summary>The reaction of the first Pirate hitbox the probe overlaps.</summary>
    internal static bool Find(RoomEnemyPlace enemy, EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        Position16<RoomPixels> origin = enemy.Position.Whole;
        foreach (EnemyCollisionHitbox hitbox in SpacePirateCollisionDefinitions.Frames.Of(enemy.Frame))
        {
            int left = CoordinateAxis.Wrap(origin.X + hitbox.Left);
            int top = CoordinateAxis.Wrap(origin.Y + hitbox.Top);
            int right = CoordinateAxis.Wrap(origin.X + hitbox.Right);
            int bottom = CoordinateAxis.Wrap(origin.Y + hitbox.Bottom);
            if (!probe.Overlaps(left, top, right, bottom))
                continue;
            reaction = probe.SelectsShot ? hitbox.Shot : hitbox.Touch;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Whether <paramref name="reaction"/> is one of the Pirate shot reactions the hitboxes select, which a normal bomb
    /// triggers as it does the common one.
    /// </summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    internal static bool OwnsShotReaction(EnemyReaction? reaction) =>
        (reaction == SpacePirateReactions.Shot || reaction == SpacePirateReactions.GoldNinjaVulnerableHitboxShot || reaction == SpacePirateReactions.GoldNinjaInvincibleHitboxShot);
}
