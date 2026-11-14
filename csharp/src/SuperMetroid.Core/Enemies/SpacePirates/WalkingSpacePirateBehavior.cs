using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The walking space pirate species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class WalkingSpacePirateBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.PirateGoldWalking, EnemyDefinitionId.PirateGreenWalking, EnemyDefinitionId.PirateGreyWalking, EnemyDefinitionId.PirateMagentaWalking, EnemyDefinitionId.PirateRedWalking, EnemyDefinitionId.PirateSilverWalking];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new WalkingSpacePirateEntity(this, slot, common);

    /// <summary>Supplies solid-ground and ledge probes for the patrol AI.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Allocates the shared Pirate/Mother Brain laser projectile.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Runs common Samus contact for the ordinary Pirate families.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Handles Pirate death animation and Gold Ninja's special pickup scatter.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds the patrol collision owner and laser pool.</summary>
    internal WalkingSpacePirateBehavior(EnemyTerrain terrain, EnemyProjectileEngine projectiles, EnemyAudioQueue audio, EnemyCollision collision, EnemyDeathsAndDrops deathsAndDrops)
    {
        _collision = collision;
        _deathsAndDrops = deathsAndDrops;
        _audio = audio;
        _terrain = terrain;
        _projectiles = projectiles;
    }

    /// <summary>
    /// Carries out one cue of the walking Space Pirates' animations. Choosing a movement continues at the walk or laser
    /// animation it selects, as the cue returns that animation; every other cue runs on.
    /// </summary>
    private WalkingSpacePirateSegment? ApplyWalkingSpacePirateCue(RoomEnemySlot slot, WalkingSpacePirateCue cue, SamusState? samus)
    {
        switch (cue)
        {
            case WalkingSpacePirateCue.SetAction setAction:
                // The pirate animation does not jump to the cue's action. It stores that action
                // for main AI to dispatch next frame, then runs on.
                RequireWalkingSpacePirateState(slot).Action = setAction.Action;
                return null;
            case WalkingSpacePirateCue.FireLaserLeft fireLaserLeft:
                SpawnWalkingSpacePirateLaser(slot, direction: HorizontalDirection.Left, fireLaserLeft.LaserHeight);
                return null;
            case WalkingSpacePirateCue.FireLaserRight fireLaserRight:
                SpawnWalkingSpacePirateLaser(slot, direction: HorizontalDirection.Right, fireLaserRight.LaserHeight);
                return null;
            case WalkingSpacePirateCue.Signal { Kind: WalkingSpacePirateCueKind.ChooseMovement }:
                return SelectWalkingSpacePirateMovement(slot, samus);
            default:
                throw new InvalidOperationException($"The walking Space Pirates' animations have no {cue} cue.");
        }
    }

    /// <summary>Fixed-point downward displacement used for the ground probe.</summary>
    private static readonly SignedSixteenSixteen WalkingPirateOnePixelDown = SignedSixteenSixteen.FromWholePixels(1);
    /// <summary>Left patrol edge is probed one pixel farther than the right edge.</summary>
    private const int WalkingPirateLeftLedgeProbePixels = 17;
    /// <summary>Right patrol ledge probe distance in room pixels.</summary>
    private const int WalkingPirateRightLedgeProbePixels = 16;
    /// <summary>The right patrol step, seven thirty-seconds of a pixel.</summary>
    private static readonly SignedSixteenSixteen WalkingPirateRightStepDisplacement = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(56);
    /// <summary>The left patrol step: the right step mirrored, and one 65536th of a pixel further left.</summary>
    private static readonly SignedSixteenSixteen WalkingPirateLeftStepDisplacement =
        -WalkingPirateRightStepDisplacement - SignedSixteenSixteen.FromParts(0, 1);
    /// <summary>Maximum vertical separation for choosing the laser attack.</summary>
    private const int WalkingPirateFiringBandPixels = 16;
    /// <summary>Horizontal collision window used by the pirate's projectile flinch path.</summary>
    private const int WalkingPirateFlinchBoxPixels = 32;
    /// <summary>Horizontal distance from the enemy center to the laser muzzle.</summary>
    private const int WalkingPirateLaserMuzzleXOffset = 24;
    /// <summary>Slow laser's whole-pixel movement per frame.</summary>
    private const int WalkingPirateSlowLaserPixelsPerFrame = 2;
    /// <summary>Fast laser's whole-pixel movement per frame.</summary>
    private const int WalkingPirateFastLaserPixelsPerFrame = 4;

    /// <summary>
    /// Queues <paramref name="sound"/> as a space pirate makes it, after the update's earlier requests: walking, wall and
    /// ninja pirates and Mother Brain's pirate laser all request their sounds through this.
    /// </summary>
    internal void RequestSpacePirateSound(SoundEffectId sound) =>
        _audio.QueueEnemySound(sound, priority: SoundRequestPriority.Normal);

    /// <summary>Recognizes the six walking variants that share patrol AI and laser behavior.</summary>
    internal static bool IsWalkingSpacePirateDefinition(EnemyDefinitionId definition) => definition is
        EnemyDefinitionId.PirateGreyWalking or
        EnemyDefinitionId.PirateGreenWalking or
        EnemyDefinitionId.PirateRedWalking or
        EnemyDefinitionId.PirateGoldWalking or
        EnemyDefinitionId.PirateMagentaWalking or
        EnemyDefinitionId.PirateSilverWalking;

    /// <summary>Initializes the walking Space Pirate.</summary>
    internal static void InitializeWalkingSpacePirate(RoomEnemySlot slot)
    {
        var state = new WalkingSpacePirateEnemyState();
        slot.SetSpeciesState(state);

        // The facing option alone selects the initial facing. The slow-laser option is an
        // independent projectile-flinch option and must not accidentally select right art.
        WalkingSpacePirateSpawnSettings settings = slot.SpawnSettings<WalkingSpacePirateSpawnSettings>();
        WalkingSpacePirateOptions options = settings.Options;
        state.Playback = new(WalkingSpacePirateAnimations.StartingWith(
            (options & WalkingSpacePirateOptions.StartsFacingRight) != WalkingSpacePirateOptions.None
            ? WalkingSpacePirateSegment.WalkingRight
            : WalkingSpacePirateSegment.WalkingLeft), AnimationPlace.StartOfAnimation);
        state.Action = WalkingSpacePirateAction.FollowingAnimation;

        // The posts wrap across the room's coordinate seam, as the game's additions do, so a modded patrol
        // reach can straddle it; the patrol compares positions by their signed difference, which still
        // sees each post on its own side of the pirate.
        state.RightPost = new(CoordinateAxis.Wrap(slot.Position.X.Whole + settings.PatrolReach), 0);
        state.LeftPost = new(CoordinateAxis.Wrap(slot.Position.X.Whole - settings.PatrolReach), 0);
    }

    /// <summary>Runs the walking Space Pirate's main AI.</summary>
    private void RunWalkingSpacePirateMain(
        RoomEnemySlot slot,
        WalkingSpacePirateEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        SamusProjectileSystem? samusProjectiles)
    {
        switch (state.Action)
        {
            case WalkingSpacePirateAction.FollowingAnimation:
                break;

            case WalkingSpacePirateAction.WalkingLeft:
                RunWalkingSpacePirateLeft(slot, state, RequireSamus(), RequireLevel());
                break;

            case WalkingSpacePirateAction.WalkingRight:
                RunWalkingSpacePirateRight(slot, state, RequireSamus(), RequireLevel());
                break;

            default:
                throw new InvalidDataException(
                    $"Walking Space Pirate has no behaviour for action {state.Action?.ToString() ?? "none"}.");
        }

        // The slow-laser option enables the retail flinch detector after normal AI.
        // The detector is intentionally allowed to replace an animation selected just above.
        if (((slot.SpawnSettings<WalkingSpacePirateSpawnSettings>().Options &
                WalkingSpacePirateOptions.SlowLaserAndProjectileFlinch) != WalkingSpacePirateOptions.None) &&
            samus is not null)
            TryTriggerWalkingSpacePirateFlinch(slot, state, samus, samusProjectiles);

        SamusState RequireSamus() => samus ?? throw new InvalidOperationException(
            "Walking Space Pirate movement requires the active Samus position.");
        RoomLevelData RequireLevel() => level ?? throw new InvalidOperationException(
            "Walking Space Pirate movement requires room collision data.");
    }

    /// <summary>Runs the asymmetric left walk.</summary>
    private void RunWalkingSpacePirateLeft(
        RoomEnemySlot slot,
        WalkingSpacePirateEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (WalkingSpacePirateIsVerticallyClose(slot, samus))
        {
            InstallWalkingSpacePirateAttack(slot, state, samus);
            return;
        }

        // The first one-pixel downward move doubles as the grounded test. A move that hits
        // nothing means there was no floor, and the move itself is retained as falling.
        if (!_terrain.MoveEnemyVertically(level, slot, WalkingPirateOnePixelDown))
            return;

        // Left and right are not mirrors in the game. Left probes a point 17 pixels
        // away, then walks by -14337 subpixels; right probes +16 and walks by +14336.
        // Preserve that one-subpixel/one-pixel asymmetry instead of normalizing the speed.
        int originalX = slot.Position.X.Whole;
        slot.Position = slot.Position.WithWrappedWholeX(
            slot.Position.X.Whole - WalkingPirateLeftLedgeProbePixels);
        bool floorAhead = _terrain.MoveEnemyVertically(level, slot, WalkingPirateOnePixelDown);
        slot.Position = slot.Position.WithWholeX(originalX);

        if (floorAhead)
        {
            // The ignored horizontal probe has one mutable result, a displacement pair the following move
            // immediately overwrites, so it leaves no persistent actor-side effect.
            bool hitWall = _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                displacement: WalkingPirateLeftStepDisplacement);
            if (!hitWall &&
                (slot.Position.Whole - state.LeftPost).X >= 0)
            {
                return;
            }
        }

        StartWalkingSpacePirateAnimation(
            slot,
            state,
            WalkingSpacePirateSegment.LookingFacingLeft);
    }

    /// <summary>Runs the right walk.</summary>
    private void RunWalkingSpacePirateRight(
        RoomEnemySlot slot,
        WalkingSpacePirateEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (WalkingSpacePirateIsVerticallyClose(slot, samus))
        {
            InstallWalkingSpacePirateAttack(slot, state, samus);
            return;
        }

        if (!_terrain.MoveEnemyVertically(level, slot, WalkingPirateOnePixelDown))
            return;

        int originalX = slot.Position.X.Whole;
        slot.Position = slot.Position.WithWrappedWholeX(
            slot.Position.X.Whole + WalkingPirateRightLedgeProbePixels);
        bool floorAhead = _terrain.MoveEnemyVertically(level, slot, WalkingPirateOnePixelDown);
        slot.Position = slot.Position.WithWholeX(originalX);

        if (floorAhead &&
            !_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                displacement: WalkingPirateRightStepDisplacement) &&
            (slot.Position.Whole - state.RightPost).X < 0)
        {
            return;
        }

        StartWalkingSpacePirateAnimation(
            slot,
            state,
            WalkingSpacePirateSegment.LookingFacingRight);
    }

    /// <summary>Flinches when the one shot the flinch tests (<see cref="PirateFlinchFromHighestShot"/>) is close on both axes.</summary>
    private static void TryTriggerWalkingSpacePirateFlinch(
        RoomEnemySlot slot,
        WalkingSpacePirateEnemyState state,
        SamusState samus,
        SamusProjectileSystem? projectiles)
    {
        if (projectiles is null)
            return;

        // A walking pirate reacts to any shot by its kind, not its picture.
        SamusProjectile? shot = PirateFlinchFromHighestShot.Shot(projectiles.Slots, projectile => !projectile.Type.IsNone);
        if (shot is null ||
            CoordinateAxis.Distance(shot.Position.X.Whole, slot.Position.X.Whole) >=
                WalkingPirateFlinchBoxPixels ||
            CoordinateAxis.Distance(shot.Position.Y.Whole, slot.Position.Y.Whole) >=
                WalkingPirateFlinchBoxPixels)
        {
            return;
        }

        WalkingSpacePirateSegment list = CoordinateAxis.Difference(slot.Position.X.Whole, samus.Position.X.Whole) < 0
            ? WalkingSpacePirateSegment.FlinchFacingRight
            : WalkingSpacePirateSegment.FlinchFacingLeft;
        StartWalkingSpacePirateAnimation(slot, state, list);
    }

    /// <summary>Runs the movement cue's direct animation selection.</summary>
    private static WalkingSpacePirateSegment SelectWalkingSpacePirateMovement(
        RoomEnemySlot slot,
        SamusState? samus)
    {
        if (samus is null)
        {
            throw new InvalidOperationException(
                "Walking Space Pirate animation selection requires the active Samus position.");
        }

        bool samusIsRightOrAligned =
            CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole) >= 0;
        if (WalkingSpacePirateIsVerticallyClose(slot, samus))
        {
            return samusIsRightOrAligned
                ? WalkingSpacePirateSegment.FireLasersRight
                : WalkingSpacePirateSegment.FireLasersLeft;
        }

        // When Samus is not in the firing band, the Pirate walks toward the opposite side:
        // Samus right selects leftward walking, Samus left selects rightward walking.
        return samusIsRightOrAligned
            ? WalkingSpacePirateSegment.WalkingLeft
            : WalkingSpacePirateSegment.WalkingRight;
    }

    /// <summary>Tests signed vertical separation against the sixteen-pixel firing band.</summary>
    private static bool WalkingSpacePirateIsVerticallyClose(
        RoomEnemySlot slot,
        SamusState samus) =>
        CoordinateAxis.Distance(samus.Position.Y.Whole, slot.Position.Y.Whole) <
            WalkingPirateFiringBandPixels;

    /// <summary>Chooses a left or right laser animation from Samus's current side.</summary>
    private static void InstallWalkingSpacePirateAttack(
        RoomEnemySlot slot,
        WalkingSpacePirateEnemyState state,
        SamusState samus) =>
        StartWalkingSpacePirateAnimation(
            slot,
            state,
            CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole) >= 0
                ? WalkingSpacePirateSegment.FireLasersRight
                : WalkingSpacePirateSegment.FireLasersLeft);

    /// <summary>Arms a newly selected animation for processing on the next tick.</summary>
    private static void StartWalkingSpacePirateAnimation(
        RoomEnemySlot slot,
        WalkingSpacePirateEnemyState state,
        WalkingSpacePirateSegment segment)
    {
        state.Playback = new(WalkingSpacePirateAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        slot.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Runs the firing cues and the laser initializer.</summary>
    private void SpawnWalkingSpacePirateLaser(
        RoomEnemySlot source,
        HorizontalDirection direction,
        int laserHeight)
    {
        _ = SpawnSpacePirateLaser(source, direction == HorizontalDirection.Right, laserHeight);
    }

    /// <summary>
    /// Runs the common laser initializer. Wall and walking Pirates
    /// use different actor animation steps and muzzle offsets, but the allocated projectile is the
    /// same retail definition and therefore belongs in one shared implementation.
    /// </summary>
    internal bool SpawnSpacePirateLaser(
        RoomEnemySlot source,
        bool movingRight,
        int laserHeight)
    {
        EnemyProjectile? projectile = SpawnPirateMotherBrainLaser(
            source,
            new(
                CoordinateAxis.Wrap(
                    source.Position.X.Whole +
                    (movingRight ? WalkingPirateLaserMuzzleXOffset : -WalkingPirateLaserMuzzleXOffset)),
                CoordinateAxis.Wrap(source.Position.Y.Whole - laserHeight)),
            movingRight,
            LaserSpeedOf(source));
        if (projectile is null)
            return false;

        RequestSpacePirateSound(EnemySoundEffectDefinitions.PirateMotherBrainLaserSound);
        return true;
    }

    /// <summary>
    /// The speed of a pirate's laser: the initializer reads the firing pirate's placement, where walking and wall
    /// pirates name the slow laser.
    /// </summary>
    private static PirateLaserSpeed LaserSpeedOf(RoomEnemySlot pirate)
    {
        bool slow = pirate.Spawn.Population.SpawnSettings switch
        {
            WalkingSpacePirateSpawnSettings walking =>
                (walking.Options & WalkingSpacePirateOptions.SlowLaserAndProjectileFlinch) != WalkingSpacePirateOptions.None,
            WallSpacePirateSpawnSettings wall =>
                (wall.Options & WallSpacePirateOptions.SlowJumpAndLaser) != WallSpacePirateOptions.None,
            _ => throw new InvalidOperationException($"{pirate.DefinitionId} is not a pirate that fires lasers."),
        };
        return slow ? PirateLaserSpeed.Slow : PirateLaserSpeed.Fast;
    }

    /// <summary>
    /// Runs the common laser initializer after the caller has chosen
    /// its muzzle coordinate. Walking Pirates, wall Pirates, and Mother Brain enter through
    /// different actor animation steps, but the game deliberately funnels all of them through
    /// this definition so animation, speed flags, collision damage, and disposal agree.
    /// </summary>
    /// <param name="source">The enemy firing the laser, whose damage the laser takes.</param>
    /// <param name="muzzle">Where the laser starts.</param>
    /// <param name="movingRight">Whether the laser travels right.</param>
    /// <param name="speed">The laser's speed, which the firing enemy's placement names.</param>
    internal EnemyProjectile? SpawnPirateMotherBrainLaser(
        RoomEnemySlot source,
        Position16<RoomPixels> muzzle,
        bool movingRight,
        PirateLaserSpeed speed)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return null;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.PirateMotherBrainLaser,
            Laser,
            graphics: default);

        projectile.Position = FixedPosition<RoomPixels>.FromWhole(muzzle);
        projectile.StartAnimation(new SpacePirateProjectileAnimation(movingRight
            ? SpacePirateProjectileSegment.LaserRight
            : SpacePirateProjectileSegment.LaserLeft));

        // Although the definition starts the laser flying left, the initializer holds it still: the three two-frame
        // muzzle frames are stationary until the animation installs the flight and runs it at once.
        projectile.FrameBehavior = EnemyProjectileFrameBehavior.Stationary;
        projectile.State = new PirateLaserState(speed == PirateLaserSpeed.Slow);

        // The initializer replaces the projectile definition's properties with the enemy's damage and the
        // laser's own property. The laser does not take the Pirate's pictures or colours: it draws from the room's
        // shared sheet in its standard colours.
        projectile.Damage = source.Definition.Damage;
        projectile.CanDamageSamus = true;
        projectile.PersistsOnSamusContact = false;
        projectile.BlocksSamusProjectiles = false;
        return projectile;
    }

    /// <summary>Flies a laser one step in <paramref name="direction"/>, deleting it once it leaves the 256x256 camera square.</summary>
    private static void FlyPirateLaser(
        EnemyProjectile projectile,
        Position16<RoomPixels> camera,
        HorizontalDirection direction)
    {
        int pixels = projectile.StateOf<PirateLaserState>().Slow
            ? WalkingPirateSlowLaserPixelsPerFrame
            : WalkingPirateFastLaserPixelsPerFrame;
        projectile.Position = projectile.Position.WithWholeX(direction == HorizontalDirection.Right
            ? CoordinateAxis.Wrap(projectile.Position.X.Whole + pixels)
            : CoordinateAxis.Wrap(projectile.Position.X.Whole - pixels));

        int right = CoordinateAxis.Wrap(camera.X + 256);
        int bottom = CoordinateAxis.Wrap(camera.Y + 256);
        bool offScreen = CoordinateAxis.Difference(projectile.Position.X.Whole, camera.X) < 0 ||
            CoordinateAxis.Difference(projectile.Position.X.Whole, right) >= 0 ||
            CoordinateAxis.Difference(projectile.Position.Y.Whole, camera.Y) < 0 ||
            CoordinateAxis.Difference(projectile.Position.Y.Whole, bottom) >= 0;
        if (offScreen)
            projectile.Delete();
    }

    /// <summary>The direction a laser mover flies.</summary>
    private static HorizontalDirection LaserFlightDirection(EnemyProjectileFrameBehavior movement) =>
        movement == EnemyProjectileFrameBehavior.LaserFlightRight ? HorizontalDirection.Right
        : movement == EnemyProjectileFrameBehavior.LaserFlightLeft ? HorizontalDirection.Left
        : throw new InvalidOperationException($"{movement} is not a laser flight.");

    /// <summary>Requires the enemy to have been initialized as a walking pirate.</summary>
    internal static WalkingSpacePirateEnemyState RequireWalkingSpacePirateState(
        RoomEnemySlot slot) =>
        slot.SpeciesState<WalkingSpacePirateEnemyState>() ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized walking Space Pirate state.");

    /// <summary>
    /// The Pirates' and Mother Brain's laser (#1760): its spawner starts its animation, which installs its flight left or
    /// right.
    /// </summary>
    internal static readonly EnemyProjectileBehavior Laser = new LaserBehavior();

    /// <summary>A laser: it flies left or right until it leaves the camera, as its animation's cues install.</summary>
    private sealed class LaserBehavior : EnemyProjectileBehavior, IEnemyProjectileCues<SpacePirateProjectileCue>
    {
        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileFrameBehaviorContext context)
        {
            EnemyProjectileFrameBehavior motion = context.Projectile.FrameBehavior;
            if (motion == EnemyProjectileFrameBehavior.LaserFlightLeft)
            {
                FlyPirateLaser(context.Projectile, context.Camera, HorizontalDirection.Left);
            }
            else if (motion == EnemyProjectileFrameBehavior.LaserFlightRight)
            {
                FlyPirateLaser(context.Projectile, context.Camera, HorizontalDirection.Right);
            }
            else
            {
                base.Move(context);
            }
        }

        /// <inheritdoc/>
        public void RunCue(EnemyProjectileCueContext context, SpacePirateProjectileCue cue) => ApplySpacePirateProjectileCue(context, cue);
    }

    /// <summary>
    /// Carries out a cue of the Pirates' laser and claw animations: it installs the movement the cue names, and the
    /// start-moving-now cue runs a laser's installed flight at once.
    /// </summary>
    internal static void ApplySpacePirateProjectileCue(EnemyProjectileCueContext context, SpacePirateProjectileCue cue)
    {
        context.Projectile.FrameBehavior = cue.Movement;
        // The start-moving cue leaves the segment where it was, so the common animation dispatches the installed
        // movement once at once: the laser moves on the update it starts and may leave the screen then.
        if (cue.Kind == SpacePirateProjectileCueKind.StartMovingNow)
            FlyPirateLaser(context.Projectile, context.Camera, LaserFlightDirection(cue.Movement));
    }

    /// <summary>Runs the walking Space Pirate's main AI.</summary>
    internal void RunWalkingSpacePirateMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunWalkingSpacePirateMain(
            slot,
            RequireWalkingSpacePirateState(slot),
            frame.Samus,
            frame.Level,
            frame.SamusProjectiles);
    }

    /// <summary>Advances the walking Space Pirate's typed animation.</summary>
    internal void AdvanceWalkingSpacePirateTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext context)
    {
        if (!slot.AnimationClock.Expire())
            return;
        var state = RequireWalkingSpacePirateState(slot);
        AnimationPlayback<WalkingSpacePirateSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(slot, WalkingSpacePirateAnimations.Segments, ref playback,
            cue => ApplyWalkingSpacePirateCue(slot, cue, context.Samus) is { } continuesAt
                ? new AnimationContinuation<WalkingSpacePirateSegment>(WalkingSpacePirateAnimations.StartingWith(continuesAt), AnimationPlace.StartOfAnimation)
                : null);
        state.Playback = playback;
    }

    /// <summary>Runs the extended-hitbox shot reaction, including collision marking before rejection or reflection.</summary>
    internal bool ResolveSpacePirateHitboxShot(in EnemyShotContext c)
    {
        if (!c.UsesFrameHitboxes)
            return false;
        PirateHitboxShotActions.PirateHitboxShotAction pirateAction = PirateHitboxShotActions.SelectPirateHitboxShotAction(c.Enemy, c.Projectile, c.HitboxShot);
        if (pirateAction != PirateHitboxShotActions.PirateHitboxShotAction.Normal)
        {
            // The extended walker marks a non-plasma projectile as having entered an extended hitbox before the
            // selected reaction runs. Normal hits get the equivalent lifecycle transition from TryStartEnemyImpact;
            // rejected and reflected hits return before that shared path. The plasma-blocking enemy property also
            // forces the mark.
            c.Projectiles.EnemyImpacts.ApplyFrameHitboxCollisionPrelude(c.Projectile, c.MarksCollision);
        }
        if (pirateAction == PirateHitboxShotActions.PirateHitboxShotAction.Ignore)
        {
            // The Ninja body reaction deliberately returns for a just-fired Super Missile that has no link yet.
            // The frame-hitbox collision walk nevertheless exits after running that reaction.
            return true;
        }
        if (pirateAction == PirateHitboxShotActions.PirateHitboxShotAction.Reflect)
        {
            c.Enemy.InvincibilityTimer = 10;
            // A shot aimed straight left or right reflects up and away; every other aim reflects down.
            SamusProjectileDirection reflected = c.Projectile.Direction switch
            {
                SamusProjectileDirection.Left => SamusProjectileDirection.UpRight,
                SamusProjectileDirection.Right => SamusProjectileDirection.UpLeft,
                SamusProjectileDirection.UpFacingRight or SamusProjectileDirection.UpRight or SamusProjectileDirection.DownRight
                    or SamusProjectileDirection.DownFacingRight or SamusProjectileDirection.DownFacingLeft
                    or SamusProjectileDirection.DownLeft or SamusProjectileDirection.UpLeft
                    or SamusProjectileDirection.UpFacingLeft => SamusProjectileDirection.DownFacingLeft,
                _ => throw new InvalidDataException($"Projectile aim {c.Projectile.Direction} decoded to an undefined direction."),
            };
            c.Projectile.Direction = reflected;
            c.Projectile.IsSpent = false;
            c.Projectiles.EnemyImpacts.ReflectFromEnemy(c.Samus ?? throw new InvalidOperationException("A reflected beam reinherits Samus's movement and requires her."),
                c.Projectile);
            RequestSpacePirateSound(EnemySoundEffectDefinitions.WalkingPirateReflectSound);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the ordinary Pirates' bombs through the common bomb damage, showing the hit burst only where the reaction the
    /// bomb reaches shows it (<see cref="PirateHitboxShotActions.ShowsHitBurst"/>).
    /// </summary>
    internal static EnemyBombReaction SpacePirateBombReaction { get; } = new()
    {
        ShowsHitBurst = c => PirateHitboxShotActions.ShowsHitBurst(c.Enemy, c.SelectedShot),
    };

    /// <summary>Uses shared Pirate shot logic, including hitbox quake, big death explosion, and Gold Ninja drops.</summary>
    internal EnemyShotReaction SpacePirateShotReaction => field ??= new()
    {
        AfterSuperMissileQuake = ResolveSpacePirateHitboxShot,
        ShowsHitBurst = c => PirateHitboxShotActions.ShowsHitBurst(c.Enemy, c.HitboxShot),
        ReplacesGenericDeath = c =>
        {
            if (c.HitboxShot == EnemyReaction.NormalEnemyShot)
                return false;
            // The Gold Ninja's death records its position as the drop origin before the enemy's death clears the
            // slot; it then spawns the death explosion before the metal ninja Pirate's item drop spawns the drops.
            bool spawnsGoldNinjaDrops = c.Enemy.DefinitionId == EnemyDefinitionId.PirateGoldNinja;
            Position16<RoomPixels> dropOrigin = c.Enemy.Position.Whole;
            _deathsAndDrops.StartGenericEnemyDeath(
                c.Enemy,
                EnemyDeathsAndDrops.SelectNormalShotDeathAnimation(c.Enemy, c.ProjectileType, rule: ShotDeathAnimationRule.PirateBigExplosion));
            if (spawnsGoldNinjaDrops)
                _deathsAndDrops.SpawnEnemyDropScatterAround(EnemyDefinitionId.PirateGoldNinja, count: 5, dropOrigin);
            return true;
        },
    };

    /// <summary>Skips frozen contact and rejects unsupported extended hitbox reactions before normal touch damage.</summary>
    internal void ReactToSpacePirateTouch(EnemyTouchContext context)
    {
        // Every Space Pirate frame's hitboxes use the shared Pirate touch
        // reaction in every retail hitbox. Keep the reaction check explicit: silently
        // treating a later family-specific attack box as ordinary body contact would
        // recreate the exact radius-flattening bug this path is intended to remove.
        if (context.UsesFrameHitboxes && context.HitboxTouch != SpacePirateReactions.Touch)
        {
            throw new InvalidDataException(
                $"Space Pirate has no response to touch reaction {context.HitboxTouch?.ToString() ?? "(unnamed)"}.");
        }
        // The shared Pirate touch returns before common touch AI while frozen. The overlap was
        // still found, but neither Samus nor the Pirate receives contact damage.
        if (context.Slot.FrozenTimer != 0)
            return;
        _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus);
    }
}
