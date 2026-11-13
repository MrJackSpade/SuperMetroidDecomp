using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>The eye door species owner: its per-room state, behaviour and the interactions it registers.</summary>
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
    /// Replays spawning a projectile with room graphics and the definition-specific initializer selected
    /// by the eye-door room object's animation. The room object's block and room argument are retained
    /// because the game initializers read both directly from their spawning room object.
    /// </summary>
    public void SpawnEyeDoorProjectile(
        EyeDoorProjectileRequest request,
        SharedGameState system)
    {
        _room.EnsureLoaded();
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

        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return; // Spawning fails when all eighteen projectile slots are live.

        _eyeDoorProjectileSystem = system;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, graphics: default);

        Position16<RoomBlocks> block = request.RoomObjectBlock;
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
        EnemyProjectile projectile,
        EyeDoorProjectileRequest request,
        Position16<RoomBlocks> block)
    {
        Offset16<RoomPixels> origin = request.Facing switch
        {
            EyeDoorOrientation.Left => EyeDoorEffectDefinitions.LeftShotOrigin,
            EyeDoorOrientation.Right => EyeDoorEffectDefinitions.RightShotOrigin,
            _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorOrientation)} {request.Facing}."),
        };
        projectile.State = new EyeDoorShotState(request.Door ?? throw new InvalidOperationException(
            $"The eye-door attack at block {request.RoomObjectBlock} was spawned without its door."));
        projectile.Position = projectile.Position.WithWhole(block.ToRoomPixels() + origin);
    }

    /// <summary>Places sweat below the eye block and launches it toward the side the eye faces.</summary>
    private static void InitializeEyeDoorSweat(
        EnemyProjectile projectile,
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
        EnemyProjectile projectile,
        MiscDustAnimation animation,
        int placementIndex,
        Position16<RoomBlocks> block)
    {
        MiscDustProjectileSegment segment = MiscDustProjectiles.Animation(animation);
        MiscDustPlacementDefinition placement = MiscDustProjectiles.SmokePlacement(placementIndex);

        int random = _room.Random.Read();
        // Stores only the animation, keeping the definition's one-update timer.
        projectile.InstallAnimation(new MiscDustProjectileAnimation(segment));

        projectile.Position = projectile.Position.WithWhole(block.ToRoomPixels() + new Offset16<RoomPixels>(
            CoordinateAxis.AsSigned(8 + placement.Base.X + placement.XScatter.Of(random)),
            CoordinateAxis.AsSigned(8 + placement.Base.Y + placement.YScatter.Of(random))));

        // The spawn advances the global seed after sampling it for both coordinates.
        _room.Random.Generate();
    }

    /// <summary>Moves the eye shot, accelerates along its angle, and switches to impact on collision or door opening.</summary>
    internal void RunEyeDoorProjectileFrameBehavior(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        if (MoveEyeDoorEffectHorizontally(projectile, level) ||
            MoveEyeDoorEffectVertically(projectile, level))
        {
            StartEyeDoorProjectileImpact(projectile);
            return;
        }

        EyeDoorShotState shot = projectile.StateOf<EyeDoorShotState>();
        projectile.Velocity = projectile.Velocity.WithX(projectile.Velocity.X + ReadEyeDoorAcceleration(shot.Heading));
        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y +
            ReadEyeDoorAcceleration(shot.Heading.TurnedBySteps(-EnemyTrigonometryAngleDefinitions.QuarterTurn)));

        SharedGameState system = _eyeDoorProjectileSystem ??
            throw new InvalidOperationException("A live eye-door projectile has no opened-door owner.");
        if (system.HasOpenedDoor(shot.Door))
            StartEyeDoorProjectileImpact(projectile);
    }

    /// <summary>Applies sweat gravity and starts the impact animation on a downward vertical collision.</summary>
    internal static void RunEyeDoorSweatFrameBehavior(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        MoveEyeDoorEffectHorizontally(projectile, level);
        bool collidedVertically = MoveEyeDoorEffectVertically(projectile, level);
        if (!projectile.Velocity.Y.IsNegative && collidedVertically)
        {
            projectile.Position = projectile.Position.WithWrappedWholeY(projectile.Position.Y.Whole - 4);
            // The floor splash stops the drop's movement, as the animation switch does.
            projectile.FrameBehavior = EnemyProjectileFrameBehavior.Stationary;
            projectile.StartAnimation(new EyeDoorSweatAnimation(EyeDoorSweatSegment.HitFloor));
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + EyeDoorSweatGravity);
    }

    /// <summary>
    /// The eye shot's per-update acceleration along <paramref name="angle"/>'s sine: a sixteenth of the one-pixel sine
    /// velocity, rounded toward decreasing coordinates, so a small negative sine still accelerates the shot back.
    /// </summary>
    internal static SignedEightEight ReadEyeDoorAcceleration(TrigonometryAngle angle) =>
        EnemyMath.SineVelocity(angle, 1).DividedRoundingDown(16);

    /// <summary>A falling sweat drop's downward acceleration per update, in 256ths of a pixel per update.</summary>
    private static readonly SignedEightEight EyeDoorSweatGravity = SignedEightEight.FromParts(0, 12);

    /// <summary>Sends an eye-door shot into its typed impact animation, stopping its movement as the animation switch does.</summary>
    private static void StartEyeDoorProjectileImpact(EnemyProjectile projectile)
    {
        projectile.FrameBehavior = EnemyProjectileFrameBehavior.Stationary;
        projectile.StartAnimation(new EyeDoorProjectileAnimation(EyeDoorProjectileSegment.Impact));
    }

    /// <summary>Carries out one cue of the eye-door shot's animations.</summary>
    private static void ApplyEyeDoorProjectileCue(EnemyProjectile projectile, IRoomSamus? samus, EyeDoorProjectileCue cue)
    {
        switch (cue.Kind)
        {
            case EyeDoorProjectileCueKind.AimAtSamus:
                AimEyeDoorProjectileAtSamus(projectile, samus ?? throw new InvalidOperationException("The eye-door shot's aim requires Samus."));
                break;
            case EyeDoorProjectileCueKind.SetFrameBehavior:
                projectile.FrameBehavior = cue.FrameBehavior;
                break;
            case EyeDoorProjectileCueKind.ClearFrameBehavior:
                projectile.FrameBehavior = EnemyProjectileFrameBehavior.Stationary;
                break;
            case EyeDoorProjectileCueKind.Delete:
                projectile.Delete();
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EyeDoorProjectileCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>
    /// Aims the projectile at Samus: keeps the angle to
    /// Samus and launches the shot along it at unit speed.
    /// </summary>
    private static void AimEyeDoorProjectileAtSamus(EnemyProjectile projectile, IRoomSamus samus)
    {
        EyeDoorShotState shot = projectile.StateOf<EyeDoorShotState>();
        shot.Heading = EnemyMath.CalculateBearing(
            new(CoordinateAxis.Difference(samus.Position.X.Whole, projectile.Position.X.Whole), CoordinateAxis.Difference(samus.Position.Y.Whole, projectile.Position.Y.Whole))).ToMirroredTableAngle();
        projectile.Velocity = projectile.Velocity.WithX(EnemyMath.SineVelocity(shot.Heading, 1));
        projectile.Velocity = projectile.Velocity.WithY(EnemyMath.SineVelocity(shot.Heading.TurnedBySteps(-EnemyTrigonometryAngleDefinitions.QuarterTurn), 1));
    }

    /// <summary>Advances horizontal subpixels or snaps to the first solid leading edge.</summary>
    private static bool MoveEyeDoorEffectHorizontally(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        FixedCoordinate moved = projectile.Position.AdvanceX(projectile.Velocity.X.ToSixteenSixteenDelta()).X;
        int edge = projectile.Velocity.X.IsNegative
            ? CoordinateAxis.Wrap(moved.Whole - projectile.Radius.X)
            : CoordinateAxis.Wrap(moved.Whole + projectile.Radius.X - 1);
        if (EyeDoorEffectEdgeIsSolid(
            level,
            edge,
            CoordinateAxis.Wrap(projectile.Position.Y.Whole - projectile.Radius.Y),
            CoordinateAxis.Wrap(projectile.Position.Y.Whole + projectile.Radius.Y - 1),
            edge: Axis.Horizontal))
        {
            projectile.Position = projectile.Position.WithFractionX(0);
            projectile.Position = projectile.Position.WithWholeX(SnapEnemyProjectileAxis(projectile.Position.X.Whole, edge, projectile.Radius.X,
                movingBack: projectile.Velocity.X.IsNegative));
            return true;
        }

        projectile.Position = projectile.Position.WithX(moved);
        return false;
    }

    /// <summary>
    /// The collision snap of the projectile block scans: against the blocking block's face, but only
    /// toward the move. Moving forward it never pulls the projectile back, and moving back it
    /// never pushes it forward, so a projectile that starts inside a block keeps its position.
    /// </summary>
    private static int SnapEnemyProjectileAxis(int current, int edge, int radius, bool movingBack)
    {
        if (movingBack)
        {
            // The far face of the blocking block: the last pixel of its sixteen, plus one.
            int face = CoordinateAxis.Wrap((edge | 15) + 1 + radius);
            return face <= current ? face : current;
        }
        // The near face of the blocking block: its first pixel.
        int near = CoordinateAxis.Wrap((edge & ~15) - radius);
        return near >= current ? near : current;
    }

    /// <summary>Advances vertical subpixels or snaps to the first solid leading edge.</summary>
    private static bool MoveEyeDoorEffectVertically(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        FixedCoordinate moved = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta()).Y;
        int edge = projectile.Velocity.Y.IsNegative
            ? CoordinateAxis.Wrap(moved.Whole - projectile.Radius.Y)
            : CoordinateAxis.Wrap(moved.Whole + projectile.Radius.Y - 1);
        if (EyeDoorEffectEdgeIsSolid(
            level,
            edge,
            CoordinateAxis.Wrap(projectile.Position.X.Whole - projectile.Radius.X),
            CoordinateAxis.Wrap(projectile.Position.X.Whole + projectile.Radius.X - 1),
            edge: Axis.Vertical))
        {
            projectile.Position = projectile.Position.WithFractionY(0);
            projectile.Position = projectile.Position.WithWholeY(SnapEnemyProjectileAxis(projectile.Position.Y.Whole, edge, projectile.Radius.Y,
                movingBack: projectile.Velocity.Y.IsNegative));
            return true;
        }

        projectile.Position = projectile.Position.WithY(moved);
        return false;
    }

    /// <summary>Scans every touched room block along one projectile collision edge.</summary>
    /// <remarks>
    /// The block scan (<see cref="EnemyProjectileBlockScan"/>) counts the blocks to test as the distance from the start's
    /// block edge to the end, over 16, wrapping past 65535, and tests one more than that, stepping a row down the edge's
    /// column (horizontal edge) or a block along the edge's row (vertical edge): its first row is taken within the scan
    /// cycle of 256, and a scan along a row past its end continues on the next row. A first block outside the room's grid
    /// is a wall. A zero radius on a block boundary makes the count run past zero to 4095, so the scan runs on through the
    /// room until it meets a solid block.
    /// </remarks>
    /// <param name="level">The room's terrain.</param>
    /// <param name="fixedAxisPixel">The leading edge's pixel on the axis of the move.</param>
    /// <param name="spanStartPixel">The first pixel of the edge's span across the move.</param>
    /// <param name="spanEndPixel">The last pixel of the edge's span across the move.</param>
    /// <param name="edge">The edge scanned: horizontal for a horizontal move, vertical for a vertical one.</param>
    /// <exception cref="InvalidDataException">The scan runs on past the room's last block.</exception>
    internal static bool EyeDoorEffectEdgeIsSolid(
        RoomLevelData level,
        int fixedAxisPixel,
        int spanStartPixel,
        int spanEndPixel,
        Axis edge)
    {
        var scan = EnemyProjectileBlockScan.Across(fixedAxisPixel, spanStartPixel, spanEndPixel, edge);
        for (int tested = 0; tested <= scan.ExtraBlocks; tested++)
        {
            Position16<RoomBlocks> scanned = scan.Block(tested, level.Size.X);
            // A first block outside the room is a wall, so every later block steps on from a block of the grid; one
            // that runs on past the room's last block reads terrain the room does not hold.
            if (tested > 0 && !level.Size.Contains(scanned))
                throw new InvalidDataException("Enemy projectile block scan ran past the end of the room's level data.");
            if (EnemyTerrain.ResolveEnemyCollisionBlock(level, scanned) is not { } block)
                return true;

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

    /// <summary>Routes live eye shots and sweat through their distinct movement behaviours.</summary>
    internal void AddEyeDoorProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddCue<EyeDoorProjectileCue>((context, cue) => ApplyEyeDoorProjectileCue(context.Projectile, context.Samus?.RoomView, cue));
        // A shot eye-door projectile runs its typed shot animation, the animation its definition names.
        table.AddShotAnimation(RoomEnemyProjectileKind.EyeDoorProjectile, () => new EyeDoorProjectileAnimation(EyeDoorProjectileSegment.Shot));

        table.AddFrameBehavior(context =>
        {
            RunEyeDoorProjectileFrameBehavior(context.Projectile, context.Level);
        }, EnemyProjectileFrameBehavior.EyeDoorShotFlight);

        table.AddFrameBehavior(context =>
        {
            RunEyeDoorSweatFrameBehavior(context.Projectile, context.Level);
        }, EnemyProjectileFrameBehavior.EyeDoorSweatFall);
    }
}
