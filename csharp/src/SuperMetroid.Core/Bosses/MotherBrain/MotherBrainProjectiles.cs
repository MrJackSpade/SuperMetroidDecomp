using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's projectiles: bombs, onion rings, drool, lasers, hand beams, turret shots and the tank's glass.</summary>
[RoomLifetime]
internal sealed class MotherBrainProjectiles
{
    /// <summary>Provides room services, pictures, palette state, and effects used by the encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the bombs, glass, beams, turrets, and other encounter projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Publishes screen shake during the encounter's collapse and attacks.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>The medium diagonal foreground shake a hand beam leaving the arena or an onion ring exploding starts.</summary>
    private static readonly EarthquakeShake ExplosionShake =
        new(EarthquakeStrength.Medium, EarthquakeDirection.Diagonal, EarthquakeReach.Foreground);
    /// <summary>Applies the encounter's damage and death/drop rules.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Holds the shared body/head state for the current Mother Brain room.</summary>
    private readonly MotherBrainEncounter _motherBrainEncounter;
    /// <summary>The other actors in the room: the Baby, the Rinkas, the Pirate laser and the afterburn.</summary>
    private readonly IMotherBrainRoomActors _roomActors;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainProjectiles(EnemyRoomBinding room, EnemyProjectileEngine projectiles, RoomEarthquake earthquake, EnemyDeathsAndDrops deathsAndDrops, MotherBrainEncounter motherBrainEncounter, IMotherBrainRoomActors roomActors)
    {
        _room = room;
        _projectiles = projectiles;
        _earthquake = earthquake;
        _deathsAndDrops = deathsAndDrops;
        _motherBrainEncounter = motherBrainEncounter;
        _roomActors = roomActors;
    }

