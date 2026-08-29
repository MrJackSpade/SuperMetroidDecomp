namespace SuperMetroid.Core.Game;

/// <summary>
/// Spore Spawn's private tails around bank-$A0 common collision. Vulnerability damage and
/// Samus knockback stay in the shared engine; this file owns only the boss-specific motion,
/// palette, dud-hitbox, and death side effects that the cartridge executes afterward.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort SporeSpawnCloseAndMoveInstruction = 0xe729;

    private static bool SporeSpawnAcceptsProjectile(ushort projectileType) =>
        (projectileType & 0x0700) != 0 || (projectileType & 0x0010) != 0;

    /// <summary>Ports the post-common body of <c>SporeSpawn_Shot</c> at $A5:ED5A.</summary>
    private void ResolveSporeSpawnShotAfterCommon(RoomEnemySlot body)
    {
        SporeSpawnEnemyState state = RequireSporeSpawnState(body);
        if (body.FlashTimer != 0)
        {
            state.Function = SporeSpawnFunction.Moving;

            // Below 400 health the native code doubles angular speed while preserving its
            // current sign. At exactly 400 it leaves the existing delta untouched.
            if (unchecked((short)(body.Health - 400)) < 0)
            {
                state.AngleDelta = (state.AngleDelta & 0x8000) != 0
                    ? unchecked((ushort)-2)
                    : (ushort)2;
            }

            // Only the first damaging hit during an open phase reverses direction and
            // closes the head. Opcode $E771 clears this guard when the list permits it.
            if (state.DamagedFlag == 0)
            {
                state.AngleDelta = unchecked((ushort)-state.AngleDelta);
                state.DamagedFlag = 1;
                body.CurrentInstruction = SporeSpawnCloseAndMoveInstruction;
                body.InstructionTimer = 1;

                ushort paletteOffset = body.Health >= 770
                    ? (ushort)0
                    : body.Health >= 410
                        ? (ushort)32
                        : body.Health >= 70
                            ? (ushort)64
                            : (ushort)96;
                if (body.Health != state.PreviousHealth)
                {
                    state.PreviousHealth = body.Health;
                    LoadSporeSpawnHealthPalette(paletteOffset);
                }
            }
        }

        ResolveSporeSpawnDeathAfterCommon(body, state);
    }

    /// <summary>Ports <c>SporeSpawn_Func_6</c> at $A5:EDF3.</summary>
    private void ResolveSporeSpawnDeathAfterCommon(
        RoomEnemySlot body,
        SporeSpawnEnemyState? suppliedState = null)
    {
        if (body.Health != 0)
            return;
        SporeSpawnEnemyState state = suppliedState ?? RequireSporeSpawnState(body);
        if (state.DeathStarted)
            return;

        state.DeathStarted = true;
        state.SporeGenerationFlag = 0;
        body.InvincibilityTimer = 0;
        body.FlashTimer = 0;
        body.AiHandlerBits = 0;
        // Literal property $0400 removes the dead body from the interactive-enemy list.
        body.Properties = unchecked((ushort)(body.Properties | 0x0400));

        // The 65C816 loop clears native projectile indexes $1A..$00: physical slots 13..0.
        // Slots 14..17 are the stalk and deliberately survive for the death animation.
        for (int projectileSlot = 0; projectileSlot <= 13; projectileSlot++)
            _enemyProjectiles[projectileSlot].Clear();

        body.CurrentInstruction = SporeSpawnDeathInstruction;
        body.InstructionTimer = 1;
        _setAreaMiniBossDefeated?.Invoke();
        state.ScrollClampHookActive = false;
        PublishSporeSpawnPlm(header: 0xb78f);
    }

    /// <summary>Ports common bank-$A0 <c>CreateADudShot</c> for a protected head hitbox.</summary>
    private void CreateSporeSpawnDudShot(SamusProjectileSlot projectile)
    {
        _ = SpawnRoomSpriteObject(
            projectile.XPosition,
            projectile.YPosition,
            RoomSpriteObjectKind.EnemyProjectileDud,
            graphicsIndex: 0);
        LastEnemyProjectileDudSoundEffect = 0x003d;
        projectile.Direction = unchecked((ushort)(projectile.Direction | 0x0010));
    }
}
