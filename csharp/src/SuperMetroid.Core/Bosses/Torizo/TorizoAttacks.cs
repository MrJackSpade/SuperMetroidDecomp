using SuperMetroid.Core.Animation;
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
    /// <summary>Bomb and Golden Torizo's state lookups, facing, posture and the phase their branches test.</summary>
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

    /// <summary>
    /// Torizo's drool, swipes, explosions, sonic booms, eye beam, landing dust and statue fragments (#1760): their
    /// spawners start their animations, and a shot one runs the shared delete animation.
    /// </summary>
    internal EnemyProjectileBehavior TorizoProjectiles => field ??= new TorizoProjectileBehavior(_room, _projectiles, _audio, _deathsAndDrops, _access);

    /// <summary>A Chozo orb, Bomb or Golden Torizo's (#1760): a shot one runs its typed shot animation.</summary>
    internal EnemyProjectileBehavior ChozoOrb => field ??= new TorizoProjectileBehavior(_room, _projectiles, _audio, _deathsAndDrops, _access, () => new TorizoProjectileAnimation(TorizoProjectileSegment.ChozoOrbShot));

    /// <summary>A Golden Torizo egg (#1760): its definition names the Chozo orb's wall-impact animation as its shot animation.</summary>
    internal EnemyProjectileBehavior Egg => field ??= new TorizoProjectileBehavior(_room, _projectiles, _audio, _deathsAndDrops, _access, () => new TorizoProjectileAnimation(TorizoProjectileSegment.ChozoOrbWallImpact));

    /// <summary>A super missile Golden Torizo catches and throws (#1760): a shot one runs its typed impact.</summary>
    internal EnemyProjectileBehavior SuperMissile => field ??= new TorizoProjectileBehavior(_room, _projectiles, _audio, _deathsAndDrops, _access, () => new TorizoProjectileAnimation(TorizoProjectileSegment.SuperMissileImpact));

    /// <summary>
    /// One of the Torizos' projectiles: every motion of the statue fragments, orbs, sonic booms, drool, eggs,
    /// super missiles and eye beam, and the Torizo projectile animations' cues, which may continue an animation elsewhere.
    /// </summary>
    /// <param name="room">The loaded room, whose definitions name the orbs' drops.</param>
    /// <param name="projectiles">The enemy projectiles: their first place, the one-in-four branch and the scatter.</param>
    /// <param name="audio">The enemy sound queue the projectiles' sounds join.</param>
    /// <param name="deathsAndDrops">The drops a shot Chozo orb leaves.</param>
    /// <param name="access">Bomb and Golden Torizo's state and facing.</param>
    /// <param name="shot">The animation a destroying shot starts, or null for the shared delete animation.</param>
    private sealed class TorizoProjectileBehavior(
        EnemyRoomBinding room,
        EnemyProjectileEngine projectiles,
        EnemyAudioQueue audio,
        EnemyDeathsAndDrops deathsAndDrops,
        TorizoEncounterAccess access,
        Func<EnemyProjectileAnimation>? shot = null)
        : EnemyProjectileMotionBehavior<TorizoProjectileMotion>(StartingMotions, shot), IEnemyProjectileBranchingCues<TorizoProjectileSegment, TorizoProjectileCue>
    {
        /// <summary>The sonic booms, eye beam, orbs, eggs and held super missile move from their spawn; the rest hold still until their animations start them.</summary>
        private static readonly Dictionary<RoomEnemyProjectileKind, TorizoProjectileMotion?> StartingMotions = new()
        {
            [RoomEnemyProjectileKind.BombTorizoSonicBoom] = TorizoProjectileMotion.SonicBoomFlying,
            [RoomEnemyProjectileKind.GoldenTorizoSonicBoom] = TorizoProjectileMotion.SonicBoomFlying,
            [RoomEnemyProjectileKind.GoldenTorizoEyeBeam] = TorizoProjectileMotion.EyeBeamFlying,
            [RoomEnemyProjectileKind.BombTorizoChozoOrb] = TorizoProjectileMotion.BombOrbFalling,
            [RoomEnemyProjectileKind.GoldenTorizoChozoOrb] = TorizoProjectileMotion.GoldenOrbBouncing,
            [RoomEnemyProjectileKind.GoldenTorizoEgg] = TorizoProjectileMotion.EggBouncing,
            [RoomEnemyProjectileKind.GoldenTorizoSuperMissile] = TorizoProjectileMotion.SuperMissileHeld,
            [RoomEnemyProjectileKind.BombTorizoLowHealthDrool] = null,
            [RoomEnemyProjectileKind.BombTorizoInitialDrool] = null,
            [RoomEnemyProjectileKind.BombTorizoExplosiveSwipe] = null,
            [RoomEnemyProjectileKind.BombTorizoLowHealthExplosion] = null,
            [RoomEnemyProjectileKind.BombTorizoDeathExplosion] = null,
            [RoomEnemyProjectileKind.BombTorizoStatueBreaking] = null,
            [RoomEnemyProjectileKind.BombTorizoRightFootDust] = null,
            [RoomEnemyProjectileKind.BombTorizoLeftFootDust] = null,
        };

        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, TorizoProjectileMotion motion)
        {
            EnemyProjectile projectile = context.Projectile;
            switch (motion)
            {
                case TorizoProjectileMotion.StatueFragmentFalling:
                    DropStatueFragment(projectile, context.Level);
                    break;
                case TorizoProjectileMotion.BombOrbFalling:
                    DropBombOrb(projectile, context.Level);
                    break;
                case TorizoProjectileMotion.GoldenOrbBouncing:
                    BounceGoldenOrb(projectile, context.Level);
                    break;
                case TorizoProjectileMotion.SonicBoomFlying:
                    FlySonicBoom(projectile, context.Level);
                    break;
                case TorizoProjectileMotion.DroolFalling:
                    DropDrool(projectile, context.Level);
                    break;
                case TorizoProjectileMotion.EggBouncing:
                    BounceEgg(projectile, context.Level);
                    break;
                case TorizoProjectileMotion.EggCharging:
                    RunGoldenTorizoEggHorizontalCharge(projectile, context.Level);
                    break;
                case TorizoProjectileMotion.EggDropping:
                    RunGoldenTorizoEggFall(projectile, context.Level);
                    break;
                case TorizoProjectileMotion.SuperMissileHeld:
                    HoldSuperMissile(projectile);
                    break;
                case TorizoProjectileMotion.SuperMissileThrown:
                    RunGoldenTorizoSuperMissileFlight(projectile, context.Level);
                    break;
                case TorizoProjectileMotion.EyeBeamFlying:
                    FlyEyeBeam(projectile, context.Level);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(TorizoProjectileMotion)} {motion}.");
            }
        }

        /// <inheritdoc/>
        public AnimationContinuation<TorizoProjectileSegment>? RunCue(EnemyProjectileCueContext context, TorizoProjectileCue cue,
            AnimationPlayback<TorizoProjectileSegment> pastCue) =>
            ApplyTorizoProjectileCue(context.Projectile, context.Samus, cue, pastCue);

        /// <summary>
        /// Carries out one cue of a Torizo projectile's animation, returning where a conditional cue continues the play from
        /// <paramref name="pastCue"/>, the place just past it, or null to run on.
        /// </summary>
        private AnimationContinuation<TorizoProjectileSegment>? ApplyTorizoProjectileCue(EnemyProjectile projectile, SamusState? samus, TorizoProjectileCue cue,
            AnimationPlayback<TorizoProjectileSegment> pastCue)
        {
            switch (cue)
            {
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.StopMoving }:
                    projectile.HoldStill();
                    return null;
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.StartDroolFall }:
                    projectile.StartMotion(TorizoProjectileMotion.DroolFalling);
                    return null;
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.StartFragmentFall }:
                    projectile.StartMotion(TorizoProjectileMotion.StatueFragmentFalling);
                    return null;
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.StartEggCharge }:
                    projectile.StartMotion(TorizoProjectileMotion.EggCharging);
                    return null;
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.ThrowSuperMissile }:
                    projectile.StartMotion(TorizoProjectileMotion.SuperMissileThrown);
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
                    audio.QueueEnemySound(playSound.Sound, playSound.Priority);
                    return null;
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.Delete }:
                    projectile.Delete();
                    return null;
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.Hatch }:
                    return TorizoProjectileAnimations.ContinueWith(
                        projectile.StateOf<GoldenTorizoEggState>().FacingRight ? TorizoProjectileSegment.EggHatchedRight : TorizoProjectileSegment.EggHatchedLeft);
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.ReplayWhileEyeBeamExplosionsDisabled }:
                {
                    TorizoEnemyState state = access.GoldenTorizo ??
                        throw new InvalidOperationException("Golden Torizo's eye beam has no owning Torizo state.");
                    return state.EyeBeamExplosionsEnabled ? null : TorizoProjectileAnimations.ReplayPhase(pastCue);
                }
                case TorizoProjectileCue.GoToSometimes goToSometimes:
                    return projectiles.TakeOneInFourBranch() ? TorizoProjectileAnimations.ContinueWith(goToSometimes.Target) : null;
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
                    projectiles.Scatter(projectile, scatter.Spread);
                    return null;
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.ReturnToOrigin }:
                    projectile.Position = projectile.Position.WithWhole(projectile.StateOf<AnchoredProjectileState>().Anchor);
                    return null;
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.RaiseDust }:
                    projectile.Position = projectile.Position.WithWhole(projectile.Position.Whole + TorizoProjectilePlacementDefinitions.DustRise);
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
                default:
                    throw new InvalidOperationException($"Undefined {nameof(TorizoProjectileCueKind)} {cue}.");
            }
        }

        /// <summary>Keeps the held super missile beside Golden Torizo's hand, clearing it if its owner is gone.</summary>
        private void HoldSuperMissile(EnemyProjectile projectile)
        {
            TorizoEnemyState? state = access.GoldenTorizo;
            if (state is null)
            {
                projectile.Delete();
                return;
            }

            RoomEnemySlot torizo = state.Slot;
            Offset16<RoomPixels> held = TorizoProjectilePlacementDefinitions.HeldSuperMissile;
            projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + (access.FacesRight(torizo) ? held : held.MirroredX()));
        }

        /// <summary>
        /// Runs the orb's drop step. The cue names two drop definitions, Bomb Torizo's Crateria orb and Golden Torizo's
        /// orb outside Crateria; the loaded variant selects between them, the room-scoped equivalent of consulting the area.
        /// </summary>
        private void RequestTorizoChozoOrbDrop(EnemyProjectile projectile)
        {
            if (projectile.Kind is not (
                    RoomEnemyProjectileKind.BombTorizoChozoOrb or
                    RoomEnemyProjectileKind.GoldenTorizoChozoOrb))
            {
                throw new InvalidOperationException(
                    $"Projectile {projectile.Kind} reached the Torizo-orb drop.");
            }

            EnemyDefinitionId definitionId = access.GoldenTorizo is not null
                ? EnemyDefinitionId.GoldenTorizoOrb
                : EnemyDefinitionId.BombTorizoOrb;

            RoomEnemyDefinition definition = room.LoadedDefinitions.ReadEnemyDefinition(definitionId);
            deathsAndDrops.SpawnEnemyDropFromChanceTable(
                projectile.Position.Whole,
                definition.DropChances);
        }

        /// <summary>
        /// Drops the statue fragment: a rising or still-airborne piece accelerates by
        /// <see cref="FirstSlotFallAcceleration"/>.
        /// </summary>
        private void DropStatueFragment(
            EnemyProjectile projectile,
            RoomLevelData level)
        {
            bool collided = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical);
            if (projectile.Velocity.Y.IsNegative || !collided)
            {
                EnemyProjectile firstSlot = projectiles.EnemyProjectiles[0];
                SignedEightEight acceleration = FirstSlotFallAcceleration.Of(
                    firstSlot.IsActive,
                    firstSlot.IsActive && firstSlot.State is BombTorizoStatueFragmentState piece ? piece.Acceleration : null);
                // A piece's speed stops at the runaway speed, so the acceleration never carries it out of range.
                projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + acceleration);
                if (IsRunaway(projectile.Velocity.Y))
                    projectile.Velocity = projectile.Velocity.WithY(TorizoMotionDefinitions.RunawaySpeed);
                return;
            }

            projectile.HoldStill();
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
            TorizoProjectiles,
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
        Offset16<RoomPixels> drool = TorizoProjectilePlacementDefinitions.LowHealthDrool;
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + (_access.FacesRight(torizo) ? drool : drool.MirroredX()));

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
    }

    /// <summary>
    /// The drool a low-health drool starts: the entry of <see cref="TorizoRandomDefinitions.LowHealthDroolAnimations"/>
    /// the sample's <see cref="TorizoRandomDefinitions.LowHealthDroolDelay"/> draws.
    /// </summary>
    internal static TorizoProjectileSegment SelectLowHealthDroolAnimation(int random) =>
        TorizoRandomDefinitions.LowHealthDroolAnimations[TorizoRandomDefinitions.LowHealthDroolDelay.Of(random)];

    /// <summary>Places a swipe explosion at the left-facing hand placement <paramref name="placement"/>, mirroring it for right-facing Torizo.</summary>
    internal void SpawnBombTorizoExplosiveSwipe(RoomEnemySlot torizo, Offset16<RoomPixels> placement)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoExplosiveSwipe,
            TorizoProjectiles,
            graphics: default);
        projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.ExplosiveSwipe));

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
            TorizoProjectiles,
            graphics: default);
        projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.LowHealthExplosion));
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
            TorizoProjectiles,
            graphics: default);
        projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.DeathExplosion));
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
            ChozoOrb,
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
            TorizoProjectiles,
            SpriteGraphics.SpawnedBy(torizo));

        Offset16<RoomPixels> placement = TorizoRandomDefinitions.SonicBoomHigh.IsSetIn(_room.Random.Generate())
            ? TorizoProjectilePlacementDefinitions.HighSonicBoom
            : TorizoProjectilePlacementDefinitions.LowSonicBoom;
        if (_access.FacesRight(torizo))
        {
            projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + placement);
            projectile.Velocity = projectile.Velocity.WithX(TorizoMotionDefinitions.SonicBoomLaunchSpeed);
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.SonicBoomFiredRight));
        }
        else
        {
            projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + placement.MirroredX());
            projectile.Velocity = projectile.Velocity.WithX(-TorizoMotionDefinitions.SonicBoomLaunchSpeed);
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
            ChozoOrb,
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
            Egg,
            SpriteGraphics.SpawnedBy(torizo));

        TorizoEnemyState state = _access.RequireTorizoState(torizo);
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
            SuperMissile,
            SpriteGraphics.SpawnedBy(torizo));
        bool facingRight = _access.FacesRight(torizo);
        Offset16<RoomPixels> caught = TorizoProjectilePlacementDefinitions.CaughtSuperMissile;
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + (facingRight ? caught : caught.MirroredX()));
        projectile.StartAnimation(new TorizoProjectileAnimation(facingRight ? TorizoProjectileSegment.SuperMissileRight : TorizoProjectileSegment.SuperMissileLeft));
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
            TorizoProjectiles,
            SpriteGraphics.SpawnedBy(torizo));
        bool facingRight = _access.FacesRight(torizo);
        projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.EyeBeam));
        Offset16<RoomPixels> muzzle = TorizoProjectilePlacementDefinitions.EyeBeamMuzzle;
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + (facingRight ? muzzle : muzzle.MirroredX()));
        // The beam's launch draws the two velocity jitters every randomized launch draws; its aimed heading then sets its
        // velocity, so the draws only advance the room's random numbers (#1945).
        _room.Random.Generate();
        _room.Random.Generate();

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
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, TorizoProjectiles, graphics: default);
        projectile.StartAnimation(new TorizoProjectileAnimation(foot == HorizontalDirection.Right ? TorizoProjectileSegment.LandingDustRightFoot : TorizoProjectileSegment.LandingDustLeftFoot));
        Offset16<RoomPixels> dust = TorizoProjectilePlacementDefinitions.RightFootDust;
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + (foot == HorizontalDirection.Right ? dust : dust.MirroredX()));
    }

    /// <summary>Moves Bomb Torizo's orb through walls and floor, disabling damage on impact and adding gravity in flight.</summary>
    internal static void DropBombOrb(
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
            projectile.Position = projectile.Position.WithWholeY(FloorBurstY(projectile));
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.ChozoOrbFloorImpact));
            projectile.CanDamageSamus = false;
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + TorizoMotionDefinitions.BombOrbGravity);
        if (IsRunaway(projectile.Velocity.Y))
            projectile.Delete();
    }

    /// <summary>Accelerates the sonic boom horizontally and switches to its non-damaging wall-impact animation.</summary>
    internal static void FlySonicBoom(
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
    /// Drops the drool: Bomb Torizo's six gut-break droplets and its
    /// low-health initial drool alike. The peculiar positive-three clamp is authentic: both the
    /// negative and nonnegative drag branches load three when they cross zero.
    /// </summary>
    internal static void DropDrool(
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
            projectile.Position = projectile.Position.WithWhole(projectile.Position.Whole + TorizoProjectilePlacementDefinitions.DroolSplashLift);
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.DroolFloorImpact));
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + TorizoMotionDefinitions.DroolAndMissileGravity);
        if (IsRunaway(projectile.Velocity.Y))
            projectile.Delete();
    }

    /// <summary>Bounces Golden Torizo's orb off walls and the floor, slowing horizontal motion until floor impact.</summary>
    internal static void BounceGoldenOrb(
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
                projectile.Position = projectile.Position.WithWholeY(FloorBurstY(projectile));
                projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.ChozoOrbFloorImpact));
                projectile.CanDamageSamus = false;
                return;
            }
        }

        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + TorizoMotionDefinitions.GoldenOrbGravity);
    }

    /// <summary>Counts down the egg's hatch timer while bouncing it off terrain with horizontal drag and gravity.</summary>
    internal static void BounceEgg(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        GoldenTorizoEggState egg = projectile.StateOf<GoldenTorizoEggState>();
        if (--egg.HatchTimer < 0)
        {
            // The hatch timer steps the resting animation past its rest.
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
            projectile.StartMotion(TorizoProjectileMotion.EggDropping);
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
            new(CoordinateAxis.Difference(samus.Position.X.Whole, projectile.Position.X.Whole), CoordinateAxis.Difference(samus.Position.Y.Whole, projectile.Position.Y.Whole))).ToMirroredTrigonometryAngle();
        TrigonometryAngle angle = awayFromSamus ? bearing.InSecondHalfTurn() : bearing.InFirstHalfTurn();

        // Preserve the up-zero angle convention: X is sine, Y is
        // negative cosine, the sine a quarter turn back. Starting both reads at sine rotates the shot.
        projectile.Velocity = projectile.Velocity.WithX(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(4 *
            Trigonometry.SignedSine(angle)));
        projectile.Velocity = projectile.Velocity.WithY(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(4 *
            Trigonometry.SignedSine(angle - TrigonometryAngle.QuarterTurn)));
    }

    /// <summary>Selects wall or floor impact artwork when the eye beam strikes terrain.</summary>
    internal static void FlyEyeBeam(
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
            projectile.Position = projectile.Position.WithWholeY(FloorBurstY(projectile));
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.EyeBeamFloorImpact));
        }
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
            TorizoProjectiles,
            graphics: default);

        projectile.StartAnimation(new TorizoProjectileAnimation(definition.Animation));
        projectile.Position = projectile.Position.WithWhole(request.RoomObjectBlock.ToRoomPixels() + definition.Offset);
        projectile.Velocity = projectile.Velocity.WithY(definition.InitialFallSpeed);
        projectile.State = new BombTorizoStatueFragmentState(definition.Acceleration);
        return projectile;
    }

    /// <summary>Whether a projectile has run away: it moves at least <see cref="TorizoMotionDefinitions.RunawaySpeed"/>.</summary>
    private static bool IsRunaway(SignedEightEight speed) => speed >= TorizoMotionDefinitions.RunawaySpeed;

    /// <summary>
    /// Whether a bounced Chozo orb's rebound has settled: an upward speed under half a pixel per frame.
    /// </summary>
    private static bool HasSettled(SignedEightEight speed) =>
        speed.IsNegative && speed >= -TorizoMotionDefinitions.SettledBounceSpeed;

    /// <summary>
    /// Where a projectile that struck a floor bursts: a little below the top of the block it lies in. A block's top is at
    /// most 65520, so the burst stays in the room's coordinates.
    /// </summary>
    private static int FloorBurstY(EnemyProjectile projectile) =>
        RoomGridConversions.BlockStart(projectile.Position.Y.Whole) + TorizoProjectilePlacementDefinitions.FloorBurstDepth;
}
