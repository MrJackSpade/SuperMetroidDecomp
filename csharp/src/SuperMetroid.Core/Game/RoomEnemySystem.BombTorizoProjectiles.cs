using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    private static readonly short[] BombTorizoSwipeXOffsets =
        [-30, -40, -47, -31, -21, -1, -28, -43, -48, -31, -21];
    private static readonly short[] BombTorizoSwipeYOffsets =
        [-52, -28, -11, 9, 21, 20, -52, -27, -10, 9, 20];
    private static readonly short[] BombTorizoExplosionXOffsets = [0, 12, -12, 0, 16, -16];
    private static readonly short[] BombTorizoExplosionYOffsets = [-8, -8, -8, -20, -20, -20];

    private void SpawnBombTorizoLowHealthDrool(RoomEnemySlot torizo) =>
        SpawnBombTorizoDrool(torizo, RoomEnemyProjectileKind.BombTorizoLowHealthDrool);

    private void SpawnBombTorizoInitialDrool(RoomEnemySlot torizo) =>
        SpawnBombTorizoDrool(torizo, RoomEnemyProjectileKind.BombTorizoInitialDrool);

    private void SpawnBombTorizoDrool(
        RoomEnemySlot torizo,
        RoomEnemyProjectileKind kind)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            kind,
            unchecked((ushort)(torizo.VramTilesIndex | torizo.PaletteIndex)));

        ushort random = _nextRandom!();
        projectile.YPosition = kind == RoomEnemyProjectileKind.BombTorizoInitialDrool
            ? unchecked((ushort)(torizo.YPosition + (random & 3) - 5))
            : unchecked((ushort)(torizo.YPosition - 5));

        if (kind == RoomEnemyProjectileKind.BombTorizoInitialDrool)
        {
            projectile.YVelocity = unchecked((ushort)((random & 0x001f) + 48));
            ushort xJitter = unchecked((ushort)(_nextRandom!() & 3));
            projectile.XPosition = (torizo.Parameter1 & 0x4000) != 0
                ? unchecked((ushort)(torizo.XPosition + xJitter))
                : (torizo.Parameter1 & 0x8000) != 0
                    ? unchecked((ushort)(torizo.XPosition + xJitter + 8))
                    : unchecked((ushort)(torizo.XPosition + xJitter - 8));
            return;
        }

        // The recurring low-health drool chooses a random direction from the cartridge sine
        // table. Preserve the exact signed samples and 8.8 velocity representation.
        int angle;
        if ((torizo.Parameter1 & 0x4000) != 0)
        {
            angle = random & 0x00ff;
        }
        else
        {
            int baseAngle = (torizo.Parameter1 & 0x8000) != 0 ? 32 : 224;
            angle = unchecked((byte)(baseAngle + (random & 0x000f) - 8));
        }
        projectile.XVelocity = unchecked((ushort)(short)ReadWord(
            _bus!,
            0xa0b443 + (((angle + 64) & 0xff) * 2)));
        projectile.YVelocity = unchecked((ushort)(short)ReadWord(
            _bus!,
            0xa0b443 + ((angle & 0xff) * 2)));
        projectile.XPosition = (torizo.Parameter1 & 0x8000) != 0
            ? unchecked((ushort)(torizo.XPosition + 8))
            : unchecked((ushort)(torizo.XPosition - 8));
    }

    private void SpawnBombTorizoExplosiveSwipe(RoomEnemySlot torizo, ushort parameter)
    {
        int index = parameter >> 1;
        if ((uint)index >= (uint)BombTorizoSwipeXOffsets.Length)
        {
            throw new InvalidDataException(
                $"Bomb Torizo swipe parameter ${parameter:X4} exceeds its eleven-entry tables.");
        }

        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;
        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoExplosiveSwipe,
            graphicsIndex: 0);

        short xOffset = BombTorizoSwipeXOffsets[index];
        projectile.XPosition = (torizo.Parameter1 & 0x8000) != 0
            ? unchecked((ushort)(torizo.XPosition - xOffset))
            : unchecked((ushort)(torizo.XPosition + xOffset));
        projectile.YPosition = unchecked((ushort)(
            torizo.YPosition + BombTorizoSwipeYOffsets[index]));
    }

    private void SpawnBombTorizoLowHealthExplosion(RoomEnemySlot torizo, ushort parameter)
    {
        // The initializer adds two to the operand and adds another two while facing left,
        // then treats the result as a word-table byte offset.
        int adjusted = parameter + 2 + ((torizo.Parameter1 & 0x8000) == 0 ? 2 : 0);
        int index = adjusted >> 1;
        if ((uint)index >= (uint)BombTorizoExplosionXOffsets.Length)
        {
            throw new InvalidDataException(
                $"Bomb Torizo explosion parameter ${parameter:X4} selects table index {index}.");
        }

        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;
        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoLowHealthExplosion,
            graphicsIndex: 0);
        projectile.XPosition = unchecked((ushort)(
            torizo.XPosition + BombTorizoExplosionXOffsets[index]));
        projectile.YPosition = unchecked((ushort)(
            torizo.YPosition + BombTorizoExplosionYOffsets[index]));
        projectile.Variable0 = projectile.XPosition;
        projectile.Variable1 = projectile.YPosition;
    }

    private void SpawnBombTorizoDeathExplosion(RoomEnemySlot torizo)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;
        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoDeathExplosion,
            graphicsIndex: 0);
        projectile.XPosition = torizo.XPosition;
        projectile.YPosition = torizo.YPosition;
        projectile.Variable0 = torizo.XPosition;
        projectile.Variable1 = torizo.YPosition;
    }

    private void SpawnBombTorizoChozoOrb(RoomEnemySlot torizo)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;
        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoChozoOrb,
            unchecked((ushort)(torizo.VramTilesIndex | torizo.PaletteIndex)));

        // $86:AC08/$AC12 are five-word {list,x,xVelocity,y,yVelocity} records. The
        // initializer adds a signed random byte minus $80 to each velocity.
        int tuple = (torizo.Parameter1 & 0x8000) != 0 ? 0x86ac08 : 0x86ac12;
        projectile.InstructionPointer = ReadWord(_bus!, tuple);
        projectile.XPosition = unchecked((ushort)(torizo.XPosition +
            unchecked((short)ReadWord(_bus!, tuple + 2))));
        projectile.XVelocity = unchecked((ushort)(
            unchecked((short)ReadWord(_bus!, tuple + 4)) +
            (unchecked((byte)_nextRandom!()) - 0x80)));
        projectile.YPosition = unchecked((ushort)(torizo.YPosition +
            unchecked((short)ReadWord(_bus!, tuple + 6))));
        projectile.YVelocity = unchecked((ushort)(
            unchecked((short)ReadWord(_bus!, tuple + 8)) +
            (unchecked((byte)_nextRandom!()) - 0x80)));
    }

    private void SpawnBombTorizoSonicBoom(RoomEnemySlot torizo, ushort parameter)
        => SpawnTorizoSonicBoom(
            torizo,
            parameter,
            RoomEnemyProjectileKind.BombTorizoSonicBoom);

    private void SpawnGoldenTorizoSonicBoom(RoomEnemySlot torizo, ushort parameter)
        => SpawnTorizoSonicBoom(
            torizo,
            parameter,
            RoomEnemyProjectileKind.GoldenTorizoSonicBoom);

    private void SpawnTorizoSonicBoom(
        RoomEnemySlot torizo,
        ushort parameter,
        RoomEnemyProjectileKind kind)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;
        InitializeEnemyProjectileFromDefinition(
            projectile,
            kind,
            unchecked((ushort)(torizo.VramTilesIndex | torizo.PaletteIndex)));

        projectile.DirectionParameter = parameter;
        projectile.YPosition = unchecked((ushort)(
            torizo.YPosition + ((_nextRandom!() & 1) != 0 ? -12 : 20)));
        if ((torizo.Parameter1 & 0x8000) != 0)
        {
            projectile.XPosition = unchecked((ushort)(torizo.XPosition + 32));
            projectile.XVelocity = 624;
            projectile.InstructionPointer = 0xadd2;
        }
        else
        {
            projectile.XPosition = unchecked((ushort)(torizo.XPosition - 32));
            projectile.XVelocity = unchecked((ushort)-624);
            projectile.InstructionPointer = 0xadbf;
        }
    }

    private void SpawnGoldenTorizoChozoOrb(RoomEnemySlot torizo)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.GoldenTorizoChozoOrb,
            unchecked((ushort)(torizo.VramTilesIndex | torizo.PaletteIndex)));
        InitializeTorizoRandomizedProjectileTuple(
            projectile,
            torizo,
            (torizo.Parameter1 & 0x8000) != 0 ? 0x86ac99 : 0x86aca3);
    }

    private void SpawnGoldenTorizoEgg(RoomEnemySlot torizo)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.GoldenTorizoEgg,
            unchecked((ushort)(torizo.VramTilesIndex | torizo.PaletteIndex)));

        // $86:B001 contains an authentic bug: it masks the literal opcode byte $E2,
        // not a random value, producing a fixed 66-frame timer. Preserve that behavior.
        projectile.Variable1 = (0x00e2 & 0x001f) + 64;
        projectile.Variable0 = torizo.Parameter1;
        InitializeTorizoRandomizedProjectileTuple(
            projectile,
            torizo,
            unchecked((short)torizo.Parameter1) < 0 ? 0x86b02f : 0x86b039);
    }

    private void SpawnGoldenTorizoSuperMissile(RoomEnemySlot torizo)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.GoldenTorizoSuperMissile,
            unchecked((ushort)(torizo.VramTilesIndex | torizo.PaletteIndex)));
        bool facingRight = (torizo.Parameter1 & 0x8000) != 0;
        projectile.Variable0 = unchecked((ushort)(torizo.SlotIndex * 2));
        projectile.XPosition = unchecked((ushort)(torizo.XPosition + (facingRight ? 30 : -30)));
        projectile.YPosition = unchecked((ushort)(torizo.YPosition - 52));
        projectile.InstructionPointer = ReadWord(_bus!, facingRight ? 0x86b20b : 0x86b209);
    }

    private void SpawnGoldenTorizoEyeBeam(RoomEnemySlot torizo, ushort parameter)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.GoldenTorizoEyeBeam,
            unchecked((ushort)(torizo.VramTilesIndex | torizo.PaletteIndex)));
        projectile.DirectionParameter = parameter;
        bool facingRight = (torizo.Parameter1 & 0x8000) != 0;
        InitializeTorizoRandomizedProjectileTuple(
            projectile,
            torizo,
            facingRight ? 0x86b376 : 0x86b380);

        int angle = ((_nextRandom!() & 0x001e) - 16 + 192 + (facingRight ? 0 : 128)) & 0x01ff;
        int sineIndex = angle >> 1;
        projectile.XVelocity = unchecked((ushort)(short)(8 * unchecked((short)ReadWord(
            _bus!,
            0xa0b443 + (((sineIndex + 64) & 0xff) * 2)))));
        projectile.YVelocity = unchecked((ushort)(short)(8 * unchecked((short)ReadWord(
            _bus!,
            0xa0b443 + ((sineIndex & 0xff) * 2)))));
    }

    private void InitializeTorizoRandomizedProjectileTuple(
        RoomEnemyProjectileSlot projectile,
        RoomEnemySlot torizo,
        int tupleAddress)
    {
        // Bank $86 stores {list, x offset, x velocity, y offset, y velocity}. Each
        // velocity receives its own signed random-byte displacement in [-128, 127].
        projectile.InstructionPointer = ReadWord(_bus!, tupleAddress);
        projectile.XPosition = unchecked((ushort)(torizo.XPosition +
            unchecked((short)ReadWord(_bus!, tupleAddress + 2))));
        projectile.XVelocity = unchecked((ushort)(
            unchecked((short)ReadWord(_bus!, tupleAddress + 4)) +
            unchecked((byte)_nextRandom!()) - 128));
        projectile.YPosition = unchecked((ushort)(torizo.YPosition +
            unchecked((short)ReadWord(_bus!, tupleAddress + 6))));
        projectile.YVelocity = unchecked((ushort)(
            unchecked((short)ReadWord(_bus!, tupleAddress + 8)) +
            unchecked((byte)_nextRandom!()) - 128));
    }

    private void SpawnBombTorizoLandingDust(RoomEnemySlot torizo, bool rightFoot)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;
        RoomEnemyProjectileKind kind = rightFoot
            ? RoomEnemyProjectileKind.BombTorizoRightFootDust
            : RoomEnemyProjectileKind.BombTorizoLeftFootDust;
        InitializeEnemyProjectileFromDefinition(projectile, kind, graphicsIndex: 0);
        projectile.XPosition = unchecked((ushort)(torizo.XPosition + (rightFoot ? 24 : -24)));
        projectile.YPosition = unchecked((ushort)(torizo.YPosition + 48));
    }

    private void RunBombTorizoChozoOrbPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (MoveProjectileAxis(projectile, level, horizontal: true))
        {
            projectile.InstructionPointer = 0xab25;
            projectile.InstructionTimer = 1;
            projectile.CanDamageSamus = false;
            return;
        }

        bool verticalCollision = MoveProjectileAxis(projectile, level, horizontal: false);
        if (unchecked((short)projectile.YVelocity) >= 0 && verticalCollision)
        {
            projectile.YPosition = unchecked((ushort)(
                (projectile.YPosition & 0xfff0) + 6));
            projectile.InstructionPointer = 0xab41;
            projectile.InstructionTimer = 1;
            projectile.CanDamageSamus = false;
            return;
        }

        projectile.YVelocity = unchecked((ushort)(projectile.YVelocity + 18));
        if ((projectile.YVelocity & 0xf000) == 0x1000)
            projectile.Clear();
    }

    private void RunBombTorizoSonicBoomPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (MoveProjectileAxis(projectile, level, horizontal: true))
        {
            projectile.InstructionPointer = 0xade5;
            projectile.InstructionTimer = 1;
            projectile.Variable0 = projectile.XPosition;
            projectile.Variable1 = projectile.YPosition;
            projectile.CanDamageSamus = false;
            return;
        }

        projectile.XVelocity = unchecked((ushort)(projectile.XVelocity +
            (unchecked((short)projectile.XVelocity) < 0 ? -16 : 16)));
        if ((projectile.XVelocity & 0xf000) == 0x1000)
            projectile.Clear();
    }

    private void RunGoldenTorizoChozoOrbPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (MoveProjectileAxis(projectile, level, horizontal: true))
            projectile.XVelocity = unchecked((ushort)-unchecked((short)projectile.XVelocity));

        bool verticalCollision = MoveProjectileAxis(projectile, level, horizontal: false);
        if (verticalCollision && unchecked((short)projectile.YVelocity) >= 0)
        {
            short xVelocity = unchecked((short)projectile.XVelocity);
            projectile.XVelocity = unchecked((ushort)(xVelocity >= 0
                ? xVelocity - 64
                : xVelocity + 64));
            projectile.YVelocity = unchecked((ushort)-(projectile.YVelocity >> 1));
            if ((projectile.YVelocity & 0xff80) == 0xff80)
            {
                projectile.YPosition = unchecked((ushort)(
                    (projectile.YPosition & 0xfff0) + 6));
                projectile.InstructionPointer = 0xab41;
                projectile.InstructionTimer = 1;
                projectile.CanDamageSamus = false;
                return;
            }
        }

        projectile.YVelocity = unchecked((ushort)(projectile.YVelocity + 24));
    }

    private void RunGoldenTorizoEggPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        projectile.Variable1 = unchecked((ushort)(projectile.Variable1 - 1));
        if (unchecked((short)projectile.Variable1) < 0)
        {
            projectile.InstructionPointer = unchecked((ushort)(projectile.InstructionPointer + 2));
            projectile.InstructionTimer = 1;
            projectile.XVelocity = (projectile.Variable0 & 0x8000) != 0
                ? (ushort)256
                : unchecked((ushort)-256);
            return;
        }

        if (MoveProjectileAxis(projectile, level, horizontal: true))
        {
            projectile.XVelocity = unchecked((ushort)-unchecked((short)projectile.XVelocity));
            projectile.Variable0 ^= 0x8000;
        }

        if (MoveProjectileAxis(projectile, level, horizontal: false) &&
            unchecked((short)projectile.YVelocity) >= 0)
        {
            short xVelocity = unchecked((short)projectile.XVelocity);
            projectile.XVelocity = unchecked((ushort)(xVelocity + (xVelocity < 0 ? 32 : -32)));
            projectile.YVelocity = unchecked((ushort)-unchecked((short)projectile.YVelocity));
        }

        projectile.YVelocity = unchecked((ushort)(projectile.YVelocity + 48));
        if ((projectile.YVelocity & 0xf000) == 0x1000)
            projectile.Clear();
    }

    private void RunGoldenTorizoSuperMissilePreInstruction(RoomEnemyProjectileSlot projectile)
    {
        TorizoEnemyState? state = GoldenTorizo;
        if (state is null)
        {
            projectile.Clear();
            return;
        }

        RoomEnemySlot torizo = state.Slot;
        projectile.XPosition = unchecked((ushort)(torizo.XPosition +
            ((torizo.Parameter1 & 0x8000) != 0 ? 32 : -32)));
        projectile.YPosition = unchecked((ushort)(torizo.YPosition - 52));
    }

    private void RunGoldenTorizoEyeBeamPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (MoveProjectileAxis(projectile, level, horizontal: true))
        {
            projectile.InstructionPointer = 0xb3cd;
            projectile.InstructionTimer = 1;
            return;
        }

        if (MoveProjectileAxis(projectile, level, horizontal: false))
        {
            projectile.YPosition = unchecked((ushort)(
                (projectile.YPosition & 0xfff0) + 6));
            projectile.InstructionPointer = 0xb3e5;
            projectile.InstructionTimer = 1;
        }
    }
}
