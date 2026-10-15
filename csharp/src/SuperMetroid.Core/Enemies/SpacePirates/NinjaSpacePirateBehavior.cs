using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The ninja space pirate species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class NinjaSpacePirateBehavior : IEnemySpecies
{
    /// <summary>Provides the shared RNG used by divekick selection and installed palette artwork.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves floor contact during divekick descent.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Allocates thrown claw projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Spawns paired landing dust sprites.</summary>
    private readonly RoomSpriteObjectPool _spriteObjects;
    /// <summary>Provides CGRAM for the gold ninja's copied palette.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Receives sound requests from shared Space Pirate instruction opcodes.</summary>
    private readonly WalkingSpacePirateBehavior _walkingSpacePirateBehavior;

    /// <summary>Captures the room, collision, projectile, sprite, and sound owners for ninja AI.</summary>
    internal NinjaSpacePirateBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyProjectileEngine projectiles, RoomSpriteObjectPool spriteObjects, EnemyPresentation presentation, WalkingSpacePirateBehavior walkingSpacePirateBehavior)
    {
        _walkingSpacePirateBehavior = walkingSpacePirateBehavior;
        _room = room;
        _terrain = terrain;
        _projectiles = projectiles;
        _spriteObjects = spriteObjects;
        _presentation = presentation;
    }

    /// <summary>Native 8.8 speed assigned at the start of a divekick jump and descent.</summary>
    private const ushort NinjaPirateInitialDiveSpeed = 1536;
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

    /// <summary>Per-slot ninja state, including patrol posts and active instruction choice.</summary>
    internal readonly NinjaSpacePirateEnemyState?[] _ninjaSpacePirateStates =
        new NinjaSpacePirateEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Identifies all six ninja Space Pirate enemy definitions that share this AI.</summary>
    internal static bool IsNinjaSpacePirateDefinition(EnemyDefinitionId definition) => definition is
        EnemyDefinitionId.PirateGreyNinja or
        EnemyDefinitionId.PirateGreenNinja or
        EnemyDefinitionId.PirateRedNinja or
        EnemyDefinitionId.PirateGoldNinja or
        EnemyDefinitionId.PirateMagentaNinja or
        EnemyDefinitionId.PirateSilverNinja;

    /// <summary>Ports <c>InitAI_PirateNinja</c> at <c>$B2:F5DE</c>.</summary>
    internal void InitializeNinjaSpacePirate(RoomEnemySlot slot)
    {
        var state = new NinjaSpacePirateEnemyState(slot);
        _ninjaSpacePirateStates[slot.SlotIndex] = state;

        bool startsAtLeft = ((NinjaSpacePirateParameterFlags)slot.Parameter1 &
            NinjaSpacePirateParameterFlags.StartsAtLeftPostFacingRight) != 0;
        NinjaSpacePirateProgram initial = startsAtLeft
            ? NinjaSpacePirateProgram.InitialFacingRight
            : NinjaSpacePirateProgram.InitialFacingLeft;
        state.Program = AnimationProgramCursor<NinjaSpacePirateProgram>.Start(initial);
        state.ActiveProgram = initial;

        // Parameter two describes the requested distance between posts. The native setup
        // first establishes those requested posts around the population X coordinate, then
        // deliberately replaces them with a symmetric pair derived from its acceleration
        // sum. That adjustment is why the final span can differ slightly from parameter two.
        ushort requestedLeft = startsAtLeft
            ? slot.Position.X.Whole
            : unchecked((ushort)(slot.Position.X.Whole - slot.Parameter2));
        ushort requestedRight = startsAtLeft
            ? unchecked((ushort)(slot.Position.X.Whole + slot.Parameter2))
            : slot.Position.X.Whole;
        ushort requestedHalfSpan = unchecked((ushort)(requestedRight - requestedLeft));
        requestedHalfSpan >>= 1;
        state.PostsMidpointX = unchecked((ushort)(requestedLeft + requestedHalfSpan));

        // `$B2:F632` truncates the half-span to one byte before moving it to the high byte.
        // The do/while always executes once, including a zero-span debug population.
        ushort distanceInSubpixels = unchecked((ushort)SnesEightEight.FromWhole((byte)requestedHalfSpan));
        ushort speed = 0;
        ushort accumulatedDistance = 0;
        do
        {
            speed = unchecked((ushort)(speed + 32));
            accumulatedDistance = unchecked((ushort)(accumulatedDistance + speed));
        }
        while (unchecked((short)(accumulatedDistance - distanceInSubpixels)) < 0);

        state.CalculatedSpinJumpSpeed = speed;
        ushort adjustedHalfSpan = SnesLittleEndian.HighByte(accumulatedDistance);
        state.RightPostX = unchecked((ushort)(state.PostsMidpointX + adjustedHalfSpan));
        state.LeftPostX = unchecked((ushort)(state.PostsMidpointX - adjustedHalfSpan));
        slot.Position = slot.Position.WithX(new(startsAtLeft ? state.LeftPostX : state.RightPostX, 0));
        state.Function = NinjaSpacePirateFunction.NoOperation;
        state.SpawnY = slot.Position.Y.Whole;

        // Native copies the gold non-ninja Pirate's color image, not this actor's initial
        // room-graphics palette. Share that installed image so the same authored edit
        // affects both consumers without changing the ninja's animation or slot state.
        (_room.Artwork ?? throw new InvalidOperationException(
            "Gold Space Pirate requires installed palette artwork."))
            .LoadPaletteTo(EnemyDefinitionId.PirateGoldWall, new EnemyPalettes(_presentation.Cgram).NinjaSpacePirate);
    }

    /// <summary>Dispatches the literal function installed in native variable A.</summary>
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
                StepNinjaPirateSpinJump(slot, state, movingRight: false, rising: true);
                return;
            case NinjaSpacePirateFunction.SpinJumpLeftFalling:
                StepNinjaPirateSpinJump(slot, state, movingRight: false, rising: false);
                return;
            case NinjaSpacePirateFunction.SpinJumpRightRising:
                StepNinjaPirateSpinJump(slot, state, movingRight: true, rising: true);
                return;
            case NinjaSpacePirateFunction.SpinJumpRightFalling:
                StepNinjaPirateSpinJump(slot, state, movingRight: true, rising: false);
                return;
            case NinjaSpacePirateFunction.ReadyToDivekick:
                RunNinjaPirateReadyToDivekick(
                    slot,
                    state,
                    RequireSamus(),
                    samusProjectiles);
                return;
            case NinjaSpacePirateFunction.DivekickLeftJump:
                StepNinjaPirateDiveJump(slot, state, movingRight: false);
                return;
            case NinjaSpacePirateFunction.DivekickRightJump:
                StepNinjaPirateDiveJump(slot, state, movingRight: true);
                return;
            case NinjaSpacePirateFunction.DivekickLeftDive:
                StepNinjaPirateDive(slot, state, level, movingRight: false);
                return;
            case NinjaSpacePirateFunction.DivekickRightDive:
                StepNinjaPirateDive(slot, state, level, movingRight: true);
                return;
            case NinjaSpacePirateFunction.DivekickLeftWalkToPost:
                StepNinjaPirateWalkToPost(slot, state, movingRight: false);
                return;
            case NinjaSpacePirateFunction.DivekickRightWalkToPost:
                StepNinjaPirateWalkToPost(slot, state, movingRight: true);
                return;
            default:
                throw new InvalidDataException(
                    $"Ninja Space Pirate function $B2:{(ushort)state.Function:X4} is not translated.");
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
        if (Math.Abs(unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole))) >=
            NinjaPirateActivationDistance)
        {
            _ = TryNinjaPirateProjectileFlinch(slot, state, samus, samusProjectiles);
            return;
        }

        NinjaSpacePirateProgram active = unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole)) < 0
            ? NinjaSpacePirateProgram.ActiveFacingRight
            : NinjaSpacePirateProgram.ActiveFacingLeft;
        state.ActiveProgram = active;
        InstallNinjaPirateInstruction(slot, state, active);
    }

    /// <summary>Checks flinch, standing kick, spin jump, then periodic claw attack in native priority order.</summary>
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

    /// <summary>Flinches only for the highest occupied Samus projectile when it is close on both axes.</summary>
    private static bool TryNinjaPirateProjectileFlinch(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state,
        SamusState samus,
        SamusProjectileSystem? samusProjectiles)
    {
        if (samusProjectiles is null)
            return false;

        // `$B2:F72E` selects only the highest occupied Samus projectile slot. If that one is
        // far away, it returns immediately instead of searching a lower slot that is closer.
        SamusProjectileSlot? projectile = null;
        for (int index = Math.Min(4, samusProjectiles.Slots.Count - 1); index >= 0; index--)
        {
            // Native `$B2:F72E` selects by nonzero projectile type, not by bank-$93's
            // animation-list pointer. This includes the invisible zero-list Murder Beam.
            if (samusProjectiles.Slots[index].HasEnemyCollisionPayload)
            {
                projectile = samusProjectiles.Slots[index];
                break;
            }
        }
        if (projectile is null ||
            Math.Abs(unchecked((short)(projectile.Position.X.Whole - slot.Position.X.Whole))) >=
                NinjaPirateFlinchDistance ||
            Math.Abs(unchecked((short)(projectile.Position.Y.Whole - slot.Position.Y.Whole))) >=
                NinjaPirateFlinchDistance)
        {
            return false;
        }

        InstallNinjaPirateInstruction(
            slot,
            state,
            unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole)) < 0
                ? NinjaSpacePirateProgram.FlinchFacingRight
                : NinjaSpacePirateProgram.FlinchFacingLeft);
        return true;
    }

    /// <summary>Starts a facing kick when Samus is within the strict contact range.</summary>
    private static bool TryNinjaPirateStandingKick(RoomEnemySlot slot, NinjaSpacePirateEnemyState state, SamusState samus)
    {
        if (Math.Abs(unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole))) >=
                NinjaPirateKickDistance ||
            Math.Abs(unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole))) >=
                NinjaPirateKickDistance)
        {
            return false;
        }

        InstallNinjaPirateInstruction(
            slot,
            state,
            unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole)) < 0
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
        if (Math.Abs(unchecked((short)(state.PostsMidpointX - samus.Position.X.Whole))) >=
            NinjaPirateMidpointTriggerDistance)
        {
            return false;
        }

        InstallNinjaPirateInstruction(
            slot,
            state,
            slot.Position.X.Whole == state.LeftPostX
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
        if ((slot.FrameCounter & 0x003f) != 0)
            return;

        if (slot.Position.X.Whole == state.LeftPostX)
        {
            if (unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole)) < 0)
                return;
            InstallNinjaPirateInstruction(
                slot,
                state,
                NinjaSpacePirateProgram.ClawAttackLeft);
            return;
        }

        if (unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole)) >= 0)
            return;
        InstallNinjaPirateInstruction(
            slot,
            state,
            NinjaSpacePirateProgram.ClawAttackRight);
    }

    /// <summary>Retries native RNG until a nonzero dive choice after higher-priority reactions fail.</summary>
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

        if (Math.Abs(unchecked((short)(state.PostsMidpointX - samus.Position.X.Whole))) >=
            NinjaPirateMidpointTriggerDistance)
        {
            return;
        }

        // The table contains three identical entries per side. The random retry is retained
        // because advancing the native RNG until its low two bits are nonzero is observable
        // even though all three accepted values select the same list.
        ushort choice;
        do
            choice = unchecked((ushort)(_room.Random.Generate() & 3));
        while (choice == 0);

        InstallNinjaPirateInstruction(
            slot,
            state,
            slot.Position.X.Whole == state.LeftPostX
                ? NinjaSpacePirateProgram.DivekickRightJump
                : NinjaSpacePirateProgram.DivekickLeftJump);
    }

    /// <summary>Moves through a rising and falling arc between patrol posts, then emits landing dust.</summary>
    private void StepNinjaPirateSpinJump(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state,
        bool movingRight,
        bool rising)
    {
        int xPixels = SnesEightEight.WholeOf(state.Speed);
        slot.Position = slot.Position.WithWholeX(unchecked((ushort)(slot.Position.X.Whole + (movingRight ? xPixels : -xPixels))));
        slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole +
            (rising ? -NinjaPirateSpinVerticalPixels : NinjaPirateSpinVerticalPixels))));

        if (rising)
        {
            state.Speed = unchecked((ushort)(state.Speed + 32));
            bool crossedMidpoint = movingRight
                ? unchecked((short)(slot.Position.X.Whole - state.PostsMidpointX)) >= 0
                : unchecked((short)(slot.Position.X.Whole - state.PostsMidpointX)) < 0;
            if (crossedMidpoint)
            {
                state.Function = movingRight
                    ? NinjaSpacePirateFunction.SpinJumpRightFalling
                    : NinjaSpacePirateFunction.SpinJumpLeftFalling;
            }
            return;
        }

        state.Speed = unchecked((ushort)(state.Speed - 32));
        if (state.Speed != 0)
            return;

        state.Function = NinjaSpacePirateFunction.NoOperation;
        slot.Position = slot.Position.WithWholeX(movingRight ? state.RightPostX : state.LeftPostX);
        InstallNinjaPirateInstruction(
            slot,
            state,
            movingRight
                ? NinjaSpacePirateProgram.LandFacingRight
                : NinjaSpacePirateProgram.LandFacingLeft);
        SpawnNinjaPirateLandingDust(slot, state);
    }

    /// <summary>Sets dive speed and a side-dependent target midway between a post and the patrol center.</summary>
    private static void InitializeNinjaPirateDive(
        NinjaSpacePirateEnemyState state,
        bool movingRight)
    {
        state.Speed = NinjaPirateInitialDiveSpeed;
        state.DiveTargetX = movingRight
            ? unchecked((ushort)(state.LeftPostX +
                ((ushort)(state.PostsMidpointX - state.LeftPostX) >> 1)))
            : unchecked((ushort)(state.PostsMidpointX +
                ((ushort)(state.RightPostX - state.PostsMidpointX) >> 1)));
    }

    /// <summary>Rises until signed speed turns negative, then switches to the descent list.</summary>
    private static void StepNinjaPirateDiveJump(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state,
        bool movingRight)
    {
        slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole - (SnesEightEight.WholeOf(state.Speed)))));
        state.Speed = unchecked((ushort)(state.Speed - 64));
        if (unchecked((short)state.Speed) >= 0)
            return;

        state.Function = movingRight
            ? NinjaSpacePirateFunction.DivekickRightDive
            : NinjaSpacePirateFunction.DivekickLeftDive;
        InstallNinjaPirateInstruction(
            slot,
            state,
            movingRight
                ? NinjaSpacePirateProgram.DivekickRightDive
                : NinjaSpacePirateProgram.DivekickLeftDive);
        state.Speed = NinjaPirateInitialDiveSpeed;
    }

    /// <summary>Moves horizontally and descends with native packed speed until floor or speed ends the dive.</summary>
    private void StepNinjaPirateDive(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state,
        RoomLevelData? level,
        bool movingRight)
    {
        if (level is null)
        {
            throw new InvalidOperationException(
                "Ninja Space Pirate divekick movement requires room collision data.");
        }

        slot.Position = slot.Position.WithWholeX(unchecked((ushort)(slot.Position.X.Whole +
            (movingRight ? NinjaPirateDiveHorizontalPixels : -NinjaPirateDiveHorizontalPixels))));

        // MoveEnemyDownBy_14_12 receives the high speed byte as the signed integer word and
        // the low byte as only the low eight bits of the fractional word. This odd packing is
        // preserved literally; treating Speed as a conventional 8.8 fixed value is subtly
        // different from the 65816 register setup at $B2:F9CB/$FA9F.
        int displacement = (SnesSixteenSixteen.FromWhole(SnesEightEight.WholeOf(state.Speed))) | (SnesEightEight.FractionOf(state.Speed));
        bool landed = _terrain.MoveEnemyVertically(level, slot, displacement);
        if (!landed)
        {
            state.Speed = unchecked((ushort)(state.Speed - 64));
            landed = unchecked((short)state.Speed) < 0 || SnesUnsignedEightEight.Whole(state.Speed) == 0;
        }
        if (!landed)
            return;

        state.Function = movingRight
            ? NinjaSpacePirateFunction.DivekickRightWalkToPost
            : NinjaSpacePirateFunction.DivekickLeftWalkToPost;
        InstallNinjaPirateInstruction(
            slot,
            state,
            movingRight
                ? NinjaSpacePirateProgram.WalkToRightPost
                : NinjaSpacePirateProgram.WalkToLeftPost);
        slot.Position = slot.Position.WithY(new(state.SpawnY, 0));
        SpawnNinjaPirateLandingDust(slot, state);
    }

    /// <summary>Walks back to the selected post and restores its landing instruction list.</summary>
    private static void StepNinjaPirateWalkToPost(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state,
        bool movingRight)
    {
        slot.Position = slot.Position.WithWholeX(unchecked((ushort)(slot.Position.X.Whole +
            (movingRight ? NinjaPirateReturnWalkPixels : -NinjaPirateReturnWalkPixels))));
        bool reachedPost = movingRight
            ? unchecked((short)(slot.Position.X.Whole - state.RightPostX)) >= 0
            : unchecked((short)(slot.Position.X.Whole - state.LeftPostX)) < 0;
        if (!reachedPost)
            return;

        slot.Position = slot.Position.WithWholeX(movingRight ? state.RightPostX : state.LeftPostX);
        InstallNinjaPirateInstruction(
            slot,
            state,
            movingRight
                ? NinjaSpacePirateProgram.LandFacingRight
                : NinjaSpacePirateProgram.LandFacingLeft);
        state.Function = NinjaSpacePirateFunction.NoOperation;
    }

    /// <summary>Attempts a dust sprite on each side of the landing position and counts successful spawns.</summary>
    private void SpawnNinjaPirateLandingDust(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state)
    {
        ushort graphicsIndex = 0;
        if (_spriteObjects.SpawnRoomSpriteObject(
                new(unchecked((ushort)(slot.Position.X.Whole - 8)), unchecked((ushort)(slot.Position.Y.Whole + 28))),
                RoomSpriteObjectKind.NinjaPirateLandingDust,
                graphicsIndex) is not null)
        {
            state.LandingDustCount++;
        }
        if (_spriteObjects.SpawnRoomSpriteObject(
                new(unchecked((ushort)(slot.Position.X.Whole + 8)), unchecked((ushort)(slot.Position.Y.Whole + 28))),
                RoomSpriteObjectKind.NinjaPirateLandingDust,
                graphicsIndex) is not null)
        {
            state.LandingDustCount++;
        }
    }

    /// <summary>Allocates a claw with the selected throw direction, offset, graphics, and outbound speed.</summary>
    internal void SpawnNinjaPirateClaw(
        RoomEnemySlot source,
        NinjaSpacePirateEnemyState state,
        ushort direction,
        Offset16<RoomPixels> spawnOffset)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.PirateClaw,
            unchecked((ushort)(source.VramTilesIndex | source.PaletteIndex)));
        projectile.Position = FixedPosition<RoomPixels>.FromWhole(source.Position.Whole + spawnOffset);
        projectile.DirectionParameter = direction;
        projectile.InstructionPointer = direction == 0
            ? SpacePirateProjectileInstructionProgramDefinitions.ClawLeft
            : SpacePirateProjectileInstructionProgramDefinitions.ClawRight;
        projectile.InstructionTimer = NativeCountdown.Start(1);
        projectile.PreInstruction = (ushort)EnemyProjectilePreInstruction.RTS_86A05B;
        projectile.Variable0 = 0x0800;
        projectile.Variable1 = 1;
        state.SpawnedClawCount++;
    }

    /// <summary>Runs claw pre-instructions $86:A0D1/$A124 and exact camera culling.</summary>
    internal static void RunNinjaPirateClawPreInstruction(
        RoomEnemyProjectileSlot projectile,
        Position16<RoomPixels> camera)
    {
        int pixels = SnesLittleEndian.HighByte(projectile.Variable0);
        bool outbound = projectile.Variable1 != 0;
        bool thrownRight = ClosedNativeWords.Decode<EnemyProjectilePreInstruction>(
            projectile.PreInstruction, "enemy-projectile pre-instruction") ==
            EnemyProjectilePreInstruction.PirateClaw_Right;
        int direction = thrownRight ? 1 : -1;

        // Outbound motion follows the throw direction while decelerating to zero; inbound
        // motion reverses direction and accelerates indefinitely until the 256x256 camera
        // check deletes the claw. Y advances one pixel on both halves of the flight.
        projectile.Position = projectile.Position.WithWholeX(unchecked((ushort)(projectile.Position.X.Whole +
            (outbound ? direction * pixels : -direction * pixels))));
        projectile.Variable0 = unchecked((ushort)(projectile.Variable0 +
            (outbound ? -32 : 32)));
        if (outbound && projectile.Variable0 == 0)
            projectile.Variable1 = 0;
        projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(projectile.Position.Y.Whole + 1)));

        ushort right = unchecked((ushort)(camera.X + 256));
        ushort bottom = unchecked((ushort)(camera.Y + 256));
        bool offScreen = unchecked((short)(projectile.Position.X.Whole - camera.X)) < 0 ||
            unchecked((short)(projectile.Position.X.Whole - right)) >= 0 ||
            unchecked((short)(projectile.Position.Y.Whole - camera.Y)) < 0 ||
            unchecked((short)(projectile.Position.Y.Whole - bottom)) >= 0;
        if (offScreen)
            projectile.Clear();
    }

    /// <summary>
    /// Carries out one cue of the ninja Space Pirates' animations. Becoming active continues at the active stance facing
    /// Samus, as the native opcode returns that list; every other cue runs on.
    /// </summary>
    private NinjaSpacePirateProgram? ApplyNinjaSpacePirateCue(RoomEnemySlot slot, NinjaSpacePirateCue cue, SamusState? samus)
    {
        NinjaSpacePirateEnemyState state = RequireNinjaSpacePirateState(slot);
        switch (cue.Kind)
        {
            case NinjaSpacePirateCueKind.SetFunction:
                state.Function = cue.Function;
                return null;
            case NinjaSpacePirateCueKind.SetPalette:
                slot.PaletteIndex = cue.Value;
                return null;
            case NinjaSpacePirateCueKind.PlaySound:
                _walkingSpacePirateBehavior.LastSpacePirateSoundEffect = cue.Value;
                return null;
            case NinjaSpacePirateCueKind.ThrowClaw:
                SpawnNinjaPirateClaw(slot, state, cue.Value, new Offset16<RoomPixels>(cue.Offset, cue.OffsetY));
                return null;
            case NinjaSpacePirateCueKind.StartActive:
                if (samus is null)
                {
                    throw new InvalidOperationException(
                        "Ninja Space Pirate facing selection requires the active Samus actor.");
                }
                state.ActiveProgram = unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole)) < 0
                    ? NinjaSpacePirateProgram.ActiveFacingRight
                    : NinjaSpacePirateProgram.ActiveFacingLeft;
                slot.InstructionTimer = NativeCountdown.Start(1);
                // The native instruction returns the selected list, not its next word.
                // Re-entering that list runs its function cue and restores decision AI
                // after the idle animation explicitly installed the no-op function.
                return state.ActiveProgram;
            case NinjaSpacePirateCueKind.ResetSpeed:
                state.Speed = 0;
                return null;
            case NinjaSpacePirateCueKind.StartLeftDivekick:
                InitializeNinjaPirateDive(state, movingRight: false);
                return null;
            case NinjaSpacePirateCueKind.StartRightDivekick:
                InitializeNinjaPirateDive(state, movingRight: true);
                return null;
            default:
                throw new InvalidOperationException($"The ninja Space Pirates' animations have no {cue.Kind} cue.");
        }
    }

    /// <summary>Installs an animation and restarts its frame timer.</summary>
    private static void InstallNinjaPirateInstruction(
        RoomEnemySlot slot,
        NinjaSpacePirateEnemyState state,
        NinjaSpacePirateProgram program)
    {
        state.Program = AnimationProgramCursor<NinjaSpacePirateProgram>.Start(program);
        slot.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>Gets the state attached to a ninja slot and rejects an uninitialized actor.</summary>
    private NinjaSpacePirateEnemyState RequireNinjaSpacePirateState(RoomEnemySlot slot) =>
        _ninjaSpacePirateStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized ninja Space Pirate state.");

    /// <summary>Binds both claw directions to the same outbound-and-return pre-instruction.</summary>
    internal static void RegisterNinjaSpacePirateProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            RunNinjaPirateClawPreInstruction(context.Projectile, context.Camera);
        }, EnemyProjectilePreInstruction.PirateClaw_Left, EnemyProjectilePreInstruction.PirateClaw_Right);
    }

    /// <summary>Registers initialization for the six ninja Space Pirate definitions.</summary>
    internal void RegisterNinjaSpacePirateInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_PirateNinja, slot => IsNinjaSpacePirateDefinition(slot.EnemyDefinitionPointer), (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeNinjaSpacePirate(slot);
        });
    }

    /// <summary>Registers the ninja function dispatcher for initialized actors.</summary>
    internal void RegisterNinjaSpacePirateMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_PirateNinja, slot => IsNinjaSpacePirateDefinition(slot.EnemyDefinitionPointer), (slot, frame, nmiFrameCounter8) =>
        {
            RunNinjaSpacePirateMain(
                slot,
                RequireNinjaSpacePirateState(slot),
                frame.Samus,
                frame.Level,
                frame.SamusProjectiles);
        });
    }

    /// <summary>Runs the ninja Space Pirates' animations as typed programs (#641).</summary>
    internal void RegisterNinjaSpacePirateTypedAnimations(EnemyAiTable table) =>
        table.AddTypedAnimation(slot => IsNinjaSpacePirateDefinition(slot.EnemyDefinitionPointer), (slot, context) =>
        {
            if (!EnemyProgramAnimation.Expire(slot))
                return;
            var state = RequireNinjaSpacePirateState(slot);
            AnimationProgramCursor<NinjaSpacePirateProgram> cursor = state.Program;
            EnemyProgramAnimation.Run(slot, NinjaSpacePirateAnimations.Programs, ref cursor, NinjaSpacePirateFrameDefinitions.Spritemap,
                    (cue, _) => ApplyNinjaSpacePirateCue(slot, cue, context.Samus));
            state.Program = cursor;
        });


    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        Array.Clear(_ninjaSpacePirateStates);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }

    /// <summary>Accepts gold ninja extended-hitbox bomb overlaps without damage or generic death.</summary>
    internal static void RegisterGoldNinjaPirateBombReactions(EnemyAiTable table)
    {
        table.AddBombSpecial(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.PirateGoldNinja &&
                                  c.UsesExtendedHitboxes && c.SelectedShotAi != EnemyInteractionCallback.NormalEnemyShot,
            c =>
            {
                EnemyCollision.PirateHitboxShotAction pirateAction = EnemyCollision.SelectPirateNormalBombHitboxShotAction(c.Enemy, c.SelectedShotAiPointer);
                if (pirateAction != EnemyCollision.PirateHitboxShotAction.Ignore)
                    throw new InvalidDataException("Gold Ninja normal-bomb callbacks must ignore family $0500.");
                // Both gold-Ninja component callbacks compare family $0500 against Power Bomb family $0300 and return
                // immediately. The bank-$A0 extended collision walker has already marked the bomb, so this is an
                // accepted physical overlap without damage, impact replacement, reflection, or the Lower-Norfair
                // special-drop death tail.
            });
    }
}
