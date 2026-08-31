namespace SuperMetroid.Core.Game;

/// <summary>
/// Mother Brain bomb definition <c>$86:CB59</c>, including its shared-pool initializer,
/// normal-bomb destruction scan, staged bounce physics, and terminal replacement effects.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private static ReadOnlySpan<ushort> MotherBrainBombYAccelerations =>
        [0x0007, 0x0010, 0x0020, 0x0040, 0x0070, 0x00b0, 0x00f0, 0x0130, 0x0170, 0x0000];

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
            acceleration = 0x0007;
        }
        else
        {
            int accelerationIndex = bomb.Variable1 >> 1;
            if ((uint)accelerationIndex >= (uint)MotherBrainBombYAccelerations.Length)
            {
                throw new InvalidDataException(
                    $"Mother Brain bomb bounce-table offset ${bomb.Variable1:X4} is outside " +
                    "$86:C550-C563.");
            }

            acceleration = MotherBrainBombYAccelerations[accelerationIndex];
            if (acceleration == 0)
            {
                ExpireMotherBrainBomb(bomb, state);
                return;
            }
        }

        if (MoveMotherBrainBomb(bomb, acceleration))
            bomb.Variable1 = unchecked((ushort)(bomb.Variable1 + 2));
    }

    private bool TryDestroyMotherBrainBombWithSamusBomb(
        RoomEnemyProjectileSlot bomb,
        MotherBrainEnemyState state,
        SamusBombProjectileSystem? samusBombs)
    {
        if (samusBombs is null || samusBombs.BombCounter == 0)
            return false;

        foreach (SamusBombProjectileSlot samusBomb in samusBombs.Slots)
        {
            if ((samusBomb.Type & 0x0f00) != SamusBombProjectileSystem.NormalBombType ||
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
            state.LastBombDropRequest = new MotherBrainBombDropRequest(
                x,
                y,
                state.Head!.EnemyDefinitionPointer);
            SpawnEnemyDropFromEnemyHeader(
                x,
                y,
                state.Head.EnemyDefinitionPointer);
            return true;
        }

        return false;
    }

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

    private static void DeleteMotherBrainBomb(
        RoomEnemyProjectileSlot bomb,
        MotherBrainEnemyState state)
    {
        state.BombCounter = unchecked((ushort)(state.BombCounter - 1));
        bomb.XVelocity = 0;
        bomb.YVelocity = 0;
        bomb.Clear();
    }

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
