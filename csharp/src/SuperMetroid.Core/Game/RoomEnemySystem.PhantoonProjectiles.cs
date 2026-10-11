using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$86 projectile half of Phantoon's intro and ordinary flame patterns. These actors
/// occupy the shared eighteen-slot pool and retain their cartridge definition pointers,
/// collision properties, bytecode, radii, and integer sine/cosine rounding.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort PhantoonStartingFlameWaitingPreInstruction =
        (ushort)EnemyProjectilePreInstruction.PhantoonStartingFlames;
    private const ushort PhantoonStartingFlameOrbitPreInstruction =
        (ushort)EnemyProjectilePreInstruction.PhantoonStartingFlames_Activated;

    private bool SpawnPhantoonStartingFlame(RoomEnemySlot body, byte directionIndex)
    {
        RoomEnemyProjectileSlot? flame = AllocateEnemyProjectile();
        if (flame is null)
            return false;

        InitializeEnemyProjectileFromDefinition(
            flame,
            RoomEnemyProjectileKind.PhantoonStartingFlame,
            unchecked((ushort)(body.PaletteIndex | body.VramTilesIndex)));
        flame.Position = flame.Position.WithFractionX(0).WithFractionY(0);
        flame.Velocity = new(new SnesSignedEightEight(0), new SnesSignedEightEight(0));
        flame.Variable0 = unchecked((byte)(directionIndex * 0x20));
        PositionPhantoonFlameAroundBody(flame, body, flame.Variable0, radius: 0x30);
        flame.PreInstruction = PhantoonStartingFlameWaitingPreInstruction;
        return true;
    }

    /// <summary>
    /// Initializes destroyable flame $86:9C29. The high parameter byte chooses one of the
    /// four native producers: casual fall, rage split, vertical rain, or spiral.
    /// </summary>
    private bool SpawnPhantoonDestroyableFlame(RoomEnemySlot body, PhantoonFlameSpawnType type, byte index)
    {
        RoomEnemyProjectileSlot? flame = AllocateEnemyProjectile();
        if (flame is null)
            return false;

        InitializeEnemyProjectileFromDefinition(
            flame,
            RoomEnemyProjectileKind.PhantoonDestroyableFlame,
            unchecked((ushort)(body.PaletteIndex | body.VramTilesIndex)));
        flame.Position = flame.Position.WithFractionX(0).WithFractionY(0);
        flame.Velocity = flame.Velocity.WithY(new SnesSignedEightEight(0));
        switch (type)
        {
            case PhantoonFlameSpawnType.Casual:
                flame.Velocity = flame.Velocity.WithX(new SnesSignedEightEight(0));
                flame.Position = flame.Position.WithWhole(new(body.Position.X.Whole, unchecked((ushort)(body.Position.Y.Whole + 32))));
                flame.InstructionPointer =
                    PhantoonProjectileInstructionProgramDefinitions.CasualFalling;
                flame.PreInstruction =
                    (ushort)EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Casual_Falling;

                // $86:985F changes the spawn definition's $8028 properties to $2028.
                // The falling flame therefore cannot hurt Samus and cannot be shot until
                // it has struck the floor and changed into the bouncing form.
                flame.CanDamageSamus = false;
                flame.BlocksSamusProjectiles = false;
                break;

            case PhantoonFlameSpawnType.Enraged:
                flame.Velocity = flame.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(index >= 8
                    ? -PhantoonFlameMotionRomData.RageAngleStep : PhantoonFlameMotionRomData.RageAngleStep))));
                flame.Variable0 = PhantoonFlameSpawnDefinitions.RageAngle(index);
                flame.Position = flame.Position.WithWhole(new(body.Position.X.Whole, unchecked((ushort)(body.Position.Y.Whole + 32))));
                flame.PreInstruction =
                    (ushort)EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Enraged;
                break;

            case PhantoonFlameSpawnType.Rain:
                // The index byte is packed as dx: the low nibble is a column and the high
                // nibble is the delayed-fall timer divided by eight.
                byte column = unchecked((byte)(index & 0x0f));
                if (column > 8)
                    throw new InvalidDataException($"Phantoon rain column {column} exceeds eight.");
                flame.Velocity = flame.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)((index & 0xf0) >> 1))));
                flame.Position = flame.Position.WithWholeX(PhantoonFlameSpawnDefinitions.RainX(column));
                flame.Position = flame.Position.WithWholeY(40);
                flame.PreInstruction =
                    (ushort)EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Rain;
                break;

            case PhantoonFlameSpawnType.Spiral:
                if (index > 7)
                    throw new InvalidDataException($"Phantoon spiral direction {index} exceeds seven.");
                flame.Velocity = flame.Velocity.WithX(new SnesSignedEightEight(128));
                flame.Variable0 = PhantoonFlameSpawnDefinitions.SpiralAngle(index);
                flame.Position = flame.Position.WithWhole(new(body.Position.X.Whole, unchecked((ushort)(body.Position.Y.Whole + 16))));
                flame.PreInstruction =
                    (ushort)EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Spiral;
                break;

            default:
                throw new InvalidOperationException($"Undefined Phantoon flame spawn type {type}.");
        }
        return true;
    }

    private void RunPhantoonStartingFlameWaiting(RoomEnemyProjectileSlot flame)
    {
        PhantoonEnemyState? state = Phantoon;
        if (state is null || state.Body.VariableB == 0)
            return;
        flame.PreInstruction = PhantoonStartingFlameOrbitPreInstruction;
        flame.Velocity = new(new SnesSignedEightEight(180), new SnesSignedEightEight(48));
    }

    private void RunPhantoonStartingFlameOrbit(RoomEnemyProjectileSlot flame)
    {
        if (Phantoon is not { } state)
        {
            flame.Clear();
            return;
        }

        if (flame.Velocity.X.RawValue != 0)
        {
            flame.Velocity = flame.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(flame.Velocity.X.RawValue - 1))));
        }
        // $86:9B4C reads the 16-bit NMI_FrameCounter.
        else if ((_currentEnemyProjectileFrame16 & 1) != 0)
        {
            flame.Velocity = flame.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(flame.Velocity.Y.RawValue - 1))));
            if (flame.Velocity.Y.RawValue == 0)
            {
                flame.Position = flame.Position.WithWhole(new(state.Body.Position.X.Whole, unchecked((ushort)(state.Body.Position.Y.Whole + 16))));
                flame.InstructionPointer =
                    PhantoonProjectileInstructionProgramDefinitions.Delete;
                flame.InstructionTimer = 1;
                return;
            }
        }

        flame.Variable0 = unchecked((byte)(flame.Variable0 + 1));
        PositionPhantoonFlameAroundBody(
            flame,
            state.Body,
            flame.Variable0,
            unchecked((byte)flame.Velocity.Y.RawValue));
    }

    private static void RunPhantoonCasualFlameFalling(
        RoomEnemyProjectileSlot flame,
        RoomLevelData level)
    {
        flame.Velocity = flame.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(flame.Velocity.Y.RawValue + 0x10))));
        if (!MoveProjectileAxis(flame, level, horizontal: false))
            return;

        flame.CanDamageSamus = true;
        flame.BlocksSamusProjectiles = true;
        flame.PreInstruction =
            (ushort)EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Casual_HitGround;
        flame.InstructionPointer =
            PhantoonProjectileInstructionProgramDefinitions.CasualHitGround;
        flame.InstructionTimer = 1;
        flame.Variable0 = 8;
        flame.Position = flame.Position.WithWholeY(unchecked((ushort)(flame.Position.Y.Whole + 8)));
    }

    private static void RunPhantoonCasualFlameImpactPause(
        RoomEnemyProjectileSlot flame,
        ushort nmiFrameCounter)
    {
        ushort oldTimer = flame.Variable0;
        flame.Variable0 = unchecked((ushort)(flame.Variable0 - 1));
        if (oldTimer != 1 && unchecked((short)flame.Variable0) >= 0)
            return;

        flame.PreInstruction =
            (ushort)EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Casual_Bouncing;
        flame.InstructionPointer =
            PhantoonProjectileInstructionProgramDefinitions.CasualBouncing;
        flame.InstructionTimer = 1;
        flame.Position = flame.Position.WithWholeY(unchecked((ushort)(flame.Position.Y.Whole - 8)));
        flame.Velocity = flame.Velocity.WithY(new SnesSignedEightEight(0xfd00));
        flame.Variable0 = 0;
        // $86:99EB reads the 16-bit NMI_FrameCounter.
        flame.Velocity = flame.Velocity.WithX(new SnesSignedEightEight((nmiFrameCounter & 1) == 0 ? (ushort)0x0080 : (ushort)0xff80));
    }

    private static void RunPhantoonCasualFlameBouncing(
        RoomEnemyProjectileSlot flame,
        RoomLevelData level)
    {
        flame.Velocity = flame.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(flame.Velocity.Y.RawValue + 0x10))));
        if (MoveProjectileAxis(flame, level, horizontal: false))
        {
            flame.Variable0 = unchecked((ushort)(flame.Variable0 + 1));
            if (flame.Variable0 < 3)
            {
                flame.Velocity = flame.Velocity.WithY(new SnesSignedEightEight(flame.Variable0 == 1 ? (ushort)0xfe00 : (ushort)0xff00));
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
        flame.InstructionPointer =
            PhantoonProjectileInstructionProgramDefinitions.CasualResting;
        flame.InstructionTimer = 1;
        flame.PreInstruction = (ushort)EnemyProjectilePreInstruction.RTS_869A44;
    }

    private void RunPhantoonEnragedFlame(RoomEnemyProjectileSlot flame)
    {
        if (Phantoon is not { } state)
        {
            flame.Clear();
            return;
        }

        flame.Velocity = flame.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(flame.Velocity.Y.RawValue + PhantoonFlameMotionRomData.RageRadiusStep))));
        flame.Variable0 = unchecked((byte)(flame.Variable0 + (byte)flame.Velocity.X.RawValue));
        PositionPhantoonFlameAroundBody(
            flame,
            state.Body,
            flame.Variable0,
            unchecked((byte)flame.Velocity.Y.RawValue));
        DeletePhantoonFlameOutsideRoom(flame);
    }

    private void RunPhantoonRainFlame(
        RoomEnemyProjectileSlot flame,
        RoomLevelData level)
    {
        if (flame.Velocity.X.RawValue != 0)
        {
            flame.Velocity = flame.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(flame.Velocity.X.RawValue - 1))));
            if (flame.Velocity.X.RawValue != 0 && unchecked((short)flame.Velocity.X.RawValue) >= 0)
                return;
            // Expiry falls through to movement on this call, including signed
            // underflow. Keep each actor's native queue call rather than a last-SFX latch.
            QueueEnemySound(PhantoonFlameMotionRomData.RainFallSound, PhantoonFlameMotionRomData.RainSoundQueueCapacity);
        }

        flame.Velocity = flame.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(flame.Velocity.Y.RawValue + 16))));
        if (!MoveProjectileAxis(flame, level, horizontal: false))
            return;
        flame.InstructionPointer = PhantoonProjectileInstructionProgramDefinitions.RainImpact;
        flame.InstructionTimer = 1;
        flame.Position = flame.Position.WithWholeY(unchecked((ushort)(flame.Position.Y.Whole + 8)));
        flame.PreInstruction = (ushort)EnemyProjectilePreInstruction.RTS_869A44;
        QueueEnemySound(PhantoonFlameMotionRomData.RainFallSound, PhantoonFlameMotionRomData.RainSoundQueueCapacity);
    }

    private void RunPhantoonSpiralFlame(RoomEnemyProjectileSlot flame)
    {
        if (Phantoon is not { } state)
        {
            flame.Clear();
            return;
        }

        flame.Velocity = flame.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(flame.Velocity.Y.RawValue + PhantoonFlameMotionRomData.SpiralRadiusStep))));
        flame.Variable0 = unchecked((byte)(flame.Variable0 + PhantoonFlameMotionRomData.SpiralAngleStep));
        PositionPhantoonFlameAroundBody(
            flame,
            state.Body,
            flame.Variable0,
            unchecked((byte)flame.Velocity.Y.RawValue));
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
        flame.Position = flame.Position.WithWhole(new(unchecked((ushort)(body.Position.X.Whole + xOffset)), unchecked((ushort)(body.Position.Y.Whole + 16 + yOffset))));
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
        if (unchecked((short)flame.Position.X.Whole) < 0 || flame.Position.X.Whole >= 256 ||
            unchecked((short)flame.Position.Y.Whole) < 0 || flame.Position.Y.Whole >= 256)
        {
            flame.InstructionPointer = PhantoonProjectileInstructionProgramDefinitions.Delete;
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

        RoomEnemyDefinition eye = ResolveRoomEnemyDefinition(_bus!, EnemyDefinitionId.PhantoonEye);
        _phantoonFlameDropRequests.Add(new PhantoonFlameDropRequest());
        SpawnEnemyDropFromChanceTable(
            flame.Position.Whole,
            eye.ItemDropChancesPointer);
    }
}
