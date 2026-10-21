using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
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
    internal static bool IsRidleyDefinition(EnemyDefinitionId definitionPointer) =>
        definitionPointer is EnemyDefinitionId.RidleyCeres or EnemyDefinitionId.Ridley;

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

        // Norfair and Ceres select the same authored $A6:E46A health rows. Keep
        // Norfair's health thresholds here, but use the installed palette so
        // editing that one asset updates both encounters consistently.
        int row = state.HealthStage - 1;
        (_bossArtwork.CeresRidleyColors ?? throw new InvalidOperationException(
            "Ridley damage requires installed health colors."))
            .ApplyHealth(_presentation.Cgram, row);
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
            if (RidleyCollisionDefinitions.IsOutsideInteractionWindow(body.Position.Whole, camera))
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
    internal static void RegisterRidleyShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Ridley &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.RidleyShot)
        {
            BeforeSuperMissileQuake = (in EnemyShotContext c) =>
            {
                if (!c.UsesExtendedHitboxes || c.HitboxShotAi != EnemyInteractionCallback.RidleyShot)
                {
                    throw new InvalidDataException(
                        $"Lower Norfair Ridley requires extended shot AI " +
                        $"$A6:{EnemyAiCodePointers.BankA6.RidleyShot:X4}; extended={c.UsesExtendedHitboxes}, " +
                        $"selected={c.HitboxShotAi?.ToString() ?? "(unnamed)"}, map=$A6:{c.Enemy.Frame}.");
                }
                return false;
            },
            ExemptFromGenericDeath = true,
        });
    }

    /// <summary>Routes bombs through Ridley's native shot callback without generic death.</summary>
    internal static void RegisterRidleyBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Ridley &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.RidleyShot)
        {
            RunsGenericDeath = false,
        });
    }

    /// <summary>The callback of the first Ridley hitbox the probe overlaps, or null outside its compiled frames.</summary>
    internal static bool? FindRidleyHitboxCallback(RoomEnemySlot enemy, EnemyHitboxProbe probe, out EnemyInteractionCallback? callback)
    {
        callback = null;
        if (!(RidleyInteractions.IsRidleyDefinition(enemy.EnemyDefinitionPointer) &&
            enemy.Definition.Bank == RidleyCollisionDefinitions.Bank &&
            (RidleyCollisionDefinitions.HasFrame(EnemyFrames.Spritemap(enemy.Frame)) ||
            probe.ArtworkInstalled)))
            return null;

        // The OAM component offsets in an installed composition are editable.
        // Contact and shot collision instead use the eleven fixed cartridge
        // body frames and their engine-owned hitbox lists.
        foreach (RidleyCollisionComponent component in
                 RidleyCollisionDefinitions.ComponentsAt(EnemyFrames.Spritemap(enemy.Frame)))
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            ushort componentX = componentOrigin.X;
            ushort componentY = componentOrigin.Y;
            foreach (EnemyHitboxRectangle hitbox in component.Hitboxes)
            {
                ushort left = unchecked((ushort)(componentX + hitbox.Left));
                ushort top = unchecked((ushort)(componentY + hitbox.Top));
                ushort right = unchecked((ushort)(componentX + hitbox.Right));
                ushort bottom = unchecked((ushort)(componentY + hitbox.Bottom));
                if (!probe.Overlaps(left, top, right, bottom))
                    continue;
                callback = RidleyHitboxCallbackCodec.ToCallback(probe.SelectShotCallback);
                return true;
            }
        }
        return false;
    
    }

    /// <summary>Ridley's Power Bomb reaction, exempt from the generic death.</summary>
    internal static void RegisterRidleyPowerBombReactions(EnemySharedReactions table) =>
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.EnemyDefinitionPointer is EnemyDefinitionId.Ridley or EnemyDefinitionId.RidleyCeres &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.RidleyPowerBomb)
        {
            ExemptFromGenericDeath = true,
        });
}
