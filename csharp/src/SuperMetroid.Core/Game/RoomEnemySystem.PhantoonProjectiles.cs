using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$86 projectile half of Phantoon's intro and ordinary flame patterns. These actors
/// occupy the shared eighteen-slot pool and retain their cartridge definition pointers,
/// collision properties, bytecode, radii, and integer sine/cosine rounding.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort PhantoonFlameDeleteInstruction = 0x97f8;
    private const ushort PhantoonStartingFlameWaitingPreInstruction =
        EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_PhantoonStartingFlames;
    private const ushort PhantoonStartingFlameOrbitPreInstruction =
        EnemyProjectileCodePointers.PreInst_EnemyProjectile_PhantoonStartingFlames_Activated;

    private bool SpawnPhantoonStartingFlame(RoomEnemySlot body, byte directionIndex)
    {
        RoomEnemyProjectileSlot? flame = AllocateEnemyProjectile();
        if (flame is null)
            return false;

        InitializeEnemyProjectileFromDefinition(
            flame,
            RoomEnemyProjectileKind.PhantoonStartingFlame,
            unchecked((ushort)(body.PaletteIndex | body.VramTilesIndex)));
        flame.XSubposition = 0;
        flame.YSubposition = 0;
        flame.XVelocity = 0;
        flame.YVelocity = 0;
        flame.Variable0 = unchecked((byte)(directionIndex * 0x20));
        PositionPhantoonFlameAroundBody(flame, body, flame.Variable0, radius: 0x30);
        flame.PreInstruction = PhantoonStartingFlameWaitingPreInstruction;
        return true;
    }

    /// <summary>
    /// Initializes destroyable flame $86:9C29. The high parameter byte chooses one of the
    /// four native producers: casual fall, rage split, vertical rain, or spiral.
    /// </summary>
    private bool SpawnPhantoonDestroyableFlame(RoomEnemySlot body, ushort parameter)
    {
        RoomEnemyProjectileSlot? flame = AllocateEnemyProjectile();
        if (flame is null)
            return false;

        InitializeEnemyProjectileFromDefinition(
            flame,
            RoomEnemyProjectileKind.PhantoonDestroyableFlame,
            unchecked((ushort)(body.PaletteIndex | body.VramTilesIndex)));
        flame.XSubposition = 0;
        flame.YSubposition = 0;
        flame.YVelocity = 0;
        byte type = unchecked((byte)(parameter >> 8));
        byte index = unchecked((byte)parameter);
        switch (type)
        {
            case 0:
                flame.XVelocity = 0;
                flame.XPosition = body.XPosition;
                flame.YPosition = unchecked((ushort)(body.YPosition + 32));
                flame.InstructionPointer =
                    EnemyProjectileInstructionLists.PhantoonCasualFlameFalling;
                flame.PreInstruction =
                    EnemyProjectileCodePointers.PreInst_EnemyProj_PhantoonDestroyableFlame_Casual_Falling;

                // $86:985F changes the spawn definition's $8028 properties to $2028.
                // The falling flame therefore cannot hurt Samus and cannot be shot until
                // it has struck the floor and changed into the bouncing form.
                flame.CanDamageSamus = false;
                flame.BlocksSamusProjectiles = false;
                break;

            case 2:
                flame.XVelocity = unchecked((ushort)(index >= 8
                    ? -PhantoonFlameMotionRomData.RageAngleStep : PhantoonFlameMotionRomData.RageAngleStep));
                flame.Variable0 = PhantoonFlameSpawnDefinitions.RageAngle(index);
                flame.XPosition = body.XPosition;
                flame.YPosition = unchecked((ushort)(body.YPosition + 32));
                flame.PreInstruction =
                    EnemyProjectileCodePointers.PreInst_EnemyProj_PhantoonDestroyableFlame_Enraged;
                break;

            case 4:
                // The low parameter byte is packed as dx: the low nibble is a column and
                // the high nibble is the delayed-fall timer divided by eight.
                byte column = unchecked((byte)(index & 0x0f));
                if (column > 8)
                    throw new InvalidDataException($"Phantoon rain column {column} exceeds eight.");
                flame.XVelocity = unchecked((ushort)((parameter & 0x00f0) >> 1));
                flame.XPosition = PhantoonFlameSpawnDefinitions.RainX(column);
                flame.YPosition = 40;
                flame.PreInstruction =
                    EnemyProjectileCodePointers.PreInst_EnemyProj_PhantoonDestroyableFlame_Rain;
                break;

            case 6:
                if (index > 7)
                    throw new InvalidDataException($"Phantoon spiral direction {index} exceeds seven.");
                flame.XVelocity = 128;
                flame.Variable0 = PhantoonFlameSpawnDefinitions.SpiralAngle(index);
                flame.XPosition = body.XPosition;
                flame.YPosition = unchecked((ushort)(body.YPosition + 16));
                flame.PreInstruction =
                    EnemyProjectileCodePointers.PreInst_EnemyProj_PhantoonDestroyableFlame_Spiral;
                break;

            default:
                throw new InvalidDataException(
                    $"Phantoon flame parameter ${parameter:X4} selects invalid type {type}.");
        }
        return true;
    }

    private void RunPhantoonStartingFlameWaiting(RoomEnemyProjectileSlot flame)
    {
        PhantoonEnemyState? state = _phantoonState;
        if (state is null || state.Body.VariableB == 0)
            return;
        flame.PreInstruction = PhantoonStartingFlameOrbitPreInstruction;
        flame.XVelocity = 180;
        flame.YVelocity = 48;
    }

    private void RunPhantoonStartingFlameOrbit(
        RoomEnemyProjectileSlot flame,
        byte nmiFrameCounter8)
    {
        if (_phantoonState is not { } state)
        {
            flame.Clear();
            return;
        }

        if (flame.XVelocity != 0)
        {
            flame.XVelocity = unchecked((ushort)(flame.XVelocity - 1));
        }
        else if ((nmiFrameCounter8 & 1) != 0)
        {
            flame.YVelocity = unchecked((ushort)(flame.YVelocity - 1));
            if (flame.YVelocity == 0)
            {
                flame.XPosition = state.Body.XPosition;
                flame.YPosition = unchecked((ushort)(state.Body.YPosition + 16));
                flame.InstructionPointer = PhantoonFlameDeleteInstruction;
                flame.InstructionTimer = 1;
                return;
            }
        }

        flame.Variable0 = unchecked((byte)(flame.Variable0 + 1));
        PositionPhantoonFlameAroundBody(
            flame,
            state.Body,
            flame.Variable0,
            unchecked((byte)flame.YVelocity));
    }

    private void RunPhantoonCasualFlameFalling(
        RoomEnemyProjectileSlot flame,
        RoomLevelData level)
    {
        flame.YVelocity = unchecked((ushort)(flame.YVelocity + 0x10));
        if (!MoveProjectileAxis(flame, level, horizontal: false))
            return;

        flame.CanDamageSamus = true;
        flame.BlocksSamusProjectiles = true;
        flame.PreInstruction =
            EnemyProjectileCodePointers.PreInst_EnemyProj_PhantoonDestroyableFlame_Casual_HitGround;
        flame.InstructionPointer = EnemyProjectileInstructionLists.PhantoonCasualFlameLanded;
        flame.InstructionTimer = 1;
        flame.Variable0 = 8;
        flame.YPosition = unchecked((ushort)(flame.YPosition + 8));
    }

    private static void RunPhantoonCasualFlameImpactPause(
        RoomEnemyProjectileSlot flame,
        byte nmiFrameCounter8)
    {
        ushort oldTimer = flame.Variable0;
        flame.Variable0 = unchecked((ushort)(flame.Variable0 - 1));
        if (oldTimer != 1 && unchecked((short)flame.Variable0) >= 0)
            return;

        flame.PreInstruction =
            EnemyProjectileCodePointers.PreInst_EnemyProj_PhantoonDestroyableFlame_Casual_Bouncing;
        flame.InstructionPointer = EnemyProjectileInstructionLists.PhantoonCasualFlameBouncing;
        flame.InstructionTimer = 1;
        flame.YPosition = unchecked((ushort)(flame.YPosition - 8));
        flame.YVelocity = 0xfd00;
        flame.Variable0 = 0;
        flame.XVelocity = (nmiFrameCounter8 & 1) == 0 ? (ushort)0x0080 : (ushort)0xff80;
    }

    private void RunPhantoonCasualFlameBouncing(
        RoomEnemyProjectileSlot flame,
        RoomLevelData level)
    {
        flame.YVelocity = unchecked((ushort)(flame.YVelocity + 0x10));
        if (MoveProjectileAxis(flame, level, horizontal: false))
        {
            flame.Variable0 = unchecked((ushort)(flame.Variable0 + 1));
            if (flame.Variable0 < 3)
            {
                flame.YVelocity = flame.Variable0 == 1 ? (ushort)0xfe00 : (ushort)0xff00;
                return;
            }
            RestPhantoonCasualFlame(flame);
            return;
        }

        if (MoveProjectileAxis(flame, level, horizontal: true))
            RestPhantoonCasualFlame(flame);
    }

    private static void RestPhantoonCasualFlame(RoomEnemyProjectileSlot flame)
    {
        flame.InstructionPointer = EnemyProjectileInstructionLists.PhantoonEnragedFlame;
        flame.InstructionTimer = 1;
        flame.PreInstruction = EnemyProjectileCodePointers.RTS_869A44;
    }

    private void RunPhantoonEnragedFlame(RoomEnemyProjectileSlot flame)
    {
        if (_phantoonState is not { } state)
        {
            flame.Clear();
            return;
        }

        flame.YVelocity = unchecked((ushort)(flame.YVelocity + PhantoonFlameMotionRomData.RageRadiusStep));
        flame.Variable0 = unchecked((byte)(flame.Variable0 + (byte)flame.XVelocity));
        PositionPhantoonFlameAroundBody(
            flame,
            state.Body,
            flame.Variable0,
            unchecked((byte)flame.YVelocity));
        DeletePhantoonFlameOutsideRoom(flame);
    }

    private void RunPhantoonRainFlame(
        RoomEnemyProjectileSlot flame,
        RoomLevelData level)
    {
        if (flame.XVelocity != 0)
        {
            flame.XVelocity = unchecked((ushort)(flame.XVelocity - 1));
            if (flame.XVelocity != 0 && unchecked((short)flame.XVelocity) >= 0)
                return;
            // Expiry falls through to movement on this call, including signed
            // underflow. Keep each actor's native queue call rather than a last-SFX latch.
            QueueEnemySound(PhantoonFlameMotionRomData.RainFallSound, PhantoonFlameMotionRomData.RainSoundQueueCapacity);
        }

        flame.YVelocity = unchecked((ushort)(flame.YVelocity + 16));
        if (!MoveProjectileAxis(flame, level, horizontal: false))
            return;
        flame.InstructionPointer = EnemyProjectileInstructionLists.PhantoonFlameRainImpact;
        flame.InstructionTimer = 1;
        flame.YPosition = unchecked((ushort)(flame.YPosition + 8));
        flame.PreInstruction = EnemyProjectileCodePointers.RTS_869A44;
        QueueEnemySound(PhantoonFlameMotionRomData.RainFallSound, PhantoonFlameMotionRomData.RainSoundQueueCapacity);
    }

    private void RunPhantoonSpiralFlame(RoomEnemyProjectileSlot flame)
    {
        if (_phantoonState is not { } state)
        {
            flame.Clear();
            return;
        }

        flame.YVelocity = unchecked((ushort)(flame.YVelocity + PhantoonFlameMotionRomData.SpiralRadiusStep));
        flame.Variable0 = unchecked((byte)(flame.Variable0 + PhantoonFlameMotionRomData.SpiralAngleStep));
        PositionPhantoonFlameAroundBody(
            flame,
            state.Body,
            flame.Variable0,
            unchecked((byte)flame.YVelocity));
        DeletePhantoonFlameOutsideRoom(flame);
    }

    private static void PositionPhantoonFlameAroundBody(
        RoomEnemyProjectileSlot flame,
        RoomEnemySlot body,
        ushort angle,
        ushort radius)
    {
        // Unlike the common byte-table enemy sine routine, this callback uses a
        // word sample. Its high byte preserves the exact full radius at cardinal angles.
        int xOffset = ReadPhantoonFlameComponent(angle, radius);
        int yOffset = ReadPhantoonFlameComponent(unchecked((byte)(angle - 64)), radius);
        flame.XPosition = unchecked((ushort)(body.XPosition + xOffset));
        flame.YPosition = unchecked((ushort)(body.YPosition + 16 + yOffset));
    }

    /// <summary>Ports $86:9BA2/$9BF3: unsigned half-wave multiplication followed by whole-result negation.</summary>
    private static int ReadPhantoonFlameComponent(ushort angle, ushort radius)
    {
        int byteAngle = angle & 255;
        int sample = EnemyTrigonometryTables.SignedSine((byte)(byteAngle & 127));
        int magnitude = ((sample & 255) * (radius & 255) >> 8) + (sample >> 8) * (radius & 255);
        return byteAngle < 128 ? magnitude : -magnitude;
    }

    private static void DeletePhantoonFlameOutsideRoom(RoomEnemyProjectileSlot flame)
    {
        if (unchecked((short)flame.XPosition) < 0 || flame.XPosition >= 256 ||
            unchecked((short)flame.YPosition) < 0 || flame.YPosition >= 256)
        {
            flame.InstructionPointer = PhantoonFlameDeleteInstruction;
            flame.InstructionTimer = 1;
        }
    }

    /// <summary>
    /// Ports projectile instruction <c>$86:980E</c>. The cartridge forwards the flame's
    /// exact position and enemy header <c>$A0:E4FF</c> to <c>Spawn_Enemy_Drops</c>; the
    /// translated pickup owner is still an outer subsystem, so publish the same semantic
    /// request with the header-derived item table rather than silently discarding the drop.
    /// </summary>
    private void RequestPhantoonFlameDrop(RoomEnemyProjectileSlot flame)
    {
        if (flame.Kind != RoomEnemyProjectileKind.PhantoonDestroyableFlame)
        {
            throw new InvalidOperationException(
                $"Projectile {flame.Kind} reached Phantoon drop opcode $980E.");
        }

        RoomEnemyDefinition eye = ReadDefinition(_bus!, PhantoonEyeDefinition);
        _phantoonFlameDropRequests.Add(new PhantoonFlameDropRequest(
            flame.XPosition,
            flame.YPosition,
            PhantoonEyeDefinition,
            eye.ItemDropChancesPointer));
        SpawnEnemyDropFromChanceTable(
            flame.XPosition,
            flame.YPosition,
            eye.ItemDropChancesPointer);
    }
}
