using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.NoobTube;

/// <summary>The Noob tube species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class NoobTubeBehavior
{
    /// <summary>The animation each shard of the burst runs, in burst order.</summary>
    private static readonly NoobTubeProjectileSegment[] ShardAnimations =
    [
        NoobTubeProjectileSegment.Shard0, NoobTubeProjectileSegment.Shard1, NoobTubeProjectileSegment.Shard2,
        NoobTubeProjectileSegment.Shard3, NoobTubeProjectileSegment.Shard4, NoobTubeProjectileSegment.Shard5,
        NoobTubeProjectileSegment.Shard6, NoobTubeProjectileSegment.Shard7, NoobTubeProjectileSegment.Shard8,
        NoobTubeProjectileSegment.Shard9,
    ];

    /// <summary>The pictures and shading every tube piece draws with: the shared sheet in the tube's crack colours.</summary>
    private static readonly SpriteGraphics TubePieceGraphics =
        SpriteGraphics.SharedIn(PictureShading.Shaded(ChozoAndTubeShades.TubeCracks));

    /// <summary>Provides room-load state and the RNG for shard and bubble launch angles.</summary>
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
    /// Executes the initializer selected by the tube-crack definitions. The request
    /// retains the executing room object's block because the projectile spawn reads the current room object
    /// while constructing each actor.
    /// </summary>
    public void SpawnNoobTubeProjectile(
        NoobTubeProjectileRequest request)
    {
        _room.EnsureLoaded();

        RoomEnemyProjectileKind kind = request.Definition switch
        {
            NoobTubeProjectileDefinition.Crack => RoomEnemyProjectileKind.NoobTubeCrack,
            NoobTubeProjectileDefinition.Shard => RoomEnemyProjectileKind.NoobTubeShard,
            NoobTubeProjectileDefinition.ReleasedAirBubble =>
                RoomEnemyProjectileKind.NoobTubeReleasedAirBubble,
            _ => throw new InvalidOperationException(
                $"Undefined n00b-tube projectile definition {request.Definition}."),
        };

        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        // The tube's pieces draw from the shared sheet in the crack colours the room's crack initializer installs (#1512).
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, graphics: TubePieceGraphics);

        int blockX = request.RoomObjectBlock.X;
        int blockY = request.RoomObjectBlock.Y;
        switch (kind)
        {
            case RoomEnemyProjectileKind.NoobTubeCrack:
                if (request.Piece is not null)
                    throw new InvalidDataException("The n00b tube has one crack; a crack names no piece.");
                projectile.Position = projectile.Position.WithWhole(new(CoordinateAxis.Wrap(
                    blockX * 16 + NoobTubeDebrisDefinitions.CrackXOffset), CoordinateAxis.Wrap(
                    blockY * 16 + NoobTubeDebrisDefinitions.CrackYOffset)));
                projectile.State = new NoobTubeCrackState();
                return;

            case RoomEnemyProjectileKind.NoobTubeShard:
            {
                int index = RequireNoobTubePiece(
                    request.Piece,
                    NoobTubeDebrisDefinitions.ShardOffsets.Length,
                    "shard");
                projectile.State = new NoobTubeDebrisState(
                    blockX * 16 + NoobTubeDebrisDefinitions.CrackXOffset +
                    NoobTubeDebrisDefinitions.ShardOffsets[index].X);
                projectile.Position = projectile.Position.WithWrappedWholeY(
                    blockY * 16 + NoobTubeDebrisDefinitions.CrackYOffset +
                    NoobTubeDebrisDefinitions.ShardOffsets[index].Y);
                projectile.StartAnimation(new NoobTubeProjectileAnimation(ShardAnimations[index]));
                projectile.Velocity = NoobTubeDebrisDefinitions.ShardVelocities[index];
                return;
            }

            case RoomEnemyProjectileKind.NoobTubeReleasedAirBubble:
            {
                int index = RequireNoobTubePiece(
                    request.Piece,
                    NoobTubeDebrisDefinitions.BubbleOffsets.Length,
                    "released-air bubble");
                projectile.State = new NoobTubeDebrisState(
                    blockX * 16 + NoobTubeDebrisDefinitions.BubbleOffsets[index].X);
                projectile.Position = projectile.Position.WithWrappedWholeY(
                    blockY * 16 + NoobTubeDebrisDefinitions.BubbleOffsets[index].Y);
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

    /// <summary>Requires the shard or bubble a request names to be one of the <paramref name="pieceCount"/> the burst has.</summary>
    private static int RequireNoobTubePiece(
        int? piece,
        int pieceCount,
        string family)
    {
        if (piece is not { } named || named < 0 || named >= pieceCount)
        {
            throw new InvalidDataException(
                $"N00b-tube {family} piece '{piece}' is not one of the burst's {pieceCount}.");
        }
        return named;
    }

    /// <summary>Flickers the crack: alternate updates park it off to the side and return it to its shown column.</summary>
    private void FlickerNoobTubeCrack(EnemyProjectile projectile)
    {
        NoobTubeCrackState crack = projectile.StateOf<NoobTubeCrackState>();
        if (projectile.Position.X.Whole != NoobTubeDebrisDefinitions.HiddenXPosition)
            crack.ShownX = projectile.Position.X.Whole;
        projectile.Position = projectile.Position.WithWholeX(NoobTubeCadenceDefinitions.CrackHidden.IsSetIn(_projectiles.CurrentFrame8)
            ? NoobTubeDebrisDefinitions.HiddenXPosition
            : crack.ShownX);
    }

    /// <summary>Drops the detached crack at its fixed fall speed.</summary>
    private static void DropNoobTubeCrack(EnemyProjectile projectile) =>
        projectile.Position = projectile.Position.AdvanceY(NoobTubeDebrisDefinitions.CrackFallVelocity.ToSixteenSixteenDelta());

    /// <summary>Flies a newly emitted shard by its velocity and deletes it once it leaves the screen vertically.</summary>
    private static void FlyNoobTubeShard(EnemyProjectile projectile, int cameraY)
    {
        AdvanceNoobTubeShardX(projectile, projectile.Velocity.X);
        projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
        EnemyProjectileEngine.DeleteEnemyProjectileIfVerticallyOffScreen(projectile, cameraY);
    }

    /// <summary>Sways a falling shard along its arc, drops it, and deletes it once it leaves the screen vertically.</summary>
    private static void DropNoobTubeShard(EnemyProjectile projectile, int cameraY)
    {
        MoveNoobTubeProjectileHorizontallyAlongArc(projectile, angleStep: 2);
        projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
        EnemyProjectileEngine.DeleteEnemyProjectileIfVerticallyOffScreen(projectile, cameraY);
    }

    /// <summary>Sways a falling bubble along its arc, then moves it vertically once as a flying bubble does.</summary>
    private static void DropNoobTubeBubble(EnemyProjectile projectile)
    {
        MoveNoobTubeProjectileHorizontallyAlongArc(projectile, angleStep: 4);
        MoveNoobTubeBubbleVertically(projectile);
    }

    /// <summary>Advances the debris's own 16.16 horizontal position.</summary>
    private static void AdvanceNoobTubeShardX(EnemyProjectile projectile, SignedEightEight velocity)
    {
        NoobTubeDebrisState debris = projectile.StateOf<NoobTubeDebrisState>();
        debris.X = EnemyMath.AddEightBitVelocity(debris.X, velocity);
    }

    /// <summary>Uses the sine of the debris's sway to advance its own horizontal position, then steps the sway.</summary>
    internal static void MoveNoobTubeProjectileHorizontallyAlongArc(
        EnemyProjectile projectile,
        int angleStep)
    {
        NoobTubeDebrisState debris = projectile.StateOf<NoobTubeDebrisState>();
        AdvanceNoobTubeShardX(projectile, SwayVelocity(debris.SwayAngle));
        debris.AdvanceSway(angleStep);
    }

    /// <summary>
    /// The debris's sideways speed at sway angle <paramref name="angle"/>: a quarter of the one-pixel sine velocity,
    /// rounded toward decreasing coordinates.
    /// </summary>
    internal static SignedEightEight SwayVelocity(TrigonometryAngle angle) => EnemyMath.SineVelocity(angle, 1).DividedRoundingDown(4);

    /// <summary>Advances the bubble by its vertical velocity and publishes its own horizontal position.</summary>
    private static void MoveNoobTubeBubbleVertically(EnemyProjectile projectile)
    {
        projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta()).WithWholeX(projectile.StateOf<NoobTubeDebrisState>().X.Whole);
    }

    /// <summary>Connects the crack, shard and bubble movers to the projectile engine.</summary>
    internal void AddNoobTubeProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddFrameBehavior(context => FlickerNoobTubeCrack(context.Projectile), EnemyProjectileFrameBehavior.NoobTubeCrackFlickering);
        table.AddFrameBehavior(context => DropNoobTubeCrack(context.Projectile), EnemyProjectileFrameBehavior.NoobTubeCrackFalling);
        table.AddFrameBehavior(context => FlyNoobTubeShard(context.Projectile, context.Camera.Y), EnemyProjectileFrameBehavior.NoobTubeShardFlying);
        table.AddFrameBehavior(context => DropNoobTubeShard(context.Projectile, context.Camera.Y), EnemyProjectileFrameBehavior.NoobTubeShardFalling);
        table.AddFrameBehavior(context => DropNoobTubeBubble(context.Projectile), EnemyProjectileFrameBehavior.NoobTubeBubbleFalling);
        table.AddFrameBehavior(context => MoveNoobTubeBubbleVertically(context.Projectile), EnemyProjectileFrameBehavior.NoobTubeBubbleFlying);
    }

    /// <summary>Carries out the cues of the crack, shard and bubble animations.</summary>
    internal void AddNoobTubeProjectileCues(EnemyProjectileHandlerTable table) =>
        table.AddCue<NoobTubeProjectileCue>((context, cue) => ApplyNoobTubeProjectileCue(context.Projectile, cue));

    /// <summary>Carries out one cue of the crack, shard and bubble animations.</summary>
    private void ApplyNoobTubeProjectileCue(EnemyProjectile projectile, NoobTubeProjectileCue cue)
    {
        bool atOwnColumn = NoobTubeCadenceDefinitions.DebrisAtOwnColumn.IsSetIn(_projectiles.CurrentFrame8);
        switch (cue.Kind)
        {
            case NoobTubeProjectileCueKind.FlickerReflected:
                // The shard shows its first pose at its column, or the reflected pose mirrored across the tube.
                int column = projectile.StateOf<NoobTubeDebrisState>().X.Whole;
                projectile.Position = projectile.Position.WithWholeX(atOwnColumn
                    ? column
                    : CoordinateAxis.Wrap(NoobTubeDebrisDefinitions.ShardMirrorSpan - column));
                ShardAnimation(projectile).ShowsReflectedPose = !atOwnColumn;
                break;
            case NoobTubeProjectileCueKind.FlickerHidden:
                projectile.Position = projectile.Position.WithWholeX(atOwnColumn ? projectile.StateOf<NoobTubeDebrisState>().X.Whole : NoobTubeDebrisDefinitions.HiddenXPosition);
                break;
            case NoobTubeProjectileCueKind.AssignShardFallingAngle:
                projectile.StateOf<NoobTubeDebrisState>().SwayPhase = _room.Random.Next(NoobTubeDebrisDefinitions.FallingSwayPhase);
                projectile.Velocity = projectile.Velocity.WithY(NoobTubeDebrisDefinitions.ShardFallYVelocity);
                break;
            case NoobTubeProjectileCueKind.AssignBubbleFallingAngle:
                projectile.StateOf<NoobTubeDebrisState>().SwayPhase = _room.Random.Next(NoobTubeDebrisDefinitions.FallingSwayPhase);
                break;
            case NoobTubeProjectileCueKind.SetFrameBehavior:
                projectile.FrameBehavior = cue.FrameBehavior;
                break;
            case NoobTubeProjectileCueKind.Delete:
                projectile.Delete();
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(NoobTubeProjectileCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>The typed animation a shard's flicker cue runs in.</summary>
    private static NoobTubeProjectileAnimation ShardAnimation(EnemyProjectile projectile) =>
        projectile.Animation as NoobTubeProjectileAnimation
        ?? throw new InvalidOperationException($"A n00b-tube flicker cue ran outside its typed program; the slot runs {projectile.Animation}.");
}
