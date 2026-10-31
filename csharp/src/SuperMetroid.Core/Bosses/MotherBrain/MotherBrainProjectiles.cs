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

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's projectiles: bombs, onion rings, drool, lasers, hand beams, turret shots and the tank's glass.</summary>
[RoomLifetime]
internal sealed class MotherBrainProjectiles
{
    /// <summary>Provides room services, tiles, palette state, and effects used by the encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the bombs, glass, beams, turrets, and other encounter projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Publishes screen shake during the encounter's collapse and attacks.</summary>
    private readonly RoomEarthquake _earthquake;
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

    /// <summary>Carries out one cue of Mother Brain's effect programs.</summary>
    private void ApplyMotherBrainEffectCue(RoomEnemyProjectileSlot projectile, MotherBrainEffectCue cue)
    {
        switch (cue)
        {
            case MotherBrainEffectCue.SetRadius setRadius:
                projectile.Radius = setRadius.Radius;
                break;
            case MotherBrainEffectCue.Command { Kind: MotherBrainEffectCueKind.UsePaletteZero }:
                projectile.Graphics = default;
                break;
            case MotherBrainEffectCue.Command { Kind: MotherBrainEffectCueKind.DropDrool }:
                // After changing to the falling pre-instruction, the released drool drops twelve pixels before its first
                // falling frame.
                projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(projectile.Position.Y.Whole + 12)));
                break;
            case MotherBrainEffectCue.Command { Kind: MotherBrainEffectCueKind.EndSmallPurpleBreath }:
                (_motherBrainEncounter.MotherBrain ?? throw new InvalidOperationException(
                    "Mother Brain's small purple breath ran without its encounter state."))
                    .SmallPurpleBreathActive = false;
                break;
            case MotherBrainEffectCue.SetPreInstruction setPreInstruction:
                projectile.PreInstruction = setPreInstruction.PreInstruction;
                break;
            case MotherBrainEffectCue.Command { Kind: MotherBrainEffectCueKind.ClearPreInstruction }:
                projectile.PreInstruction = EnemyProjectilePreInstruction.RTS_868170;
                break;
            case MotherBrainEffectCue.Command { Kind: MotherBrainEffectCueKind.Delete }:
                projectile.Clear();
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
            RoomEnemyProjectileSlot projectile = context.Projectile;
            switch (cue)
            {
                case MotherBrainTurretCue.UsePaletteZero:
                    projectile.Graphics = default;
                    return;
                case MotherBrainTurretCue.ClearPreInstruction:
                    projectile.PreInstruction = EnemyProjectilePreInstruction.RTS_868170;
                    return;
                case MotherBrainTurretCue.Delete:
                    projectile.Clear();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(MotherBrainTurretCue)} {cue}.");
            }
        });
        table.AddCue<MotherBrainGlassCue>((context, cue) =>
        {
            RoomEnemyProjectileSlot projectile = context.Projectile;
            switch (cue)
            {
                case MotherBrainGlassCue.Delete:
                    projectile.Clear();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(MotherBrainGlassCue)} {cue}.");
            }
        });
        table.AddPreInstruction(context =>
        {
            RunMotherBrainTurretPreInstruction(context.Projectile, context.Camera);
        }, EnemyProjectilePreInstruction.MotherBrainsTurrets);

        table.AddPreInstruction(context =>
        {
            RunMotherBrainTurretBulletPreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.MotherBrainsTurretBullets);

        table.AddPreInstruction(context =>
        {
            RunMotherBrainGlassShardPreInstruction(context.Projectile);
        }, EnemyProjectilePreInstruction.MotherBrainGlassShattering_Shard);

        table.AddPreInstruction(context =>
        {
            RunMotherBrainTopTubePreInstruction(context.Projectile);
        }, EnemyProjectilePreInstruction.MotherBrainsTubeFalling);

        table.AddPreInstruction(context =>
        {
            RunMotherBrainAttachedDroolPreInstruction(context.Projectile);
        }, EnemyProjectilePreInstruction.MotherBrainsDrool);

        table.AddPreInstruction(context =>
        {
            RunMotherBrainFallingDroolPreInstruction(context.Projectile);
        }, EnemyProjectilePreInstruction.MotherBrainsDrool_Falling);

        table.AddPreInstruction(context =>
        {
            RunMotherBrainOnionRingPreInstruction(
                context.Projectile,
                context.Samus,
                context.Camera.X);
        }, EnemyProjectilePreInstruction.MotherBrainsOnionRings);

        table.AddPreInstruction(context =>
        {
            RunMotherBrainBombPreInstruction(context.Projectile, context.SamusBombs);
        }, EnemyProjectilePreInstruction.MotherBrainsBomb);

        table.AddPreInstruction(context =>
        {
            RunMotherBrainRainbowChargingPreInstruction(context.Projectile);
        }, EnemyProjectilePreInstruction.MotherBrainRainbowBeam_Charging);

        table.AddPreInstruction(context =>
        {
            RunMotherBrainRainbowExplosionPreInstruction(context.Projectile, context.Samus);
        }, EnemyProjectilePreInstruction.MotherBrainsRainbowBeamExplosion);

        table.AddPreInstruction(context =>
        {
            RunMotherBrainDeathExplosion(context.Projectile);
        }, EnemyProjectilePreInstruction.MotherBrainDeathExplosionPreInstruction);

        table.AddPreInstruction(context =>
        {
            RunMotherBrainDoorFragment(context.Projectile);
        }, EnemyProjectilePreInstruction.MotherBrainDeathDoorFragmentPreInstruction);

        table.AddPreInstruction(context =>
        {
            PinMotherBrainEscapeSubtitle(context.Projectile);
        }, EnemyProjectilePreInstruction.MotherBrainDeathSubtitlePreInstruction);

        table.AddCue<MotherBrainEffectCue>((context, cue) => ApplyMotherBrainEffectCue(context.Projectile, cue));
        // A touched onion ring runs its typed impact program, the list its definition names.
        table.AddTouchAnimation(RoomEnemyProjectileKind.MotherBrainOnionRing,
            () => new MotherBrainEffectAnimation(MotherBrainEffectProgram.OnionRingHit));

        table.AddCue<MotherBrainHandBeamCue>((context, cue) =>
        {
            RoomEnemyProjectileSlot projectile = context.Projectile;
            switch (cue)
            {
                case MotherBrainHandBeamCue.SpawnNextBeam:
                    SpawnMotherBrainHandBeamFired();
                    return;
                case MotherBrainHandBeamCue.Delete:
                    projectile.Clear();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(MotherBrainHandBeamCue)} {cue}.");
            }
        });
    }

    /// <summary>Ports pre-instruction <c>$86:C4C8-C604</c>.</summary>
    internal void RunMotherBrainBombPreInstruction(
        RoomEnemyProjectileSlot bomb,
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
            // Only the initial fall applies two units of horizontal drag: the speed loses two
            // units, stopping at zero, and keeps its direction. The speed is at most 32768
            // units, so the reduced speed always fits a signed 8.8 velocity.
            int velocity = bomb.Velocity.X.SignedRawValue;
            int slowed = Math.Max(0, Math.Abs(velocity) - 2);
            bomb.Velocity = bomb.Velocity.WithX(SignedEightEight.FromSignedRaw((short)(velocity < 0 ? -slowed : slowed)));
            acceleration = MotherBrainBombBounceDefinitions.FallAcceleration;
        }
        else
        {
            int accelerationIndex = bombState.Bounces;
            if (accelerationIndex >= MotherBrainBombBounceDefinitions.StageCount)
            {
                throw new InvalidDataException(
                    $"Mother Brain bomb bounce {accelerationIndex} is past its {MotherBrainBombBounceDefinitions.StageCount} authored bounces.");
            }

            acceleration = MotherBrainBombBounces.YAcceleration(accelerationIndex);
            if (acceleration == 0)
            {
                ExpireMotherBrainBomb(bomb, state);
                return;
            }
        }

        if (MoveMotherBrainBomb(bomb, acceleration))
            bombState.Bounces++;
    }

    /// <summary>Consumes an overlapping Samus bomb and resolves the Mother Brain bomb explosion.</summary>
    private bool TryDestroyMotherBrainBombWithSamusBomb(
        RoomEnemyProjectileSlot bomb,
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
            _deathsAndDrops.SpawnEnemyDropFromEnemyHeader(
                position,
                state.Head.DefinitionId);
            return true;
        }

        return false;
    }

    /// <summary>Advances a bomb through its native trajectory and reports arena-boundary expiry.</summary>
    private static bool MoveMotherBrainBomb(
        RoomEnemyProjectileSlot bomb,
        int acceleration)
    {
        bomb.Velocity = bomb.Velocity.WithY(new SignedEightEight(unchecked((ushort)(bomb.Velocity.Y.RawValue + acceleration))));
        bomb.Position = bomb.Position.Advance(bomb.Velocity.ToSixteenSixteen());

        // Both boundaries are signed CMP/BMI tests. X reflects without clamping; the floor
        // clamps Y to $D0 and resets the two authored bounce velocities.
        if (unchecked((short)(bomb.Position.X.Whole - 240)) >= 0)
            bomb.Velocity = bomb.Velocity.WithX(new SignedEightEight(unchecked((ushort)-bomb.Velocity.X.RawValue)));
        if (unchecked((short)(bomb.Position.Y.Whole - 208)) < 0)
            return false;

        bomb.Position = bomb.Position.WithWholeY(208);
        int bounceSpeed = bomb.StateOf<MotherBrainBombState>().BounceSpeed;
        bomb.Velocity = new(SignedEightEight.FromSignedRaw(unchecked((short)bomb.Velocity.X.RawValue) < 0
            ? -bounceSpeed
            : bounceSpeed), SignedEightEight.FromSignedRaw(-512));
        return true;
    }

    /// <summary>Removes an expired bomb and emits the associated explosion effect.</summary>
    private void ExpireMotherBrainBomb(
        RoomEnemyProjectileSlot bomb,
        MotherBrainEnemyState state)
    {
        Position16<RoomPixels> impact = bomb.Position.Whole;
        int afterburnCount = unchecked((byte)bomb.Position.X.Fraction);
        DeleteMotherBrainBomb(bomb, state);

        // `$C50B-$C54B` clears the source slot before both allocations. They therefore use
        // the normal descending shared-pool competition, including legitimate same-slot
        // replacement that the outer scheduler will continue processing on this pass.
        _roomActors.SpawnAfterburnCenter(
            RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter,
            impact,
            afterburnCount);
        _projectiles.SpawnRoomGraphicsDustExplosion(impact, MiscDustAnimation.SmallExplosion);
        _motherBrainEncounter.RequestCueSound(MotherBrainSoundDefinitions.Explosion);
    }

    /// <summary>Clears a bomb slot when its native lifetime or collision ends.</summary>
    private static void DeleteMotherBrainBomb(
        RoomEnemyProjectileSlot bomb,
        MotherBrainEnemyState state)
    {
        state.RecordBombRemoved();
        bomb.Velocity = new(new SignedEightEight(0), new SignedEightEight(0));
        bomb.Clear();
    }

    /// <summary>Uses strict hitbox intersection for bomb-to-bomb contact.</summary>
    private static bool MotherBrainBombStrictOverlap(
        Position16<RoomPixels> first,
        Extent16<RoomPixels> firstRadius,
        Position16<RoomPixels> second,
        Extent16<RoomPixels> secondRadius) =>
        EnemyMath.WrappedMagnitude(unchecked((ushort)(first.X - second.X))) <
            firstRadius.X + secondRadius.X &&
        EnemyMath.WrappedMagnitude(unchecked((ushort)(first.Y - second.Y))) <
            firstRadius.Y + secondRadius.Y;

    /// <summary>Advances a death explosion projectile through its installed program.</summary>
    internal void RunMotherBrainDeathExplosion(RoomEnemyProjectileSlot projectile)
    {
        var body = _motherBrainEncounter.MotherBrain?.Body ?? throw new InvalidOperationException("Body-relative death explosion has no Mother Brain owner.");
        projectile.Position = projectile.Position.WithWhole(new(unchecked((ushort)(body.Position.X.Whole + projectile.Velocity.X.RawValue)), unchecked((ushort)(body.Position.Y.Whole + projectile.Velocity.Y.RawValue))));
    }

    /// <summary>Updates a door fragment's fall and expiration after the room collapse.</summary>
    internal void RunMotherBrainDoorFragment(RoomEnemyProjectileSlot fragment)
    {
        // The speed loses the drag, stopping at zero, and keeps its direction; a speed of at most
        // 32768 units less the drag always fits a signed 8.8 velocity.
        int velocity = fragment.Velocity.X.SignedRawValue;
        int speed = Math.Max(0, Math.Abs(velocity) - MotherBrainDeathDefinitions.DoorFragmentDrag);
        fragment.Velocity = fragment.Velocity.WithX(SignedEightEight.FromSignedRaw((short)(velocity < 0 ? -speed : speed)));
        fragment.Velocity = fragment.Velocity.WithY(new SignedEightEight(unchecked((ushort)(fragment.Velocity.Y.RawValue + MotherBrainDeathDefinitions.DoorFragmentGravity))));
        fragment.Position = fragment.Position.Advance(fragment.Velocity.ToSixteenSixteen());
        // The fragment's lifetime expires when it passes zero.
        MotherBrainDoorFragmentState fragmentState = fragment.StateOf<MotherBrainDoorFragmentState>();
        CountdownStep lifetime = fragmentState.Lifetime.Decrement();
        fragmentState.Lifetime = lifetime.Remaining;
        if (lifetime.IsNonNegative) return;
        Position16<RoomPixels> dust = fragment.Position.Whole - new Offset16<RoomPixels>(0, MotherBrainDeathDefinitions.DoorDustYOffset);
        fragment.Clear();
        _projectiles.SpawnRoomGraphicsDustExplosion(dust, MotherBrainDeathDefinitions.DoorDustParameter);
    }

    /// <summary>Keeps the escape subtitle projectile fixed to its screen position.</summary>
    internal static void PinMotherBrainEscapeSubtitle(RoomEnemyProjectileSlot subtitle)
    {
        subtitle.Velocity = new(new SignedEightEight(0), new SignedEightEight(0));
        subtitle.Position = subtitle.Position.WithWhole(new(MotherBrainDeathDefinitions.SubtitleX, MotherBrainDeathDefinitions.SubtitleY));
    }

    /// <summary>Runs pre-instruction $86:CBE7 for the four falling ceiling-tube actors.</summary>
    internal void RunMotherBrainTopTubePreInstruction(RoomEnemyProjectileSlot projectile)
    {
        MotherBrainTopTubeState tube = projectile.StateOf<MotherBrainTopTubeState>();
        if (tube.DustPending)
        {
            _projectiles.SpawnRoomGraphicsDustExplosion(
                new(projectile.Position.X.Whole, unchecked((ushort)(projectile.Position.Y.Whole + 8))),
                MiscDustAnimation.SmallDustCloud);
            tube.DustPending = false;
        }

        projectile.Velocity = projectile.Velocity.WithY(new SignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + 6))));
        projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
        if (projectile.Position.Y.Whole < 208)
            return;

        Position16<RoomPixels> position = projectile.Position.Whole;
        projectile.Clear();
        _projectiles.SpawnRoomGraphicsDustExplosion(position, MiscDustAnimation.Smoke);
    }

    /// <summary>
    /// Consumes one request emitted by PLM instruction <c>$D30B</c>. The request retains the
    /// executing PLM's block origin because initializer <c>$CDC5</c> calls
    /// <c>CalculatePlmBlockCoords(plm_id)</c> after the shared projectile allocator runs.
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

        RoomEnemyProjectileSlot? shard = _projectiles.AllocateEnemyProjectile();
        if (shard is null)
            return; // SpawnEprojInner silently loses actors when all eighteen slots are full.

        MotherBrainProjectileStarts.Initialize(
            shard,
            RoomEnemyProjectileKind.MotherBrainGlassShard,
            graphics: default);

        // `$86:CDC5` doubles the complete RNG word and masks it to an even nine-bit angle.
        // The low eight-bit angle selects signed sine/cosine velocities; bits 5..8 choose
        // one of sixteen ROM-authored shard animation lists.
        int angleOffset = (_room.Random.Generate() * 2) & 510;
        int angle = angleOffset >> 1;
        shard.Velocity = shard.Velocity.WithX(new SignedEightEight(ReadSignedSineSample(angle + 64)));
        shard.Velocity = shard.Velocity.WithY(new SignedEightEight(unchecked((ushort)(
            unchecked((short)ReadSignedSineSample(angle)) * 4))));
        int animationIndex = ((angleOffset >> 4) & 30) >> 1;
        shard.StartAnimation(new MotherBrainGlassAnimation(MotherBrainGlassAnimations.ShardProgram(animationIndex)));
        shard.Graphics = MotherBrainGlassShardDefinitions.Graphics;

        MotherBrainGlassShardPlacement placement =
            MotherBrainGlassShards.Placement(request.Shard);
        shard.Position = shard.Position.WithWhole(request.PlmBlock.ToRoomPixels() + placement.Offset);

        // Position jitter consumes two additional RNG samples in this order. It is not
        // cosmetic randomness: later turret and sparkle behavior observes the advanced seed.
        shard.Position = shard.Position.WithWholeX(unchecked((ushort)(
            shard.Position.X.Whole + (_room.Random.Generate() % 16) - 8)));
        shard.Position = shard.Position.WithWholeY(unchecked((ushort)(
            shard.Position.Y.Whole + (_room.Random.Generate() % 16) - 8)));
    }

    /// <summary>Ports glass-shard pre-instruction <c>$86:CE9B</c>.</summary>
    internal void RunMotherBrainGlassShardPreInstruction(RoomEnemyProjectileSlot shard)
    {
        shard.Position = shard.Position.Advance(shard.Velocity.ToSixteenSixteen());

        // Native tests the whole-coordinate high byte rather than camera visibility. A shard
        // is deleted as soon as wrapped Y leaves page zero; X may freely leave the viewport.
        if (RoomGridConversions.PixelToScreen(shard.Position.Y.Whole) != 0)
        {
            shard.Clear();
            return;
        }

        shard.Velocity = shard.Velocity.WithY(new SignedEightEight(unchecked((ushort)(shard.Velocity.Y.RawValue + 32))));

        // The masked test succeeds for 1/16 of samples. The condition itself consumes one
        // random value every live shard frame; a successful sparkle consumes two more below.
        // Bits 5 and 10 of the random word both clear: one sample in four.
        if ((_room.Random.Generate() & 1056) == 0)
            SpawnMotherBrainGlassSparkle(shard);
    }

    /// <summary>Ports sparkle initializer <c>$86:CE6D</c> using the spawning shard as parameter.</summary>
    internal void SpawnMotherBrainGlassSparkle(RoomEnemyProjectileSlot source)
    {
        RoomEnemyProjectileSlot? sparkle = _projectiles.AllocateEnemyProjectile();
        if (sparkle is null)
            return;

        MotherBrainProjectileStarts.Initialize(
            sparkle,
            RoomEnemyProjectileKind.MotherBrainGlassSparkle,
            graphics: default);
        sparkle.StartAnimation(new MotherBrainGlassAnimation(MotherBrainGlassProgram.Sparkle));
        sparkle.Position = sparkle.Position.WithWhole(new(unchecked((ushort)(
            source.Position.X.Whole + (_room.Random.Generate() % 32) - 16)), unchecked((ushort)(
            source.Position.Y.Whole + (_room.Random.Generate() % 32) - 16))));
        sparkle.Graphics = MotherBrainGlassShardDefinitions.Graphics;
    }

    /// <summary>Reads the native signed sine table as its raw two-byte sample.</summary>
    internal static ushort ReadSignedSineSample(int index) =>
        unchecked((ushort)Trigonometry.SignedNegativeCosineWord(index));

    /// <summary>
    /// Ports fired-child initializer <c>$86:C684-C75C</c>. Each child first advances the
    /// shared aimed cursor, then receives a randomized secondary displacement used only for
    /// its spawn frame. Surviving actors become stationary animated emitters; out-of-bounds
    /// children turn into the cartridge's large impact dust and earthquake.
    /// </summary>
    internal void SpawnMotherBrainHandBeamFired()
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ?? throw new InvalidOperationException(
            "Mother Brain hand-beam bytecode ran without its encounter state.");
        RoomEnemyProjectileSlot? beam = _projectiles.AllocateEnemyProjectile();
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
        ByteAngle scatterAngle = new ByteAngle(unchecked((byte)state.HandBeamNextAngle)).AddTableUnits(unchecked((byte)angleRandom));
        int speedRandom = _room.Random.Generate();
        // Three bits of the sample's upper random byte: a whole speed of zero to seven.
        int scatterSpeed = UnsignedEightEight.FromParts(whole: (byte)(SharedRandomState.UpperByte(speedRandom) & 7), fraction: 0);
        beam.Velocity = beam.Velocity.WithX(new SignedEightEight(EnemyMath.MultiplySineSample((ushort)scatterSpeed, scatterAngle)));
        beam.Velocity = beam.Velocity.WithY(new SignedEightEight(EnemyMath.MultiplySineSample(
            (ushort)scatterSpeed,
            scatterAngle.AddTableUnits(ByteAngle.QuarterTurn.RawValue))));
        beam.Position = beam.Position.Advance(beam.Velocity.ToSixteenSixteen());

        if (IsMotherBrainHandBeamOutsideArena(beam))
        {
            Position16<RoomPixels> impact = beam.Position.Whole;
            beam.Clear();
            _projectiles.SpawnRoomGraphicsDustExplosion(
                impact,
                MiscDustAnimation.BigExplosion);
            _motherBrainEncounter.RequestCueSound(MotherBrainSoundDefinitions.Explosion);
            _earthquake.EarthquakeTimer = WrappingCountdown.Start(10);
            _earthquake.EarthquakeType = 5;
            return;
        }

        beam.Velocity = new(new SignedEightEight(0), new SignedEightEight(0));
        beam.StartAnimation(new MotherBrainHandBeamAnimation());
    }

    /// <summary>Detects when a fired hand beam has crossed the fight arena's bounds.</summary>
    private static bool IsMotherBrainHandBeamOutsideArena(
        RoomEnemyProjectileSlot beam) =>
        unchecked((short)(beam.Position.Y.Whole - 34)) < 0 ||
        unchecked((short)(beam.Position.Y.Whole - 206)) >= 0 ||
        unchecked((short)(beam.Position.X.Whole - 2)) < 0 ||
        unchecked((short)(beam.Position.X.Whole - 238)) >= 0;

    /// <summary>Exact six-position mouth attachment table at <c>$86:C86E-C885</c>.</summary>
    internal void RunMotherBrainAttachedDroolPreInstruction(
        RoomEnemyProjectileSlot drool)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ?? throw new InvalidOperationException(
            "Mother Brain drool ran without its multipart encounter state.");
        RoomEnemySlot head = state.Head;
        int parameter = drool.StateOf<MotherBrainDroolState>().MouthPoint;

        drool.Position = drool.Position.WithWhole(head.Position.Whole + MotherBrainBehavior.MotherBrainDroolOffsets[parameter]);
        drool.Velocity = new(new SignedEightEight(0), new SignedEightEight(0));
    }

    /// <summary>
    /// Ports <c>$86:C886-C8AF</c>. The native actor uses signed 8.8 velocity, adds twelve
    /// units of gravity per frame, and switches to its four-map splash at world Y $D7.
    /// </summary>
    internal static void RunMotherBrainFallingDroolPreInstruction(
        RoomEnemyProjectileSlot drool)
    {
        drool.Velocity = drool.Velocity.WithY(new SignedEightEight(unchecked((ushort)(drool.Velocity.Y.RawValue + 12))));
        drool.Position = drool.Position.AdvanceY(drool.Velocity.Y.ToSixteenSixteenDelta());
        if (drool.Position.Y.Whole < 215)
            return;

        drool.Position = drool.Position.WithWholeY(unchecked((ushort)(drool.Position.Y.Whole - 4)));
        drool.StartAnimation(new MotherBrainEffectAnimation(MotherBrainEffectProgram.DroolHitFloor));
    }

    /// <summary>
    /// Ports <c>$86:C335-C431</c>. A live Baby slot is tested before Samus and the room,
    /// matching the private projectile's cross-enemy collision order during phase three.
    /// </summary>
    internal void RunMotherBrainOnionRingPreInstruction(
        RoomEnemyProjectileSlot ring,
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
            // `$C3B1-$C3C8` deletes every later ring as soon as the still-registered Baby
            // reaches zero health. Native clears only the projectile ID on this branch.
            if (baby.Health == 0)
            {
                ring.Kind = RoomEnemyProjectileKind.None;
                return;
            }

            if (MotherBrainOnionRingOverlapsBaby(ring, baby))
            {
                state.PendingBabyCryCount++;
                _ = baby.ApplyMotherBrainOnionRingHit();
                // `$86:C399-$C3A5` writes the Baby slot's Enemy.health from this projectile
                // pass; it must not wait for the Baby's next AI turn to publish it.
                RoomEnemySlot babySlot = state.BabyMetroidSlot ?? throw new InvalidDataException(
                    "A Mother Brain onion ring hit the Baby without its physical slot.");
                babySlot.Health = baby.Health;
                ExplodeMotherBrainOnionRing(ring, ring.Position.Whole);
                return;
            }
        }

        // This private collision path intentionally ignores Samus's current invincibility
        // timer. It writes a fresh $60 only after applying suit-divided damage.
        if (samus is not null && MotherBrainOnionRingOverlapsSamus(ring, samus))
        {
            Position16<RoomPixels> ringPosition = ring.Position.Whole;
            int damage = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
                ? 80 >> 2
                : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                    ? 80 >> 1
                    : 80;
            samus.Health = samus.Health <= damage
                ? 0
                : samus.Health - damage;
            samus.InvincibilityTimer = WrappingCountdown.Start(96);
            samus.KnockbackTimer = WrappingCountdown.Start(5);
            samus.KnockbackXDirection = unchecked((short)(samus.Position.X.Whole - ringPosition.X)) >= 0
                ? KnockbackHorizontalDirection.Right
                : KnockbackHorizontalDirection.Left;
            ExplodeMotherBrainOnionRing(ring, ringPosition);
            return;
        }

        int signedY = unchecked((short)ring.Position.Y.Whole);
        int signedX = unchecked((short)ring.Position.X.Whole);
        int screenX = unchecked((short)(ring.Position.X.Whole - cameraX));
        if (signedY < 32 || ring.Position.Y.Whole >= 216 || signedX < 0 ||
            screenX < 0 || screenX >= 248)
        {
            _earthquake.EarthquakeTimer = WrappingCountdown.Start(10);
            _earthquake.EarthquakeType = 5;
            ExplodeMotherBrainOnionRing(ring, ring.Position.Whole);
        }
    }

    /// <summary>Positions an unlaunched ring at the moving head's mouth.</summary>
    internal static void PinMotherBrainOnionRingToMouth(
        RoomEnemyProjectileSlot ring,
        MotherBrainEnemyState state)
    {
        RoomEnemySlot head = state.Head;
        ring.Position = ring.Position.WithWhole(new(unchecked((ushort)(head.Position.X.Whole + 10)), unchecked((ushort)(head.Position.Y.Whole + 16))));
    }

    /// <summary>Checks ring contact against Samus's current hitbox.</summary>
    private static bool MotherBrainOnionRingOverlapsSamus(
        RoomEnemyProjectileSlot ring,
        SamusState samus) =>
        EnemyMath.WrappedMagnitude(unchecked((ushort)(ring.Position.X.Whole - samus.Position.X.Whole))) <
            ring.Radius.X + samus.Kinematics.Radius.X &&
        EnemyMath.WrappedMagnitude(unchecked((ushort)(ring.Position.Y.Whole - samus.Position.Y.Whole))) <
            ring.Radius.Y + samus.Kinematics.Radius.Y;

    /// <summary>Checks ring contact against the Baby Metroid's current hitbox.</summary>
    private static bool MotherBrainOnionRingOverlapsBaby(
        RoomEnemyProjectileSlot ring,
        BabyMetroidCutsceneState baby) =>
        EnemyMath.WrappedMagnitude(unchecked((ushort)(ring.Position.X.Whole - baby.Position.X.Whole))) <
            ring.Radius.X + BabyMetroidCutsceneState.XHitboxRadius &&
        EnemyMath.WrappedMagnitude(unchecked((ushort)(ring.Position.Y.Whole - baby.Position.Y.Whole))) <
            ring.Radius.Y + BabyMetroidCutsceneState.YHitboxRadius;

    /// <summary>Ends a ring on contact and spawns its explosion effect.</summary>
    private void ExplodeMotherBrainOnionRing(
        RoomEnemyProjectileSlot ring,
        Position16<RoomPixels> position)
    {
        // `$C410` clears the source before allocating parameter-three room dust. If the
        // allocator reuses this physical slot, the outer projectile scheduler will advance
        // the newborn dust list during the same pass, matching EprojRunOne's original X.
        ring.Clear();
        _projectiles.SpawnRoomGraphicsDustExplosion(position, MiscDustAnimation.SmallExplosion);
        _motherBrainEncounter.RequestCueSound(MotherBrainSoundDefinitions.Explosion);
    }

    /// <summary>Ports <c>PreInstruction_EnemyProjectile_MotherBrainsTurrets</c>.</summary>
    internal void RunMotherBrainTurretPreInstruction(
        RoomEnemyProjectileSlot turret,
        Position16<RoomPixels> camera)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ??
            throw new InvalidDataException("A Mother Brain turret has no owning encounter state.");
        bool onScreen = MotherBrainTurretIsOnScreen(turret, camera);

        // Off-screen turrets deliberately freeze both timers until scrolled into view. Once
        // the shared deletion flag is set they disappear silently off-screen, but visible
        // turrets replace themselves with dust animation parameter $C.
        if (!onScreen)
        {
            if (state.DeleteTurretsAndRinkas)
                turret.Clear();
            return;
        }
        if (state.DeleteTurretsAndRinkas)
        {
            Position16<RoomPixels> position = turret.Position.Whole;
            turret.Clear();
            _projectiles.SpawnRoomGraphicsDustExplosion(position, MiscDustAnimation.Smoke);
            return;
        }

        // Each wait is a DEC, BEQ countdown that is reloaded when it reaches zero.
        MotherBrainTurretState turretState = turret.StateOf<MotherBrainTurretState>();
        CountdownStep rotationWait = turretState.RotationWait.Decrement();
        turretState.RotationWait = rotationWait.Remaining;
        if (rotationWait.IsZero)
        {
            ResetMotherBrainTurretRotationTimer(turret);
            SelectNextMotherBrainTurretDirection(turret);
            MotherBrainTurretDirection direction =
                MotherBrainTurretDefinitions.Ring.At(turretState.RingPosition);
            turret.StartAnimation(new MotherBrainTurretAnimation(MotherBrainTurretProgram.TurretPose, direction));
        }

        CountdownStep firingWait = turretState.FiringWait.Decrement();
        turretState.FiringWait = firingWait.Remaining;
        if (firingWait.IsZero)
        {
            ResetMotherBrainTurretCooldown(turret);
            SpawnMotherBrainTurretBullet(turret);
        }
    }

    /// <summary>Ports the signed direction-delta and allowed-rotation test at $86:C050.</summary>
    internal static void SelectNextMotherBrainTurretDirection(RoomEnemyProjectileSlot turret)
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

        // A disallowed next step reverses the signed high-byte delta and immediately takes
        // one step in the new direction. The cartridge does not perform a second mask test.
        // The direction and the delta each keep eight bits, so the reversed step wraps the
        // way the stored bytes do.
        state.RotationStep = -rotationDelta;
        state.RingPosition = currentDirection + state.RotationStep;
    }

    /// <summary>Restarts the turret's wait before it chooses another firing direction.</summary>
    internal void ResetMotherBrainTurretRotationTimer(RoomEnemyProjectileSlot turret)
    {
        int sample = unchecked((byte)_room.Random.Generate());
        turret.StateOf<MotherBrainTurretState>().RotationWait = WrappingCountdown.Start(Math.Max(sample, MotherBrainTurretDefinitions.MinimumRotationDelay));
    }

    /// <summary>Restarts the turret's pause between fired bullets.</summary>
    internal void ResetMotherBrainTurretCooldown(RoomEnemyProjectileSlot turret)
    {
        int sample = unchecked((byte)_room.Random.Generate());
        turret.StateOf<MotherBrainTurretState>().FiringWait = WrappingCountdown.Start(Math.Max(sample, MotherBrainTurretDefinitions.MinimumFiringCooldown));
    }

    /// <summary>Ports the asymmetric viewport bounds at $86:C0B4.</summary>
    private static bool MotherBrainTurretIsOnScreen(
        RoomEnemyProjectileSlot turret,
        Position16<RoomPixels> camera)
    {
        if (unchecked((short)turret.Position.Y.Whole) < 0 ||
            unchecked((short)(turret.Position.Y.Whole + 16 - camera.Y)) < 0 ||
            unchecked((ushort)(turret.Position.Y.Whole + 16 - camera.Y)) >= 256)
        {
            return false;
        }

        return unchecked((short)turret.Position.X.Whole) >= 0 &&
            unchecked((short)(turret.Position.X.Whole + 4 - camera.X)) >= 0 &&
            unchecked((ushort)(turret.Position.X.Whole + 4 - camera.X)) < 264;
    }

    /// <summary>Ports turret-bullet initialization at $86:BF59.</summary>
    internal void SpawnMotherBrainTurretBullet(RoomEnemyProjectileSlot turret)
    {
        RoomEnemyProjectileSlot? bullet = _projectiles.AllocateEnemyProjectile();
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
        // The bullet list's first instruction (`$86:C173`) jumps to the pose for this direction.
        bullet.StartAnimation(new MotherBrainTurretAnimation(MotherBrainTurretProgram.BulletPose, direction));
    }

    /// <summary>Ports the bullet property flicker, 8.8 movement, and point collision.</summary>
    internal static void RunMotherBrainTurretBulletPreInstruction(
        RoomEnemyProjectileSlot bullet,
        RoomLevelData level)
    {
        // Property $8000 is toggled before the projectile-vs-projectile pass every frame,
        // reproducing the bullet's alternating vulnerability rather than a visual alpha hack.
        bullet.BlocksSamusProjectiles = !bullet.BlocksSamusProjectiles;
        bullet.Position = bullet.Position.Advance(bullet.Velocity.ToSixteenSixteen());

        if (EnemyProjectileEngine.NonAirBlockAt(level, bullet.Position.Whole))
            bullet.Clear();
    }

    /// <summary>Pins the charging effect to the head while the beam winds up.</summary>
    internal void RunMotherBrainRainbowChargingPreInstruction(
        RoomEnemyProjectileSlot projectile)
    {
        RoomEnemySlot head = _motherBrainEncounter.MotherBrain?.Head ?? throw new InvalidOperationException(
            "Mother Brain rainbow charge ran without the physical head slot.");
        projectile.Position = projectile.Position.WithWhole(head.Position.Whole);
    }

    /// <summary>Advances the rainbow impact effect through its timed lifetime.</summary>
    internal static void RunMotherBrainRainbowExplosionPreInstruction(
        RoomEnemyProjectileSlot projectile,
        SamusState? samus)
    {
        SamusState target = samus ?? throw new InvalidOperationException(
            "Mother Brain rainbow explosion requires the live Samus actor.");
        projectile.Position = projectile.Position.WithWhole(new(unchecked((ushort)(target.Position.X.Whole + projectile.Velocity.X.RawValue)), unchecked((ushort)(target.Position.Y.Whole + projectile.Velocity.Y.RawValue))));
    }
}
