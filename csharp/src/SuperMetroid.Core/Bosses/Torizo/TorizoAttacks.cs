using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Bomb and Golden Torizo's projectiles: drool, swipes, sonic booms, Chozo orbs, eggs, super missiles, eye beams and explosions.</summary>
[RoomLifetime]
internal sealed class TorizoAttacks
{
    /// <summary>Supplies the live room services, boss flag, random state, and artwork for both statue variants.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the eighteen-slot enemy projectile pool used by Torizo attacks.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Queues the statue-break sound with the enemy audio limits.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Spawns the orb drops and the death scatter after the authored sequences.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Bomb and Golden Torizo's state lookups, facing, posture and the function their branches test.</summary>
    private readonly TorizoEncounterAccess _access;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal TorizoAttacks(EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemyAudioQueue audio, EnemyDeathsAndDrops deathsAndDrops, TorizoEncounterAccess access)
    {
        _room = room;
        _projectiles = projectiles;
        _audio = audio;
        _deathsAndDrops = deathsAndDrops;
        _access = access;
    }

    /// <summary>Golden Torizo's state while Golden Torizo owns slot zero.</summary>
    public TorizoEnemyState? GoldenTorizo =>
        _access.TorizoState is { IsGolden: true } ? _access.TorizoState : null;

    /// <summary>Installs movement handlers and typed animation cues for statue fragments, orbs, attacks, and reflected missiles.</summary>
    internal void AddBombTorizoProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddFrameBehavior(context =>
        {
            RunBombTorizoStatueBreakingFrameBehavior(context.Projectile, context.Level);
        }, EnemyProjectileFrameBehavior.BombTorizoStatueFragmentFall);

        table.AddFrameBehavior(context =>
        {
            RunBombTorizoChozoOrbFrameBehavior(context.Projectile, context.Level);
        }, EnemyProjectileFrameBehavior.BombTorizoOrbFall);

        table.AddFrameBehavior(context =>
        {
            RunGoldenTorizoChozoOrbFrameBehavior(context.Projectile, context.Level);
        }, EnemyProjectileFrameBehavior.GoldenTorizoOrbBounce);

        table.AddFrameBehavior(context =>
        {
            RunBombTorizoSonicBoomFrameBehavior(context.Projectile, context.Level);
        }, EnemyProjectileFrameBehavior.TorizoSonicBoomFlight);

        table.AddFrameBehavior(context =>
        {
            RunBombTorizoDroolFrameBehavior(context.Projectile, context.Level);
        }, EnemyProjectileFrameBehavior.BombTorizoDroolFall);

        table.AddFrameBehavior(context =>
        {
            RunGoldenTorizoEggFrameBehavior(context.Projectile, context.Level);
        }, EnemyProjectileFrameBehavior.GoldenTorizoEggBounce);

        table.AddFrameBehavior(context =>
        {
            RunGoldenTorizoEggHorizontalCharge(context.Projectile, context.Level);
        }, EnemyProjectileFrameBehavior.GoldenTorizoEggCharge);

        table.AddFrameBehavior(context =>
        {
            RunGoldenTorizoEggFall(context.Projectile, context.Level);
        }, EnemyProjectileFrameBehavior.GoldenTorizoEggDrop);

        table.AddFrameBehavior(context =>
        {
            RunGoldenTorizoSuperMissileFrameBehavior(context.Projectile);
        }, EnemyProjectileFrameBehavior.GoldenTorizoSuperMissileHeld);

        table.AddFrameBehavior(context =>
        {
            RunGoldenTorizoSuperMissileFlight(context.Projectile, context.Level);
        }, EnemyProjectileFrameBehavior.GoldenTorizoSuperMissileThrown);

        table.AddFrameBehavior(context =>
        {
            RunGoldenTorizoEyeBeamFrameBehavior(context.Projectile, context.Level);
        }, EnemyProjectileFrameBehavior.GoldenTorizoEyeBeamFlight);

