using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>The eye door species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class EyeDoorBehavior
{
    /// <summary>Provides the loaded room and random seed for smoke placement.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates eye-door projectile, sweat, and smoke slots.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Binds eye-door effects to a room and its enemy projectile pool.</summary>
    internal EyeDoorBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles)
    {
        _room = room;
        _projectiles = projectiles;
    }

    /// <summary>Door-bit owner captured when a live eye projectile is spawned.</summary>
    private SharedGameState? _eyeDoorProjectileSystem;

    /// <summary>
    /// Replays <c>SpawnEprojWithRoomGfx</c> and the definition-specific initializer selected
    /// by the eye-door PLM instruction. The PLM block index and room argument are retained
    /// because the cartridge initializers read both directly from their spawning PLM slot.
    /// </summary>
    public void SpawnEyeDoorProjectile(
        EyeDoorProjectileRequest request,
        int roomWidthInBlocks,
        SharedGameState system)
    {
        _room.EnsureLoaded();
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(roomWidthInBlocks);
        Ensure.NotNull(system);

        RoomEnemyProjectileKind kind = request.Definition switch
        {
            EyeDoorProjectileDefinition.Projectile =>
                RoomEnemyProjectileKind.EyeDoorProjectile,
            EyeDoorProjectileDefinition.Sweat =>
                RoomEnemyProjectileKind.EyeDoorSweat,
            EyeDoorProjectileDefinition.ScatteredSmoke or EyeDoorProjectileDefinition.CenteredSmoke =>
                RoomEnemyProjectileKind.EyeDoorSmoke,
            _ => throw new InvalidOperationException(
                $"Undefined {nameof(EyeDoorProjectileDefinition)} {request.Definition}."),
        };

        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return; // Native SpawnEproj returns carry set when all eighteen slots are live.

        _eyeDoorProjectileSystem = system;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, graphicsIndex: 0);

        var block = new Position16<RoomBlocks>(
            checked((ushort)(request.PlmBlockIndex % roomWidthInBlocks)),
            checked((ushort)(request.PlmBlockIndex / roomWidthInBlocks)));
        switch (kind)
        {
            case RoomEnemyProjectileKind.EyeDoorProjectile:
                InitializeEyeDoorProjectile(projectile, request, block);
                return;

            case RoomEnemyProjectileKind.EyeDoorSweat:
                InitializeEyeDoorSweat(projectile, request.Facing, block);
                return;

            case RoomEnemyProjectileKind.EyeDoorSmoke:
                if (request.Definition == EyeDoorProjectileDefinition.ScatteredSmoke)
                    InitializeEyeDoorSmoke(projectile, EyeDoorEffectDefinitions.ScatteredSmokeAnimation,
                        EyeDoorEffectDefinitions.ScatteredSmokePlacement, block);
                else
                    InitializeEyeDoorSmoke(projectile, EyeDoorEffectDefinitions.CenteredSmokeAnimation,
                        EyeDoorEffectDefinitions.CenteredSmokePlacement, block);
                return;

            case RoomEnemyProjectileKind.None:
            case RoomEnemyProjectileKind.PrePhantoonRoom:
            case RoomEnemyProjectileKind.YappingMawBody:
            case RoomEnemyProjectileKind.SkreeParticleDownRight:
            case RoomEnemyProjectileKind.SkreeParticleUpRight:
            case RoomEnemyProjectileKind.SkreeParticleDownLeft:
            case RoomEnemyProjectileKind.SkreeParticleUpLeft:
            case RoomEnemyProjectileKind.MetareeParticleDownRight:
            case RoomEnemyProjectileKind.MetareeParticleUpRight:
            case RoomEnemyProjectileKind.MetareeParticleDownLeft:
            case RoomEnemyProjectileKind.MetareeParticleUpLeft:
            case RoomEnemyProjectileKind.CrocomireProjectile:
            case RoomEnemyProjectileKind.CrocomireBridgeCrumbling:
            case RoomEnemyProjectileKind.CrocomireSpikeWallPieces:
            case RoomEnemyProjectileKind.KraidSpitRock:
            case RoomEnemyProjectileKind.KraidCeilingRock:
            case RoomEnemyProjectileKind.KraidRisingRockLeft:
            case RoomEnemyProjectileKind.KraidRisingRockRight:
            case RoomEnemyProjectileKind.PhantoonDestroyableFlame:
            case RoomEnemyProjectileKind.PhantoonStartingFlame:
            case RoomEnemyProjectileKind.DraygonGoop:
            case RoomEnemyProjectileKind.DraygonWallTurret:
            case RoomEnemyProjectileKind.MotherBrainRoomTurret:
            case RoomEnemyProjectileKind.MotherBrainRoomTurretBullet:
            case RoomEnemyProjectileKind.MotherBrainGlassShard:
            case RoomEnemyProjectileKind.MotherBrainGlassSparkle:
            case RoomEnemyProjectileKind.MotherBrainOnionRing:
            case RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion:
            case RoomEnemyProjectileKind.MotherBrainBomb:
            case RoomEnemyProjectileKind.MotherBrainHandBeamCharging:
            case RoomEnemyProjectileKind.MotherBrainHandBeamFired:
            case RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging:
            case RoomEnemyProjectileKind.MotherBrainPurpleBreathBig:
            case RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall:
            case RoomEnemyProjectileKind.MotherBrainDrool:
            case RoomEnemyProjectileKind.MotherBrainDyingDrool:
            case RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion:
            case RoomEnemyProjectileKind.MotherBrainDeathExplosion:
            case RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment:
            case RoomEnemyProjectileKind.MotherBrainEscapeSubtitle:
            case RoomEnemyProjectileKind.MotherBrainTopRightTube:
            case RoomEnemyProjectileKind.MotherBrainTopLeftTube:
            case RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube:
            case RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube:
            case RoomEnemyProjectileKind.CeresRidleyFireball:
            case RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter:
            case RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnCenter:
            case RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight:
            case RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnLeft:
            case RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnUp:
            case RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnDown:
            case RoomEnemyProjectileKind.CeresFallingDebrisLight:
            case RoomEnemyProjectileKind.CeresFallingDebrisDark:
            case RoomEnemyProjectileKind.GunshipLiftoffDustCloud:
            case RoomEnemyProjectileKind.AlcoonFireball:
            case RoomEnemyProjectileKind.PowampSpike:
            case RoomEnemyProjectileKind.WorkRobotLaserUpLeft:
            case RoomEnemyProjectileKind.WorkRobotLaserHorizontal:
            case RoomEnemyProjectileKind.WorkRobotLaserDownLeft:
            case RoomEnemyProjectileKind.WorkRobotLaserUpRight:
            case RoomEnemyProjectileKind.WorkRobotLaserDownRight:
            case RoomEnemyProjectileKind.FallingSpark:
            case RoomEnemyProjectileKind.NuclearWaffleBody:
            case RoomEnemyProjectileKind.FakeKraidSpit:
            case RoomEnemyProjectileKind.FakeKraidSpikeLeft:
            case RoomEnemyProjectileKind.FakeKraidSpikeRight:
            case RoomEnemyProjectileKind.KiHunterAcidSpitLeft:
            case RoomEnemyProjectileKind.KiHunterAcidSpitRight:
            case RoomEnemyProjectileKind.PirateMotherBrainLaser:
            case RoomEnemyProjectileKind.PirateClaw:
            case RoomEnemyProjectileKind.PolypRock:
            case RoomEnemyProjectileKind.CacatacSpike:
            case RoomEnemyProjectileKind.StokeProjectile:
            case RoomEnemyProjectileKind.NamiheFireball:
            case RoomEnemyProjectileKind.FuneFireball:
            case RoomEnemyProjectileKind.LavaThrownByMagdollite:
            case RoomEnemyProjectileKind.DragonFireball:
            case RoomEnemyProjectileKind.MiscDustExplosion:
            case RoomEnemyProjectileKind.EnemyDeathPickup:
            case RoomEnemyProjectileKind.EnemyDeathExplosion:
            case RoomEnemyProjectileKind.KagoBug:
            case RoomEnemyProjectileKind.BotwoonBody:
            case RoomEnemyProjectileKind.BotwoonSpit:
            case RoomEnemyProjectileKind.BombTorizoLowHealthDrool:
            case RoomEnemyProjectileKind.BombTorizoInitialDrool:
            case RoomEnemyProjectileKind.BombTorizoExplosiveSwipe:
            case RoomEnemyProjectileKind.BombTorizoLowHealthExplosion:
            case RoomEnemyProjectileKind.BombTorizoDeathExplosion:
            case RoomEnemyProjectileKind.BombTorizoStatueBreaking:
            case RoomEnemyProjectileKind.BombTorizoChozoOrb:
            case RoomEnemyProjectileKind.BombTorizoSonicBoom:
            case RoomEnemyProjectileKind.GoldenTorizoChozoOrb:
            case RoomEnemyProjectileKind.GoldenTorizoSonicBoom:
            case RoomEnemyProjectileKind.BombTorizoRightFootDust:
            case RoomEnemyProjectileKind.BombTorizoLeftFootDust:
            case RoomEnemyProjectileKind.GoldenTorizoEgg:
            case RoomEnemyProjectileKind.GoldenTorizoSuperMissile:
            case RoomEnemyProjectileKind.GoldenTorizoEyeBeam:
            case RoomEnemyProjectileKind.TourianStatueSplash:
            case RoomEnemyProjectileKind.TourianStatueEyeGlow:
            case RoomEnemyProjectileKind.TourianStatueParticle:
            case RoomEnemyProjectileKind.TourianStatueTail:
            case RoomEnemyProjectileKind.TourianStatueSoul:
            case RoomEnemyProjectileKind.TourianStatueRidley:
            case RoomEnemyProjectileKind.TourianStatuePhantoon:
            case RoomEnemyProjectileKind.TourianStatueBaseDecoration:
            case RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep:
            case RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstepAlternate:
            case RoomEnemyProjectileKind.TourianStatueDescentDust:
            case RoomEnemyProjectileKind.ShaktoolAttackFrontCircle:
            case RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle:
            case RoomEnemyProjectileKind.ShaktoolAttackBackCircle:
            case RoomEnemyProjectileKind.SporeSpawnStalk:
            case RoomEnemyProjectileKind.SporeSpawnSpore:
            case RoomEnemyProjectileKind.SporeSpawnSpawner:
            case RoomEnemyProjectileKind.SaveStationElectricity:
            case RoomEnemyProjectileKind.DownwardGateMoving:
            case RoomEnemyProjectileKind.DownwardGateClosed:
            case RoomEnemyProjectileKind.NoobTubeCrack:
            case RoomEnemyProjectileKind.NoobTubeShard:
            case RoomEnemyProjectileKind.NoobTubeReleasedAirBubble:
            default:
                throw new InvalidOperationException(
                    $"Eye-door projectile kind {kind} escaped its initializer dispatcher.");
        }
    }

    /// <summary>Places the eye's shot on the side the eye faces and retains its door bit.</summary>
    private static void InitializeEyeDoorProjectile(
        RoomEnemyProjectileSlot projectile,
        EyeDoorProjectileRequest request,
        Position16<RoomBlocks> block)
    {
        Offset16<RoomPixels> origin = request.Facing switch
        {
            EyeDoorOrientation.Left => EyeDoorEffectDefinitions.LeftShotOrigin,
            EyeDoorOrientation.Right => EyeDoorEffectDefinitions.RightShotOrigin,
            _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorOrientation)} {request.Facing}."),
        };
        projectile.Variable1 = request.DoorBit;
        projectile.Position = projectile.Position.WithWhole(block.ToRoomPixels() + origin);
    }

    /// <summary>Places sweat below the eye block and launches it toward the side the eye faces.</summary>
    private static void InitializeEyeDoorSweat(
        RoomEnemyProjectileSlot projectile,
        EyeDoorOrientation facing,
        Position16<RoomBlocks> block)
    {
        projectile.Position = projectile.Position.WithWhole(block.ToRoomPixels() + EyeDoorEffectDefinitions.SweatOrigin);
        projectile.Velocity = facing switch
        {
            EyeDoorOrientation.Left => EyeDoorEffectDefinitions.LeftSweatLaunch,
            EyeDoorOrientation.Right => EyeDoorEffectDefinitions.RightSweatLaunch,
            _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorOrientation)} {facing}."),
        };
    }

    /// <summary>
    /// Starts smoke showing <paramref name="animation"/> scattered by misc-dust placement <paramref name="placementIndex"/>:
    /// uses one shared random sample for both smoke offsets, then advances the seed.
    /// </summary>
    internal void InitializeEyeDoorSmoke(
        RoomEnemyProjectileSlot projectile,
        MiscDustAnimation animation,
        int placementIndex,
        Position16<RoomBlocks> block)
    {
        MiscDustProjectileProgram program = MiscDustProjectileDefinitions.Program(animation);
        MiscDustPlacementDefinition placement = MiscDustProjectileDefinitions.SmokePlacement(checked((ushort)placementIndex));

        int random = _room.Random.Read();
        // Native stores only the list pointer, keeping the definition's one-update timer.
        projectile.InstallAnimation(new MiscDustProjectileAnimation(program));

        projectile.Position = projectile.Position.WithWhole(block.ToRoomPixels() + new Offset16<RoomPixels>(
            unchecked((short)(8 + placement.Base.X + (random & placement.RandomMask.X))),
            unchecked((short)(8 + placement.Base.Y + ((SnesLittleEndian.HighByte(random)) & placement.RandomMask.Y)))));

        // $E4FA advances the global seed after sampling it for both coordinates.
        _room.Random.Generate();
    }

    /// <summary>Moves the eye shot, accelerates along its angle, and switches to impact on collision or door opening.</summary>
    internal void RunEyeDoorProjectilePreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (MoveEyeDoorEffectHorizontally(projectile, level) ||
            MoveEyeDoorEffectVertically(projectile, level))
        {
            StartEyeDoorProjectileImpact(projectile);
            return;
        }

        int angle = projectile.Variable0 >> 1;
        projectile.Velocity = projectile.Velocity.WithX(new SignedEightEight(unchecked((ushort)(projectile.Velocity.X.RawValue +
            ReadEyeDoorAcceleration(angle)))));
        projectile.Velocity = projectile.Velocity.WithY(new SignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue +
            ReadEyeDoorAcceleration(angle - 64)))));

        SharedGameState system = _eyeDoorProjectileSystem ??
            throw new InvalidOperationException("A live eye-door projectile has no door-bit owner.");
        if (system.HasOpenedDoorBit(projectile.Variable1))
            StartEyeDoorProjectileImpact(projectile);
    }

    /// <summary>Applies sweat gravity and enters the impact list on a downward vertical collision.</summary>
    internal static void RunEyeDoorSweatPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        MoveEyeDoorEffectHorizontally(projectile, level);
        bool collidedVertically = MoveEyeDoorEffectVertically(projectile, level);
        if (unchecked((short)projectile.Velocity.Y.RawValue) >= 0 && collidedVertically)
        {
            projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(projectile.Position.Y.Whole - 4)));
            // The floor splash stops the drop's movement, as the native list switch does.
            projectile.PreInstruction = EnemyProjectilePreInstruction.EyeDoorSmokeInertPreInstruction;
            projectile.StartAnimation(new EyeDoorSweatAnimation(EyeDoorSweatProgram.HitFloor));
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(new SignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + 12))));
    }

    /// <summary>Scales one signed sine-table sample to the eye shot's per-frame acceleration.</summary>
    internal static short ReadEyeDoorAcceleration(int tableIndex)
    {
        int sample = EnemyTrigonometryTables.SignedSine(unchecked(new ByteAngle((byte)tableIndex)));
        return unchecked((short)(sample >> 4));
    }

    /// <summary>Sends an eye-door shot into its typed impact program, stopping its movement as the native list switch does.</summary>
    private static void StartEyeDoorProjectileImpact(RoomEnemyProjectileSlot projectile)
    {
        projectile.PreInstruction = EnemyProjectilePreInstruction.EyeDoorSmokeInertPreInstruction;
        projectile.StartAnimation(new EyeDoorProjectileAnimation(EyeDoorProjectileProgram.Impact));
    }

    /// <summary>Carries out one cue of the eye-door shot's programs.</summary>
    private static void ApplyEyeDoorProjectileCue(RoomEnemyProjectileSlot projectile, IRoomSamus? samus, EyeDoorProjectileCue cue)
    {
        switch (cue.Kind)
        {
            case EyeDoorProjectileCueKind.AimAtSamus:
                AimEyeDoorProjectileAtSamus(projectile, samus ?? throw new InvalidOperationException("The eye-door shot's aim requires Samus."));
                break;
            case EyeDoorProjectileCueKind.SetPreInstruction:
                projectile.PreInstruction = cue.PreInstruction;
                break;
            case EyeDoorProjectileCueKind.ClearPreInstruction:
                projectile.PreInstruction = EnemyProjectilePreInstruction.RTS_868170;
                break;
            case EyeDoorProjectileCueKind.Delete:
                projectile.Clear();
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EyeDoorProjectileCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>
    /// Ports <c>Instruction_EnemyProjectile_CalculateDirectionTowardsSamus</c> ($86:82A5): stores the doubled angle to
    /// Samus and launches the shot along it at unit speed.
    /// </summary>
    private static void AimEyeDoorProjectileAtSamus(RoomEnemyProjectileSlot projectile, IRoomSamus samus)
    {
        projectile.Variable0 = unchecked((ushort)(2 * EnemyMath.CalculateBearing(
            new(unchecked((short)(samus.Position.X.Whole - projectile.Position.X.Whole)), unchecked((short)(samus.Position.Y.Whole - projectile.Position.Y.Whole)))).RawValue));
        int angle = projectile.Variable0 >> 1;
        projectile.Velocity = projectile.Velocity.WithX(new SignedEightEight(unchecked((ushort)EnemyTrigonometryTables.SignedSine(new ByteAngle((byte)angle)))));
        projectile.Velocity = projectile.Velocity.WithY(new SignedEightEight(unchecked((ushort)EnemyTrigonometryTables.SignedSine(new ByteAngle((byte)(angle - 64))))));
    }

    /// <summary>Advances horizontal subpixels or snaps to the first solid leading edge.</summary>
    private static bool MoveEyeDoorEffectHorizontally(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        FixedCoordinate moved = projectile.Position.AdvanceX(projectile.Velocity.X.ToSixteenSixteenDelta()).X;
        int edge = unchecked((short)projectile.Velocity.X.RawValue) < 0
            ? unchecked((ushort)(moved.Whole - projectile.Radius.X))
            : unchecked((ushort)(moved.Whole + projectile.Radius.X - 1));
        if (EyeDoorEffectEdgeIsSolid(
            level,
            edge,
            unchecked((ushort)(projectile.Position.Y.Whole - projectile.Radius.Y)),
            unchecked((ushort)(projectile.Position.Y.Whole + projectile.Radius.Y - 1)),
            edge: Axis.Horizontal))
        {
            projectile.Position = projectile.Position.WithFractionX(0);
            projectile.Position = projectile.Position.WithWholeX(SnapEnemyProjectileAxis(projectile.Position.X.Whole, edge, projectile.Radius.X,
                movingBack: unchecked((short)projectile.Velocity.X.RawValue) < 0));
            return true;
        }

        projectile.Position = projectile.Position.WithX(moved);
        return false;
    }

    /// <summary>
    /// The collision snap of $86:894F/$86:8A0D: against the blocking block's face, but only
    /// toward the move. Moving forward it never pulls the projectile back, and moving back it
    /// never pushes it forward, so a projectile that starts inside a block keeps its position.
    /// </summary>
    private static ushort SnapEnemyProjectileAxis(int current, int edge, int radius, bool movingBack)
    {
        if (movingBack)
        {
            // The far face of the blocking block: the last pixel of its sixteen, plus one.
            int face = unchecked((ushort)((edge | 15) + 1 + radius));
            return checked((ushort)(face <= current ? face : current));
        }
        // The near face of the blocking block: its first pixel.
        int near = unchecked((ushort)((edge & ~15) - radius));
        return checked((ushort)(near >= current ? near : current));
    }

    /// <summary>Advances vertical subpixels or snaps to the first solid leading edge.</summary>
    private static bool MoveEyeDoorEffectVertically(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        FixedCoordinate moved = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta()).Y;
        int edge = unchecked((short)projectile.Velocity.Y.RawValue) < 0
            ? unchecked((ushort)(moved.Whole - projectile.Radius.Y))
            : unchecked((ushort)(moved.Whole + projectile.Radius.Y - 1));
        if (EyeDoorEffectEdgeIsSolid(
            level,
            edge,
            unchecked((ushort)(projectile.Position.X.Whole - projectile.Radius.X)),
            unchecked((ushort)(projectile.Position.X.Whole + projectile.Radius.X - 1)),
            edge: Axis.Vertical))
        {
            projectile.Position = projectile.Position.WithFractionY(0);
            projectile.Position = projectile.Position.WithWholeY(SnapEnemyProjectileAxis(projectile.Position.Y.Whole, edge, projectile.Radius.Y,
                movingBack: unchecked((short)projectile.Velocity.Y.RawValue) < 0));
            return true;
        }

        projectile.Position = projectile.Position.WithY(moved);
        return false;
    }

    /// <summary>Scans every touched room block along one projectile collision edge.</summary>
    /// <remarks>
    /// $86:88B6/$86:897B count the blocks to test as ((end) - (start &amp; $FFF0)) &gt;&gt; 4 in 16 bits
    /// and test one more than that, stepping the level index by a row (horizontal edge) or a
    /// block (vertical edge). A zero radius on a block boundary makes the count underflow to
    /// $0FFF, so the scan runs on through the room until it meets a solid block.
    /// </remarks>
    private static bool EyeDoorEffectEdgeIsSolid(
        RoomLevelData level,
        int fixedAxisPixel,
        int spanStartPixel,
        int spanEndPixel,
        Axis edge)
    {
        int fixedBlock = unchecked((ushort)fixedAxisPixel) >> 4;
        int startBlock = unchecked((ushort)spanStartPixel) >> 4;
        int extraBlocks = unchecked((ushort)(spanEndPixel - (spanStartPixel & ~15))) >> 4;
        int width = level.Size.X;
        int linear = edge == Axis.Vertical ? fixedBlock * width + startBlock : startBlock * width + fixedBlock;
        int step = edge == Axis.Vertical ? 1 : width;
        for (int tested = 0; tested <= extraBlocks; tested++, linear += step)
        {
            if (tested > 0 && linear >= width * level.Size.Y)
            {
                throw new InvalidDataException(
                    "Enemy projectile block scan ran past the room's level data into other WRAM.");
            }
            int blockX = tested == 0 ? (edge == Axis.Vertical ? startBlock : fixedBlock) : linear % width;
            int blockY = tested == 0 ? (edge == Axis.Vertical ? fixedBlock : startBlock) : linear / width;
            int blockIndex = EnemyTerrain.ResolveEnemyCollisionBlockIndex(level, new(unchecked((ushort)(blockX)), unchecked((ushort)(blockY))));
            if (blockIndex < 0)
                return true;

            RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
            if (block.CollisionType is not (
                RoomCollisionType.Air or
                RoomCollisionType.SpikeAir or
                RoomCollisionType.SpecialAir or
                RoomCollisionType.ShootableAir or
                RoomCollisionType.UnusedAir or
                RoomCollisionType.BombableAir or
                RoomCollisionType.HorizontalExtension or
                RoomCollisionType.VerticalExtension or
                RoomCollisionType.GrappleBlock))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>Routes live eye shots and sweat through their distinct movement routines.</summary>
    internal void AddEyeDoorProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddCue<EyeDoorProjectileCue>((context, cue) => ApplyEyeDoorProjectileCue(context.Projectile, context.Samus?.RoomView, cue));
        // A shot eye-door projectile runs its typed shot program, the list its definition names.
        table.AddShotAnimation(RoomEnemyProjectileKind.EyeDoorProjectile, () => new EyeDoorProjectileAnimation(EyeDoorProjectileProgram.Shot));

        table.AddPreInstruction(context =>
        {
            RunEyeDoorProjectilePreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.EyeDoorProjectilePreInstruction);

        table.AddPreInstruction(context =>
        {
            RunEyeDoorSweatPreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.EyeDoorSweatPreInstruction);
    }
}
