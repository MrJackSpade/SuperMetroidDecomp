using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The Platforms species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class PlatformsBehavior : IEnemySpecies
{
    /// <summary>Stores terrain.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Initializes a new platforms behavior instance.</summary>
    internal PlatformsBehavior(EnemyTerrain terrain)
    {
        _terrain = terrain;
    }

    /// <summary>Executes the Tripper/Kamer platform family's private animation instructions.</summary>
    private bool TryProcessPlatformInstruction(RoomEnemySlot slot, ushort word, ref ushort cursor)
    {
        if (!IsPlatformDefinition(slot.EnemyDefinitionPointer) ||
            !Enum.IsDefined((PlatformInstruction)word))
            return false;

        switch ((PlatformInstruction)word)
        {
            case PlatformInstruction.SetMovingLeftXMovement:
            case PlatformInstruction.SetMovingLeftXMovementDuplicate:
                // The "ordinary" and duplicate commands are byte-for-byte equivalent:
                // both publish horizontal dispatcher index zero before the next frame.
                SetPlatformHorizontalMovementFromInstruction(slot, PlatformHorizontalMovement.Left);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case PlatformInstruction.SetMovingRightXMovement:
            case PlatformInstruction.SetMovingRightXMovementDuplicate:
                SetPlatformHorizontalMovementFromInstruction(slot, PlatformHorizontalMovement.Right);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            default:
                throw new InvalidOperationException(
                    $"Tripper/Kamer platform does not own instruction ${word:X4}.");
        }
    }



    /// <summary>Stores platform movement functions.</summary>
    internal readonly PlatformMovementFunctions[] _platformMovementFunctions = new PlatformMovementFunctions[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Stores platform previous positions.</summary>
    internal readonly ushort[] _platformPreviousPositions = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Stores platform vertically moving flags.</summary>
    internal readonly ushort[] _platformVerticallyMovingFlags = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Stores platform vertically still flags.</summary>
    internal readonly ushort[] _platformVerticallyStillFlags = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Stores platform maximum yspeed table indexes.</summary>
    internal readonly ushort[] _platformMaximumYSpeedTableIndexes = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Stores platform previous ymovement functions.</summary>
    internal readonly ushort[] _platformPreviousYMovementFunctions = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Stores platform suspensor platform flags.</summary>
    internal readonly ushort[] _platformSuspensorPlatformFlags = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Stores platform states.</summary>
    internal readonly PlatformEnemyState?[] _platformStates =
        new PlatformEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>is platform definition.</summary>
    internal static bool IsPlatformDefinition(EnemyDefinitionId definition) =>
        definition is EnemyDefinitionId.Tripper or EnemyDefinitionId.Kamer2;

    /// <summary>Ports the two entry points and common initializer at $A3:9C9F-$9D15.</summary>
    internal void InitializePlatform(RoomEnemySlot slot)
    {
        bool isKamer = slot.EnemyDefinitionPointer == EnemyDefinitionId.Kamer2;
        var state = new PlatformEnemyState(slot, new PlatformSharedTables { MovementFunctions = _platformMovementFunctions, PreviousPositions = _platformPreviousPositions, VerticallyMovingFlags = _platformVerticallyMovingFlags, VerticallyStillFlags = _platformVerticallyStillFlags, MaximumYSpeedTableIndexes = _platformMaximumYSpeedTableIndexes, PreviousYMovementFunctions = _platformPreviousYMovementFunctions, SuspensorPlatformFlags = _platformSuspensorPlatformFlags });
        _platformStates[slot.SlotIndex] = state;

        state.IsSuspensorPlatform = isKamer;
        // Main AI dispatches the complete word through a two-entry table. Initialization's
        // art branch merely tests zero/nonzero, so preserve a corrupt value here and let the
        // typed dispatcher reject it instead of silently normalizing it to index one.
        state.XMovement = (PlatformHorizontalMovement)slot.Parameter1;

        slot.CurrentInstruction = PlatformInstructionProgramDefinitions.SelectProgram(
            isKamer, verticallyMoving: false, state.XMovement);

        // The low byte is a common-linear-speed magnitude. Multiplication by eight is the
        // native byte offset across {right whole/fraction, left whole/fraction}.
        ushort speedTableOffset = unchecked((ushort)((SnesLittleEndian.LowByte(slot.Parameter2)) * 8));
        (state.RightVelocity, state.RightSubvelocity) =
            EnemyMath.ReadLinearEnemySpeed(speedTableOffset);
        (state.LeftVelocity, state.LeftSubvelocity) =
            EnemyMath.ReadLinearEnemySpeed(unchecked((ushort)(speedTableOffset + 4)));

        state.YMovement = PlatformVerticalMovement.Rising;
        state.PreviousYMovement = PlatformVerticalMovement.Rising;
        state.VerticallyMovingArtInstalled = false;
        state.VerticallyStillArtInstalled = false;
        state.TargetYPosition = unchecked((ushort)(slot.Position.Y.Whole + 1));
        state.YSpeedTableIndex = 0;
        state.MaximumYSpeedTableIndex = unchecked((ushort)(SnesLittleEndian.HighByte(slot.Parameter2)));
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

        MovePlatformHorizontally(slot, state, level);
        MovePlatformVertically(slot, state, samus, level);

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

    /// <summary>move platform horizontally.</summary>
    private void MovePlatformHorizontally(
        RoomEnemySlot slot,
        PlatformEnemyState state,
        RoomLevelData level)
    {
        state.PreviousPosition = slot.Position.X.Whole;
        bool movingLeft = state.XMovement switch
        {
            PlatformHorizontalMovement.Left => true,
            PlatformHorizontalMovement.Right => false,
            _ => throw new InvalidDataException(
                $"Platform X movement index {(ushort)state.XMovement} exceeds its two-entry table."),
        };
        int displacement = movingLeft ? state.LeftDisplacement : state.RightDisplacement;
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

    /// <summary>move platform vertically.</summary>
    private void MovePlatformVertically(
        RoomEnemySlot slot,
        PlatformEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        switch (state.YMovement)
        {
            case PlatformVerticalMovement.Rising:
                RaisePlatform(slot, state, level);
                return;
            case PlatformVerticalMovement.Sinking:
                SinkPlatform(slot, state, samus, level);
                return;
            default:
                throw new InvalidDataException(
                    $"Platform Y movement index {(ushort)state.YMovement} exceeds its two-entry table.");
        }
    }

    /// <summary>raise platform.</summary>
    private void RaisePlatform(
        RoomEnemySlot slot,
        PlatformEnemyState state,
        RoomLevelData level)
    {
        // Signed BMI comparison is intentional. Normal platforms stop once they have risen
        // from their depressed position to one pixel below their original spawn center.
        if (unchecked((short)(slot.Position.Y.Whole - state.TargetYPosition)) < 0)
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

    /// <summary>sink platform.</summary>
    private void SinkPlatform(
        RoomEnemySlot slot,
        PlatformEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        // `$A3:9DE7-$9DED` performs a comparison followed by a JSR to an immediate RTS; it
        // has no branch or side effect. Sinking therefore always continues to acceleration.
        IncrementAndClampPlatformYSpeed(state);

        // Horizontal movement saved the pre-move X position in the aliased word. Add the
        // wrapped whole-pixel delta to the shared producer word; fractional displacement is
        // deliberately untouched because this enemy writes only `$0B58`.
        ushort xDelta = unchecked((ushort)(slot.Position.X.Whole - state.PreviousPosition));
        samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(unchecked((short)(xDelta)), 0);

        state.PreviousPosition = slot.Position.Y.Whole;
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
        ushort yDelta = unchecked((ushort)(slot.Position.Y.Whole - state.PreviousPosition));
        samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(0, unchecked((short)(yDelta)));
    }

    /// <summary>increment and clamp platform yspeed.</summary>
    private static void IncrementAndClampPlatformYSpeed(PlatformEnemyState state)
    {
        state.YSpeedTableIndex = unchecked((ushort)(state.YSpeedTableIndex + 1));
        if (unchecked((short)(
                state.YSpeedTableIndex - state.MaximumYSpeedTableIndex)) >= 0)
        {
            state.YSpeedTableIndex = state.MaximumYSpeedTableIndex;
        }
    }

    /// <summary>set platform vertically moving instruction.</summary>
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

    /// <summary>set platform vertically still instruction.</summary>
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

    /// <summary>install platform vertically moving for current direction.</summary>
    internal static void InstallPlatformVerticallyMovingForCurrentDirection(
        RoomEnemySlot slot,
        PlatformEnemyState state)
    {
        ushort instruction = PlatformInstructionProgramDefinitions.SelectProgram(
            state.IsSuspensorPlatform, verticallyMoving: true, state.XMovement);
        InstallPlatformInstruction(slot, instruction);
    }

    /// <summary>install platform vertically still for current direction.</summary>
    internal static void InstallPlatformVerticallyStillForCurrentDirection(
        RoomEnemySlot slot,
        PlatformEnemyState state)
    {
        ushort instruction = PlatformInstructionProgramDefinitions.SelectProgram(
            state.IsSuspensorPlatform, verticallyMoving: false, state.XMovement);
        InstallPlatformInstruction(slot, instruction);
    }

    /// <summary>install platform instruction.</summary>
    private static void InstallPlatformInstruction(RoomEnemySlot slot, ushort instruction)
    {
        slot.CurrentInstruction = instruction;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    /// <summary>Ports the four equivalent list commands at $A3:9C6B/$9C76/$9C81/$9C8C.</summary>
    private void SetPlatformHorizontalMovementFromInstruction(
        RoomEnemySlot slot,
        PlatformHorizontalMovement movement)
    {
        RequirePlatformState(slot).XMovement = movement;
    }

    /// <summary>require platform state.</summary>
    private PlatformEnemyState RequirePlatformState(RoomEnemySlot slot) =>
        _platformStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Tripper/Kamer state.");

    /// <summary>register platforms bomb reactions.</summary>
    internal void RegisterPlatformsBombReactions(EnemyAiTable table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Tripper &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.TripperShot)
        {
            AfterCommon = c =>
            {
                if (c.Enemy.FrozenTimer != 0)
                    c.Enemy.SpritemapPointer = TripperKamerVisualDefinitions.FrozenFrame(RequirePlatformState(c.Enemy).XMovement);
            },
        });
    }

    /// <summary>register platforms initialization ai.</summary>
    internal void RegisterPlatformsInitializationAi(EnemyAiTable table)
    {

        EnemyInitializationAiHandler shared67 = (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializePlatform(slot);
        };

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Kamer2, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Kamer2, shared67);

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Tripper, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Tripper, shared67);
    }

    /// <summary>register platforms main ai.</summary>
    internal void RegisterPlatformsMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Tripper_Kamer2, slot => IsPlatformDefinition(slot.EnemyDefinitionPointer), (slot, frame, nmiFrameCounter8) =>
        {
            RunPlatformMain(slot, RequirePlatformState(slot), frame.Samus, frame.Level);
        });
    }

    /// <summary>register platforms owned instructions.</summary>
    internal void RegisterPlatformsOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessPlatformInstruction(slot, word, ref cursor);
        });
    }

    /// <summary>register platforms shot reactions.</summary>
    internal void RegisterPlatformsShotReactions(EnemyAiTable table)
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

    /// <summary>register platforms touch.</summary>
    internal static void RegisterPlatformsTouch(EnemyAiTable table)
    {
        // `$A3:9F07` is a literal RTL. Platform solidity and the asymmetric rider test live in other handlers;
        // ordinary body overlap must neither injure Samus nor synthesize knockback here.
        table.AddTouch(slot => IsPlatformDefinition(slot.EnemyDefinitionPointer) &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.PlatformNoOpTouch,
            static _ => { });
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        Array.Clear(_platformStates);
        Array.Clear(_platformMovementFunctions);
        Array.Clear(_platformPreviousPositions);
        Array.Clear(_platformVerticallyMovingFlags);
        Array.Clear(_platformVerticallyStillFlags);
        Array.Clear(_platformMaximumYSpeedTableIndexes);
        Array.Clear(_platformPreviousYMovementFunctions);
        Array.Clear(_platformSuspensorPlatformFlags);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }
}
