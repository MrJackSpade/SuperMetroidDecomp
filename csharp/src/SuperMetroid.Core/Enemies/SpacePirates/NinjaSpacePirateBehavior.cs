using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The ninja space pirate species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class NinjaSpacePirateBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.PirateGoldNinja, EnemyDefinitionId.PirateGreenNinja, EnemyDefinitionId.PirateGreyNinja, EnemyDefinitionId.PirateMagentaNinja, EnemyDefinitionId.PirateRedNinja, EnemyDefinitionId.PirateSilverNinja];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new NinjaSpacePirateEntity(this, slot, common);

    /// <summary>Provides the shared RNG used by divekick selection.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves floor contact during divekick descent.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Allocates thrown claw projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Spawns paired landing dust sprites.</summary>
    private readonly RoomSpriteObjects _spriteObjects;

    /// <summary>Receives sound requests from shared Space Pirate program steps.</summary>
    private readonly WalkingSpacePirateBehavior _walkingSpacePirateBehavior;

    /// <summary>Captures the room, collision, projectile, sprite, and sound owners for ninja AI.</summary>
    internal NinjaSpacePirateBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyProjectileEngine projectiles, RoomSpriteObjects spriteObjects, WalkingSpacePirateBehavior walkingSpacePirateBehavior)
    {
        _walkingSpacePirateBehavior = walkingSpacePirateBehavior;
        _room = room;
        _terrain = terrain;
        _projectiles = projectiles;
        _spriteObjects = spriteObjects;
    }

    /// <summary>8.8 speed assigned at the start of a divekick jump and descent.</summary>
    private const int NinjaPirateInitialDiveSpeed = 1536;
    /// <summary>
    /// The widest half-span between a ninja's posts, in pixels: the spin's distance then stays below half the counting
    /// range as it accumulates. Every retail ninja's posts lie 0 or 192 pixels apart (#1620).
    /// </summary>
    private const int LongestPostHalfSpan = 127;
    /// <summary>Maximum horizontal distance at which an idle ninja activates.</summary>
    private const int NinjaPirateActivationDistance = 128;
    /// <summary>Strict distance on both axes for a reaction to the highest occupied projectile.</summary>
    private const int NinjaPirateFlinchDistance = 32;
    /// <summary>Strict midpoint distance that permits a spin jump or divekick choice.</summary>
    private const int NinjaPirateMidpointTriggerDistance = 32;
    /// <summary>Strict X and Y distance for a standing kick.</summary>
    private const int NinjaPirateKickDistance = 40;
    /// <summary>Whole-pixel horizontal step during divekick descent.</summary>
    private const int NinjaPirateDiveHorizontalPixels = 5;
    /// <summary>Whole-pixel step while returning to a patrol post after a dive.</summary>
    private const int NinjaPirateReturnWalkPixels = 2;
    /// <summary>Whole-pixel vertical step used by each spin-jump update.</summary>
    private const int NinjaPirateSpinVerticalPixels = 2;

    /// <summary>Identifies all six ninja Space Pirate enemy definitions that share this AI.</summary>
    internal static bool IsNinjaSpacePirateDefinition(EnemyDefinitionId definition) => definition is
        EnemyDefinitionId.PirateGreyNinja or
        EnemyDefinitionId.PirateGreenNinja or
        EnemyDefinitionId.PirateRedNinja or
        EnemyDefinitionId.PirateGoldNinja or
        EnemyDefinitionId.PirateMagentaNinja or
        EnemyDefinitionId.PirateSilverNinja;

    /// <summary>Initializes the Ninja Space Pirate.</summary>
    internal static void InitializeNinjaSpacePirate(RoomEnemySlot slot)
    {
        var state = new NinjaSpacePirateEnemyState();
        slot.SetSpeciesState(state);

        NinjaSpacePirateSpawnSettings settings = slot.SpawnSettings<NinjaSpacePirateSpawnSettings>();
        bool startsAtLeft = (settings.Options &
            NinjaSpacePirateOptions.StartsAtLeftPostFacingRight) != NinjaSpacePirateOptions.None;
        NinjaSpacePirateProgram initial = startsAtLeft
            ? NinjaSpacePirateProgram.InitialFacingRight
            : NinjaSpacePirateProgram.InitialFacingLeft;
        state.Playback = new(NinjaSpacePirateAnimations.StartingWith(initial), AnimationPlace.StartOfAnimation);
        state.ActiveProgram = initial;

        // The post distance describes the requested distance between posts. The setup
        // first establishes those requested posts around the population X coordinate, then
        // deliberately replaces them with a symmetric pair derived from its acceleration
        // sum. That adjustment is why the final span can differ slightly from the post distance.
        int requestedLeft = startsAtLeft
            ? slot.Position.X.Whole
            : CoordinateAxis.Wrap(slot.Position.X.Whole - settings.PostDistance);
        int requestedRight = startsAtLeft
            ? CoordinateAxis.Wrap(slot.Position.X.Whole + settings.PostDistance)
            : slot.Position.X.Whole;
        int requestedHalfSpan = CoordinateAxis.Wrap(requestedRight - requestedLeft);
        requestedHalfSpan >>= 1;
        state.PostsMidpoint = new(CoordinateAxis.Wrap(requestedLeft + requestedHalfSpan), 0);

        // The spin's speed grows until the distance it covers reaches the half-span; it runs at least once, so posts no
        // distance apart still spin.
        int distanceInSubpixels = EightEight.FromWhole(Ensure.BetweenInclusive(0, LongestPostHalfSpan, requestedHalfSpan));
        int speed = 0;
        int accumulatedDistance = 0;
        do
        {
            speed += 32;
            accumulatedDistance += speed;
        }
        while (accumulatedDistance < distanceInSubpixels);

        state.CalculatedSpinJumpSpeed = speed;
        int adjustedHalfSpan = UnsignedEightEight.Whole(accumulatedDistance);
        var halfSpan = new Offset16<RoomPixels>(adjustedHalfSpan, 0);
        state.RightPost = state.PostsMidpoint + halfSpan;
        state.LeftPost = state.PostsMidpoint - halfSpan;
        slot.Position = slot.Position.WithX(new((startsAtLeft ? state.LeftPost : state.RightPost).X, 0));
        state.Function = NinjaSpacePirateFunction.NoOperation;
        state.SpawnPosition = slot.Position.Whole;
    }

    /// <summary>Runs the Ninja Space Pirate's current behaviour.</summary>
    private void RunNinjaSpacePirateMain(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        SamusProjectileSystem? samusProjectiles)
    {
        switch (state.Function)
        {
            case NinjaSpacePirateFunction.NoOperation:
                return;
            case NinjaSpacePirateFunction.Initial:
                RunNinjaPirateInitial(slot, state, RequireSamus(), samusProjectiles);
                return;
            case NinjaSpacePirateFunction.Active:
                RunNinjaPirateActive(slot, state, RequireSamus(), samusProjectiles);
                return;
            case NinjaSpacePirateFunction.SpinJumpLeftRising:
                StepNinjaPirateSpinJump(slot, state, direction: HorizontalDirection.Left, vertical: VerticalDirection.Up);
                return;
            case NinjaSpacePirateFunction.SpinJumpLeftFalling:
                StepNinjaPirateSpinJump(slot, state, direction: HorizontalDirection.Left, vertical: VerticalDirection.Down);
                return;
            case NinjaSpacePirateFunction.SpinJumpRightRising:
                StepNinjaPirateSpinJump(slot, state, direction: HorizontalDirection.Right, vertical: VerticalDirection.Up);
                return;
            case NinjaSpacePirateFunction.SpinJumpRightFalling:
                StepNinjaPirateSpinJump(slot, state, direction: HorizontalDirection.Right, vertical: VerticalDirection.Down);
                return;
            case NinjaSpacePirateFunction.ReadyToDivekick:
                RunNinjaPirateReadyToDivekick(
                    slot,
                    state,
                    RequireSamus(),
                    samusProjectiles);
                return;
            case NinjaSpacePirateFunction.DivekickLeftJump:
                StepNinjaPirateDiveJump(slot, state, direction: HorizontalDirection.Left);
                return;
            case NinjaSpacePirateFunction.DivekickRightJump:
                StepNinjaPirateDiveJump(slot, state, direction: HorizontalDirection.Right);
                return;
            case NinjaSpacePirateFunction.DivekickLeftDive:
                StepNinjaPirateDive(slot, state, level, direction: HorizontalDirection.Left);
                return;
            case NinjaSpacePirateFunction.DivekickRightDive:
                StepNinjaPirateDive(slot, state, level, direction: HorizontalDirection.Right);
                return;
            case NinjaSpacePirateFunction.DivekickLeftWalkToPost:
                StepNinjaPirateWalkToPost(slot, state, direction: HorizontalDirection.Left);
                return;
            case NinjaSpacePirateFunction.DivekickRightWalkToPost:
                StepNinjaPirateWalkToPost(slot, state, direction: HorizontalDirection.Right);
                return;
            default:
                throw new InvalidDataException(
                    $"Ninja Space Pirate function {state.Function?.ToString() ?? "none"} is not translated.");
        }

        SamusState RequireSamus() => samus ?? throw new InvalidOperationException(
            "Ninja Space Pirate decision AI requires the active Samus actor.");
    }

    /// <summary>Waits for Samus to approach, then selects the active animation facing her.</summary>
    private static void RunNinjaPirateInitial(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state,
        SamusState samus,
        SamusProjectileSystem? samusProjectiles)
    {
        if (CoordinateAxis.Distance(slot.Position.X.Whole, samus.Position.X.Whole) >=
            NinjaPirateActivationDistance)
        {
            _ = TryNinjaPirateProjectileFlinch(slot, state, samus, samusProjectiles);
            return;
        }

        NinjaSpacePirateProgram active = CoordinateAxis.Difference(slot.Position.X.Whole, samus.Position.X.Whole) < 0
            ? NinjaSpacePirateProgram.ActiveFacingRight
            : NinjaSpacePirateProgram.ActiveFacingLeft;
        state.ActiveProgram = active;
        InstallNinjaPirateInstruction(slot, state, active);
    }

    /// <summary>Checks flinch, standing kick, spin jump, then periodic claw attack in priority order.</summary>
    private static void RunNinjaPirateActive(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state,
        SamusState samus,
        SamusProjectileSystem? samusProjectiles)
    {
        if (TryNinjaPirateProjectileFlinch(slot, state, samus, samusProjectiles) ||
            TryNinjaPirateStandingKick(slot, state, samus) ||
            TryNinjaPirateSpinJump(slot, state, samus))
        {
            return;
        }

        TryNinjaPirateClawAttack(slot, state, samus);
    }

    /// <summary>Flinches when the one shot the flinch tests (<see cref="PirateFlinchFromHighestShot"/>) is close on both axes.</summary>
    private static bool TryNinjaPirateProjectileFlinch(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state,
        SamusState samus,
        SamusProjectileSystem? samusProjectiles)
    {
        if (samusProjectiles is null)
            return false;

        // A ninja pirate reacts to a beam or missile by its kind, not its picture, so the invisible Murder Beam counts.
        SamusProjectile? projectile = PirateFlinchFromHighestShot.Shot(
            samusProjectiles.Slots, shot => shot.HasEnemyCollisionPayload);
        if (projectile is null ||
            CoordinateAxis.Distance(projectile.Position.X.Whole, slot.Position.X.Whole) >=
                NinjaPirateFlinchDistance ||
            CoordinateAxis.Distance(projectile.Position.Y.Whole, slot.Position.Y.Whole) >=
                NinjaPirateFlinchDistance)
        {
            return false;
        }

        InstallNinjaPirateInstruction(
            slot,
            state,
            CoordinateAxis.Difference(slot.Position.X.Whole, samus.Position.X.Whole) < 0
                ? NinjaSpacePirateProgram.FlinchFacingRight
                : NinjaSpacePirateProgram.FlinchFacingLeft);
        return true;
    }

    /// <summary>Starts a facing kick when Samus is within the strict contact range.</summary>
    private static bool TryNinjaPirateStandingKick(RoomEnemySlot slot, NinjaSpacePirateEnemyState state, SamusState samus)
    {
        if (CoordinateAxis.Distance(samus.Position.X.Whole, slot.Position.X.Whole) >=
                NinjaPirateKickDistance ||
            CoordinateAxis.Distance(samus.Position.Y.Whole, slot.Position.Y.Whole) >=
                NinjaPirateKickDistance)
        {
            return false;
        }

        InstallNinjaPirateInstruction(
            slot,
            state,
            CoordinateAxis.Difference(slot.Position.X.Whole, samus.Position.X.Whole) < 0
                ? NinjaSpacePirateProgram.KickFacingRight
                : NinjaSpacePirateProgram.KickFacingLeft);
        return true;
    }

    /// <summary>Starts a jump toward the opposite post when Samus nears the patrol midpoint.</summary>
    private static bool TryNinjaPirateSpinJump(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state,
        SamusState samus)
    {
        if (Math.Abs((state.PostsMidpoint - samus.Position.Whole).X) >=
            NinjaPirateMidpointTriggerDistance)
        {
            return false;
        }

        InstallNinjaPirateInstruction(
            slot,
            state,
            slot.Position.X.Whole == state.LeftPost.X
                ? NinjaSpacePirateProgram.SpinJumpRight
                : NinjaSpacePirateProgram.SpinJumpLeft);
        return true;
    }

    /// <summary>Starts a claw throw on the periodic frame when Samus is on the allowed side.</summary>
    private static void TryNinjaPirateClawAttack(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state,
        SamusState samus)
    {
        if (!NinjaSpacePirateCadenceDefinitions.ClawAttackEverySixtyFourthUpdate.IsDue(slot.FrameCounter))
            return;

        if (slot.Position.X.Whole == state.LeftPost.X)
        {
            if (CoordinateAxis.Difference(slot.Position.X.Whole, samus.Position.X.Whole) < 0)
                return;
            InstallNinjaPirateInstruction(
                slot,
                state,
                NinjaSpacePirateProgram.ClawAttackLeft);
            return;
        }

        if (CoordinateAxis.Difference(slot.Position.X.Whole, samus.Position.X.Whole) >= 0)
            return;
        InstallNinjaPirateInstruction(
            slot,
            state,
            NinjaSpacePirateProgram.ClawAttackRight);
    }

    /// <summary>Retries RNG until a nonzero dive choice after higher-priority reactions fail.</summary>
    private void RunNinjaPirateReadyToDivekick(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state,
        SamusState samus,
        SamusProjectileSystem? samusProjectiles)
    {
        if (TryNinjaPirateProjectileFlinch(slot, state, samus, samusProjectiles) ||
            TryNinjaPirateStandingKick(slot, state, samus))
        {
            return;
        }

        if (Math.Abs((state.PostsMidpoint - samus.Position.Whole).X) >=
            NinjaPirateMidpointTriggerDistance)
        {
            return;
        }

        // The table contains three identical entries per side. The random retry is retained
        // because advancing the RNG until its low two bits are nonzero is observable
        // even though all three accepted values select the same list.
        int choice;
        do
            choice = _room.Random.Next(SpacePirateDecisions.DivekickChoice);
        while (choice == 0);

        InstallNinjaPirateInstruction(
            slot,
            state,
            slot.Position.X.Whole == state.LeftPost.X
                ? NinjaSpacePirateProgram.DivekickRightJump
                : NinjaSpacePirateProgram.DivekickLeftJump);
    }

    /// <summary>Moves through a rising and falling arc between patrol posts, then emits landing dust.</summary>
    private void StepNinjaPirateSpinJump(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state,
        HorizontalDirection direction,
        VerticalDirection vertical)
    {
        int xPixels = state.Speed.UnsignedWholePixels;
        slot.Position = slot.Position.WithWrappedWholeX(slot.Position.X.Whole + (direction == HorizontalDirection.Right ? xPixels : -xPixels));
        slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole +
            (vertical == VerticalDirection.Up ? -NinjaPirateSpinVerticalPixels : NinjaPirateSpinVerticalPixels));

        if (vertical == VerticalDirection.Up)
        {
            state.ChangeSpeed(32);
            bool crossedMidpoint = direction == HorizontalDirection.Right
                ? (slot.Position.Whole - state.PostsMidpoint).X >= 0
                : (slot.Position.Whole - state.PostsMidpoint).X < 0;
            if (crossedMidpoint)
            {
                state.Function = direction == HorizontalDirection.Right
                    ? NinjaSpacePirateFunction.SpinJumpRightFalling
                    : NinjaSpacePirateFunction.SpinJumpLeftFalling;
            }
            return;
        }

        state.ChangeSpeed(-32);
        if (state.Speed != new SignedEightEight(0))
            return;

        state.Function = NinjaSpacePirateFunction.NoOperation;
        slot.Position = slot.Position.WithWholeX((direction == HorizontalDirection.Right ? state.RightPost : state.LeftPost).X);
        InstallNinjaPirateInstruction(
            slot,
            state,
            direction == HorizontalDirection.Right
                ? NinjaSpacePirateProgram.LandFacingRight
                : NinjaSpacePirateProgram.LandFacingLeft);
        SpawnNinjaPirateLandingDust(slot);
    }

    /// <summary>
    /// Sets the divekick's initial rising speed. The game also stores a target midway between a post and the
    /// patrol centre, which nothing reads; it is not kept.
    /// </summary>
    private static void InitializeNinjaPirateDive(NinjaSpacePirateEnemyState state) =>
        state.Speed = new SignedEightEight(NinjaPirateInitialDiveSpeed);

    /// <summary>Rises until signed speed turns negative, then switches to the descent list.</summary>
    private static void StepNinjaPirateDiveJump(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state,
        HorizontalDirection direction)
    {
        slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole - state.Speed.UnsignedWholePixels);
        state.ChangeSpeed(-64);
        if (!state.Speed.IsNegative)
            return;

        state.Function = direction == HorizontalDirection.Right
            ? NinjaSpacePirateFunction.DivekickRightDive
            : NinjaSpacePirateFunction.DivekickLeftDive;
        InstallNinjaPirateInstruction(
            slot,
            state,
            direction == HorizontalDirection.Right
                ? NinjaSpacePirateProgram.DivekickRightDive
                : NinjaSpacePirateProgram.DivekickLeftDive);
        state.Speed = new SignedEightEight(NinjaPirateInitialDiveSpeed);
    }

    /// <summary>
    /// Moves horizontally and falls by the divekick's step (<see cref="NinjaPirateDiveFallStep"/>) until the floor or the
    /// speed ends the dive.
    /// </summary>
    internal void StepNinjaPirateDive(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state,
        RoomLevelData? level,
        HorizontalDirection direction)
    {
        if (level is null)
        {
            throw new InvalidOperationException(
                "Ninja Space Pirate divekick movement requires room collision data.");
        }

        slot.Position = slot.Position.WithWrappedWholeX(slot.Position.X.Whole +
            (direction == HorizontalDirection.Right ? NinjaPirateDiveHorizontalPixels : -NinjaPirateDiveHorizontalPixels));

        bool landed = _terrain.MoveEnemyVertically(level, slot, NinjaPirateDiveFallStep.Of(state.Speed));
        if (!landed)
        {
            state.ChangeSpeed(-64);
            landed = state.Speed.IsNegative || state.Speed.UnsignedWholePixels == 0;
        }
        if (!landed)
            return;

        state.Function = direction == HorizontalDirection.Right
            ? NinjaSpacePirateFunction.DivekickRightWalkToPost
            : NinjaSpacePirateFunction.DivekickLeftWalkToPost;
        InstallNinjaPirateInstruction(
            slot,
            state,
            direction == HorizontalDirection.Right
                ? NinjaSpacePirateProgram.WalkToRightPost
                : NinjaSpacePirateProgram.WalkToLeftPost);
        // Landing at either post stores the spawn row's whole Y only; the dive's Y fraction stays.
        slot.Position = slot.Position.WithWholeY(state.SpawnPosition.Y);
        SpawnNinjaPirateLandingDust(slot);
    }

    /// <summary>Walks back to the selected post and restores its landing program.</summary>
    private static void StepNinjaPirateWalkToPost(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state,
        HorizontalDirection direction)
    {
        slot.Position = slot.Position.WithWrappedWholeX(slot.Position.X.Whole +
            (direction == HorizontalDirection.Right ? NinjaPirateReturnWalkPixels : -NinjaPirateReturnWalkPixels));
        bool reachedPost = direction == HorizontalDirection.Right
            ? (slot.Position.Whole - state.RightPost).X >= 0
            : (slot.Position.Whole - state.LeftPost).X < 0;
        if (!reachedPost)
            return;

        slot.Position = slot.Position.WithWholeX((direction == HorizontalDirection.Right ? state.RightPost : state.LeftPost).X);
        InstallNinjaPirateInstruction(
            slot,
            state,
            direction == HorizontalDirection.Right
                ? NinjaSpacePirateProgram.LandFacingRight
                : NinjaSpacePirateProgram.LandFacingLeft);
        state.Function = NinjaSpacePirateFunction.NoOperation;
    }

    /// <summary>Spawns a dust cloud on each side of the landing position.</summary>
    private void SpawnNinjaPirateLandingDust(RoomEnemySlot slot)
    {
        SpriteGraphics graphics = SpriteGraphics.SharedStandard;
        _spriteObjects.SpawnRoomSpriteObject(
            new(CoordinateAxis.Wrap(slot.Position.X.Whole - 8), CoordinateAxis.Wrap(slot.Position.Y.Whole + 28)),
            RoomSpriteObjectKind.NinjaPirateLandingDust,
            graphics);
        _spriteObjects.SpawnRoomSpriteObject(
            new(CoordinateAxis.Wrap(slot.Position.X.Whole + 8), CoordinateAxis.Wrap(slot.Position.Y.Whole + 28)),
            RoomSpriteObjectKind.NinjaPirateLandingDust,
            graphics);
    }

    /// <summary>A thrown claw's launch speed, eight pixels per update, in 1/256 pixel per update.</summary>
    private const int NinjaPirateClawLaunchSpeed = 2048;

    /// <summary>How much a claw's speed changes each update, in 1/256 pixel per update.</summary>
    private const int NinjaPirateClawSpeedChange = 32;

    /// <summary>Allocates a claw with the selected throw direction, offset, graphics, and outbound speed.</summary>
    internal void SpawnNinjaPirateClaw(
        RoomEnemySlot source,
        NinjaSpacePirateEnemyState state,
        bool throwsRight,
        Offset16<RoomPixels> spawnOffset)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.PirateClaw,
            SpriteGraphics.SpawnedBy(source));
        projectile.Position = FixedPosition<RoomPixels>.FromWhole(source.Position.Whole + spawnOffset);
        projectile.StartAnimation(new SpacePirateProjectileAnimation(throwsRight
            ? SpacePirateProjectileProgram.ClawRight
            : SpacePirateProjectileProgram.ClawLeft));
        projectile.FrameBehavior = EnemyProjectileFrameBehavior.Stationary;
        projectile.State = new NinjaPirateClawState(NinjaPirateClawLaunchSpeed);
        state.SpawnedClawCount++;
    }

    /// <summary>Flies a claw thrown toward <paramref name="thrown"/> out and back, with exact camera culling.</summary>
    private static void FlyNinjaPirateClaw(
        EnemyProjectile projectile,
        Position16<RoomPixels> camera,
        HorizontalDirection thrown)
    {
        NinjaPirateClawState claw = projectile.StateOf<NinjaPirateClawState>();
        int pixels = UnsignedEightEight.Whole(claw.Speed);
        bool outbound = claw.Outbound;
        int direction = thrown == HorizontalDirection.Right ? 1 : -1;

        // Outbound motion follows the throw direction while decelerating to zero; inbound
        // motion reverses direction and accelerates indefinitely until the 256x256 camera
        // check deletes the claw. Y advances one pixel on both halves of the flight.
        projectile.Position = projectile.Position.WithWrappedWholeX(projectile.Position.X.Whole +
            (outbound ? direction * pixels : -direction * pixels));
        claw.Speed = outbound
            ? WrappingCounter.SubtractCount(claw.Speed, NinjaPirateClawSpeedChange)
            : WrappingCounter.AddCount(claw.Speed, NinjaPirateClawSpeedChange);
        if (outbound && claw.Speed == 0)
            claw.Outbound = false;
        projectile.Position = projectile.Position.WithWrappedWholeY(projectile.Position.Y.Whole + 1);

        int right = CoordinateAxis.Wrap(camera.X + 256);
        int bottom = CoordinateAxis.Wrap(camera.Y + 256);
        bool offScreen = CoordinateAxis.Difference(projectile.Position.X.Whole, camera.X) < 0 ||
            CoordinateAxis.Difference(projectile.Position.X.Whole, right) >= 0 ||
            CoordinateAxis.Difference(projectile.Position.Y.Whole, camera.Y) < 0 ||
            CoordinateAxis.Difference(projectile.Position.Y.Whole, bottom) >= 0;
        if (offScreen)
            projectile.Delete();
    }

    /// <summary>
    /// Carries out one cue of the ninja Space Pirates' animations. Becoming active continues at the active stance facing
    /// Samus, as the cue returns that animation; every other cue runs on.
    /// </summary>
    private NinjaSpacePirateProgram? ApplyNinjaSpacePirateCue(RoomEnemySlot slot, NinjaSpacePirateCue cue, SamusState? samus)
    {
        NinjaSpacePirateEnemyState state = slot.RequireSpeciesState<NinjaSpacePirateEnemyState>();
        switch (cue)
        {
            case NinjaSpacePirateCue.SetFunction setFunction:
                state.Function = setFunction.Function;
                return null;
            case NinjaSpacePirateCue.SetShading setShading:
                slot.Shading = setShading.Shading;
                return null;
            case NinjaSpacePirateCue.PlaySound playSound:
                _walkingSpacePirateBehavior.RequestSpacePirateSound(playSound.Sound ?? throw new InvalidOperationException(
                    "A ninja Space Pirate sound cue carries no sound."));
                return null;
            case NinjaSpacePirateCue.ThrowClaw throwClaw:
                SpawnNinjaPirateClaw(slot, state, throwClaw.Throw == HorizontalDirection.Right, throwClaw.Offset);
                return null;
            case NinjaSpacePirateCue.Command { Kind: NinjaSpacePirateCueKind.StartActive }:
                if (samus is null)
                {
                    throw new InvalidOperationException(
                        "Ninja Space Pirate facing selection requires the active Samus actor.");
                }
                state.ActiveProgram = CoordinateAxis.Difference(slot.Position.X.Whole, samus.Position.X.Whole) < 0
                    ? NinjaSpacePirateProgram.ActiveFacingRight
                    : NinjaSpacePirateProgram.ActiveFacingLeft;
                slot.FrameHold = WrappingCountdown.Start(1);
                // The instruction returns the selected list, not its next word.
                // Re-entering that list runs its function cue and restores decision AI
                // after the idle animation explicitly installed the no-op function.
                return state.ActiveProgram;
            case NinjaSpacePirateCue.Command { Kind: NinjaSpacePirateCueKind.ResetSpeed }:
                state.Speed = default;
                return null;
            case NinjaSpacePirateCue.Command { Kind: NinjaSpacePirateCueKind.StartLeftDivekick }:
                InitializeNinjaPirateDive(state);
                return null;
            case NinjaSpacePirateCue.Command { Kind: NinjaSpacePirateCueKind.StartRightDivekick }:
                InitializeNinjaPirateDive(state);
                return null;
            default:
                throw new InvalidOperationException($"The ninja Space Pirates' animations have no {cue} cue.");
        }
    }

    /// <summary>Installs an animation and restarts its frame timer.</summary>
    private static void InstallNinjaPirateInstruction(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state,
        NinjaSpacePirateProgram program)
    {
        state.Playback = new(NinjaSpacePirateAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
        slot.FrameHold = WrappingCountdown.Start(1);
    }

    /// <summary>Binds both claw throws to the outbound-and-return flight in their direction.</summary>
    internal static void AddNinjaSpacePirateProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddFrameBehavior(
            context => FlyNinjaPirateClaw(context.Projectile, context.Camera, HorizontalDirection.Left),
            EnemyProjectileFrameBehavior.PirateClawThrownLeft);
        table.AddFrameBehavior(
            context => FlyNinjaPirateClaw(context.Projectile, context.Camera, HorizontalDirection.Right),
            EnemyProjectileFrameBehavior.PirateClawThrownRight);
    }

    /// <summary>Runs the ninja Space Pirate's main AI.</summary>
    internal void RunNinjaSpacePirateMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunNinjaSpacePirateMain(
            slot,
            slot.RequireSpeciesState<NinjaSpacePirateEnemyState>(),
            frame.Samus,
            frame.Level,
            frame.SamusProjectiles);
    }

    /// <summary>Advances the ninja Space Pirate's typed animation.</summary>
    internal void AdvanceNinjaSpacePirateTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext context) =>
        EnemyProgramAnimation.Advance(slot, NinjaSpacePirateAnimations.Segments, NinjaSpacePirateFrameDefinitions.ShownFrames.Of,
            static (NinjaSpacePirateEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyNinjaSpacePirateCue(slot, cue, context.Samus) is { } continuesAt
                ? new AnimationContinuation<NinjaSpacePirateProgram>(NinjaSpacePirateAnimations.StartingWith(continuesAt), AnimationPlace.StartOfAnimation)
                : null);


    /// <summary>Accepts gold ninja extended-hitbox bomb overlaps without damage or generic death.</summary>
    internal static void AddGoldNinjaPirateBombReactions(EnemySharedReactions table)
    {
        table.AddBombSpecial(c => c.Enemy.DefinitionId == EnemyDefinitionId.PirateGoldNinja &&
                                  c.UsesFrameHitboxes && c.SelectedShotAi != EnemyReaction.NormalEnemyShot,
            c =>
            {
                PirateHitboxShotActions.PirateHitboxShotAction pirateAction = PirateHitboxShotActions.SelectPirateNormalBombHitboxShotAction(c.Enemy, c.SelectedShotAi);
                if (pirateAction != PirateHitboxShotActions.PirateHitboxShotAction.Ignore)
                    throw new InvalidDataException("Gold Ninja normal-bomb callbacks must ignore the normal-bomb family.");
                // Both gold-Ninja component callbacks compare the normal-bomb family against the Power Bomb family and return
                // immediately. The extended collision walker has already marked the bomb, so this is an
                // accepted physical overlap without damage, impact replacement, reflection, or the Lower-Norfair
                // special-drop death tail.
            });
    }
}
