using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>Spawns the recurring drool projectile used by Bomb Torizo's low-health attack.</summary>
    /// <param name="torizo">Low-health Bomb Torizo whose position, facing, and random stream seed the shot.</param>
    private void SpawnBombTorizoLowHealthDrool(RoomEnemySlot torizo) =>
        SpawnBombTorizoDrool(torizo, RoomEnemyProjectileKind.BombTorizoLowHealthDrool);

    /// <summary>Spawns the initial gut-break droplet before Bomb Torizo's recurring low-health volleys.</summary>
    /// <param name="torizo">Bomb Torizo supplying the initial droplet's origin and orientation.</param>
    private void SpawnBombTorizoInitialDrool(RoomEnemySlot torizo) =>
        SpawnBombTorizoDrool(torizo, RoomEnemyProjectileKind.BombTorizoInitialDrool);

    /// <summary>Creates either Bomb Torizo drool variant, preserving its distinct launch and random-consumption rules.</summary>
    /// <param name="torizo">Enemy whose state determines the projectile's spawn position and trajectory.</param>
    /// <param name="kind">Initial gut-break drool or recurring low-health drool projectile kind.</param>
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
                BombTorizoDroolInstructionProgramDefinitions.SelectLowHealthInitialProgram(
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
            int jitteredY = (random & 3) + torizo.YPosition;
            int yWithOffset = jitteredY + 0xfffb;
            projectile.YPosition = unchecked((ushort)yWithOffset);
            projectile.YVelocity = unchecked((ushort)((random & 0x001f) + 0x30 + (yWithOffset >> 16)));
            // $86:A68D-$A6C3: the X offset's ADC carries out of CLC; ADC X. Every branch
            // stores a zero X velocity.
            int jitteredX = (_nextRandom!() & 3) + torizo.XPosition;
            int carry = jitteredX >> 16;
            projectile.XPosition = (torizo.Parameter1 & 0x4000) != 0
                ? unchecked((ushort)jitteredX)
                : (torizo.Parameter1 & 0x8000) != 0
                    ? unchecked((ushort)(jitteredX + 8 + carry))
                    : unchecked((ushort)(jitteredX + 0xfff8 + carry));
            projectile.XVelocity = 0;
            return;
        }
        projectile.YPosition = unchecked((ushort)(torizo.YPosition - 5));

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
        projectile.XVelocity = unchecked((ushort)
            EnemyTrigonometryTables.SignedSine(unchecked((byte)angle)));
        projectile.YVelocity = unchecked((ushort)
            EnemyTrigonometryTables.SignedNegativeCosineWord(angle));
        projectile.XPosition = (torizo.Parameter1 & 0x8000) != 0
            ? unchecked((ushort)(torizo.XPosition + 8))
            : unchecked((ushort)(torizo.XPosition - 8));
    }

    /// <summary>Creates the swipe hitbox at the facing-adjusted offset selected by the attack parameter.</summary>
    /// <param name="torizo">Bomb Torizo whose facing and position anchor the swipe.</param>
    /// <param name="parameter">Attack-table selector for the swipe's horizontal and vertical offsets.</param>
    private void SpawnBombTorizoExplosiveSwipe(RoomEnemySlot torizo, ushort parameter)
    {
        BombTorizoSwipeDefinition definition =
            BombTorizoAttackDefinitions.Swipe(parameter);

        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;
        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoExplosiveSwipe,
            graphicsIndex: 0);

        projectile.XPosition = (torizo.Parameter1 & 0x8000) != 0
            ? unchecked((ushort)(torizo.XPosition - definition.XOffset))
            : unchecked((ushort)(torizo.XPosition + definition.XOffset));
        projectile.YPosition = unchecked((ushort)(
            torizo.YPosition + definition.YOffset));
    }

    /// <summary>Spawns the selected low-health explosion and stores its origin for subsequent movement.</summary>
    /// <param name="torizo">Bomb Torizo anchoring the explosion's attack-relative position.</param>
    /// <param name="parameter">Selector for the low-health explosion definition.</param>
    private void SpawnBombTorizoLowHealthExplosion(RoomEnemySlot torizo, ushort parameter)
    {
        BombTorizoExplosionDefinition definition =
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
        projectile.XPosition = unchecked((ushort)(
            torizo.XPosition + definition.XOffset));
        projectile.YPosition = unchecked((ushort)(
            torizo.YPosition + definition.YOffset));
        projectile.Variable0 = projectile.XPosition;
        projectile.Variable1 = projectile.YPosition;
    }

    /// <summary>Creates the death explosion at Bomb Torizo's current position and records that origin.</summary>
    /// <param name="torizo">Dying enemy whose position anchors the explosion.</param>
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

    /// <summary>Spawns Bomb Torizo's bouncing Chozo orb using its facing-specific launch definition.</summary>
    /// <param name="torizo">Enemy supplying the orb's origin and facing.</param>
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

    /// <summary>Launches Bomb Torizo's sonic boom in the direction encoded by its attack parameter.</summary>
    /// <param name="torizo">Enemy whose position and facing determine the projectile trajectory.</param>
    /// <param name="parameter">Direction value published by the attack instruction.</param>
    private void SpawnBombTorizoSonicBoom(RoomEnemySlot torizo, ushort parameter)
        => SpawnTorizoSonicBoom(
            torizo,
            parameter,
            RoomEnemyProjectileKind.BombTorizoSonicBoom);

    /// <summary>Launches Golden Torizo's sonic boom through the shared Torizo projectile path.</summary>
    /// <param name="torizo">Golden Torizo supplying the launch position and facing.</param>
    /// <param name="parameter">Direction value published by the attack instruction.</param>
    private void SpawnGoldenTorizoSonicBoom(RoomEnemySlot torizo, ushort parameter)
        => SpawnTorizoSonicBoom(
            torizo,
            parameter,
            RoomEnemyProjectileKind.GoldenTorizoSonicBoom);

    /// <summary>Initializes the shared Bomb/Golden Torizo sonic boom with facing-dependent direction and randomized height.</summary>
    /// <param name="torizo">Torizo actor anchoring the projectile.</param>
    /// <param name="parameter">Direction parameter copied into the projectile.</param>
    /// <param name="kind">Concrete projectile variant, selecting its instruction program and definition.</param>
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
            projectile.InstructionPointer =
                TorizoSonicBoomInstructionProgramDefinitions.FiredRight;
        }
        else
        {
            projectile.XPosition = unchecked((ushort)(torizo.XPosition - 32));
            projectile.XVelocity = unchecked((ushort)-624);
            projectile.InstructionPointer =
                TorizoSonicBoomInstructionProgramDefinitions.FiredLeft;
        }
    }

    /// <summary>Spawns Golden Torizo's bouncing orb using its variant-specific launch values.</summary>
    /// <param name="torizo">Golden Torizo supplying the orb's origin and facing.</param>
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

    /// <summary>Creates a timed Golden Torizo egg whose horizontal charge and fall follow its projectile handlers.</summary>
    /// <param name="torizo">Golden Torizo supplying egg origin, charge direction, and launch definition.</param>
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

    /// <summary>Creates a Golden Torizo super missile attached to the actor until its reflection launch begins.</summary>
    /// <param name="torizo">Golden Torizo whose slot, position, and facing initialize the missile.</param>
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
        projectile.InstructionPointer =
            GoldenTorizoProjectileDefinitions.GetReflectedSuperMissileInstruction(facingRight);
    }

    /// <summary>Spawns a Golden Torizo eye beam with the requested direction and randomized trigonometric velocity.</summary>
    /// <param name="torizo">Enemy whose position and facing anchor the beam.</param>
    /// <param name="parameter">Direction parameter copied to the projectile.</param>
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
        projectile.XVelocity = unchecked((ushort)(8 *
            EnemyTrigonometryTables.SignedNegativeCosineWord(sineIndex + 64)));
        projectile.YVelocity = unchecked((ushort)(8 *
            EnemyTrigonometryTables.SignedNegativeCosineWord(sineIndex)));
    }

    /// <summary>Applies a Torizo projectile definition and consumes two random bytes for velocity variation.</summary>
    /// <param name="projectile">Allocated projectile to initialize.</param>
    /// <param name="torizo">Actor whose coordinates form the projectile origin.</param>
    /// <param name="definition">Facing-selected list pointer, offsets, and base velocities.</param>
    private void InitializeTorizoRandomizedProjectile(
        RoomEnemyProjectileSlot projectile,
        RoomEnemySlot torizo,
        TorizoRandomizedProjectileDefinition definition)
    {
        projectile.InstructionPointer = definition.InstructionList;
        projectile.XPosition = unchecked((ushort)(torizo.XPosition +
            definition.XOffset));
        projectile.XVelocity = unchecked((ushort)(
            definition.BaseXVelocity +
            unchecked((byte)_nextRandom!()) - 128));
        projectile.YPosition = unchecked((ushort)(torizo.YPosition +
            definition.YOffset));
        projectile.YVelocity = unchecked((ushort)(
            definition.BaseYVelocity +
            unchecked((byte)_nextRandom!()) - 128));
    }

    /// <summary>Spawns landing dust beside the selected Bomb Torizo foot.</summary>
    /// <param name="torizo">Enemy whose position anchors the dust effect.</param>
    /// <param name="rightFoot">Selects the right-foot or left-foot effect and horizontal offset.</param>
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

    /// <summary>Advances Bomb Torizo's orb, converting wall or descending floor collisions into non-damaging impact lists.</summary>
    /// <param name="projectile">Orb state updated in place; impact transitions disable damage.</param>
    /// <param name="level">Room geometry used for axis-separated collision movement.</param>
    private void RunBombTorizoChozoOrbPreInstruction(
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
        if (unchecked((short)projectile.YVelocity) >= 0 && verticalCollision)
        {
            projectile.YPosition = unchecked((ushort)(
                (projectile.YPosition & 0xfff0) + 6));
            projectile.InstructionPointer =
                TorizoChozoOrbInstructionProgramDefinitions.FloorImpact;
            projectile.InstructionTimer = 1;
            projectile.CanDamageSamus = false;
            return;
        }

        projectile.YVelocity = unchecked((ushort)(projectile.YVelocity + 18));
        if ((projectile.YVelocity & 0xf000) == 0x1000)
            projectile.Clear();
    }

    /// <summary>Moves Bomb Torizo's sonic boom horizontally, then accelerates it until it exits its lifetime range.</summary>
    /// <param name="projectile">Sonic-boom state updated in place.</param>
    /// <param name="level">Room geometry used to detect the wall-impact transition.</param>
    private void RunBombTorizoSonicBoomPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (MoveProjectileAxis(projectile, level, horizontal: true))
        {
            projectile.InstructionPointer =
                TorizoSonicBoomInstructionProgramDefinitions.WallImpact;
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

    /// <summary>
    /// Ports <c>$86:A887</c>, shared by Bomb Torizo's six gut-break droplets and its
    /// low-health initial drool. The peculiar positive-three clamp is authentic: both the
    /// negative and nonnegative drag branches load <c>$0003</c> when they cross zero.
    /// </summary>
    private void RunBombTorizoDroolPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (MoveProjectileAxis(projectile, level, horizontal: true))
        {
            projectile.InstructionPointer =
                BombTorizoDroolInstructionProgramDefinitions.WallImpact;
            projectile.InstructionTimer = 1;
            return;
        }

        short xVelocity = unchecked((short)projectile.XVelocity);
        if (xVelocity < 0)
        {
            int dragged = xVelocity + 4;
            projectile.XVelocity = unchecked((ushort)(short)(dragged < 0 ? dragged : 3));
        }
        else
        {
            int dragged = xVelocity - 4;
            projectile.XVelocity = unchecked((ushort)(short)(dragged >= 0 ? dragged : 3));
        }

        bool verticalCollision = MoveProjectileAxis(projectile, level, horizontal: false);
        if (unchecked((short)projectile.YVelocity) >= 0 && verticalCollision)
        {
            projectile.YPosition = unchecked((ushort)(projectile.YPosition - 3));
            projectile.InstructionPointer =
                BombTorizoDroolInstructionProgramDefinitions.FloorImpact;
            projectile.InstructionTimer = 1;
            return;
        }

        projectile.YVelocity = unchecked((ushort)(projectile.YVelocity + 16));
        if ((projectile.YVelocity & 0xf000) == 0x1000)
            projectile.Clear();
    }

    /// <summary>Advances Golden Torizo's bouncing orb, damping horizontal motion and reflecting from the floor.</summary>
    /// <param name="projectile">Orb state updated in place; a low bounce selects the floor-impact list.</param>
    /// <param name="level">Room geometry used for horizontal and vertical collision checks.</param>
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
                projectile.InstructionPointer =
                    TorizoChozoOrbInstructionProgramDefinitions.FloorImpact;
                projectile.InstructionTimer = 1;
                projectile.CanDamageSamus = false;
                return;
            }
        }

        projectile.YVelocity = unchecked((ushort)(projectile.YVelocity + 24));
    }

    /// <summary>Runs the egg's arcing phase until its timer expires and it switches to a horizontal charge.</summary>
    /// <param name="projectile">Egg state containing the charge direction and remaining arc timer.</param>
    /// <param name="level">Room geometry used for wall bounce and floor reflection.</param>
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

    /// <summary>Moves the egg horizontally during its charge and changes to wall-impact handling on collision.</summary>
    /// <param name="projectile">Egg state whose velocity and pre-instruction may be updated.</param>
    /// <param name="level">Room geometry used to detect a wall collision.</param>
    private void RunGoldenTorizoEggHorizontalCharge(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (MoveProjectileAxis(projectile, level, horizontal: true))
        {
            projectile.PreInstruction =
                EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_GoldenTorizoEgg_HitWall;
            projectile.YVelocity = 0;
            return;
        }

        projectile.XVelocity = unchecked((ushort)(projectile.XVelocity +
            ((projectile.Variable0 & 0x8000) != 0 ? 48 : -48)));
    }

    /// <summary>Applies gravity during the egg's fall and selects the facing-specific break list on landing.</summary>
    /// <param name="projectile">Egg state updated until it collides with the floor.</param>
    /// <param name="level">Room geometry used for vertical movement and collision.</param>
    private void RunGoldenTorizoEggFall(
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

        projectile.YVelocity = unchecked((ushort)(projectile.YVelocity + 48));
    }

    /// <summary>Keeps the missile aligned to Golden Torizo's firing position, deleting it if the actor is absent.</summary>
    /// <param name="projectile">Attached missile state whose position follows the live Golden Torizo slot.</param>
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

    /// <summary>Moves the launched missile, selects its impact list on collision, and applies gravity until it leaves range.</summary>
    /// <param name="projectile">Missile state updated in place.</param>
    /// <param name="level">Room geometry used to detect horizontal or descending floor impact.</param>
    private void RunGoldenTorizoSuperMissileFlight(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        bool impact = MoveProjectileAxis(projectile, level, horizontal: true);
        if (!impact)
        {
            bool verticalCollision = MoveProjectileAxis(projectile, level, horizontal: false);
            impact = verticalCollision && unchecked((short)projectile.YVelocity) >= 0;
        }

        if (impact)
        {
            projectile.InstructionPointer =
                GoldenTorizoSuperMissileInstructionProgramDefinitions.Impact;
            projectile.InstructionTimer = 1;
            return;
        }

        projectile.YVelocity = unchecked((ushort)(projectile.YVelocity + 16));
        if ((projectile.YVelocity & 0xf000) == 0x1000)
            projectile.Clear();
    }

    /// <summary>Calculates a missile launch velocity directed toward Samus or away from her.</summary>
    /// <param name="projectile">Missile receiving the computed X and Y velocities.</param>
    /// <param name="samus">Target position used to calculate the cartridge angle.</param>
    /// <param name="awayFromSamus">When true, selects the opposite half-turn from the target angle.</param>
    private static void SetGoldenTorizoSuperMissileVelocity(
        RoomEnemyProjectileSlot projectile,
        SamusState samus,
        bool awayFromSamus)
    {
        byte angle = CalculateCartridgeAngle(
            unchecked((short)(samus.XPosition - projectile.XPosition)),
            unchecked((short)(samus.YPosition - projectile.YPosition)));
        angle = awayFromSamus
            ? unchecked((byte)(angle | 0x80))
            : unchecked((byte)(angle & 0x7f));

        // Preserve the native up-zero angle convention: X is sine, Y is
        // negative cosine. Starting both reads at sine rotates the shot.
        projectile.XVelocity = unchecked((ushort)(4 *
            EnemyTrigonometryTables.SignedNegativeCosineWord(angle + 64)));
        projectile.YVelocity = unchecked((ushort)(4 *
            EnemyTrigonometryTables.SignedNegativeCosineWord(angle)));
    }

    /// <summary>Moves the eye beam along both axes and selects the corresponding impact list on wall or floor collision.</summary>
    /// <param name="projectile">Eye-beam state updated in place.</param>
    /// <param name="level">Room geometry used for axis-separated collision movement.</param>
    private void RunGoldenTorizoEyeBeamPreInstruction(
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
            projectile.YPosition = unchecked((ushort)(
                (projectile.YPosition & 0xfff0) + 6));
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
        ushort header = ReadEnemyProjectileInstructionMechanicsWord(
            projectile,
            unchecked((ushort)(instructionCursor + (golden ? 4 : 2))));
        ushort expectedHeader = golden
            ? TorizoChozoOrbInstructionProgramDefinitions.GoldenOrbEnemyHeader
            : TorizoChozoOrbInstructionProgramDefinitions.BombOrbEnemyHeader;
        if (header != expectedHeader)
        {
            throw new InvalidDataException(
                $"Torizo-orb drop operand selected header ${header:X4}; expected " +
                $"${expectedHeader:X4} for golden={golden}.");
        }

        RoomEnemyDefinition definition = ResolveRoomEnemyDefinition(_bus!, header);
        _torizoOrbDropRequests.Add(new TorizoOrbDropRequest());
        SpawnEnemyDropFromChanceTable(
            projectile.XPosition,
            projectile.YPosition,
            definition.ItemDropChancesPointer);
    }
}