    /// <summary>Carries out one cue of Mother Brain's effect animations.</summary>
    private void ApplyMotherBrainEffectCue(EnemyProjectile projectile, MotherBrainEffectCue cue)
    {
        switch (cue)
        {
            case MotherBrainEffectCue.SetRadius setRadius:
                projectile.Radius = setRadius.Radius;
                break;
            case MotherBrainEffectCue.Signal { Kind: MotherBrainEffectCueKind.DrawFromSharedSheet }:
                projectile.Graphics = SpriteGraphics.SharedStandard;
                break;
            case MotherBrainEffectCue.Signal { Kind: MotherBrainEffectCueKind.DropDrool }:
                // After changing to the falling frame behaviour, the released drool drops twelve pixels before its first
                // falling frame.
                projectile.Position = projectile.Position.WithWrappedWholeY(projectile.Position.Y.Whole + 12);
                break;
            case MotherBrainEffectCue.Signal { Kind: MotherBrainEffectCueKind.EndSmallPurpleBreath }:
                (_motherBrainEncounter.MotherBrain ?? throw new InvalidOperationException(
                    "Mother Brain's small purple breath ran without its encounter state."))
                    .SmallPurpleBreathActive = false;
                break;
            case MotherBrainEffectCue.SetFrameBehavior setFrameBehavior:
                projectile.FrameBehavior = setFrameBehavior.FrameBehavior;
                break;
            case MotherBrainEffectCue.Signal { Kind: MotherBrainEffectCueKind.ClearFrameBehavior }:
                projectile.FrameBehavior = EnemyProjectileFrameBehavior.Stationary;
                break;
            case MotherBrainEffectCue.Signal { Kind: MotherBrainEffectCueKind.Delete }:
                projectile.Delete();
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(MotherBrainEffectCueKind)} {cue}.");
        }
    }

    /// <summary>Registers the encounter-owned bombs, glass, beams, rings, turrets, tubes, and effects.</summary>
    internal void AddMotherBrainProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddTouchAnimation(RoomEnemyProjectileKind.MotherBrainRoomTurretBullet, () => new MotherBrainTurretAnimation());
        table.AddShotAnimation(RoomEnemyProjectileKind.MotherBrainRoomTurretBullet, () => new MotherBrainTurretAnimation());
        table.AddCue<MotherBrainTurretCue>((context, cue) =>
        {
            EnemyProjectile projectile = context.Projectile;
            switch (cue)
            {
                case MotherBrainTurretCue.DrawFromSharedSheet:
                    projectile.Graphics = SpriteGraphics.SharedStandard;
                    return;
                case MotherBrainTurretCue.ClearFrameBehavior:
                    projectile.FrameBehavior = EnemyProjectileFrameBehavior.Stationary;
                    return;
                case MotherBrainTurretCue.Delete:
                    projectile.Delete();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(MotherBrainTurretCue)} {cue}.");
            }
        });
        table.AddCue<MotherBrainGlassCue>((context, cue) =>
        {
            EnemyProjectile projectile = context.Projectile;
            switch (cue)
            {
                case MotherBrainGlassCue.Delete:
                    projectile.Delete();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(MotherBrainGlassCue)} {cue}.");
            }
        });
        table.AddFrameBehavior(context =>
        {
            RunMotherBrainTurretFrameBehavior(context.Projectile, context.Camera);
        }, EnemyProjectileFrameBehavior.MotherBrainTurretAiming);

        table.AddFrameBehavior(context =>
        {
            RunMotherBrainTurretBulletFrameBehavior(context.Projectile, context.Level);
        }, EnemyProjectileFrameBehavior.MotherBrainTurretBulletFlight);

        table.AddFrameBehavior(context =>
        {
            RunMotherBrainGlassShardFrameBehavior(context.Projectile);
        }, EnemyProjectileFrameBehavior.MotherBrainGlassShardFlight);

        table.AddFrameBehavior(context =>
        {
            RunMotherBrainTopTubeFrameBehavior(context.Projectile);
        }, EnemyProjectileFrameBehavior.MotherBrainTubeFall);

        table.AddFrameBehavior(context =>
        {
            RunMotherBrainAttachedDroolFrameBehavior(context.Projectile);
        }, EnemyProjectileFrameBehavior.MotherBrainDroolHanging);

        table.AddFrameBehavior(context =>
        {
            RunMotherBrainFallingDroolFrameBehavior(context.Projectile);
        }, EnemyProjectileFrameBehavior.MotherBrainDroolFall);

        table.AddFrameBehavior(context =>
        {
            RunMotherBrainOnionRingFrameBehavior(
                context.Projectile,
                context.Samus,
                context.Camera.X);
        }, EnemyProjectileFrameBehavior.MotherBrainOnionRingFlight);

        table.AddFrameBehavior(context =>
        {
            RunMotherBrainBombFrameBehavior(context.Projectile, context.SamusBombs);
        }, EnemyProjectileFrameBehavior.MotherBrainBombBounce);

        table.AddFrameBehavior(context =>
        {
            RunMotherBrainRainbowChargingFrameBehavior(context.Projectile);
        }, EnemyProjectileFrameBehavior.MotherBrainRainbowBeamCharge);

        table.AddFrameBehavior(context =>
        {
            RunMotherBrainRainbowExplosionFrameBehavior(context.Projectile, context.Samus);
        }, EnemyProjectileFrameBehavior.MotherBrainRainbowImpactTracking);

        table.AddFrameBehavior(context =>
        {
            RunMotherBrainDeathExplosion(context.Projectile);
        }, EnemyProjectileFrameBehavior.MotherBrainDeathExplosionTracking);

        table.AddFrameBehavior(context =>
        {
            RunMotherBrainDoorFragment(context.Projectile);
        }, EnemyProjectileFrameBehavior.MotherBrainDeathDoorFragmentFall);

        table.AddFrameBehavior(context =>
        {
            PinMotherBrainEscapeSubtitle(context.Projectile);
        }, EnemyProjectileFrameBehavior.MotherBrainDeathSubtitleScreenPin);

        table.AddCue<MotherBrainEffectCue>((context, cue) => ApplyMotherBrainEffectCue(context.Projectile, cue));
        // A touched onion ring runs its typed impact animation, the animation its definition names.
        table.AddTouchAnimation(RoomEnemyProjectileKind.MotherBrainOnionRing,
            () => new MotherBrainEffectAnimation(MotherBrainEffectSegment.OnionRingHit));

        table.AddCue<MotherBrainHandBeamCue>((context, cue) =>
        {
            EnemyProjectile projectile = context.Projectile;
            switch (cue)
            {
                case MotherBrainHandBeamCue.SpawnNextBeam:
                    SpawnMotherBrainHandBeamFired();
                    return;
                case MotherBrainHandBeamCue.Delete:
                    projectile.Delete();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(MotherBrainHandBeamCue)} {cue}.");
            }
        });
    }

    /// <summary>Runs the bomb's frame behaviour.</summary>
    internal void RunMotherBrainBombFrameBehavior(
        EnemyProjectile bomb,
        SamusBombProjectileSystem? samusBombs)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ?? throw new InvalidOperationException(
            "Mother Brain bomb ran without its multipart encounter state.");
        if (TryDestroyMotherBrainBombWithSamusBomb(bomb, state, samusBombs))
            return;

        MotherBrainBombState bombState = bomb.StateOf<MotherBrainBombState>();
        int acceleration;
        if (bombState.Bounces == 0)
        {
            // Only the initial fall applies its horizontal drag: the speed loses it, stopping at rest, and keeps its
            // direction.
            bomb.Velocity = bomb.Velocity.WithX(bomb.Velocity.X.SlowedBy(MotherBrainBombBounceDefinitions.FallDrag));
            acceleration = MotherBrainBombBounceDefinitions.FallAcceleration;
        }
        else
        {
            if (MotherBrainBombBounces.YAcceleration(bombState.Bounces) is not { } bounceAcceleration)
            {
                ExpireMotherBrainBomb(bomb, state);
                return;
            }

            acceleration = bounceAcceleration;
        }

        if (MoveMotherBrainBomb(bomb, acceleration))
            bombState.Bounces++;
    }

    /// <summary>Consumes an overlapping Samus bomb and resolves the Mother Brain bomb explosion.</summary>
    private bool TryDestroyMotherBrainBombWithSamusBomb(
        EnemyProjectile bomb,
        MotherBrainEnemyState state,
        SamusBombProjectileSystem? samusBombs)
    {
        if (samusBombs is null || samusBombs.BombCounter == 0)
            return false;

        foreach (SamusBombProjectileSlot samusBomb in samusBombs.Slots)
        {
            if (samusBomb.Type.Family != SamusProjectileFamily.Bomb ||
                !samusBomb.BombTimer.IsZero ||
                !MotherBrainBombStrictOverlap(
                    bomb.Position.Whole,
                    bomb.Radius,
                    samusBomb.Position.Whole,
                    samusBomb.Radius))
            {
                continue;
            }

            Position16<RoomPixels> position = bomb.Position.Whole;
            DeleteMotherBrainBomb(bomb, state);
            _projectiles.SpawnRoomGraphicsDustExplosion(position, MiscDustAnimation.SmallDustCloud);
            _deathsAndDrops.SpawnEnemyDropFromDefinition(
                position,
                state.Head.DefinitionId);
            return true;
        }

        return false;
    }

    /// <summary>Advances a bomb through its trajectory and reports arena-boundary expiry.</summary>
    private static bool MoveMotherBrainBomb(
        EnemyProjectile bomb,
        int acceleration)
    {
        // Every floor bounce restarts the fall at two pixels an update upward, and between bounces the bomb falls back no
        // further than from the height its launch reaches to its floor at 208, so the speed stays within a few pixels an update.
        bomb.Velocity = bomb.Velocity.WithY(bomb.Velocity.Y + SignedEightEight.FromTwoHundredFiftySixthsOfPixel(acceleration));
        bomb.Position = bomb.Position.Advance(bomb.Velocity.ToSixteenSixteen());

        // Both boundaries check whether the wrapped difference is negative. X reflects without clamping; the floor
        // clamps Y to 208 and resets the two authored bounce velocities.
        if (CoordinateAxis.Difference(bomb.Position.X.Whole, 240) >= 0)
            bomb.Velocity = bomb.Velocity.WithX(bomb.Velocity.X.Negated());
        if (CoordinateAxis.Difference(bomb.Position.Y.Whole, 208) < 0)
            return false;

        bomb.Position = bomb.Position.WithWholeY(208);
        SignedEightEight bounceSpeed = bomb.StateOf<MotherBrainBombState>().BounceSpeed;
        bomb.Velocity = new(bounceSpeed.AlongDirectionOf(bomb.Velocity.X), SignedEightEight.FromPixels(-2));
        return true;
    }

    /// <summary>Removes an expired bomb and emits the associated explosion effect.</summary>
    private void ExpireMotherBrainBomb(
        EnemyProjectile bomb,
        MotherBrainEnemyState state)
    {
        Position16<RoomPixels> impact = bomb.Position.Whole;
        int afterburnCount = bomb.StateOf<MotherBrainBombState>().Afterburns;
        DeleteMotherBrainBomb(bomb, state);

        // The bomb's explosion clears the source slot before both allocations. They therefore use
        // the normal descending shared-pool competition, including legitimate same-slot
        // replacement that the outer scheduler will continue processing on this pass.
        _roomActors.SpawnAfterburnCenter(
            RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter,
            impact,
            afterburnCount);
        _projectiles.SpawnRoomGraphicsDustExplosion(impact, MiscDustAnimation.SmallExplosion);
        _motherBrainEncounter.RequestCueSound(MotherBrainSoundDefinitions.Explosion);
    }

    /// <summary>Clears a bomb slot when its lifetime or collision ends.</summary>
    private static void DeleteMotherBrainBomb(
        EnemyProjectile bomb,
        MotherBrainEnemyState state)
    {
        state.RecordBombRemoved();
        bomb.Velocity = EightEightVector<RoomPixels>.Zero;
        bomb.Delete();
    }

    /// <summary>Uses strict hitbox intersection for bomb-to-bomb contact.</summary>
    private static bool MotherBrainBombStrictOverlap(
        Position16<RoomPixels> first,
        Extent16<RoomPixels> firstRadius,
        Position16<RoomPixels> second,
        Extent16<RoomPixels> secondRadius) =>
        CoordinateAxis.Distance(first.X, second.X) <
            firstRadius.X + secondRadius.X &&
        CoordinateAxis.Distance(first.Y, second.Y) <
            firstRadius.Y + secondRadius.Y;

    /// <summary>Advances a death explosion projectile through its installed animation.</summary>
    internal void RunMotherBrainDeathExplosion(EnemyProjectile projectile)
    {
        var body = _motherBrainEncounter.MotherBrain?.Body ?? throw new InvalidOperationException("Body-relative death explosion has no Mother Brain owner.");
        projectile.Position = projectile.Position.WithWhole(body.Position.Whole + projectile.StateOf<MotherBrainFollowingExplosionState>().Offset);
    }

    /// <summary>Updates a door fragment's fall and expiration after the room collapse.</summary>
    internal void RunMotherBrainDoorFragment(EnemyProjectile fragment)
    {
        // The speed loses the drag, stopping at rest, and keeps its direction.
        fragment.Velocity = fragment.Velocity.WithX(fragment.Velocity.X.SlowedBy(MotherBrainDeathDefinitions.DoorFragmentDrag));
        // A fragment falls for its lifetime of 32 updates at most, so its gravity adds at most four pixels an update.
        fragment.Velocity = fragment.Velocity.WithY(fragment.Velocity.Y + MotherBrainDeathDefinitions.DoorFragmentGravity);
        fragment.Position = fragment.Position.Advance(fragment.Velocity.ToSixteenSixteen());
        // The fragment's lifetime expires when it passes zero.
        MotherBrainDoorFragmentState fragmentState = fragment.StateOf<MotherBrainDoorFragmentState>();
        if (--fragmentState.Lifetime >= 0) return;
        Position16<RoomPixels> dust = fragment.Position.Whole - new Offset16<RoomPixels>(0, MotherBrainDeathDefinitions.DoorDustYOffset);
        fragment.Delete();
        _projectiles.SpawnRoomGraphicsDustExplosion(dust, MotherBrainDeathDefinitions.DoorDustParameter);
    }

    /// <summary>Keeps the escape subtitle projectile fixed to its screen position.</summary>
    internal static void PinMotherBrainEscapeSubtitle(EnemyProjectile subtitle)
    {
        subtitle.Velocity = EightEightVector<RoomPixels>.Zero;
        subtitle.Position = subtitle.Position.WithWhole(new(MotherBrainDeathDefinitions.SubtitleX, MotherBrainDeathDefinitions.SubtitleY));
    }

    /// <summary>Runs the frame behaviour of the four falling ceiling-tube actors.</summary>
    internal void RunMotherBrainTopTubeFrameBehavior(EnemyProjectile projectile)
    {
        MotherBrainTopTubeState tube = projectile.StateOf<MotherBrainTopTubeState>();
        if (tube.DustPending)
        {
            _projectiles.SpawnRoomGraphicsDustExplosion(
                new(projectile.Position.X.Whole, CoordinateAxis.Wrap(projectile.Position.Y.Whole + 8)),
                MiscDustAnimation.SmallDustCloud);
            tube.DustPending = false;
        }

        // A tube falls from the ceiling to its floor at 208 and is removed there, so its speed stays a few pixels an update.
        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + SignedEightEight.FromTwoHundredFiftySixthsOfPixel(6));
        projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
        if (projectile.Position.Y.Whole < 208)
            return;

        Position16<RoomPixels> position = projectile.Position.Whole;
        projectile.Delete();
        _projectiles.SpawnRoomGraphicsDustExplosion(position, MiscDustAnimation.Smoke);
    }

    /// <summary>
    /// Consumes one request emitted by the glass room object's shard step. The request retains the
    /// glass's block origin because the shard initializer calls
    /// for the room object's block coordinates after the shared projectile allocator runs.
    /// </summary>
    public void SpawnMotherBrainGlassProjectile(MotherBrainGlassProjectileRequest request)
    {
        _room.EnsureLoaded();
        if (request.Kind != RoomEnemyProjectileKind.MotherBrainGlassShard)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                $"Mother Brain glass requested unsupported projectile {request.Kind}.");
        }
        if (!Enum.IsDefined(request.Shard))
        {
            throw new InvalidDataException(
                $"Mother Brain glass shard {request.Shard} is not one of the three emitters.");
        }

        EnemyProjectile? shard = _projectiles.AllocateEnemyProjectile();
        if (shard is null)
            return; // The projectile spawner silently loses actors when all eighteen slots are full.

        MotherBrainProjectileStarts.Initialize(
            shard,
            RoomEnemyProjectileKind.MotherBrainGlassShard,
            graphics: default);

        // The shard's launch angle is a random angle of 0 through 255. It selects signed sine/cosine velocities and the
        // shard's spin animation.
        var angle = new TrigonometryAngle(MotherBrainGlassShardDefinitions.LaunchAngle.Of(_room.Random.Generate()));
        shard.Velocity = shard.Velocity.WithX(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(Trigonometry.SignedSine(angle)));
        // The vertical speed is four times the negative cosine, the sine a quarter turn back; a sample lies within one
        // pixel either way, so four times one stays within four.
        shard.Velocity = shard.Velocity.WithY(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(
            4 * Trigonometry.SignedSine(angle - TrigonometryAngle.QuarterTurn)));
        shard.StartAnimation(new MotherBrainGlassAnimation(MotherBrainGlassAnimations.ShardAnimations.At(angle)));
        shard.Graphics = MotherBrainGlassShardDefinitions.Graphics(MotherBrainColours.GlassAndTubes);

        MotherBrainGlassShardPlacement placement =
            MotherBrainGlassShards.Placement(request.Shard);
        shard.Position = shard.Position.WithWhole(request.RoomObjectBlock.ToRoomPixels() + placement.Offset);

        // Position jitter consumes two additional RNG samples in this order. It is not
        // cosmetic randomness: later turret and sparkle behavior observes the advanced seed.
        shard.Position = shard.Position.WithWrappedWholeX(
            shard.Position.X.Whole + _room.Random.Next(MotherBrainGlassShardDefinitions.ShardJitter) - 8);
        shard.Position = shard.Position.WithWrappedWholeY(
            shard.Position.Y.Whole + _room.Random.Next(MotherBrainGlassShardDefinitions.ShardJitter) - 8);
    }

    /// <summary>Runs the glass shard's frame behaviour.</summary>
    internal void RunMotherBrainGlassShardFrameBehavior(EnemyProjectile shard)
    {
        shard.Position = shard.Position.Advance(shard.Velocity.ToSixteenSixteen());

        // Tests which screen of the room its Y lies on rather than camera visibility. A shard
        // is deleted as soon as wrapped Y leaves the top screen; X may freely leave the viewport.
        if (RoomGridConversions.PixelToScreen(shard.Position.Y.Whole) != 0)
        {
            shard.Delete();
            return;
        }

        // A shard is removed once it leaves the first 256 lines, so it falls at most that far and its speed stays a few
        // pixels an update above its launch speed.
        shard.Velocity = shard.Velocity.WithY(shard.Velocity.Y + SignedEightEight.FromTwoHundredFiftySixthsOfPixel(32));

        // The condition itself consumes one random value every live shard frame; a successful
        // sparkle consumes two more below. Both of the sample's sparkle checks clear: one sample in four.
        int sparkleSample = _room.Random.Generate();
        if (!MotherBrainGlassShardDefinitions.SparkleCheckLowBit.IsSetIn(sparkleSample) &&
            !MotherBrainGlassShardDefinitions.SparkleCheckHighBit.IsSetIn(sparkleSample))
            SpawnMotherBrainGlassSparkle(shard);
    }

    /// <summary>Runs the sparkle initializer using the spawning shard as parameter.</summary>
    internal void SpawnMotherBrainGlassSparkle(EnemyProjectile source)
    {
        EnemyProjectile? sparkle = _projectiles.AllocateEnemyProjectile();
        if (sparkle is null)
            return;

        MotherBrainProjectileStarts.Initialize(
            sparkle,
            RoomEnemyProjectileKind.MotherBrainGlassSparkle,
            graphics: default);
        sparkle.StartAnimation(new MotherBrainGlassAnimation(MotherBrainGlassSegment.Sparkle));
        sparkle.Position = sparkle.Position.WithWhole(new(CoordinateAxis.Wrap(
            source.Position.X.Whole + _room.Random.Next(MotherBrainGlassShardDefinitions.SparkleOffset) - 16), CoordinateAxis.Wrap(
            source.Position.Y.Whole + _room.Random.Next(MotherBrainGlassShardDefinitions.SparkleOffset) - 16)));
        sparkle.Graphics = MotherBrainGlassShardDefinitions.Graphics(MotherBrainColours.GlassAndTubes);
    }

    /// <summary>
    /// Runs the fired child's initializer. Each child first advances the
    /// shared aimed cursor, then receives a randomized secondary displacement used only for
    /// its spawn frame. Surviving actors become stationary animated emitters; out-of-bounds
    /// children turn into the game's large impact dust and earthquake.
    /// </summary>
    internal void SpawnMotherBrainHandBeamFired()
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ?? throw new InvalidOperationException(
            "Mother Brain hand-beam bytecode ran without its encounter state.");
        EnemyProjectile? beam = _projectiles.AllocateEnemyProjectile();
        if (beam is null)
            return;

        MotherBrainProjectileStarts.Initialize(
            beam,
            RoomEnemyProjectileKind.MotherBrainHandBeamFired,
            graphics: default);
        beam.Position = state.HandBeamNextPosition;
        beam.Velocity = state.HandBeamNextVelocity;

        beam.Position = beam.Position.Advance(beam.Velocity.ToSixteenSixteen());
        state.HandBeamNextPosition = beam.Position;

        int angleRandom = _room.Random.Generate();
        TrigonometryAngle scatterAngle = state.HandBeamNextAngle.TurnedBySteps(MotherBrainRandomDefinitions.HandBeamScatterTurn.Of(angleRandom));
        int speedRandom = _room.Random.Generate();
        SignedEightEight scatterSpeed = SignedEightEight.FromPixels(MotherBrainRandomDefinitions.HandBeamScatterSpeed.Of(speedRandom));
        beam.Velocity = beam.Velocity.WithX(EnemyMath.MultiplySineSample(scatterSpeed, scatterAngle));
        beam.Velocity = beam.Velocity.WithY(EnemyMath.MultiplySineSample(
            scatterSpeed,
            scatterAngle.TurnedBy(TrigonometryAngle.QuarterTurn)));
        beam.Position = beam.Position.Advance(beam.Velocity.ToSixteenSixteen());

        if (IsMotherBrainHandBeamOutsideArena(beam))
        {
            Position16<RoomPixels> impact = beam.Position.Whole;
            beam.Delete();
            _projectiles.SpawnRoomGraphicsDustExplosion(
                impact,
                MiscDustAnimation.BigExplosion);
            _motherBrainEncounter.RequestCueSound(MotherBrainSoundDefinitions.Explosion);
            _earthquake.Start(ExplosionShake, 10);
            return;
        }

        beam.Velocity = EightEightVector<RoomPixels>.Zero;
        beam.StartAnimation(new MotherBrainHandBeamAnimation());
    }

    /// <summary>Detects when a fired hand beam has crossed the fight arena's bounds.</summary>
    private static bool IsMotherBrainHandBeamOutsideArena(
        EnemyProjectile beam) =>
        CoordinateAxis.Difference(beam.Position.Y.Whole, 34) < 0 ||
        CoordinateAxis.Difference(beam.Position.Y.Whole, 206) >= 0 ||
        CoordinateAxis.Difference(beam.Position.X.Whole, 2) < 0 ||
        CoordinateAxis.Difference(beam.Position.X.Whole, 238) >= 0;

    /// <summary>Exact six-position mouth attachment table.</summary>
    internal void RunMotherBrainAttachedDroolFrameBehavior(
        EnemyProjectile drool)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ?? throw new InvalidOperationException(
            "Mother Brain drool ran without its multipart encounter state.");
        RoomEnemySlot head = state.Head;
        int parameter = drool.StateOf<MotherBrainDroolState>().MouthPoint;

        drool.Position = drool.Position.WithWhole(head.Position.Whole + MotherBrainBehavior.MotherBrainDroolOffsets[parameter]);
        drool.Velocity = EightEightVector<RoomPixels>.Zero;
    }

    /// <summary>
    /// Runs the falling drool's frame behaviour. The actor uses signed 8.8 velocity, adds twelve
    /// units of gravity per frame, and switches to its four-map splash at world Y 215.
    /// </summary>
    internal static void RunMotherBrainFallingDroolFrameBehavior(
        EnemyProjectile drool)
    {
        // The drool splashes once it falls to 215, so its speed stays a few pixels an update.
        drool.Velocity = drool.Velocity.WithY(drool.Velocity.Y + SignedEightEight.FromTwoHundredFiftySixthsOfPixel(12));
        drool.Position = drool.Position.AdvanceY(drool.Velocity.Y.ToSixteenSixteenDelta());
        if (drool.Position.Y.Whole < 215)
            return;

        drool.Position = drool.Position.WithWrappedWholeY(drool.Position.Y.Whole - 4);
        drool.StartAnimation(new MotherBrainEffectAnimation(MotherBrainEffectSegment.DroolHitFloor));
    }

    /// <summary>
    /// Runs the onion ring's frame behaviour. A live Baby slot is tested before Samus and the room,
    /// matching the private projectile's cross-enemy collision order during phase three.
    /// </summary>
    internal void RunMotherBrainOnionRingFrameBehavior(
        EnemyProjectile ring,
        SamusState? samus,
        int cameraX)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ?? throw new InvalidOperationException(
            "Mother Brain onion ring ran without its multipart encounter state.");

        // The ring stays pinned to the mouth while its attach delay runs out.
        MotherBrainOnionRingState ringState = ring.StateOf<MotherBrainOnionRingState>();
        if (ringState.AttachDelay != 0)
        {
            ringState.AttachDelay--;
            PinMotherBrainOnionRingToMouth(ring, state);
            return;
        }

        ring.Position = ring.Position.Advance(ring.Velocity.ToSixteenSixteen());

        BabyMetroidCutsceneState? baby = state.BabyMetroid;
        if (baby is { IsDeleted: false })
        {
            // The ring deletes every later ring as soon as the still-registered Baby
            // reaches zero health, without an explosion.
            if (baby.Health == 0)
            {
                ring.Delete();
                return;
            }

            if (MotherBrainOnionRingOverlapsBaby(ring, baby))
            {
                state.PendingBabyCryCount++;
                _ = baby.ApplyMotherBrainOnionRingHit(MotherBrainAttackDamage.OnionRing);
                // The ring writes the Baby slot's Enemy.health from this projectile
                // pass; it must not wait for the Baby's next AI turn to publish it.
                RoomEnemySlot babySlot = state.BabyMetroidSlot ?? throw new InvalidDataException(
                    "A Mother Brain onion ring hit the Baby while the Baby has no enemy in the room.");
                babySlot.Health = baby.Health;
                ExplodeMotherBrainOnionRing(ring, ring.Position.Whole);
                return;
            }
        }

        // This private collision path intentionally ignores Samus's current invincibility
        // timer. It writes a fresh 96 only after applying suit-divided damage.
        if (samus is not null && MotherBrainOnionRingOverlapsSamus(ring, samus))
        {
            Position16<RoomPixels> ringPosition = ring.Position.Whole;
            SamusSuitDamage.Hurt(samus, MotherBrainAttackDamage.OnionRing);
            samus.InvincibilityTimer = WrappingCountdown.Start(96);
            samus.KnockbackTimer = 5;
            samus.KnockbackXDirection = CoordinateAxis.Difference(samus.Position.X.Whole, ringPosition.X) >= 0
                ? KnockbackHorizontalDirection.Right
                : KnockbackHorizontalDirection.Left;
            ExplodeMotherBrainOnionRing(ring, ringPosition);
            return;
        }

        int signedY = CoordinateAxis.AsSigned(ring.Position.Y.Whole);
        int signedX = CoordinateAxis.AsSigned(ring.Position.X.Whole);
        int screenX = CoordinateAxis.Difference(ring.Position.X.Whole, cameraX);
        if (signedY < 32 || ring.Position.Y.Whole >= 216 || signedX < 0 ||
            screenX < 0 || screenX >= 248)
        {
            _earthquake.Start(ExplosionShake, 10);
            ExplodeMotherBrainOnionRing(ring, ring.Position.Whole);
        }
    }

    /// <summary>Positions an unlaunched ring at the moving head's mouth.</summary>
    internal static void PinMotherBrainOnionRingToMouth(
        EnemyProjectile ring,
        MotherBrainEnemyState state)
    {
        RoomEnemySlot head = state.Head;
        ring.Position = ring.Position.WithWhole(new(CoordinateAxis.Wrap(head.Position.X.Whole + 10), CoordinateAxis.Wrap(head.Position.Y.Whole + 16)));
    }

    /// <summary>Checks ring contact against Samus's current hitbox.</summary>
    private static bool MotherBrainOnionRingOverlapsSamus(
        EnemyProjectile ring,
        SamusState samus) =>
        CoordinateAxis.Distance(ring.Position.X.Whole, samus.Position.X.Whole) <
            ring.Radius.X + samus.Kinematics.Radius.X &&
        CoordinateAxis.Distance(ring.Position.Y.Whole, samus.Position.Y.Whole) <
            ring.Radius.Y + samus.Kinematics.Radius.Y;

    /// <summary>Checks ring contact against the Baby Metroid's current hitbox.</summary>
    private static bool MotherBrainOnionRingOverlapsBaby(
        EnemyProjectile ring,
        BabyMetroidCutsceneState baby) =>
        CoordinateAxis.Distance(ring.Position.X.Whole, baby.Position.X.Whole) <
            ring.Radius.X + BabyMetroidCutsceneState.XHitboxRadius &&
        CoordinateAxis.Distance(ring.Position.Y.Whole, baby.Position.Y.Whole) <
            ring.Radius.Y + BabyMetroidCutsceneState.YHitboxRadius;

    /// <summary>Ends a ring on contact and spawns its explosion effect.</summary>
    private void ExplodeMotherBrainOnionRing(
        EnemyProjectile ring,
        Position16<RoomPixels> position)
    {
        // The ring's impact clears the source before allocating the small-explosion room dust. If the
        // allocator reuses this place in the projectile pool, the outer projectile pass will advance
        // the newborn dust animation during the same pass.
        ring.Delete();
        _projectiles.SpawnRoomGraphicsDustExplosion(position, MiscDustAnimation.SmallExplosion);
        _motherBrainEncounter.RequestCueSound(MotherBrainSoundDefinitions.Explosion);
    }

    /// <summary>Runs one of Mother Brain's turrets per frame.</summary>
    internal void RunMotherBrainTurretFrameBehavior(
        EnemyProjectile turret,
        Position16<RoomPixels> camera)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ??
            throw new InvalidDataException("A Mother Brain turret has no owning encounter state.");
        bool onScreen = MotherBrainTurretIsOnScreen(turret, camera);

        // Off-screen turrets deliberately freeze both timers until scrolled into view. Once
        // the shared deletion flag is set they disappear silently off-screen, but visible
        // turrets replace themselves with the dust animation's smoke.
        if (!onScreen)
        {
            if (state.DeleteTurretsAndRinkas)
                turret.Delete();
            return;
        }
        if (state.DeleteTurretsAndRinkas)
        {
            Position16<RoomPixels> position = turret.Position.Whole;
            turret.Delete();
            _projectiles.SpawnRoomGraphicsDustExplosion(position, MiscDustAnimation.Smoke);
            return;
        }

        // Each wait counts down and is reloaded when it reaches zero.
        MotherBrainTurretState turretState = turret.StateOf<MotherBrainTurretState>();
        if (--turretState.RotationWait == 0)
        {
            ResetMotherBrainTurretRotationTimer(turret);
            SelectNextMotherBrainTurretDirection(turret);
            MotherBrainTurretDirection direction =
                MotherBrainTurretDefinitions.Ring.At(turretState.RingPosition);
            turret.StartAnimation(new MotherBrainTurretAnimation(MotherBrainTurretSegment.TurretPose, direction));
        }

        if (--turretState.FiringWait == 0)
        {
            ResetMotherBrainTurretCooldown(turret);
            SpawnMotherBrainTurretBullet(turret);
        }
    }

    /// <summary>Runs the signed direction-delta and allowed-rotation test.</summary>
    internal static void SelectNextMotherBrainTurretDirection(EnemyProjectile turret)
    {
        MotherBrainTurretState state = turret.StateOf<MotherBrainTurretState>();
        int currentDirection = state.RingPosition;
        int rotationDelta = state.RotationStep;
        int candidate = (currentDirection + rotationDelta) & 7;
        bool allowed = MotherBrainTurrets.IsRotationAllowed(
            state.Turret,
            MotherBrainTurretDefinitions.Ring.At(candidate));

        if (allowed)
        {
            state.RingPosition = candidate;
            return;
        }

        // A disallowed next step reverses the signed rotation delta and immediately takes
        // one step in the new direction, with no second sector test.
        // Every turret's sector spans at least three ring positions, so the step back stays inside it.
        state.RotationStep = -rotationDelta;
        state.RingPosition = currentDirection + state.RotationStep;
    }

    /// <summary>Restarts the turret's wait before it chooses another firing direction.</summary>
    internal void ResetMotherBrainTurretRotationTimer(EnemyProjectile turret)
    {
        int sample = _room.Random.Next(MotherBrainRandomDefinitions.TurretRotationWait);
        turret.StateOf<MotherBrainTurretState>().RotationWait = Math.Max(sample, MotherBrainTurretDefinitions.MinimumRotationDelay);
    }

    /// <summary>Restarts the turret's pause between fired bullets.</summary>
    internal void ResetMotherBrainTurretCooldown(EnemyProjectile turret)
    {
        int sample = _room.Random.Next(MotherBrainRandomDefinitions.TurretFiringWait);
        turret.StateOf<MotherBrainTurretState>().FiringWait = Math.Max(sample, MotherBrainTurretDefinitions.MinimumFiringCooldown);
    }

    /// <summary>Runs the asymmetric viewport bounds.</summary>
    private static bool MotherBrainTurretIsOnScreen(
        EnemyProjectile turret,
        Position16<RoomPixels> camera)
    {
        if (CoordinateAxis.AsSigned(turret.Position.Y.Whole) < 0 ||
            CoordinateAxis.AsSigned(turret.Position.Y.Whole + 16 - camera.Y) < 0 ||
            CoordinateAxis.Wrap(turret.Position.Y.Whole + 16 - camera.Y) >= 256)
        {
            return false;
        }

        return CoordinateAxis.AsSigned(turret.Position.X.Whole) >= 0 &&
            CoordinateAxis.AsSigned(turret.Position.X.Whole + 4 - camera.X) >= 0 &&
            CoordinateAxis.Wrap(turret.Position.X.Whole + 4 - camera.X) < 264;
    }

    /// <summary>Runs turret-bullet initialization.</summary>
    internal void SpawnMotherBrainTurretBullet(EnemyProjectile turret)
    {
        EnemyProjectile? bullet = _projectiles.AllocateEnemyProjectile();
        if (bullet is null)
            return;

        MotherBrainProjectileStarts.Initialize(
            bullet,
            RoomEnemyProjectileKind.MotherBrainRoomTurretBullet,
            MotherBrainTurretDefinitions.Graphics);
        MotherBrainTurretDirection direction =
            MotherBrainTurretDefinitions.Ring.At(turret.StateOf<MotherBrainTurretState>().RingPosition);
        MotherBrainTurretDirectionDefinition definition =
            MotherBrainTurrets.ForDirection(direction);
        bullet.Velocity = definition.BulletVelocity;
        bullet.Position = bullet.Position.WithWhole(turret.Position.Whole + definition.BulletOffset);
        // The bullet animation's first cue jumps to the pose for this direction.
        bullet.StartAnimation(new MotherBrainTurretAnimation(MotherBrainTurretSegment.BulletPose, direction));
    }

    /// <summary>Runs the bullet property flicker, 8.8 movement, and point collision.</summary>
    internal static void RunMotherBrainTurretBulletFrameBehavior(
        EnemyProjectile bullet,
        RoomLevelData level)
    {
        // The vulnerability bit is toggled before the projectile-vs-projectile pass every frame,
        // reproducing the bullet's alternating vulnerability rather than a visual alpha hack.
        bullet.BlocksSamusProjectiles = !bullet.BlocksSamusProjectiles;
        bullet.Position = bullet.Position.Advance(bullet.Velocity.ToSixteenSixteen());

        if (EnemyProjectileEngine.NonAirBlockAt(level, bullet.Position.Whole))
            bullet.Delete();
    }

    /// <summary>Pins the charging effect to the head while the beam winds up.</summary>
    internal void RunMotherBrainRainbowChargingFrameBehavior(
        EnemyProjectile projectile)
    {
        RoomEnemySlot head = _motherBrainEncounter.MotherBrain?.Head ?? throw new InvalidOperationException(
            "Mother Brain rainbow charge ran without the physical head slot.");
        projectile.Position = projectile.Position.WithWhole(head.Position.Whole);
    }

    /// <summary>Advances the rainbow impact effect through its timed lifetime.</summary>
    internal static void RunMotherBrainRainbowExplosionFrameBehavior(
        EnemyProjectile projectile,
        SamusState? samus)
    {
        SamusState target = samus ?? throw new InvalidOperationException(
            "Mother Brain rainbow explosion requires the live Samus actor.");
        projectile.Position = projectile.Position.WithWhole(target.Position.Whole + projectile.StateOf<MotherBrainFollowingExplosionState>().Offset);
    }
}