        table.AddBranchingCue<TorizoProjectileSegment, TorizoProjectileCue>((context, cue) => ApplyTorizoProjectileCue(context.Projectile, context.Samus, cue));
        table.AddShotAnimation(RoomEnemyProjectileKind.BombTorizoChozoOrb, () => new TorizoProjectileAnimation(TorizoProjectileSegment.ChozoOrbShot));
        table.AddShotAnimation(RoomEnemyProjectileKind.GoldenTorizoChozoOrb, () => new TorizoProjectileAnimation(TorizoProjectileSegment.ChozoOrbShot));
        // The egg's definition names the Chozo orb's wall-impact animation as its shot animation.
        table.AddShotAnimation(RoomEnemyProjectileKind.GoldenTorizoEgg, () => new TorizoProjectileAnimation(TorizoProjectileSegment.ChozoOrbWallImpact));
        table.AddShotAnimation(RoomEnemyProjectileKind.GoldenTorizoSuperMissile, () => new TorizoProjectileAnimation(TorizoProjectileSegment.SuperMissileImpact));
    }

    /// <summary>
    /// Carries out one cue of a Torizo projectile's animation, returning the segment a conditional cue continues at, or
    /// null to run on.
    /// </summary>
    internal TorizoProjectileSegment? ApplyTorizoProjectileCue(EnemyProjectile projectile, SamusState? samus, TorizoProjectileCue cue)
    {
        switch (cue)
        {
            case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.ClearFrameBehavior }:
                projectile.FrameBehavior = EnemyProjectileFrameBehavior.Stationary;
                return null;
            case TorizoProjectileCue.SetFrameBehavior setFrameBehavior:
                projectile.FrameBehavior = setFrameBehavior.FrameBehavior;
                return null;
            case TorizoProjectileCue.SetProperties setProperties:
                EnemyProjectileEngine.SetPropertyFlags(projectile, setProperties.Properties);
                if (setProperties.Depth is { } setDepth)
                    projectile.Depth = setDepth;
                return null;
            case TorizoProjectileCue.ClearProperties clearProperties:
                EnemyProjectileEngine.ClearPropertyFlags(projectile, clearProperties.Properties);
                if (clearProperties.Depth is { } clearedDepth)
                    projectile.Depth = clearedDepth;
                return null;
            case TorizoProjectileCue.PlaySound playSound:
                _audio.QueueEnemySound(playSound.Sound, playSound.Priority);
                return null;
            case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.Delete }:
                projectile.Delete();
                return null;
            case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.Hatch }:
                return projectile.StateOf<GoldenTorizoEggState>().FacingRight ? TorizoProjectileSegment.EggHatchedRight : TorizoProjectileSegment.EggHatchedLeft;
            case TorizoProjectileCue.GoToIfEyeBeamExplosionsDisabled goToIfEyeBeamExplosionsDisabled:
            {
                TorizoEnemyState state = GoldenTorizo ??
                    throw new InvalidOperationException("Golden Torizo's eye beam has no owning Torizo state.");
                return state.EyeBeamExplosionsEnabled ? null : goToIfEyeBeamExplosionsDisabled.Target;
            }
            case TorizoProjectileCue.GoToSometimes goToSometimes:
                return _projectiles.TakeOneInFourBranch() ? goToSometimes.Target : null;
            case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.AimSuperMissileTowardSamus } or TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.AimSuperMissileAwayFromSamus }:
                SetGoldenTorizoSuperMissileVelocity(
                    projectile,
                    samus ?? throw new InvalidOperationException("Golden Torizo super-missile aiming requires the active Samus actor."),
                    awayFromSamus: cue is TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.AimSuperMissileAwayFromSamus });
                return null;
            case TorizoProjectileCue.SetRadius setRadius:
                projectile.Radius = setRadius.Radius;
                return null;
            case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.SpawnOrbDrops }:
                RequestTorizoChozoOrbDrop(projectile);
                return null;
            case TorizoProjectileCue.Scatter scatter:
                _projectiles.Scatter(projectile, scatter.Spread);
                return null;
            case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.ReturnToOrigin }:
                projectile.Position = projectile.Position.WithWhole(projectile.StateOf<AnchoredProjectileState>().Anchor);
                return null;
            case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.RaiseDust }:
                projectile.Position = projectile.Position.WithWrappedWholeY(projectile.Position.Y.Whole - 4);
                return null;
            case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.StopBlockingShots }:
                projectile.BlocksSamusProjectiles = false;
                return null;
            case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.StopHarmingSamus }:
                projectile.CanDamageSamus = false;
                return null;
            case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.DrawInFront }:
                projectile.Depth = EnemyProjectileDepth.InFrontOfSamus;
                return null;
            case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.NoOperation }:
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(TorizoProjectileCueKind)} {cue}.");
        }
    }

    /// <summary>The segment a Torizo projectile's typed animation stands in.</summary>
    private static TorizoProjectileSegment RunningTorizoProjectileAnimation(EnemyProjectile projectile) =>
        projectile.Animation is TorizoProjectileAnimation animation
            ? animation.PlayingSegment
            : throw new InvalidOperationException($"Enemy projectile {projectile.Kind} is not running a Torizo program.");

    /// <summary>Requests the recurring hurt-state drool variant with randomized flight.</summary>
    internal void SpawnBombTorizoLowHealthDrool(RoomEnemySlot torizo) =>
        SpawnBombTorizoDrool(torizo, RoomEnemyProjectileKind.BombTorizoLowHealthDrool);

    /// <summary>Allocates either drool animation and consumes room RNG in the variant-specific order.</summary>
    internal void SpawnBombTorizoDrool(
        RoomEnemySlot torizo,
        RoomEnemyProjectileKind kind)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            kind,
            SpriteGraphics.SpawnedBy(torizo));
        projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.DroolNoDelay));

        int random;
        if (kind == RoomEnemyProjectileKind.BombTorizoLowHealthDrool)
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(SelectLowHealthDroolAnimation(_room.Random.Generate())));
            random = _room.Random.Generate();
        }
        else
        {
            random = _room.Random.Generate();
        }
        if (kind == RoomEnemyProjectileKind.BombTorizoInitialDrool)
        {
            // Every vertical and horizontal move of a Torizo is tested against the room's terrain, solid past its edges,
            // so a Torizo's centre lies at least its half-height (41 pixels) below the room's top and its half-width
            // (18 pixels) inside either side: the lifted drool lies inside the room and its side offset never leaves it.
            int jitteredY = TorizoRandomDefinitions.InitialDroolYJitter.Of(random) + torizo.Position.Y.Whole;
            projectile.Position = projectile.Position.WithWholeY(jitteredY - TorizoMotionDefinitions.InitialDroolLift);
            // The jitter is five bits, so the fall speed lies between 49 and 80 256ths of a pixel.
            projectile.Velocity = projectile.Velocity.WithY(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(
                TorizoRandomDefinitions.DroolFallJitter.Of(random) + TorizoMotionDefinitions.InitialDroolSlowestFall));
            // Every branch stores a zero X velocity.
            int jitteredX = _room.Random.Next(TorizoRandomDefinitions.InitialDroolXJitter) + torizo.Position.X.Whole;
            projectile.Position = projectile.Position.WithWholeX(_access.Posture(torizo).HasFlag(TorizoPostureFlags.TurningAround)
                ? jitteredX
                : _access.FacesRight(torizo)
                    ? jitteredX + TorizoMotionDefinitions.InitialDroolSideOffset
                    : jitteredX - TorizoMotionDefinitions.InitialDroolSideOffset);
            projectile.Velocity = projectile.Velocity.WithX(SignedEightEight.Zero);
            return;
        }
        projectile.Position = projectile.Position.WithWrappedWholeY(torizo.Position.Y.Whole - 5);

        // The recurring low-health drool chooses a random direction. The
        // turning branch's angle is a random turning angle; the X velocity is the signed
        // sine and the Y velocity the negative cosine of that angle.
        TrigonometryAngle angle;
        if (_access.Posture(torizo).HasFlag(TorizoPostureFlags.TurningAround))
        {
            angle = new(TorizoRandomDefinitions.DroolTurningAngle.Of(random));
        }
        else
        {
            int baseAngle = _access.FacesRight(torizo) ? 32 : 224;
            // The spread of up to fifteen units, less eight, keeps either facing's angle on the turn.
            angle = new(baseAngle + TorizoRandomDefinitions.DroolFacingSpread.Of(random) - 8);
        }
        projectile.Velocity = projectile.Velocity.WithX(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(
            Trigonometry.SignedSine(angle)));
        projectile.Velocity = projectile.Velocity.WithY(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(
            Trigonometry.SignedSine(angle - TrigonometryAngle.QuarterTurn)));
        projectile.Position = projectile.Position.WithWholeX(_access.FacesRight(torizo)
            ? CoordinateAxis.Wrap(torizo.Position.X.Whole + 8)
            : CoordinateAxis.Wrap(torizo.Position.X.Whole - 8));
    }

    /// <summary>
    /// The drool a low-health drool starts: the entry of <see cref="TorizoRandomDefinitions.LowHealthDroolAnimations"/>
    /// the sample's <see cref="TorizoRandomDefinitions.LowHealthDroolDelay"/> draws.
    /// </summary>
    internal static TorizoProjectileSegment SelectLowHealthDroolAnimation(int random) =>
        TorizoRandomDefinitions.LowHealthDroolAnimations[TorizoRandomDefinitions.LowHealthDroolDelay.Of(random)];

    /// <summary>Places a swipe explosion at the attack-table hand offset, mirroring it for right-facing Torizo.</summary>
    internal void SpawnBombTorizoExplosiveSwipe(RoomEnemySlot torizo, int swipe)
    {
        Offset16<RoomPixels> placement = BombTorizoAttacks.Swipe(swipe);

        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoExplosiveSwipe,
            graphics: default);
        projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.SwipeInitial));

        // A right-facing Torizo mirrors the left-facing hand placement horizontally.
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole +
            (_access.FacesRight(torizo) ? placement.MirroredX() : placement));
    }

    /// <summary>Places a body-break explosion at the gut or face for Torizo's facing and records its impact coordinates.</summary>
    internal void SpawnBombTorizoLowHealthExplosion(RoomEnemySlot torizo, BombTorizoExplosionSite site)
    {
        Offset16<RoomPixels> placement =
            BombTorizoAttacks.LowHealthExplosion(
                site,
                facingRight: _access.FacesRight(torizo));

        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoLowHealthExplosion,
            graphics: default);
        projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.ExplosionLowHealthInitial));
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + placement);
        projectile.State = new AnchoredProjectileState(projectile.Position.Whole);
    }

    /// <summary>Starts a death explosion at the statue center and stores that center for its later animation frames.</summary>
    internal void SpawnBombTorizoDeathExplosion(RoomEnemySlot torizo)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoDeathExplosion,
            graphics: default);
        projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.ExplosionDeathInitial));
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole);
        projectile.State = new AnchoredProjectileState(torizo.Position.Whole);
    }

    /// <summary>Allocates Bomb Torizo's Chozo orb with facing-dependent placement and randomized velocity.</summary>
    internal void SpawnBombTorizoChozoOrb(RoomEnemySlot torizo)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoChozoOrb,
            SpriteGraphics.SpawnedBy(torizo));

        InitializeTorizoRandomizedProjectile(
            projectile,
            torizo,
            TorizoRandomizedProjectiles.BombChozoOrb(
                facingRight: _access.FacesRight(torizo)));
    }

    /// <summary>Starts Bomb Torizo's sonic-boom animation at one of two randomized body heights.</summary>
    internal void SpawnBombTorizoSonicBoom(RoomEnemySlot torizo)
        => SpawnTorizoSonicBoom(
            torizo,
            RoomEnemyProjectileKind.BombTorizoSonicBoom);

    /// <summary>Starts Golden Torizo's sonic-boom variant with the shared directional flight setup.</summary>
    internal void SpawnGoldenTorizoSonicBoom(RoomEnemySlot torizo)
        => SpawnTorizoSonicBoom(
            torizo,
            RoomEnemyProjectileKind.GoldenTorizoSonicBoom);

    /// <summary>Places a sonic boom 32 pixels ahead and selects the fired-left or fired-right animation.</summary>
    private void SpawnTorizoSonicBoom(
        RoomEnemySlot torizo,
        RoomEnemyProjectileKind kind)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            kind,
            SpriteGraphics.SpawnedBy(torizo));

        projectile.Position = projectile.Position.WithWrappedWholeY(
            torizo.Position.Y.Whole + (TorizoRandomDefinitions.SonicBoomHigh.IsSetIn(_room.Random.Generate()) ? -12 : 20));
        if (_access.FacesRight(torizo))
        {
            projectile.Position = projectile.Position.WithWrappedWholeX(torizo.Position.X.Whole + 32);
            projectile.Velocity = projectile.Velocity.WithX(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(624));
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.SonicBoomFiredRight));
        }
        else
        {
            projectile.Position = projectile.Position.WithWrappedWholeX(torizo.Position.X.Whole - 32);
            projectile.Velocity = projectile.Velocity.WithX(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-624));
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.SonicBoomFiredLeft));
        }
    }

    /// <summary>Allocates Golden Torizo's orb using the variant's facing-specific offset and randomized speed.</summary>
    internal void SpawnGoldenTorizoChozoOrb(RoomEnemySlot torizo)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.GoldenTorizoChozoOrb,
            SpriteGraphics.SpawnedBy(torizo));
        InitializeTorizoRandomizedProjectile(
            projectile,
            torizo,
            TorizoRandomizedProjectiles.GoldenChozoOrb(
                facingRight: _access.FacesRight(torizo)));
    }

    /// <summary>Starts a Golden egg with the game's fixed 66-frame hatch timer and randomized launch speed.</summary>
    internal void SpawnGoldenTorizoEgg(RoomEnemySlot torizo)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.GoldenTorizoEgg,
            SpriteGraphics.SpawnedBy(torizo));

        TorizoEnemyState state = _access.RequireBombTorizoState(torizo);
        projectile.State = new GoldenTorizoEggState(facingRight: state.Posture.HasFlag(TorizoPostureFlags.FacingRight));
        InitializeTorizoRandomizedProjectile(
            projectile,
            torizo,
            TorizoRandomizedProjectiles.GoldenEgg(
                movingRight: state.Posture.HasFlag(TorizoPostureFlags.FacingRight)));
    }

    /// <summary>Attaches a caught super missile above the active hand and selects its facing-dependent animation.</summary>
    internal void SpawnGoldenTorizoSuperMissile(RoomEnemySlot torizo)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.GoldenTorizoSuperMissile,
            SpriteGraphics.SpawnedBy(torizo));
        bool facingRight = _access.FacesRight(torizo);
        projectile.Position = projectile.Position.WithWhole(new(CoordinateAxis.Wrap(torizo.Position.X.Whole + (facingRight ? 30 : -30)), CoordinateAxis.Wrap(torizo.Position.Y.Whole - 52)));
        projectile.StartAnimation(new TorizoProjectileAnimation(facingRight ? TorizoProjectileSegment.SuperMissileRightInitial : TorizoProjectileSegment.SuperMissileLeftInitial));
    }

    /// <summary>Fires an eye beam from the facing-specific offset with room-RNG angular spread.</summary>
    internal void SpawnGoldenTorizoEyeBeam(RoomEnemySlot torizo)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.GoldenTorizoEyeBeam,
            SpriteGraphics.SpawnedBy(torizo));
        bool facingRight = _access.FacesRight(torizo);
        InitializeTorizoRandomizedProjectile(
            projectile,
            torizo,
            TorizoRandomizedProjectiles.GoldenEyeBeam(facingRight));

        // The beam's heading is its facing's three eighths of a turn (or a quarter further, facing left) spread by up to
        // eight steps either way; the sum never leaves the turn.
        var heading = new TrigonometryAngle(
            _room.Random.Next(TorizoRandomDefinitions.EyeBeamSpread) - 8 + 96 + (facingRight ? 0 : 64));
        // The heading points up at zero: the horizontal speed is its sine and the vertical speed its negative cosine,
        // the sine a quarter turn back. The samples lie within one pixel either way, so eight times one stays within eight.
        projectile.Velocity = projectile.Velocity.WithX(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(8 *
            Trigonometry.SignedSine(heading)));
        projectile.Velocity = projectile.Velocity.WithY(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(8 *
            Trigonometry.SignedSine(heading - TrigonometryAngle.QuarterTurn)));
    }

    /// <summary>Applies the projectile definition and draws horizontal jitter before vertical jitter.</summary>
    internal void InitializeTorizoRandomizedProjectile(
        EnemyProjectile projectile,
        RoomEnemySlot torizo,
        TorizoRandomizedProjectileDefinition definition)
    {
        projectile.StartAnimation(new TorizoProjectileAnimation(definition.Animation));
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + definition.Offset);
        // The initializer draws the X jitter before the Y jitter.
        projectile.Velocity = new EightEightVector<RoomPixels>(
            definition.BaseVelocity.X + VelocityJitter(_room.Random.Generate()),
            definition.BaseVelocity.Y + VelocityJitter(_room.Random.Generate()));
    }

    /// <summary>A randomized projectile's jitter on one axis: the random number modulo 256, less 128, in 256ths of a pixel.</summary>
    private static SignedEightEight VelocityJitter(int random) =>
        SignedEightEight.FromTwoHundredFiftySixthsOfPixel(TorizoRandomDefinitions.VelocityJitter.Of(random) - 128);

    /// <summary>Places a foot-specific dust effect 48 pixels below the statue after a landing step.</summary>
    internal void SpawnBombTorizoLandingDust(RoomEnemySlot torizo, HorizontalDirection foot)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        RoomEnemyProjectileKind kind = foot == HorizontalDirection.Right
            ? RoomEnemyProjectileKind.BombTorizoRightFootDust
            : RoomEnemyProjectileKind.BombTorizoLeftFootDust;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, graphics: default);
        projectile.StartAnimation(new TorizoProjectileAnimation(foot == HorizontalDirection.Right ? TorizoProjectileSegment.LandingDustRightFoot : TorizoProjectileSegment.LandingDustLeftFoot));
        projectile.Position = projectile.Position.WithWhole(new(CoordinateAxis.Wrap(torizo.Position.X.Whole + (foot == HorizontalDirection.Right ? 24 : -24)), CoordinateAxis.Wrap(torizo.Position.Y.Whole + 48)));
    }

    /// <summary>Moves Bomb Torizo's orb through walls and floor, disabling damage on impact and adding gravity in flight.</summary>
    internal static void RunBombTorizoChozoOrbFrameBehavior(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.ChozoOrbWallImpact));
            projectile.CanDamageSamus = false;
            return;
        }

        bool verticalCollision = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical);
        if (!projectile.Velocity.Y.IsNegative && verticalCollision)
        {
            projectile.Position = projectile.Position.WithWrappedWholeY(
                BlockTop(projectile.Position.Y.Whole) + 6);
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.ChozoOrbFloorImpact));
            projectile.CanDamageSamus = false;
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + TorizoMotionDefinitions.BombOrbGravity);
        if (IsRunaway(projectile.Velocity.Y))
            projectile.Delete();
    }

    /// <summary>Accelerates the sonic boom horizontally and switches to its non-damaging wall-impact animation.</summary>
    internal static void RunBombTorizoSonicBoomFrameBehavior(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.SonicBoomWallImpact));
            projectile.State = new AnchoredProjectileState(projectile.Position.Whole);
            projectile.CanDamageSamus = false;
            return;
        }

        projectile.Velocity = projectile.Velocity.WithX(projectile.Velocity.X.IsNegative
            ? projectile.Velocity.X - TorizoMotionDefinitions.SonicBoomAcceleration
            : projectile.Velocity.X + TorizoMotionDefinitions.SonicBoomAcceleration);
        if (IsRunaway(projectile.Velocity.X))
            projectile.Delete();
    }

    /// <summary>
    /// Runs the drool's frame behaviour, shared by Bomb Torizo's six gut-break droplets and its
    /// low-health initial drool. The peculiar positive-three clamp is authentic: both the
    /// negative and nonnegative drag branches load three when they cross zero.
    /// </summary>
    internal static void RunBombTorizoDroolFrameBehavior(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.DroolWallImpact));
            return;
        }

        // The drag slows the drool toward rest; a step that would carry it past rest leaves it drifting right instead.
        SignedEightEight xVelocity = projectile.Velocity.X;
        SignedEightEight dragged = xVelocity.IsNegative
            ? xVelocity + TorizoMotionDefinitions.DroolDrag
            : xVelocity - TorizoMotionDefinitions.DroolDrag;
        projectile.Velocity = projectile.Velocity.WithX(dragged.IsNegative == xVelocity.IsNegative
            ? dragged
            : TorizoMotionDefinitions.DroolPastRestDrift);

        bool verticalCollision = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical);
        if (!projectile.Velocity.Y.IsNegative && verticalCollision)
        {
            projectile.Position = projectile.Position.WithWrappedWholeY(projectile.Position.Y.Whole - 3);
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.DroolFloorImpact));
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + TorizoMotionDefinitions.DroolAndMissileGravity);
        if (IsRunaway(projectile.Velocity.Y))
            projectile.Delete();
    }

    /// <summary>Bounces Golden Torizo's orb off walls and the floor, slowing horizontal motion until floor impact.</summary>
    internal static void RunGoldenTorizoChozoOrbFrameBehavior(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
            projectile.Velocity = projectile.Velocity.WithX(-projectile.Velocity.X);

        bool verticalCollision = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical);
        if (verticalCollision && !projectile.Velocity.Y.IsNegative)
        {
            SignedEightEight xVelocity = projectile.Velocity.X;
            projectile.Velocity = projectile.Velocity.WithX(xVelocity.IsNegative
                ? xVelocity + TorizoMotionDefinitions.GoldenOrbBounceDrag
                : xVelocity - TorizoMotionDefinitions.GoldenOrbBounceDrag);
            // The orb is falling here, so halving its speed in 256ths rounds toward zero before it reverses.
            projectile.Velocity = projectile.Velocity.WithY(-projectile.Velocity.Y.DividedRoundingDown(2));
            if (HasSettled(projectile.Velocity.Y))
            {
                projectile.Position = projectile.Position.WithWrappedWholeY(
                    BlockTop(projectile.Position.Y.Whole) + 6);
                projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.ChozoOrbFloorImpact));
                projectile.CanDamageSamus = false;
                return;
            }
        }

        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + TorizoMotionDefinitions.GoldenOrbGravity);
    }

    /// <summary>Counts down the egg's hatch timer while bouncing it off terrain with horizontal drag and gravity.</summary>
    internal static void RunGoldenTorizoEggFrameBehavior(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        GoldenTorizoEggState egg = projectile.StateOf<GoldenTorizoEggState>();
        if (--egg.HatchTimer < 0)
        {
            // The hatch timer steps the sleeping animation past its sleep.
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileAnimations.AfterSleep(RunningTorizoProjectileAnimation(projectile))));
            projectile.Velocity = projectile.Velocity.WithX(SignedEightEight.FromPixels(egg.FacingRight ? 1 : -1));
            return;
        }

        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.Velocity = projectile.Velocity.WithX(-projectile.Velocity.X);
            egg.FacingRight = !egg.FacingRight;
        }

        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical) &&
            !projectile.Velocity.Y.IsNegative)
        {
            SignedEightEight xVelocity = projectile.Velocity.X;
            projectile.Velocity = projectile.Velocity.WithX(xVelocity.IsNegative
                ? xVelocity + TorizoMotionDefinitions.EggBounceDrag
                : xVelocity - TorizoMotionDefinitions.EggBounceDrag);
            projectile.Velocity = projectile.Velocity.WithY(-projectile.Velocity.Y);
        }

        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + TorizoMotionDefinitions.EggAcceleration);
        if (IsRunaway(projectile.Velocity.Y))
            projectile.Delete();
    }

    /// <summary>Accelerates the hatched egg sideways and changes to its fall phase when it hits a wall.</summary>
    internal static void RunGoldenTorizoEggHorizontalCharge(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.FrameBehavior =
                EnemyProjectileFrameBehavior.GoldenTorizoEggDrop;
            projectile.Velocity = projectile.Velocity.WithY(SignedEightEight.Zero);
            return;
        }

        projectile.Velocity = projectile.Velocity.WithX(projectile.StateOf<GoldenTorizoEggState>().FacingRight
            ? projectile.Velocity.X + TorizoMotionDefinitions.EggAcceleration
            : projectile.Velocity.X - TorizoMotionDefinitions.EggAcceleration);
    }

    /// <summary>Accelerates the egg downward until floor contact selects the facing-specific break animation.</summary>
    internal static void RunGoldenTorizoEggFall(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical))
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(projectile.StateOf<GoldenTorizoEggState>().FacingRight ? TorizoProjectileSegment.EggBreakRight : TorizoProjectileSegment.EggBreakLeft));
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + TorizoMotionDefinitions.EggAcceleration);
    }

    /// <summary>Keeps the held super missile beside Golden Torizo's hand, clearing it if its owner is gone.</summary>
    internal void RunGoldenTorizoSuperMissileFrameBehavior(EnemyProjectile projectile)
    {
        TorizoEnemyState? state = GoldenTorizo;
        if (state is null)
        {
            projectile.Delete();
            return;
        }

        RoomEnemySlot torizo = state.Slot;
        projectile.Position = projectile.Position.WithWhole(new(CoordinateAxis.Wrap(torizo.Position.X.Whole +
            (_access.FacesRight(torizo) ? 32 : -32)), CoordinateAxis.Wrap(torizo.Position.Y.Whole - 52)));
    }

    /// <summary>Moves the thrown super missile until wall or downward floor impact, applying gravity in flight.</summary>
    internal static void RunGoldenTorizoSuperMissileFlight(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        bool impact = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal);
        if (!impact)
        {
            bool verticalCollision = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical);
            impact = verticalCollision && !projectile.Velocity.Y.IsNegative;
        }

        if (impact)
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.SuperMissileImpact));
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + TorizoMotionDefinitions.DroolAndMissileGravity);
        if (IsRunaway(projectile.Velocity.Y))
            projectile.Delete();
    }

    /// <summary>Aims the thrown missile toward or away from Samus using the up-zero angle convention.</summary>
    internal static void SetGoldenTorizoSuperMissileVelocity(
        EnemyProjectile projectile,
        SamusState samus,
        bool awayFromSamus)
    {
        TrigonometryAngle bearing = EnemyMath.CalculateBearing(
            new(CoordinateAxis.Difference(samus.Position.X.Whole, projectile.Position.X.Whole), CoordinateAxis.Difference(samus.Position.Y.Whole, projectile.Position.Y.Whole))).ToMirroredTableAngle();
        TrigonometryAngle angle = awayFromSamus ? bearing.InSecondHalfTurn() : bearing.InFirstHalfTurn();

        // Preserve the up-zero angle convention: X is sine, Y is
        // negative cosine, the sine a quarter turn back. Starting both reads at sine rotates the shot.
        projectile.Velocity = projectile.Velocity.WithX(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(4 *
            Trigonometry.SignedSine(angle)));
        projectile.Velocity = projectile.Velocity.WithY(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(4 *
            Trigonometry.SignedSine(angle - TrigonometryAngle.QuarterTurn)));
    }

    /// <summary>Selects wall or floor impact artwork when the eye beam strikes terrain.</summary>
    internal static void RunGoldenTorizoEyeBeamFrameBehavior(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.EyeBeamWallImpact));
            return;
        }

        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical))
        {
            projectile.Position = projectile.Position.WithWrappedWholeY(
                BlockTop(projectile.Position.Y.Whole) + 6);
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.EyeBeamFloorImpact));
        }
    }

    /// <summary>
    /// Runs the orb's drop step. The cue names two drop definitions, Bomb Torizo's area-zero orb and Golden Torizo's
    /// nonzero-area orb; the loaded variant selects between them, the room-scoped equivalent of consulting the area.
    /// </summary>
    internal void RequestTorizoChozoOrbDrop(EnemyProjectile projectile)
    {
        if (projectile.Kind is not (
                RoomEnemyProjectileKind.BombTorizoChozoOrb or
                RoomEnemyProjectileKind.GoldenTorizoChozoOrb))
        {
            throw new InvalidOperationException(
                $"Projectile {projectile.Kind} reached the Torizo-orb drop.");
        }

        EnemyDefinitionId definitionId = GoldenTorizo is not null
            ? EnemyDefinitionId.GoldenTorizoOrb
            : EnemyDefinitionId.BombTorizoOrb;

        RoomEnemyDefinition definition = _room.LoadedDefinitions.ReadEnemyDefinition(definitionId);
        _deathsAndDrops.SpawnEnemyDropFromChanceTable(
            projectile.Position.Whole,
            definition.DropChances);
    }

    /// <summary>
    /// Allocates the room-graphics enemy projectile a room object's script requests, then runs the statue fragment's
    /// initializer against the publishing room object's block coordinates.
    /// </summary>
    public EnemyProjectile? SpawnBombTorizoStatueBreakingProjectile(
        BombTorizoStatueProjectileRequest request)
    {
        _room.EnsureLoaded();
        BombTorizoStatueFragmentDefinition definition =
            BombTorizoStatueFragments.ForFragment(request.Fragment);

        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return null; // The projectile spawner silently drops work when all eighteen slots fill.

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoStatueBreaking,
            graphics: default);

        projectile.StartAnimation(new TorizoProjectileAnimation(definition.Animation));
        projectile.Position = projectile.Position.WithWhole(request.RoomObjectBlock.ToRoomPixels() + definition.Offset);
        projectile.Velocity = projectile.Velocity.WithY(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(definition.YVelocity));
        projectile.State = new BombTorizoStatueFragmentState(definition.Acceleration);
        return projectile;
    }

    /// <summary>
    /// Runs the statue fragment's frame behaviour: a rising or still-airborne piece accelerates by
    /// <see cref="FirstSlotFallAcceleration"/>.
    /// </summary>
    internal void RunBombTorizoStatueBreakingFrameBehavior(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        bool collided = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical);
        if (projectile.Velocity.Y.IsNegative || !collided)
        {
            EnemyProjectile firstSlot = _projectiles.EnemyProjectiles[0];
            SignedEightEight acceleration = FirstSlotFallAcceleration.Of(
                firstSlot.IsActive,
                firstSlot.IsActive && firstSlot.State is BombTorizoStatueFragmentState piece ? piece.Acceleration : null);
            // A piece's speed stops at the runaway speed, so the acceleration never carries it out of range.
            projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + acceleration);
            if (IsRunaway(projectile.Velocity.Y))
                projectile.Velocity = projectile.Velocity.WithY(TorizoMotionDefinitions.RunawaySpeed);
            return;
        }

        projectile.FrameBehavior =
            EnemyProjectileFrameBehavior.Stationary;
    }
    /// <summary>
    /// Whether a falling projectile has run away: its raw 8.8 speed is at least sixteen pixels per frame and below
    /// thirty-two.
    /// </summary>
    private static bool IsRunaway(SignedEightEight speed) =>
        speed >= TorizoMotionDefinitions.RunawaySpeed && speed < TorizoMotionDefinitions.TwiceRunawaySpeed;

    /// <summary>
    /// Whether a bounced Chozo orb's rebound has settled: an upward speed under half a pixel per frame.
    /// </summary>
    private static bool HasSettled(SignedEightEight speed) =>
        speed.IsNegative && speed >= -TorizoMotionDefinitions.SettledBounceSpeed;

    /// <summary>The top of the room block a whole pixel coordinate lies in.</summary>
    private static int BlockTop(int whole) => whole - whole % TorizoMotionDefinitions.BlockPixels;
}
