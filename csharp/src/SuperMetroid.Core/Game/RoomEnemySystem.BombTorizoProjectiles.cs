using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
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

        ushort random;
        if (kind == RoomEnemyProjectileKind.BombTorizoLowHealthDrool)
        {
            projectile.InstructionPointer =
                (ushort)BombTorizoDroolInstructionProgramDefinitions.SelectLowHealthInitialProgram(
                    _nextRandom!());
            random = _nextRandom!();
        }
        else
        {
            random = _nextRandom!();
        }
        if (kind == RoomEnemyProjectileKind.BombTorizoInitialDrool)
        {
            // $86:A66D-$A683 chain the carry: CLC; ADC Y; ADC #$FFFB, then the Y velocity's
            // ADC #$0030 adds that second addition's carry (set whenever Y + jitter >= 5).
            int jitteredY = (random & 3) + torizo.Position.Y.Whole;
            int yWithOffset = jitteredY + 0xfffb;
            projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)yWithOffset));
            projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)((random & 0x001f) + 0x30 + (yWithOffset >> 16)))));
            // $86:A68D-$A6C3: the X offset's ADC carries out of CLC; ADC X. Every branch
            // stores a zero X velocity.
            int jitteredX = (_nextRandom!() & 3) + torizo.Position.X.Whole;
            int carry = jitteredX >> 16;
            projectile.Position = projectile.Position.WithWholeX((torizo.Parameter1 & 0x4000) != 0
                ? unchecked((ushort)jitteredX)
                : (torizo.Parameter1 & 0x8000) != 0
                    ? unchecked((ushort)(jitteredX + 8 + carry))
                    : unchecked((ushort)(jitteredX + 0xfff8 + carry)));
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(0));
            return;
        }
        projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(torizo.Position.Y.Whole - 5)));

        // The recurring low-health drool chooses a random direction ($86:A5F9-$A62C). The
        // turning branch's word index is random & $1FE; the X velocity is the sign-extended
        // sine and the Y velocity the negative cosine of that angle.
        int angle;
        if ((torizo.Parameter1 & 0x4000) != 0)
        {
            angle = (random & 0x01fe) >> 1;
        }
        else
        {
            int baseAngle = (torizo.Parameter1 & 0x8000) != 0 ? 32 : 224;
            angle = unchecked((byte)(baseAngle + (random & 0x000f) - 8));
        }
        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)
            EnemyTrigonometryTables.SignedSine(unchecked((byte)angle)))));
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)
            EnemyTrigonometryTables.SignedNegativeCosineWord(angle))));
        projectile.Position = projectile.Position.WithWholeX((torizo.Parameter1 & 0x8000) != 0
            ? unchecked((ushort)(torizo.Position.X.Whole + 8))
            : unchecked((ushort)(torizo.Position.X.Whole - 8)));
    }

    private void SpawnBombTorizoExplosiveSwipe(RoomEnemySlot torizo, ushort parameter)
    {
        Offset16<RoomPixels> placement = BombTorizoAttackDefinitions.Swipe(parameter);

        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;
        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoExplosiveSwipe,
            graphicsIndex: 0);

        // A right-facing Torizo mirrors the left-facing hand placement horizontally.
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole +
            ((torizo.Parameter1 & 0x8000) != 0 ? placement.MirroredX() : placement));
    }

    private void SpawnBombTorizoLowHealthExplosion(RoomEnemySlot torizo, ushort parameter)
    {
        Offset16<RoomPixels> placement =
            BombTorizoAttackDefinitions.LowHealthExplosion(
                parameter,
                facingRight: (torizo.Parameter1 & 0x8000) != 0);

        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;
        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoLowHealthExplosion,
            graphicsIndex: 0);
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + placement);
        projectile.Variable0 = projectile.Position.X.Whole;
        projectile.Variable1 = projectile.Position.Y.Whole;
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
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole);
        projectile.Variable0 = torizo.Position.X.Whole;
        projectile.Variable1 = torizo.Position.Y.Whole;
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

        InitializeTorizoRandomizedProjectile(
            projectile,
            torizo,
            TorizoRandomizedProjectileDefinitions.BombChozoOrb(
                facingRight: (torizo.Parameter1 & 0x8000) != 0));
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
        projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(
            torizo.Position.Y.Whole + ((_nextRandom!() & 1) != 0 ? -12 : 20))));
        if ((torizo.Parameter1 & 0x8000) != 0)
        {
            projectile.Position = projectile.Position.WithWholeX(unchecked((ushort)(torizo.Position.X.Whole + 32)));
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(624));
            projectile.InstructionPointer =
                TorizoSonicBoomInstructionProgramDefinitions.FiredRight;
        }
        else
        {
            projectile.Position = projectile.Position.WithWholeX(unchecked((ushort)(torizo.Position.X.Whole - 32)));
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)-624)));
            projectile.InstructionPointer =
                TorizoSonicBoomInstructionProgramDefinitions.FiredLeft;
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
        InitializeTorizoRandomizedProjectile(
            projectile,
            torizo,
            TorizoRandomizedProjectileDefinitions.GoldenChozoOrb(
                facingRight: (torizo.Parameter1 & 0x8000) != 0));
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
        InitializeTorizoRandomizedProjectile(
            projectile,
            torizo,
            TorizoRandomizedProjectileDefinitions.GoldenEgg(
                movingRight: unchecked((short)torizo.Parameter1) < 0));
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
        projectile.Position = projectile.Position.WithWhole(new(unchecked((ushort)(torizo.Position.X.Whole + (facingRight ? 30 : -30))), unchecked((ushort)(torizo.Position.Y.Whole - 52))));
        projectile.InstructionPointer =
            GoldenTorizoProjectileDefinitions.GetReflectedSuperMissileInstruction(facingRight);
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
        InitializeTorizoRandomizedProjectile(
            projectile,
            torizo,
            TorizoRandomizedProjectileDefinitions.GoldenEyeBeam(facingRight));

        int angle = ((_nextRandom!() & 0x001e) - 16 + 192 + (facingRight ? 0 : 128)) & 0x01ff;
        int sineIndex = angle >> 1;
        // The native combined table starts at negative cosine, not sine. Its
        // quarter-turn offset selects sine for X; the unshifted sample gives Y.
        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(8 *
            EnemyTrigonometryTables.SignedNegativeCosineWord(sineIndex + 64)))));
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(8 *
            EnemyTrigonometryTables.SignedNegativeCosineWord(sineIndex)))));
    }

    private void InitializeTorizoRandomizedProjectile(
        RoomEnemyProjectileSlot projectile,
        RoomEnemySlot torizo,
        TorizoRandomizedProjectileDefinition definition)
    {
        projectile.InstructionPointer = definition.InstructionList;
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + definition.Offset);
        // The native initializer draws the X jitter before the Y jitter.
        short xVelocity = unchecked((short)(
            definition.BaseVelocity.X.SignedRawValue + unchecked((byte)_nextRandom!()) - 128));
        short yVelocity = unchecked((short)(
            definition.BaseVelocity.Y.SignedRawValue + unchecked((byte)_nextRandom!()) - 128));
        projectile.Velocity = new EightEightVector<RoomPixels>(xVelocity, yVelocity);
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
        projectile.Position = projectile.Position.WithWhole(new(unchecked((ushort)(torizo.Position.X.Whole + (rightFoot ? 24 : -24))), unchecked((ushort)(torizo.Position.Y.Whole + 48))));
    }

    private static void RunBombTorizoChozoOrbPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (MoveProjectileAxis(projectile, level, horizontal: true))
        {
            projectile.InstructionPointer =
                TorizoChozoOrbInstructionProgramDefinitions.WallImpact;
            projectile.InstructionTimer = 1;
            projectile.CanDamageSamus = false;
            return;
        }

        bool verticalCollision = MoveProjectileAxis(projectile, level, horizontal: false);
        if (unchecked((short)projectile.Velocity.Y.RawValue) >= 0 && verticalCollision)
        {
            projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(
                (projectile.Position.Y.Whole & 0xfff0) + 6)));
            projectile.InstructionPointer =
                TorizoChozoOrbInstructionProgramDefinitions.FloorImpact;
            projectile.InstructionTimer = 1;
            projectile.CanDamageSamus = false;
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + 18))));
        if ((projectile.Velocity.Y.RawValue & 0xf000) == 0x1000)
            projectile.Clear();
    }

    private static void RunBombTorizoSonicBoomPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (MoveProjectileAxis(projectile, level, horizontal: true))
        {
            projectile.InstructionPointer =
                TorizoSonicBoomInstructionProgramDefinitions.WallImpact;
            projectile.InstructionTimer = 1;
            projectile.Variable0 = projectile.Position.X.Whole;
            projectile.Variable1 = projectile.Position.Y.Whole;
            projectile.CanDamageSamus = false;
            return;
        }

        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.X.RawValue +
            (unchecked((short)projectile.Velocity.X.RawValue) < 0 ? -16 : 16)))));
        if ((projectile.Velocity.X.RawValue & 0xf000) == 0x1000)
            projectile.Clear();
    }

    /// <summary>
    /// Ports <c>$86:A887</c>, shared by Bomb Torizo's six gut-break droplets and its
    /// low-health initial drool. The peculiar positive-three clamp is authentic: both the
    /// negative and nonnegative drag branches load <c>$0003</c> when they cross zero.
    /// </summary>
    private static void RunBombTorizoDroolPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (MoveProjectileAxis(projectile, level, horizontal: true))
        {
            projectile.InstructionPointer =
                (ushort)BombTorizoDroolProgram.WallImpact;
            projectile.InstructionTimer = 1;
            return;
        }

        short xVelocity = unchecked((short)projectile.Velocity.X.RawValue);
        if (xVelocity < 0)
        {
            int dragged = xVelocity + 4;
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(short)(dragged < 0 ? dragged : 3))));
        }
        else
        {
            int dragged = xVelocity - 4;
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(short)(dragged >= 0 ? dragged : 3))));
        }

        bool verticalCollision = MoveProjectileAxis(projectile, level, horizontal: false);
        if (unchecked((short)projectile.Velocity.Y.RawValue) >= 0 && verticalCollision)
        {
            projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(projectile.Position.Y.Whole - 3)));
            projectile.InstructionPointer =
                (ushort)BombTorizoDroolProgram.FloorImpact;
            projectile.InstructionTimer = 1;
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + 16))));
        if ((projectile.Velocity.Y.RawValue & 0xf000) == 0x1000)
            projectile.Clear();
    }

    private static void RunGoldenTorizoChozoOrbPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (MoveProjectileAxis(projectile, level, horizontal: true))
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)-unchecked((short)projectile.Velocity.X.RawValue))));

        bool verticalCollision = MoveProjectileAxis(projectile, level, horizontal: false);
        if (verticalCollision && unchecked((short)projectile.Velocity.Y.RawValue) >= 0)
        {
            short xVelocity = unchecked((short)projectile.Velocity.X.RawValue);
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(xVelocity >= 0
                ? xVelocity - 64
                : xVelocity + 64))));
            projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)-(projectile.Velocity.Y.RawValue >> 1))));
            if ((projectile.Velocity.Y.RawValue & 0xff80) == 0xff80)
            {
                projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(
                    (projectile.Position.Y.Whole & 0xfff0) + 6)));
                projectile.InstructionPointer =
                    TorizoChozoOrbInstructionProgramDefinitions.FloorImpact;
                projectile.InstructionTimer = 1;
                projectile.CanDamageSamus = false;
                return;
            }
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + 24))));
    }

    private static void RunGoldenTorizoEggPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        projectile.Variable1 = unchecked((ushort)(projectile.Variable1 - 1));
        if (unchecked((short)projectile.Variable1) < 0)
        {
            projectile.InstructionPointer = unchecked((ushort)(projectile.InstructionPointer + 2));
            projectile.InstructionTimer = 1;
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight((projectile.Variable0 & 0x8000) != 0
                ? (ushort)256
                : unchecked((ushort)-256)));
            return;
        }

        if (MoveProjectileAxis(projectile, level, horizontal: true))
        {
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)-unchecked((short)projectile.Velocity.X.RawValue))));
            projectile.Variable0 ^= 0x8000;
        }

        if (MoveProjectileAxis(projectile, level, horizontal: false) &&
            unchecked((short)projectile.Velocity.Y.RawValue) >= 0)
        {
            short xVelocity = unchecked((short)projectile.Velocity.X.RawValue);
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(xVelocity + (xVelocity < 0 ? 32 : -32)))));
            projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)-unchecked((short)projectile.Velocity.Y.RawValue))));
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + 48))));
        if ((projectile.Velocity.Y.RawValue & 0xf000) == 0x1000)
            projectile.Clear();
    }

    private static void RunGoldenTorizoEggHorizontalCharge(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (MoveProjectileAxis(projectile, level, horizontal: true))
        {
            projectile.PreInstruction =
                (ushort)EnemyProjectilePreInstruction.GoldenTorizoEgg_HitWall;
            projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(0));
            return;
        }

        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.X.RawValue +
            ((projectile.Variable0 & 0x8000) != 0 ? 48 : -48)))));
    }

    private static void RunGoldenTorizoEggFall(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (MoveProjectileAxis(projectile, level, horizontal: false))
        {
            projectile.InstructionPointer = (projectile.Variable0 & 0x8000) != 0
                ? GoldenTorizoEggInstructionProgramDefinitions.BreakRight
                : GoldenTorizoEggInstructionProgramDefinitions.BreakLeft;
            projectile.InstructionTimer = 1;
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + 48))));
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
        projectile.Position = projectile.Position.WithWhole(new(unchecked((ushort)(torizo.Position.X.Whole +
            ((torizo.Parameter1 & 0x8000) != 0 ? 32 : -32))), unchecked((ushort)(torizo.Position.Y.Whole - 52))));
    }

    private static void RunGoldenTorizoSuperMissileFlight(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        bool impact = MoveProjectileAxis(projectile, level, horizontal: true);
        if (!impact)
        {
            bool verticalCollision = MoveProjectileAxis(projectile, level, horizontal: false);
            impact = verticalCollision && unchecked((short)projectile.Velocity.Y.RawValue) >= 0;
        }

        if (impact)
        {
            projectile.InstructionPointer =
                GoldenTorizoSuperMissileInstructionProgramDefinitions.Impact;
            projectile.InstructionTimer = 1;
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + 16))));
        if ((projectile.Velocity.Y.RawValue & 0xf000) == 0x1000)
            projectile.Clear();
    }

    private static void SetGoldenTorizoSuperMissileVelocity(
        RoomEnemyProjectileSlot projectile,
        SamusState samus,
        bool awayFromSamus)
    {
        byte angle = CalculateCartridgeAngle(
            new(unchecked((short)(samus.Position.X.Whole - projectile.Position.X.Whole)), unchecked((short)(samus.Position.Y.Whole - projectile.Position.Y.Whole))));
        angle = awayFromSamus
            ? unchecked((byte)(angle | 0x80))
            : unchecked((byte)(angle & 0x7f));

        // Preserve the native up-zero angle convention: X is sine, Y is
        // negative cosine. Starting both reads at sine rotates the shot.
        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(4 *
            EnemyTrigonometryTables.SignedNegativeCosineWord(angle + 64)))));
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(4 *
            EnemyTrigonometryTables.SignedNegativeCosineWord(angle)))));
    }

    private static void RunGoldenTorizoEyeBeamPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (MoveProjectileAxis(projectile, level, horizontal: true))
        {
            projectile.InstructionPointer =
                GoldenTorizoEyeBeamInstructionProgramDefinitions.WallImpact;
            projectile.InstructionTimer = 1;
            return;
        }

        if (MoveProjectileAxis(projectile, level, horizontal: false))
        {
            projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(
                (projectile.Position.Y.Whole & 0xfff0) + 6)));
            projectile.InstructionPointer =
                GoldenTorizoEyeBeamInstructionProgramDefinitions.FloorImpact;
            projectile.InstructionTimer = 1;
        }
    }

    /// <summary>
    /// Ports instruction <c>$86:AB8A</c>. Its two inline header operands are Bomb Torizo's
    /// area-zero orb and Golden Torizo's nonzero-area orb. This runtime already identifies
    /// the concrete loaded variant from definition <c>$EEFF/$EF7F</c>, which is the exact
    /// room-scoped equivalent of consulting the global area byte.
    /// </summary>
    private void RequestTorizoChozoOrbDrop(
        RoomEnemyProjectileSlot projectile,
        ushort instructionCursor)
    {
        if (projectile.Kind is not (
                RoomEnemyProjectileKind.BombTorizoChozoOrb or
                RoomEnemyProjectileKind.GoldenTorizoChozoOrb))
        {
            throw new InvalidOperationException(
                $"Projectile {projectile.Kind} reached Torizo-orb drop opcode $AB8A.");
        }

        bool golden = GoldenTorizo is not null;
        ushort operand = ReadEnemyProjectileInstructionMechanicsWord(
            projectile,
            unchecked((ushort)(instructionCursor + (golden ? 4 : 2))));
        EnemyDefinitionId expectedHeader = golden
            ? EnemyDefinitionId.GoldenTorizoOrb
            : EnemyDefinitionId.BombTorizoOrb;
        if (operand != (ushort)expectedHeader)
        {
            throw new InvalidDataException(
                $"Torizo-orb drop operand selected header ${operand:X4}; expected " +
                $"${(ushort)expectedHeader:X4} for golden={golden}.");
        }
        EnemyDefinitionId header = expectedHeader;

        RoomEnemyDefinition definition = ResolveRoomEnemyDefinition(_bus!, header);
        _torizoOrbDropRequests.Add(new TorizoOrbDropRequest());
        SpawnEnemyDropFromChanceTable(
            projectile.Position.Whole,
            definition.ItemDropChancesPointer);
    }
}
