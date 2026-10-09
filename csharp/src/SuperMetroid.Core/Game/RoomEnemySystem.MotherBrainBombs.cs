namespace SuperMetroid.Core.Game;

/// <summary>
/// Mother Brain bomb definition <c>$86:CB59</c>, including its shared-pool initializer,
/// normal-bomb destruction scan, staged bounce physics, and terminal replacement effects.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Ports initializer <c>$86:C482-C4C7</c>.</summary>
    private void SpawnMotherBrainBomb(
        MotherBrainEnemyState state,
        ushort afterburnCount)
    {
        RoomEnemyProjectileSlot? bomb = AllocateEnemyProjectile();
        if (bomb is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            bomb,
            RoomEnemyProjectileKind.MotherBrainBomb,
            graphicsIndex: 0x0400);
        RoomEnemySlot head = state.Head!;
        bomb.XPosition = unchecked((ushort)(head.XPosition + 0x000c));
        bomb.YPosition = unchecked((ushort)(head.YPosition + 0x0010));

        // The initializer's 8-bit store writes parameter LOW into the low byte of X
        // subposition. Every 8.8 motion delta has a zero low byte, so that value naturally
        // survives fractional motion and becomes the terminal Ridley-afterburn count.
        bomb.XSubposition = unchecked((byte)afterburnCount);
        bomb.YSubposition = 0;
        bomb.XVelocity = 0x00e0;
        bomb.YVelocity = 0x0100;
        bomb.Variable0 = 0x0070; // Horizontal speed restored after each floor bounce.
        bomb.Variable1 = 0;      // Byte offset into the ten-word acceleration table.
        bomb.DirectionParameter = afterburnCount;
        state.BombCounter = unchecked((ushort)(state.BombCounter + 1));
    }

    /// <summary>Ports pre-instruction <c>$86:C4C8-C604</c>.</summary>
    private void RunMotherBrainBombPreInstruction(
        RoomEnemyProjectileSlot bomb,
        SamusBombProjectileSystem? samusBombs)
    {
        MotherBrainEnemyState state = _motherBrain ?? throw new InvalidOperationException(
            "Mother Brain bomb ran without its multipart encounter state.");
        if (TryDestroyMotherBrainBombWithSamusBomb(bomb, state, samusBombs))
            return;

        ushort acceleration;
        if (bomb.Variable1 == 0)
        {
            // Only the initial fall applies two units of horizontal drag. Native takes the
            // absolute value, subtracts, clamps by the wrapped sign flag, then restores sign.
            bool movingLeft = (bomb.XVelocity & 0x8000) != 0;
            ushort magnitude = movingLeft
                ? unchecked((ushort)-bomb.XVelocity)
                : bomb.XVelocity;
            ushort slowed = unchecked((ushort)(magnitude - 2));
            if ((slowed & 0x8000) != 0)
                slowed = 0;
            bomb.XVelocity = movingLeft ? unchecked((ushort)-slowed) : slowed;
            acceleration = MotherBrainBombBounceDefinitions.FallAcceleration;
        }
        else
        {
            int accelerationIndex = bomb.Variable1 >> 1;
            if ((uint)accelerationIndex >= MotherBrainBombBounceDefinitions.StageCount)
            {
                throw new InvalidDataException(
                    $"Mother Brain bomb bounce-table offset ${bomb.Variable1:X4} is outside " +
                    "$86:C550-C563.");
            }

            acceleration = MotherBrainBombBounceDefinitions.YAcceleration(accelerationIndex);
            if (acceleration == 0)
            {
                ExpireMotherBrainBomb(bomb, state);
                return;
            }
        }

        if (MoveMotherBrainBomb(bomb, acceleration))
            bomb.Variable1 = unchecked((ushort)(bomb.Variable1 + 2));
    }

    /// <summary>Checks armed Samus bombs for a strict overlap and replaces a destroyed Mother Brain bomb with its drop effects.</summary>
    /// <param name="bomb">Mother Brain bomb currently running its pre-instruction.</param>
    /// <param name="state">Encounter state updated when the bomb is destroyed.</param>
    /// <param name="samusBombs">Active Samus bomb set, or null when no bomb projectiles are available.</param>
    /// <returns><see langword="true"/> when an armed Samus bomb destroys this bomb and its replacement effects are spawned.</returns>
    private bool TryDestroyMotherBrainBombWithSamusBomb(
        RoomEnemyProjectileSlot bomb,
        MotherBrainEnemyState state,
        SamusBombProjectileSystem? samusBombs)
    {
        if (samusBombs is null || samusBombs.BombCounter == 0)
            return false;

        foreach (SamusBombProjectileSlot samusBomb in samusBombs.Slots)
        {
            if (samusBomb.PackedType.Family != SamusProjectileFamily.Bomb ||
                samusBomb.BombTimer != 0 ||
                !MotherBrainBombStrictOverlap(
                    bomb.XPosition,
                    bomb.YPosition,
                    bomb.XRadius,
                    bomb.YRadius,
                    samusBomb.XPosition,
                    samusBomb.YPosition,
                    samusBomb.XRadius,
                    samusBomb.YRadius))
            {
                continue;
            }

            ushort x = bomb.XPosition;
            ushort y = bomb.YPosition;
            DeleteMotherBrainBomb(bomb, state);
            SpawnRoomGraphicsDustExplosion(x, y, animationIndex: 9);
            state.LastBombDropRequest = new MotherBrainBombDropRequest();
            SpawnEnemyDropFromEnemyHeader(
                x,
                y,
                state.Head!.EnemyDefinitionPointer);
            return true;
        }

        return false;
    }

    /// <summary>Applies one accelerated 8.8 motion step, reflects horizontal motion, and resolves floor bounce state.</summary>
    /// <param name="bomb">Projectile whose position and velocities are advanced in place.</param>
    /// <param name="acceleration">Y-velocity increment for this fall or bounce stage.</param>
    /// <returns><see langword="true"/> when the bomb reaches the floor and its bounce velocities are reset.</returns>
    private static bool MoveMotherBrainBomb(
        RoomEnemyProjectileSlot bomb,
        ushort acceleration)
    {
        bomb.YVelocity = unchecked((ushort)(bomb.YVelocity + acceleration));
        (bomb.XPosition, bomb.XSubposition) = AddEightBitVelocity(
            bomb.XPosition,
            bomb.XSubposition,
            bomb.XVelocity);
        (bomb.YPosition, bomb.YSubposition) = AddEightBitVelocity(
            bomb.YPosition,
            bomb.YSubposition,
            bomb.YVelocity);

        // Both boundaries are signed CMP/BMI tests. X reflects without clamping; the floor
        // clamps Y to $D0 and resets the two authored bounce velocities.
        if (unchecked((short)(bomb.XPosition - 0x00f0)) >= 0)
            bomb.XVelocity = unchecked((ushort)-bomb.XVelocity);
        if (unchecked((short)(bomb.YPosition - 0x00d0)) < 0)
            return false;

        bomb.YPosition = 0x00d0;
        bomb.XVelocity = (bomb.XVelocity & 0x8000) != 0
            ? unchecked((ushort)-bomb.Variable0)
            : bomb.Variable0;
        bomb.YVelocity = 0xfe00;
        return true;
    }

    /// <summary>Deletes an exhausted bomb, then emits its Ridley afterburn, dust burst, and sound request.</summary>
    /// <param name="bomb">Expired projectile whose position and afterburn count are retained for replacement effects.</param>
    /// <param name="state">Encounter state whose bomb count and sound request are updated.</param>
    private void ExpireMotherBrainBomb(
        RoomEnemyProjectileSlot bomb,
        MotherBrainEnemyState state)
    {
        ushort x = bomb.XPosition;
        ushort y = bomb.YPosition;
        byte afterburnCount = unchecked((byte)bomb.XSubposition);
        DeleteMotherBrainBomb(bomb, state);

        // `$C50B-$C54B` clears the source slot before both allocations. They therefore use
        // the normal descending shared-pool competition, including legitimate same-slot
        // replacement that the outer scheduler will continue processing on this pass.
        SpawnAfterburnCenter(
            RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter,
            x,
            y,
            afterburnCount);
        SpawnRoomGraphicsDustExplosion(x, y, animationIndex: 3);
        state.LastSoundEffectLibrary3 = 0x0013;
    }

    /// <summary>Removes the bomb from the active projectile pool and updates the encounter's live bomb count.</summary>
    /// <param name="bomb">Projectile slot to stop and clear.</param>
    /// <param name="state">Encounter state whose active bomb counter is decremented.</param>
    private static void DeleteMotherBrainBomb(
        RoomEnemyProjectileSlot bomb,
        MotherBrainEnemyState state)
    {
        state.BombCounter = unchecked((ushort)(state.BombCounter - 1));
        bomb.XVelocity = 0;
        bomb.YVelocity = 0;
        bomb.Clear();
    }

    /// <summary>Tests axis-aligned overlap using wrapped coordinate distances and strict radius boundaries.</summary>
    /// <param name="firstX">Horizontal center of the first projectile.</param>
    /// <param name="firstY">Vertical center of the first projectile.</param>
    /// <param name="firstXRadius">Horizontal radius of the first projectile.</param>
    /// <param name="firstYRadius">Vertical radius of the first projectile.</param>
    /// <param name="secondX">Horizontal center of the second projectile.</param>
    /// <param name="secondY">Vertical center of the second projectile.</param>
    /// <param name="secondXRadius">Horizontal radius of the second projectile.</param>
    /// <param name="secondYRadius">Vertical radius of the second projectile.</param>
    /// <returns><see langword="true"/> only when both wrapped axis distances are smaller than the summed radii.</returns>
    private static bool MotherBrainBombStrictOverlap(
        ushort firstX,
        ushort firstY,
        ushort firstXRadius,
        ushort firstYRadius,
        ushort secondX,
        ushort secondY,
        ushort secondXRadius,
        ushort secondYRadius) =>
        WrappedMagnitude(unchecked((ushort)(firstX - secondX))) <
            firstXRadius + secondXRadius &&
        WrappedMagnitude(unchecked((ushort)(firstY - secondY))) <
            firstYRadius + secondYRadius;
}
