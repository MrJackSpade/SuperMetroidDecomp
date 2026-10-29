using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ridley's hitboxes, weapon reactions, intangibility and health palette.</summary>
[RoomLifetime]
internal sealed class RidleyInteractions
{
    /// <summary>Publishes Ridley's health and transition palettes.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal RidleyInteractions(EnemyPresentation presentation, BossArtworkBinding bossArtwork)
    {
        _bossArtwork = bossArtwork;
        _presentation = presentation;
    }

    /// <summary>Includes both Ceres and Lower Norfair Ridley definition records.</summary>
    internal static bool IsRidleyDefinition(EnemyDefinitionId definitionId) =>
        definitionId is EnemyDefinitionId.RidleyCeres or EnemyDefinitionId.Ridley;

    /// <summary>Selects three damage-color rows at 9000, 5400, and 1800 health.</summary>
    internal void UpdateNorfairRidleyHealthPalette(RoomEnemySlot slot, RidleyEnemyState state)
    {
        state.HealthStage = slot.Health switch
        {
            < 1800 => 3,
            < 5400 => 2,
            < 9000 => 1,
            _ => 0,
        };
        if (state.HealthStage == 0)
            return;

        // Norfair and Ceres select the same authored $A6:E46A health bands. Keep
        // Norfair's health thresholds here, but use the installed palette so
        // editing that one asset updates both encounters consistently.
        int band = state.HealthStage - 1;
        (_bossArtwork.CeresRidleyColors ?? throw new InvalidOperationException(
            "Ridley damage requires installed health colors."))
            .ApplyHealth(_presentation.Colours, band);
    }

    /// <summary>
    /// Ports the collision-relevant portion of <c>HandleRidleySamusInteractionBit</c> at
    /// $A6:BCB4. Off-screen bodies disable collision and suspend the release timer.
    /// On-screen fight AI clears the gate before ticking that timer. EnemyMain has
    /// already consumed the previous gate value for this update's body collision.
    /// </summary>
    internal static void UpdateNorfairRidleyIntangibility(
        RoomEnemySlot body,
        RidleyEnemyState state,
        Position16<RoomPixels> camera)
    {
        if (state.FightMode == RidleyFightMode.Dying)
            return;
        if (state.FightMode != RidleyFightMode.NotFighting)
        {
            if (RidleyCollision.IsOutsideInteractionWindow(body.Position.Whole, camera))
            {
                body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
                return;
            }
            body.Properties = body.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        }
        if (!state.IntangibilityTimer.IsZero)
        {
            state.IntangibilityTimer = state.IntangibilityTimer.Decrement().Remaining;
            if (state.IntangibilityTimer.IsZero)
                body.Properties = body.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        }
    }

    /// <summary>Requires the extended Ridley hitbox callback and suppresses generic death on shots.</summary>
    internal static void AddRidleyShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Ridley &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.RidleyShot)
        {
            BeforeSuperMissileQuake = (in EnemyShotContext c) =>
            {
                if (!c.UsesExtendedHitboxes || c.HitboxShotAi != EnemyInteractionCallback.RidleyShot)
                {
                    throw new InvalidDataException(
                        $"Lower Norfair Ridley requires extended shot AI " +
                        $"{EnemyInteractionCallback.RidleyShot}; extended={c.UsesExtendedHitboxes}, " +
                        $"selected={c.HitboxShotAi?.ToString() ?? "(unnamed)"}, map=$A6:{c.Enemy.Frame}.");
                }
                return false;
            },
            ExemptFromGenericDeath = true,
        });
    }

    /// <summary>Routes bombs through Ridley's native shot callback without generic death.</summary>
    internal static void AddRidleyBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.DefinitionId == EnemyDefinitionId.Ridley &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.RidleyShot)
        {
            RunsGenericDeath = false,
        });
    }

    /// <summary>The callback of the first Ridley hitbox the probe overlaps, or null outside its compiled frames.</summary>
    internal static bool? FindRidleyHitboxCallback(RoomEnemySlot enemy, EnemyHitboxProbe probe, out EnemyInteractionCallback? callback)
    {
        callback = null;
        if (!(RidleyInteractions.IsRidleyDefinition(enemy.DefinitionId) &&
            (RidleyCollision.HasFrame(enemy.Frame) ||
            probe.ArtworkInstalled)))
            return null;

        // The OAM component offsets in an installed composition are editable.
        // Contact and shot collision instead use the eleven fixed cartridge
        // body frames and their engine-owned hitbox lists.
        foreach (RidleyCollisionComponent component in
                 RidleyCollision.ComponentsAt(enemy.Frame))
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            foreach (EnemyHitboxRectangle hitbox in component.Hitboxes)
            {
                if (!probe.Overlaps(
                        unchecked((ushort)(componentOrigin.X + hitbox.Left)),
                        unchecked((ushort)(componentOrigin.Y + hitbox.Top)),
                        unchecked((ushort)(componentOrigin.X + hitbox.Right)),
                        unchecked((ushort)(componentOrigin.Y + hitbox.Bottom))))
                    continue;
                callback = RidleyHitboxCallbackCodec.ToCallback(probe.SelectShotCallback);
                return true;
            }
        }
        return false;
    
    }

    /// <summary>Ridley's Power Bomb reaction, exempt from the generic death.</summary>
    internal static void AddRidleyPowerBombReactions(EnemySharedReactions table) =>
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.DefinitionId is EnemyDefinitionId.Ridley or EnemyDefinitionId.RidleyCeres &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.RidleyPowerBomb)
        {
            ExemptFromGenericDeath = true,
        });
}
