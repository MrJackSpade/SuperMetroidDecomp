using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.NoobTube;

/// <summary>The Noob tube species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class NoobTubeBehavior
{
    /// <summary>The program each native even shard spawn parameter ($00-$12) selects, by parameter / 2.</summary>
    private static readonly NoobTubeProjectileProgram[] ShardPrograms =
    [
        NoobTubeProjectileProgram.Shard0, NoobTubeProjectileProgram.Shard1, NoobTubeProjectileProgram.Shard2,
        NoobTubeProjectileProgram.Shard3, NoobTubeProjectileProgram.Shard4, NoobTubeProjectileProgram.Shard5,
        NoobTubeProjectileProgram.Shard6, NoobTubeProjectileProgram.Shard7, NoobTubeProjectileProgram.Shard8,
        NoobTubeProjectileProgram.Shard9,
    ];

    /// <summary>Provides room-load state and the native RNG for shard and bubble launch angles.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates and advances crack, glass shard, and released-air-bubble projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Binds projectile initialization to the loaded room and its projectile slots.</summary>
    internal NoobTubeBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles)
    {
        _room = room;
        _projectiles = projectiles;
    }

    /// <summary>
    /// Executes the initializer selected by definitions $D904/$D912/$D920. The request
    /// retains the executing PLM block because native bank $86 reads the global PLM index
    /// while constructing each actor.
    /// </summary>
    public void SpawnNoobTubeProjectile(
        NoobTubeProjectileRequest request,
        int roomWidthInBlocks)
    {
        _room.EnsureLoaded();
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(roomWidthInBlocks);
        if (request.PlmBlockIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(request));

        RoomEnemyProjectileKind kind = request.Definition switch
        {
            NoobTubeProjectileDefinition.Crack => RoomEnemyProjectileKind.NoobTubeCrack,
            NoobTubeProjectileDefinition.Shard => RoomEnemyProjectileKind.NoobTubeShard,
            NoobTubeProjectileDefinition.ReleasedAirBubble =>
                RoomEnemyProjectileKind.NoobTubeReleasedAirBubble,
            _ => throw new InvalidOperationException(
                $"Undefined n00b-tube projectile definition {request.Definition}."),
        };

        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, graphics: default);

        int blockX = request.PlmBlockIndex % roomWidthInBlocks;
        int blockY = request.PlmBlockIndex / roomWidthInBlocks;
        switch (kind)
        {
            case RoomEnemyProjectileKind.NoobTubeCrack:
                if (request.Parameter != 0)
                    throw new InvalidDataException("N00b-tube crack parameter must be zero.");
                projectile.Position = projectile.Position.WithWhole(new(unchecked((ushort)(
                    blockX * 16 + NoobTubeDebrisDefinitions.CrackXOffset)), unchecked((ushort)(
                    blockY * 16 + NoobTubeDebrisDefinitions.CrackYOffset))));
                return;

            case RoomEnemyProjectileKind.NoobTubeShard:
            {
                int index = ValidateEvenNoobTubeParameter(
                    request.Parameter,
                    NoobTubeDebrisDefinitions.ShardOffsets.Length,
                    "shard");
                projectile.Variable1 = unchecked((ushort)(
                    blockX * 16 + NoobTubeDebrisDefinitions.CrackXOffset +
                    NoobTubeDebrisDefinitions.ShardOffsets[index].X));
                projectile.Variable0 = 0;
                projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(
                    blockY * 16 + NoobTubeDebrisDefinitions.CrackYOffset +
                    NoobTubeDebrisDefinitions.ShardOffsets[index].Y)));
                projectile.StartAnimation(new NoobTubeProjectileAnimation(ShardPrograms[index]));
                projectile.Velocity = NoobTubeDebrisDefinitions.ShardVelocities[index];
                return;
            }

            case RoomEnemyProjectileKind.NoobTubeReleasedAirBubble:
            {
                int index = ValidateEvenNoobTubeParameter(
                    request.Parameter,
                    NoobTubeDebrisDefinitions.BubbleOffsets.Length,
                    "released-air bubble");
                projectile.Variable1 = unchecked((ushort)(
                    blockX * 16 + NoobTubeDebrisDefinitions.BubbleOffsets[index].X));
                projectile.Variable0 = 0;
                projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(
                    blockY * 16 + NoobTubeDebrisDefinitions.BubbleOffsets[index].Y)));
                projectile.Velocity = projectile.Velocity.WithY(NoobTubeDebrisDefinitions.BubbleInitialYVelocity);
                return;
            }

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
            case RoomEnemyProjectileKind.EyeDoorProjectile:
            case RoomEnemyProjectileKind.EyeDoorSweat:
            case RoomEnemyProjectileKind.EyeDoorSmoke:
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
            default:
                throw new InvalidDataException(
                    $"N00b-tube initializer lost projectile kind {kind}.");
        }
    }

    /// <summary>Converts a native even-byte table offset to a shard or bubble entry index, rejecting odd and out-of-range operands.</summary>
    private static int ValidateEvenNoobTubeParameter(
        int parameter,
        int entryCount,
        string family)
    {
        if ((parameter & 1) != 0 || parameter / 2 >= entryCount)
        {
            throw new InvalidDataException(
                $"N00b-tube {family} parameter ${parameter:X4} is outside its even table.");
        }
        return parameter / 2;
    }

    /// <summary>Runs the six crack, shard, and bubble callbacks, including flicker timing and off-screen shard deletion.</summary>
    internal void RunNoobTubeProjectilePreInstruction(
        RoomEnemyProjectileSlot projectile,
        int cameraY)
    {
        EnemyProjectilePreInstruction preInstruction = projectile.PreInstruction;
        switch (preInstruction)
        {
            case EnemyProjectilePreInstruction.NoobTubeCrackFlickering:
                if (projectile.Position.X.Whole != NoobTubeDebrisDefinitions.HiddenXPosition)
                    projectile.Variable0 = projectile.Position.X.Whole;
                projectile.Position = projectile.Position.WithWholeX((_projectiles.CurrentFrame8 & 1) != 0
                    ? NoobTubeDebrisDefinitions.HiddenXPosition
                    : checked((ushort)projectile.Variable0));
                return;

            case EnemyProjectilePreInstruction.NoobTubeCrackFalling:
                projectile.Position = projectile.Position.AdvanceY(NoobTubeDebrisDefinitions.CrackFallVelocity.ToSixteenSixteenDelta());
                return;

            case EnemyProjectilePreInstruction.NoobTubeShardFlying:
                AdvanceNoobTubeShardX(projectile, projectile.Velocity.X);
                projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
                EnemyProjectileEngine.DeleteEnemyProjectileIfVerticallyOffScreen(projectile, cameraY);
                return;

            case EnemyProjectilePreInstruction.NoobTubeShardFalling:
                MoveNoobTubeProjectileHorizontallyAlongArc(projectile, angleStep: 2);
                projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
                EnemyProjectileEngine.DeleteEnemyProjectileIfVerticallyOffScreen(projectile, cameraY);
                return;

            case EnemyProjectilePreInstruction.NoobTubeBubbleFalling:
                // `$86:D89F` performs only the curved horizontal update before falling
                // through to `$D8DF`, which applies the bubble's vertical velocity once.
                MoveNoobTubeProjectileHorizontallyAlongArc(projectile, angleStep: 4);
                MoveNoobTubeBubbleVertically(projectile);
                return;

            case EnemyProjectilePreInstruction.NoobTubeBubbleFlying:
                MoveNoobTubeBubbleVertically(projectile);
                return;

            case EnemyProjectilePreInstruction.None:
            case EnemyProjectilePreInstruction.PrePhantoonRoom:
            case EnemyProjectilePreInstruction.RTS_868170:
            case EnemyProjectilePreInstruction.RTS_86A327:
            case EnemyProjectilePreInstruction.RTS_8684FB:
            case EnemyProjectilePreInstruction.RTS_86EC94:
            case EnemyProjectilePreInstruction.RTS_86D0EB:
            case EnemyProjectilePreInstruction.RTS_86CFF7:
            case EnemyProjectilePreInstruction.RTS_86B4B0:
            case EnemyProjectilePreInstruction.RTS_868D54:
            case EnemyProjectilePreInstruction.RTS_86950C:
            case EnemyProjectilePreInstruction.RTS_869A44:
            case EnemyProjectilePreInstruction.RTS_86BBC6:
            case EnemyProjectilePreInstruction.RTS_86A05B:
            case EnemyProjectilePreInstruction.RTS_86EFDF:
            case EnemyProjectilePreInstruction.RTS_86A919:
            case EnemyProjectilePreInstruction.BombTorizoStatueFragment_Stopped:
            case EnemyProjectilePreInstruction.RTS_86DD44:
            case EnemyProjectilePreInstruction.RTS_86CAA3:
            case EnemyProjectilePreInstruction.RTS_86C76D:
            case EnemyProjectilePreInstruction.RTS_86E6D1:
            case EnemyProjectilePreInstruction.DownwardGateInertPreInstruction:
            case EnemyProjectilePreInstruction.EyeDoorSmokeInertPreInstruction:
            case EnemyProjectilePreInstruction.DownwardGateMovementPreInstruction:
            case EnemyProjectilePreInstruction.EyeDoorProjectilePreInstruction:
            case EnemyProjectilePreInstruction.EyeDoorSweatPreInstruction:
            case EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling:
            case EnemyProjectilePreInstruction.Pickup:
            case EnemyProjectilePreInstruction.MotherBrainsTurrets:
            case EnemyProjectilePreInstruction.MotherBrainsTurretBullets:
            case EnemyProjectilePreInstruction.MotherBrainGlassShattering_Shard:
            case EnemyProjectilePreInstruction.MotherBrainsTubeFalling:
            case EnemyProjectilePreInstruction.MotherBrainsDrool:
            case EnemyProjectilePreInstruction.MotherBrainsDrool_Falling:
            case EnemyProjectilePreInstruction.MotherBrainsOnionRings:
            case EnemyProjectilePreInstruction.MotherBrainsBomb:
            case EnemyProjectilePreInstruction.MotherBrainRainbowBeam_Charging:
            case EnemyProjectilePreInstruction.MotherBrainsRainbowBeamExplosion:
            case EnemyProjectilePreInstruction.MotherBrainDeathExplosionPreInstruction:
            case EnemyProjectilePreInstruction.MotherBrainDeathDoorFragmentPreInstruction:
            case EnemyProjectilePreInstruction.MotherBrainDeathSubtitlePreInstruction:
            case EnemyProjectilePreInstruction.DraygonGoop_StuckToSamus:
            case EnemyProjectilePreInstruction.DraygonsWallTurretProjectile_Fired:
            case EnemyProjectilePreInstruction.DraygonGoop:
            case EnemyProjectilePreInstruction.Spores:
            case EnemyProjectilePreInstruction.SporeSpawner:
            case EnemyProjectilePreInstruction.BotwoonsBody:
            case EnemyProjectilePreInstruction.BotwoonsSpit:
            case EnemyProjectilePreInstruction.BombTorizosChozoOrbs:
            case EnemyProjectilePreInstruction.GoldenTorizosChozoOrbs:
            case EnemyProjectilePreInstruction.TorizoSonicBoom:
            case EnemyProjectilePreInstruction.BombTorizoLowHealthDrool_Falling:
            case EnemyProjectilePreInstruction.GoldenTorizoEgg_Bouncing:
            case EnemyProjectilePreInstruction.GoldenTorizoEgg_Hatched:
            case EnemyProjectilePreInstruction.GoldenTorizoEgg_HitWall:
            case EnemyProjectilePreInstruction.GoldenTorizoSuperMissile_Held:
            case EnemyProjectilePreInstruction.GoldenTorizoSuperMissile_Thrown:
            case EnemyProjectilePreInstruction.GoldenTorizoEyeBeam:
            case EnemyProjectilePreInstruction.TourianStatueBaseDecoration_AllowProcess:
            case EnemyProjectilePreInstruction.TourianStatue_Ridley_Phantoon_BaseDecor:
            case EnemyProjectilePreInstruction.ShaktoolsAttack_Front:
            case EnemyProjectilePreInstruction.ShaktoolsAttack_MiddleBack_Moving:
            case EnemyProjectilePreInstruction.MiscDust:
            case EnemyProjectilePreInstruction.DragonFireball:
            case EnemyProjectilePreInstruction.RidleyFireball:
            case EnemyProjectilePreInstruction.HorizontalAfterburn:
            case EnemyProjectilePreInstruction.VerticalAfterburn:
            case EnemyProjectilePreInstruction.MetalSkreeParticle:
            case EnemyProjectilePreInstruction.CrocomiresProjectile_Setup:
            case EnemyProjectilePreInstruction.CrocomiresProjectile_Fired:
            case EnemyProjectilePreInstruction.CrocomireSpikeWallPieces:
            case EnemyProjectilePreInstruction.KraidRocks:
            case EnemyProjectilePreInstruction.KraidCeilingRocks:
            case EnemyProjectilePreInstruction.KraidRockSpit_UsePalette0:
            case EnemyProjectilePreInstruction.PhantoonStartingFlames:
            case EnemyProjectilePreInstruction.PhantoonStartingFlames_Activated:
            case EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Casual_Falling:
            case EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Casual_HitGround:
            case EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Casual_Bouncing:
            case EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Enraged:
            case EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Rain:
            case EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Spiral:
            case EnemyProjectilePreInstruction.CrocomireBridgeCrumbling:
            case EnemyProjectilePreInstruction.AlcoonFireball:
            case EnemyProjectilePreInstruction.KiHunterAcid_Moving:
            case EnemyProjectilePreInstruction.KiHunterAcid_Left:
            case EnemyProjectilePreInstruction.KiHunterAcid_Right:
            case EnemyProjectilePreInstruction.PowampSpike:
            case EnemyProjectilePreInstruction.WreckedShipRobotLaser:
            case EnemyProjectilePreInstruction.StokeFireball:
            case EnemyProjectilePreInstruction.CacatacSpike:
            case EnemyProjectilePreInstruction.PolypRock:
            case EnemyProjectilePreInstruction.NamiFuneFireball:
            case EnemyProjectilePreInstruction.MagdolliteLava:
            case EnemyProjectilePreInstruction.KagoBug_Idle:
            case EnemyProjectilePreInstruction.KagoBug_Jumping:
            case EnemyProjectilePreInstruction.KagoBug_Falling:
            case EnemyProjectilePreInstruction.FallingSpark:
            case EnemyProjectilePreInstruction.CeresFallingTile:
            case EnemyProjectilePreInstruction.MiniKraidSpit:
            case EnemyProjectilePreInstruction.MiniKraidSpikes:
            case EnemyProjectilePreInstruction.Pirate_MotherBrain_Laser_Left:
            case EnemyProjectilePreInstruction.Pirate_MotherBrain_Laser_Right:
            case EnemyProjectilePreInstruction.PirateClaw_Left:
            case EnemyProjectilePreInstruction.PirateClaw_Right:
            case EnemyProjectilePreInstruction.TourianStatueSplashMotion:
            case EnemyProjectilePreInstruction.TourianStatueParticleMotion:
            case EnemyProjectilePreInstruction.TourianStatueSoulMotion:
            default:
                throw new InvalidOperationException(
                    $"Undefined n00b-tube projectile pre-instruction {preInstruction}.");
        }
    }

    /// <summary>Advances the shard's separate 16.16 horizontal position, kept in its projectile variables one and zero.</summary>
    private static void AdvanceNoobTubeShardX(RoomEnemyProjectileSlot projectile, SignedEightEight velocity)
    {
        FixedCoordinate x = EnemyMath.AddEightBitVelocity(
            new FixedCoordinate(checked((ushort)projectile.Variable1), checked((ushort)projectile.Variable0)), velocity);
        projectile.Variable1 = x.Whole;
        projectile.Variable0 = x.Fraction;
    }

    /// <summary>Uses the native sine table to advance the projectile's separate horizontal word and fraction, then steps its angle.</summary>
    internal static void MoveNoobTubeProjectileHorizontallyAlongArc(
        RoomEnemyProjectileSlot projectile,
        int angleStep)
    {
        // The native even table offset ORs in a quarter turn: the angle's bit 6.
        int sine = EnemyTrigonometryTables.SignedSine(unchecked(new ByteAngle((byte)((projectile.Velocity.X.RawValue >> 1) | 64))));
        AdvanceNoobTubeShardX(projectile, SignedEightEight.FromSignedRaw(checked((short)(sine >> 2))));
        projectile.Velocity = projectile.Velocity.WithX(new SignedEightEight(unchecked((ushort)(projectile.Velocity.X.RawValue + angleStep))));
    }

    /// <summary>Advances the bubble by its vertical velocity and publishes its separately accumulated horizontal word.</summary>
    private static void MoveNoobTubeBubbleVertically(RoomEnemyProjectileSlot projectile)
    {
        projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta()).WithWholeX(checked((ushort)projectile.Variable1));
    }

    /// <summary>Connects all six native projectile pre-instruction identities to the shared room callback.</summary>
    internal void AddNoobTubeProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddPreInstruction(context =>
        {
            RunNoobTubeProjectilePreInstruction(context.Projectile, context.Camera.Y);
        }, EnemyProjectilePreInstruction.NoobTubeCrackFlickering, EnemyProjectilePreInstruction.NoobTubeCrackFalling, EnemyProjectilePreInstruction.NoobTubeShardFlying, EnemyProjectilePreInstruction.NoobTubeShardFalling, EnemyProjectilePreInstruction.NoobTubeBubbleFalling, EnemyProjectilePreInstruction.NoobTubeBubbleFlying);
    }

    /// <summary>Carries out the cues of the crack, shard and bubble programs.</summary>
    internal void AddNoobTubeProjectileCues(EnemyProjectileHandlerTable table) =>
        table.AddCue<NoobTubeProjectileCue>((context, cue) => ApplyNoobTubeProjectileCue(context.Projectile, cue));

    /// <summary>Carries out one cue of the crack, shard and bubble programs.</summary>
    private void ApplyNoobTubeProjectileCue(RoomEnemyProjectileSlot projectile, NoobTubeProjectileCue cue)
    {
        bool oddFrame = (_projectiles.CurrentFrame8 & 1) != 0;
        switch (cue.Kind)
        {
            case NoobTubeProjectileCueKind.FlickerReflected:
                // Odd frames show the first pose at the shard's column; even frames the reflected pose, mirrored across the tube.
                projectile.Position = projectile.Position.WithWholeX(oddFrame
                    ? checked((ushort)projectile.Variable1)
                    : unchecked((ushort)(NoobTubeDebrisDefinitions.ShardMirrorSpan - projectile.Variable1)));
                ShardAnimation(projectile).ShowsReflectedPose = !oddFrame;
                break;
            case NoobTubeProjectileCueKind.FlickerHidden:
                projectile.Position = projectile.Position.WithWholeX(oddFrame ? checked((ushort)projectile.Variable1) : NoobTubeDebrisDefinitions.HiddenXPosition);
                break;
            case NoobTubeProjectileCueKind.AssignShardFallingAngle:
                projectile.Velocity = new(new SignedEightEight(SnesLittleEndian.HighByte(_room.Random.Generate())), NoobTubeDebrisDefinitions.ShardFallYVelocity);
                break;
            case NoobTubeProjectileCueKind.AssignBubbleFallingAngle:
                projectile.Velocity = projectile.Velocity.WithX(new SignedEightEight(SnesLittleEndian.HighByte(_room.Random.Generate())));
                break;
            case NoobTubeProjectileCueKind.SetPreInstruction:
                projectile.PreInstruction = cue.PreInstruction;
                break;
            case NoobTubeProjectileCueKind.Delete:
                projectile.Clear();
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(NoobTubeProjectileCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>The typed animation a shard's flicker cue runs in.</summary>
    private static NoobTubeProjectileAnimation ShardAnimation(RoomEnemyProjectileSlot projectile) =>
        projectile.Animation as NoobTubeProjectileAnimation
        ?? throw new InvalidOperationException($"A n00b-tube flicker cue ran outside its typed program; the slot runs {projectile.Animation}.");
}
