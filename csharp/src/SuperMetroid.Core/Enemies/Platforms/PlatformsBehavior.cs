using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Platforms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>The Platforms species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class PlatformsBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Kamer2, EnemyDefinitionId.Tripper];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new PlatformsEntity(this, slot, common);

    /// <summary>Clips Tripper/Kamer horizontal and vertical displacement against room terrain.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Binds the platform movement family to terrain collision.</summary>
    internal PlatformsBehavior(EnemyTerrain terrain)
    {
        _terrain = terrain;
    }

    /// <summary>Carries out one cue of the platforms' animations: each publishes the horizontal direction main AI moves in.</summary>
    private static PlatformProgram? ApplyPlatformCue(RoomEnemySlot slot, PlatformCue cue)
    {
        switch (cue.Kind)
        {
            case PlatformCueKind.MoveLeft:
                // The ordinary and duplicate native commands are byte-for-byte equivalent: both publish horizontal
                // dispatcher index zero before the next frame.
                SetPlatformHorizontalMovementFromInstruction(slot, PlatformHorizontalMovement.Left);
                break;
            case PlatformCueKind.MoveRight:
                SetPlatformHorizontalMovementFromInstruction(slot, PlatformHorizontalMovement.Right);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(PlatformCueKind)} {cue.Kind}.");
        }
        return null;
    }


    /// <summary>Recognizes the two enemy definitions sharing bank-$A3 platform movement.</summary>
    internal static bool IsPlatformDefinition(EnemyDefinitionId definition) =>
        definition is EnemyDefinitionId.Tripper or EnemyDefinitionId.Kamer2;

    /// <summary>Ports the two entry points and common initializer at $A3:9C9F-$9D15.</summary>
    internal void InitializePlatform(RoomEnemySlot slot)
    {
        bool isKamer = slot.EnemyDefinitionPointer == EnemyDefinitionId.Kamer2;
        var state = new PlatformEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);

        state.IsSuspensorPlatform = isKamer;
        // Main AI dispatches the complete word through a two-entry table. Initialization's
        // art branch merely tests zero/nonzero, so preserve a corrupt value here and let the
        // typed dispatcher reject it instead of silently normalizing it to index one.
        state.XMovement = (PlatformHorizontalMovement)slot.Parameter1;

        state.Program = AnimationProgramCursor<PlatformProgram>.Start(PlatformAnimationDefinitions.SelectProgram(
            isKamer, verticallyMoving: false, state.XMovement));

        // The low byte is a common-linear-speed magnitude. Multiplication by eight is the
        // native byte offset across {right whole/fraction, left whole/fraction}.
        ushort speedTableOffset = unchecked((ushort)((SnesLittleEndian.LowByte(slot.Parameter2)) * 8));
        (short rightWhole, ushort rightFraction) = EnemyMath.ReadLinearEnemySpeed(speedTableOffset);
        state.RightSpeed = SnesSignedSixteenSixteen.FromParts(rightWhole, rightFraction);
        (short leftWhole, ushort leftFraction) = EnemyMath.ReadLinearEnemySpeed(unchecked((ushort)(speedTableOffset + 4)));
        state.LeftSpeed = SnesSignedSixteenSixteen.FromParts(leftWhole, leftFraction);

        state.YMovement = PlatformVerticalMovement.Rising;
        state.PreviousYMovement = PlatformVerticalMovement.Rising;
        state.VerticallyMovingArtInstalled = false;
        state.VerticallyStillArtInstalled = false;
        state.RestingHeight = new Position16<RoomPixels>(0, slot.Position.Y.Whole) + new Offset16<RoomPixels>(0, 1);
        state.YSpeedTableIndex = 0;
        state.MaximumYSpeedTableIndex = SnesLittleEndian.HighByte(slot.Parameter2);
    }

    /// <summary>Ports <c>MainAI_Tripper_Kamer2</c> at $A3:9D16.</summary>
    private void RunPlatformMain(
        RoomEnemySlot slot,
        PlatformEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Platform AI requires the active Samus actor.");
        if (level is null)
            throw new InvalidOperationException("Platform AI requires active room collision data.");

        // Every frame starts in the rising dispatcher and changes to sinking only when the
        // exact asymmetric "Samus from below" test succeeds. This is also the native solid-
        // platform riding gate; ordinary radius overlap would trigger from the sides.
        state.YMovement = IsSamusRidingPlatform(slot, samus)
            ? PlatformVerticalMovement.Sinking
            : PlatformVerticalMovement.Rising;

        // The sinking carry measures the horizontal step from the position before it.
        ushort preHorizontalX = slot.Position.X.Whole;
        MovePlatformHorizontally(slot, state, level);
        MovePlatformVertically(slot, state, samus, level, preHorizontalX);

        // Native main AI resets acceleration after running the first frame in a newly
        // selected vertical state. Preserve that slightly surprising ordering: a new phase
        // can use quadratic entry one now, then begins from entry zero on its next frame.
        if (state.YMovement != state.PreviousYMovement)
            state.YSpeedTableIndex = 0;
        state.PreviousYMovement = state.YMovement;
    }

    /// <summary>Exact wrapped-word port of <c>$A0:ABE7-$AC28</c>.</summary>
    internal static bool IsSamusRidingPlatform(RoomEnemySlot slot, SamusState samus)
    {
        ushort xDistance = unchecked((ushort)(samus.Position.X.Whole - slot.Position.X.Whole));
        if (unchecked((short)xDistance) < 0)
            xDistance = unchecked((ushort)-xDistance);

        // Carry clear after subtracting Samus's radius jumps directly to Y. Otherwise the
        // remainder must be strictly less than the platform radius; equality is not overlap.
        if (xDistance >= samus.Kinematics.Radius.X)
        {
            ushort remainder = unchecked((ushort)(xDistance - samus.Kinematics.Radius.X));
            if (remainder >= slot.Radius.X)
                return false;
        }

        // The +3 bias requires Samus's center to remain above the enemy center. After taking
        // the wrapped magnitude, Y accepts equality with the enemy radius (unlike X).
        ushort yDifference = unchecked((ushort)(samus.Position.Y.Whole + 3 - slot.Position.Y.Whole));
        if (unchecked((short)yDifference) >= 0)
            return false;
        ushort yDistance = unchecked((ushort)-yDifference);
        if (yDistance < samus.Kinematics.Radius.Y)
            return true;
        ushort yRemainder = unchecked((ushort)(yDistance - samus.Kinematics.Radius.Y));
        return yRemainder <= slot.Radius.Y;
    }

    /// <summary>Applies horizontal movement, reversing the dispatcher and still-art list on wall collision.</summary>
    private void MovePlatformHorizontally(
        RoomEnemySlot slot,
        PlatformEnemyState state,
        RoomLevelData level)
    {
        bool movingLeft = state.XMovement switch
        {
            PlatformHorizontalMovement.Left => true,
            PlatformHorizontalMovement.Right => false,
            _ => throw new InvalidDataException(
                $"Platform X movement index {(ushort)state.XMovement} exceeds its two-entry table."),
        };
        int displacement = (movingLeft ? state.LeftSpeed : state.RightSpeed).RawValue;
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, displacement))
            return;

        // A wall collision reverses the dispatcher and immediately installs the opposite
        // vertically-still list. The first opcode in that list republishes the same index
        // when instruction processing runs later in this enemy frame.
        state.XMovement = movingLeft
            ? PlatformHorizontalMovement.Right
            : PlatformHorizontalMovement.Left;
        InstallPlatformVerticallyStillForCurrentDirection(slot, state);
    }

    /// <summary>Routes the platform through rise or sink physics and carries a riding Samus by accepted motion.</summary>
    /// <param name="slot">The platform's slot.</param>
    /// <param name="state">The platform's state.</param>
    /// <param name="samus">Samus, carried while riding.</param>
    /// <param name="level">The room's collision data.</param>
    /// <param name="preHorizontalX">The platform's whole X before this update's horizontal movement.</param>
    private void MovePlatformVertically(
        RoomEnemySlot slot,
        PlatformEnemyState state,
        SamusState samus,
        RoomLevelData level,
        ushort preHorizontalX)
    {
        switch (state.YMovement)
        {
            case PlatformVerticalMovement.Rising:
                RaisePlatform(slot, state, level);
                return;
            case PlatformVerticalMovement.Sinking:
                SinkPlatform(slot, state, samus, level, preHorizontalX);
                return;
            default:
                throw new InvalidDataException(
                    $"Platform Y movement index {(ushort)state.YMovement} exceeds its two-entry table.");
        }
    }

    /// <summary>Returns a depressed platform toward its spawn height with the native one-pixel stop offset.</summary>
    private void RaisePlatform(
        RoomEnemySlot slot,
        PlatformEnemyState state,
        RoomLevelData level)
    {
        // Signed BMI comparison is intentional. Normal platforms stop once they have risen
        // from their depressed position to one pixel below their original spawn center.
        if (unchecked((short)(slot.Position.Y.Whole - state.RestingHeight.Y)) < 0)
        {
            state.YSpeedTableIndex = 0;
            SetPlatformVerticallyStillInstruction(slot, state);
            return;
        }

        SetPlatformVerticallyMovingInstruction(slot, state);
        IncrementAndClampPlatformYSpeed(state);
        int displacement = EnemyMath.ReadQuadraticEnemySpeed(
            state.YSpeedTableIndex,
            negative: true);
        if (!_terrain.MoveEnemyVertically(level, slot, displacement))
            return;

        state.YSpeedTableIndex = 0;
        SetPlatformVerticallyStillInstruction(slot, state);
    }

    /// <summary>Accelerates a ridden platform downward and publishes its collision-clipped displacement to Samus.</summary>
    /// <param name="slot">The platform's slot.</param>
    /// <param name="state">The platform's state.</param>
    /// <param name="samus">Samus, carried by the platform's accepted movement.</param>
    /// <param name="level">The room's collision data.</param>
    /// <param name="preHorizontalX">The platform's whole X before this update's horizontal movement.</param>
    private void SinkPlatform(
        RoomEnemySlot slot,
        PlatformEnemyState state,
        SamusState samus,
        RoomLevelData level,
        ushort preHorizontalX)
    {
        // `$A3:9DE7-$9DED` performs a comparison followed by a JSR to an immediate RTS; it
        // has no branch or side effect. Sinking therefore always continues to acceleration.
        IncrementAndClampPlatformYSpeed(state);

        // Add the wrapped whole-pixel horizontal step to Samus's extra displacement; fractional
        // displacement is deliberately untouched because this enemy carries only whole pixels.
        ushort xDelta = unchecked((ushort)(slot.Position.X.Whole - preHorizontalX));
        samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(unchecked((short)(xDelta)), 0);

        ushort preVerticalY = slot.Position.Y.Whole;
        int displacement = EnemyMath.ReadQuadraticEnemySpeed(
            state.YSpeedTableIndex,
            negative: false);
        if (_terrain.MoveEnemyVertically(level, slot, displacement))
        {
            state.YSpeedTableIndex = 0;
            SetPlatformVerticallyStillInstruction(slot, state);
        }

        // This delta is measured after collision clipping, so Samus follows the platform's
        // accepted motion rather than the requested quadratic speed.
        ushort yDelta = unchecked((ushort)(slot.Position.Y.Whole - preVerticalY));
        samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(0, unchecked((short)(yDelta)));
    }

    /// <summary>
    /// Steps the quadratic-speed table index without crossing its variant-specific cap. The index stays within
    /// 0 through the cap (at most 255) and one step can only reach 256, so the cartridge's signed word comparison
    /// is an ordinary one.
    /// </summary>
    internal static void IncrementAndClampPlatformYSpeed(PlatformEnemyState state)
    {
        state.YSpeedTableIndex++;
        if (state.YSpeedTableIndex >= state.MaximumYSpeedTableIndex)
            state.YSpeedTableIndex = state.MaximumYSpeedTableIndex;
    }

    /// <summary>Installs moving art once per phase and always clears the still-art flag at the native merge label.</summary>
    private static void SetPlatformVerticallyMovingInstruction(
        RoomEnemySlot slot,
        PlatformEnemyState state)
    {
        if (!state.VerticallyMovingArtInstalled)
        {
            state.VerticallyMovingArtInstalled = true;
            InstallPlatformVerticallyMovingForCurrentDirection(slot, state);
        }
        // `$A3:9ED9` is below the already-installed branch target, so it executes even when
        // no new list was needed this frame.
        state.VerticallyStillArtInstalled = false;
    }

    /// <summary>Installs stationary art once per phase and always clears the moving-art flag.</summary>
    private static void SetPlatformVerticallyStillInstruction(
        RoomEnemySlot slot,
        PlatformEnemyState state)
    {
        if (!state.VerticallyStillArtInstalled)
        {
            state.VerticallyStillArtInstalled = true;
            InstallPlatformVerticallyStillForCurrentDirection(slot, state);
        }
        // Same merge-label detail as the moving helper: the opposite flag is always clear.
        state.VerticallyMovingArtInstalled = false;
    }

    /// <summary>Selects the moving list from suspensor identity and current horizontal dispatcher.</summary>
    internal static void InstallPlatformVerticallyMovingForCurrentDirection(
        RoomEnemySlot slot,
        PlatformEnemyState state)
    {
        InstallPlatformProgram(slot, state, PlatformAnimationDefinitions.SelectProgram(
            state.IsSuspensorPlatform, verticallyMoving: true, state.XMovement));
    }

    /// <summary>Selects the stationary list from suspensor identity and current horizontal dispatcher.</summary>
    internal static void InstallPlatformVerticallyStillForCurrentDirection(
        RoomEnemySlot slot,
        PlatformEnemyState state)
    {
        InstallPlatformProgram(slot, state, PlatformAnimationDefinitions.SelectProgram(
            state.IsSuspensorPlatform, verticallyMoving: false, state.XMovement));
    }

    /// <summary>Starts the selected animation with a one-tick instruction timer.</summary>
    private static void InstallPlatformProgram(RoomEnemySlot slot, PlatformEnemyState state, PlatformProgram program)
    {
        state.Program = AnimationProgramCursor<PlatformProgram>.Start(program);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    /// <summary>Ports the four equivalent list commands at $A3:9C6B/$9C76/$9C81/$9C8C.</summary>
    private static void SetPlatformHorizontalMovementFromInstruction(
        RoomEnemySlot slot,
        PlatformHorizontalMovement movement)
    {
        RequirePlatformState(slot).XMovement = movement;
    }

    /// <summary>Requires typed platform movement state after the shared initializer has run.</summary>
    private static PlatformEnemyState RequirePlatformState(RoomEnemySlot slot) =>
        slot.SpeciesState<PlatformEnemyState>() ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Tripper/Kamer state.");

    /// <summary>Replaces a frozen Tripper's map after common Bomb reaction using its movement direction.</summary>
    internal static void RegisterPlatformsBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Tripper &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.TripperShot)
        {
            AfterCommon = c =>
            {
                if (!c.Enemy.FrozenTimer.IsZero)
                    c.Enemy.SpritemapPointer = TripperKamerVisualDefinitions.FrozenFrame(RequirePlatformState(c.Enemy).XMovement);
            },
        });
    }

    /// <summary>Ports Platforms's initialization AI.</summary>
    internal void RunPlatformsInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializePlatform(slot);
    }

    /// <summary>Ports Platforms's main AI.</summary>
    internal void RunPlatformsMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunPlatformMain(slot, RequirePlatformState(slot), frame.Samus, frame.Level);
    }

    /// <summary>Advances Platforms's typed animation.</summary>
    internal static void AdvancePlatformsTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        var state = RequirePlatformState(slot);
        AnimationProgramCursor<PlatformProgram> cursor = state.Program;
        EnemyProgramAnimation.Run(slot, PlatformAnimations.Programs, ref cursor, PlatformFrameDefinitions.Spritemap,
                (cue, _) => ApplyPlatformCue(slot, cue));
        state.Program = cursor;
    }

    /// <summary>Applies Tripper's direction-specific frozen map after a successful common shot freeze.</summary>
    internal static void RegisterPlatformsShotReactions(EnemySharedReactions table)
    {
        // Tripper's private tail runs after common shot AI on the immediate-freeze path and replaces the current
        // frame with a direction-specific two-piece frozen map.
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Tripper &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.TripperShot)
        {
            AfterCommon = (c, froze) =>
            {
                if (froze)
                    c.Enemy.SpritemapPointer = TripperKamerVisualDefinitions.FrozenFrame(RequirePlatformState(c.Enemy).XMovement);
            },
        });
    }

    /// <summary>Registers native RTL touch behavior so ordinary overlap does not damage Samus.</summary>
    internal static void RegisterPlatformsTouch(EnemySharedReactions table)
    {
        // `$A3:9F07` is a literal RTL. Platform solidity and the asymmetric rider test live in other handlers;
        // ordinary body overlap must neither injure Samus nor synthesize knockback here.
        table.AddTouch(slot => IsPlatformDefinition(slot.EnemyDefinitionPointer) &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.PlatformNoOpTouch,
            static _ => { });
    }

    /// <inheritdoc/>
    /// <remarks>Each platform's initializer creates its state, so a room load has nothing to reset.</remarks>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
    }
}
