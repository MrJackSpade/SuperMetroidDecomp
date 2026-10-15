using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>The Etecoon species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class EtecoonBehavior : IEnemySpecies
{
    /// <summary>Probes the floor, walls, and ceiling throughout the teaching route.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Pauses instruction timing while a room earthquake is active.</summary>
    private readonly RoomEarthquake _earthquake;

    /// <summary>Suppresses the pre-jump transition while an elevator door transition owns the room.</summary>
    private readonly ElevatorBehavior _elevatorBehavior;

    /// <summary>Binds the terrain, quake, and elevator owners consulted by Etecoon AI.</summary>
    internal EtecoonBehavior(EnemyTerrain terrain, RoomEarthquake earthquake, ElevatorBehavior elevatorBehavior)
    {
        _elevatorBehavior = elevatorBehavior;
        _terrain = terrain;
        _earthquake = earthquake;
    }

    /// <summary>Vertical distance within which Samus can wake a dormant Etecoon.</summary>
    private const int EtecoonActivationYDistance = 128;
    /// <summary>Strict proximity radius used before the initial Samus-facing pause.</summary>
    private const int EtecoonNearbyDistance = 64;
    /// <summary>Axis band used to choose cardinal rather than diagonal facing.</summary>
    private const int EtecoonDirectionBand = 32;
    /// <summary>Horizontal range that lets Samus trigger the long demonstration run.</summary>
    private const int EtecoonLongRunTriggerDistance = 48;
    /// <summary>Fixed-point lookahead distance for the right wall probe.</summary>
    private const int EtecoonRightWallProbe = 32 << 16;
    /// <summary>Room X where the left teaching route stops and begins hopping.</summary>
    private const ushort EtecoonFirstTeachingX = 537;
    /// <summary>Room X where the right teaching route begins a jump or return.</summary>
    private const ushort EtecoonRunStartX = 600;
    /// <summary>Room X where the long jump switches back to running art.</summary>
    private const ushort EtecoonLongJumpMidpointX = 680;
    /// <summary>Room X where the long route installs its landing jump.</summary>
    private const ushort EtecoonLongJumpEndX = 840;
    /// <summary>Last X that permits the alternate wait before another teaching jump.</summary>
    private const ushort EtecoonLongRunMaximumX = 832;

    // These are the literal eight ROM words at $A7:E900-$E90F. Keeping them named beside
    // their consumers avoids replacing authored fixed-point values with host-side tuning.
    /// <summary>Signed whole Y speed for an ordinary Etecoon hop.</summary>
    private const ushort EtecoonJumpYVelocity = unchecked((ushort)-3);
    /// <summary>Fractional Y speed paired with an ordinary hop.</summary>
    private const ushort EtecoonJumpYSubvelocity = 0;
    /// <summary>Signed whole Y speed for the stronger long jump.</summary>
    private const ushort EtecoonLongJumpYVelocity = unchecked((ushort)-4);
    /// <summary>Fractional Y speed paired with the long jump.</summary>
    private const ushort EtecoonLongJumpYSubvelocity = 0;
    /// <summary>Whole X speed used by rightward teaching runs.</summary>
    private const ushort EtecoonRightVelocity = 2;
    /// <summary>Fractional X speed paired with rightward runs.</summary>
    private const ushort EtecoonRightSubvelocity = 0;
    /// <summary>Signed whole X speed used by leftward teaching runs.</summary>
    private const ushort EtecoonLeftVelocity = unchecked((ushort)-2);
    /// <summary>Fractional X speed paired with leftward runs.</summary>
    private const ushort EtecoonLeftSubvelocity = 0;

    /// <summary>Per-slot route function, velocity, and timer state cleared on room load.</summary>
    internal readonly EtecoonEnemyState?[] _etecoonStates =
        new EtecoonEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Most recent library-two Etecoon sound request during this frame.</summary>
    public ushort? LastEtecoonSoundEffect { get; private set; }

    /// <summary>Ports <c>Etecoon_Init</c> at <c>$A7:E912</c>.</summary>
    internal void InitializeEtecoon(RoomEnemySlot slot)
    {
        var state = new EtecoonEnemyState(slot);
        _etecoonStates[slot.SlotIndex] = state;

        // Native ORs property $2000 (`DisableSamusColl`). The host scheduler also uses the
        // same raw bit to admit the ROM instruction interpreter, so retain the authored
        // bit instead of inferring animation activity from the population record.
        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.SpritemapPointer = CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap;
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        state.Program = AnimationProgramCursor<EtecoonProgram>.Start(EtecoonProgram.Initial);
        state.Function = EtecoonAiFunction.WaitingForSamus;
        state.FunctionTimer = NativeCountdown.Start(0xffff);
    }

    /// <summary>Ports <c>Etecoon_Main</c> and its indirect function dispatcher.</summary>
    private void RunEtecoonMain(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        // The high byte of parameter two doubles as a native 128-frame quake suspension.
        // Its low byte is the actor's permanent route (zero, one, or two) and must survive.
        if ((SnesLittleEndian.HighByteInPlace(slot.Parameter2)) != 0)
        {
            slot.Parameter2 = unchecked((ushort)(slot.Parameter2 - 256));
            return;
        }

        SamusState activeSamus = samus ?? throw new InvalidOperationException(
            "Etecoon AI requires the active Samus actor.");
        RoomLevelData activeLevel = level ?? throw new InvalidOperationException(
            "Etecoon AI requires active room collision data.");

        switch (state.Function)
        {
            case EtecoonAiFunction.WaitingForSamus:
                RunEtecoonWaiting(slot, state, activeSamus);
                break;
            case EtecoonAiFunction.PreJumpCountdown:
                RunEtecoonPreJumpCountdown(slot, state, activeSamus);
                break;
            case EtecoonAiFunction.InitialJump:
                RunEtecoonInitialJump(slot, state, activeSamus, activeLevel);
                break;
            case EtecoonAiFunction.FaceSamusPause:
                RunEtecoonFaceSamusPause(slot, state);
                break;
            case EtecoonAiFunction.RunLeftToWall:
                RunEtecoonLeftToWall(slot, state, activeLevel);
                break;
            case EtecoonAiFunction.RunRightToWall:
                RunEtecoonRightToWall(slot, state, activeLevel);
                break;
            case EtecoonAiFunction.Airborne:
                RunEtecoonAirborne(slot, state, activeSamus, activeLevel);
                break;
            case EtecoonAiFunction.WallPause:
                RunEtecoonWallPause(slot, state);
                break;
            case EtecoonAiFunction.RouteAfterLanding:
                RunEtecoonRouteAfterLanding(slot, state);
                break;
            case EtecoonAiFunction.MoveLeftToTeachingStart:
                RunEtecoonMoveLeftToTeachingStart(slot, state, activeLevel);
                break;
            case EtecoonAiFunction.MoveRightToTeachingStart:
                RunEtecoonMoveRightToTeachingStart(slot, state, activeLevel);
                break;
            case EtecoonAiFunction.RunRightToLongJump:
                RunEtecoonRightToLongJump(slot, state, activeLevel);
                break;
            case EtecoonAiFunction.LongJump:
                RunEtecoonLongJump(slot, state, activeSamus, activeLevel);
                break;
            case EtecoonAiFunction.RunRightAfterLongJump:
                RunEtecoonRightAfterLongJump(slot, state, activeLevel);
                break;
            case EtecoonAiFunction.LongJumpLanding:
                RunEtecoonLongJumpLanding(slot, state, activeSamus, activeLevel);
                break;
            case EtecoonAiFunction.TeachingJump:
                RunEtecoonTeachingJump(slot, state, activeSamus, activeLevel);
                break;
            case EtecoonAiFunction.IdleBetweenTeachingJumps:
                RunEtecoonIdleBetweenTeachingJumps(slot, state, activeSamus);
                break;
            case EtecoonAiFunction.WaitForSamusBeforeLongRun:
                RunEtecoonWaitForSamusBeforeLongRun(slot, state, activeSamus);
                break;
            case EtecoonAiFunction.RunRightToReturnJump:
                RunEtecoonRightToReturnJump(slot, state, activeLevel);
                break;
            case EtecoonAiFunction.ReturnJump:
                RunEtecoonReturnJump(slot, state, activeSamus, activeLevel);
                break;
            default:
                throw new InvalidDataException(
                    $"Etecoon function $A7:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports the dormant/wake loop at <c>$A7:E9AF</c>.</summary>
    private void RunEtecoonWaiting(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus)
    {
        // Destination OAM construction runs AI while door-transition audio is disabled.
        // Preserve the initial wake edge and the countdown until the native pause clears.
        if (_elevatorBehavior.ElevatorDoorTransitionActive)
            return;
        if (!state.FunctionTimer.IsNegative)
        {
            if (DecrementEtecoonTimerAndTestExpired(state))
            {
                InstallEtecoonInstruction(slot, state, EtecoonProgram.HoppingFacingLeft);
                state.Function = EtecoonAiFunction.PreJumpCountdown;
                state.FunctionTimer = NativeCountdown.Start(11);
            }
            return;
        }

        if (!EtecoonIsWithinY(slot, samus, EtecoonActivationYDistance))
            return;

        if ((slot.Parameter2 & 3) == 0)
            LastEtecoonSoundEffect = EnemySoundEffectDefinitions.EtecoonWakeSound;
        InstallEtecoonInstruction(slot, state, EtecoonProgram.Flexing);
        state.FunctionTimer = NativeCountdown.Start(256);
    }

    /// <summary>Expires the wake pause, installs the first jump speed, and gates its sound by Samus's X.</summary>
    private void RunEtecoonPreJumpCountdown(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus)
    {
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity, EtecoonJumpYSubvelocity);
        state.Program = AnimationProgramCursor<EtecoonProgram>.Start(EtecoonAnimations.AfterSleep(state.Program.Program));
        slot.InstructionTimer = NativeCountdown.Start(1);
        state.Function = EtecoonAiFunction.InitialJump;
        if (unchecked((short)(samus.Position.X.Whole - 256)) >= 0)
            LastEtecoonSoundEffect = EnemySoundEffectDefinitions.EtecoonJumpSound;
    }

    /// <summary>Finishes the initial hop and either faces nearby Samus or schedules another hop.</summary>
    private void RunEtecoonInitialJump(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (!MoveEtecoonVertically(slot, state, samus, level))
            return;

        if (state.Velocity.Y.WholePixels >= 0)
        {
            if (EtecoonIsWithinY(slot, samus, EtecoonNearbyDistance) &&
                EtecoonIsWithinX(slot, samus, EtecoonNearbyDistance))
            {
                ushort direction = DetermineEtecoonDirectionToSamus(slot, samus);
                if (unchecked((short)(direction - 5)) < 0)
                {
                    InstallEtecoonInstruction(slot, state, EtecoonProgram.LookRightAtSamusAndRunLeft);
                    slot.Parameter1 = 0;
                }
                else
                {
                    InstallEtecoonInstruction(slot, state, EtecoonProgram.LookLeftAtSamusAndRunRight);
                    slot.Parameter1 = 1;
                }
                state.FunctionTimer = NativeCountdown.Start(32);
                state.Function = EtecoonAiFunction.FaceSamusPause;
            }
            else
            {
                state.FunctionTimer = NativeCountdown.Start(11);
                state.Function = EtecoonAiFunction.PreJumpCountdown;
                InstallEtecoonInstruction(slot, state, EtecoonProgram.HoppingFacingLeft);
            }
            return;
        }

        SetEtecoonVerticalVelocity(state, 0, 0);
        slot.InstructionTimer = NativeCountdown.Start(3);
        state.Program = AnimationProgramCursor<EtecoonProgram>.Start(EtecoonProgram.HitCeiling);
    }

    /// <summary>Waits 32 calls before choosing a left or right wall run from the stored facing bit.</summary>
    private static void RunEtecoonFaceSamusPause(
        RoomEnemySlot slot,
        EtecoonEnemyState state)
    {
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        state.Program = AnimationProgramCursor<EtecoonProgram>.Start(EtecoonAnimations.AfterSleep(state.Program.Program));
        slot.InstructionTimer = NativeCountdown.Start(1);
        if (slot.Parameter1 != 0)
        {
            SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity, EtecoonRightSubvelocity);
            state.Function = EtecoonAiFunction.RunRightToWall;
        }
        else
        {
            SetEtecoonHorizontalVelocity(state, EtecoonLeftVelocity, EtecoonLeftSubvelocity);
            state.Function = EtecoonAiFunction.RunLeftToWall;
        }
        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity, EtecoonJumpYSubvelocity);
    }

    /// <summary>Runs left until terrain collision, then reverses into the rightward teaching path.</summary>
    private void RunEtecoonLeftToWall(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        if (!MoveEtecoonHorizontally(slot, state, level))
            return;

        SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity, EtecoonRightSubvelocity);
        state.Function = EtecoonAiFunction.RunRightToWall;
        InstallEtecoonInstruction(slot, state, EtecoonProgram.RunningRight);
        slot.Parameter1 = 1;
    }

    /// <summary>Runs right until the 32-pixel solid lookahead arms the airborne wall-jump state.</summary>
    private void RunEtecoonRightToWall(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        if (EnemyTerrain.EnemyHasSolidHighBitHorizontallyAhead(
                level,
                slot,
                EtecoonRightWallProbe))
        {
            InstallEtecoonInstruction(slot, state, EtecoonProgram.JumpingRight);
            state.Function = EtecoonAiFunction.Airborne;
            return;
        }

        MoveEtecoonHorizontally(slot, state, level);
    }

    /// <summary>Handles wall bounce or floor landing during the alternating teaching hops.</summary>
    private void RunEtecoonAirborne(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        PauseEtecoonForEarthquake(slot);
        if (MoveEtecoonHorizontally(slot, state, level))
        {
            if (slot.Parameter1 != 0)
            {
                InstallEtecoonInstruction(slot, state, EtecoonProgram.WallJumpLeftEligible);
                slot.Parameter1 = 0;
            }
            else
            {
                InstallEtecoonInstruction(slot, state, EtecoonProgram.WallJumpRightEligible);
                slot.Parameter1 = 1;
            }
            state.Function = EtecoonAiFunction.WallPause;
            state.FunctionTimer = NativeCountdown.Start(8);
            if (unchecked((short)(samus.Position.X.Whole - 256)) >= 0)
                LastEtecoonSoundEffect = EnemySoundEffectDefinitions.EtecoonWallSound;
            return;
        }

        if (!MoveEtecoonVertically(slot, state, samus, level))
            return;

        InstallEtecoonInstruction(
            slot,
            state,
            slot.Parameter1 != 0
                ? EtecoonProgram.HoppingFacingLeft
                : EtecoonProgram.HoppingFacingRight);
        state.FunctionTimer = NativeCountdown.Start(11);
        state.Function = EtecoonAiFunction.RouteAfterLanding;
        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity, EtecoonJumpYSubvelocity);
    }

    /// <summary>Waits eight calls at a wall, then launches in the stored opposite direction.</summary>
    private void RunEtecoonWallPause(RoomEnemySlot slot, EtecoonEnemyState state)
    {
        PauseEtecoonForEarthquake(slot);
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        if (slot.Parameter1 != 0)
        {
            InstallEtecoonInstruction(slot, state, EtecoonProgram.WallJumpRight);
            SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity, EtecoonRightSubvelocity);
        }
        else
        {
            InstallEtecoonInstruction(slot, state, EtecoonProgram.WallJumpLeft);
            SetEtecoonHorizontalVelocity(state, EtecoonLeftVelocity, EtecoonLeftSubvelocity);
        }
        state.Function = EtecoonAiFunction.Airborne;
        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity, EtecoonJumpYSubvelocity);
    }

    /// <summary>Selects one of the three authored post-landing routes from parameter two.</summary>
    private static void RunEtecoonRouteAfterLanding(
        RoomEnemySlot slot,
        EtecoonEnemyState state)
    {
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity, EtecoonJumpYSubvelocity);
        switch ((byte)slot.Parameter2)
        {
            case 0:
                state.Function = EtecoonAiFunction.MoveLeftToTeachingStart;
                state.Program = AnimationProgramCursor<EtecoonProgram>.Start(EtecoonProgram.RunningLeft);
                SetEtecoonHorizontalVelocity(state, EtecoonLeftVelocity, EtecoonLeftSubvelocity);
                break;
            case 1:
                state.Function = EtecoonAiFunction.MoveRightToTeachingStart;
                state.Program = AnimationProgramCursor<EtecoonProgram>.Start(EtecoonProgram.RunningRight);
                SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity, EtecoonRightSubvelocity);
                break;
            case 2:
                state.Function = EtecoonAiFunction.TeachingJump;
                state.Program = AnimationProgramCursor<EtecoonProgram>.Start(EtecoonAnimations.AfterSleep(state.Program.Program));
                SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity, EtecoonRightSubvelocity);
                break;
            default:
                throw new InvalidDataException(
                    $"Etecoon route {(byte)slot.Parameter2} exceeds the retail three-entry table.");
        }
        slot.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>Runs to the left teaching marker before starting an idle hopping interval.</summary>
    private void RunEtecoonMoveLeftToTeachingStart(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        if (unchecked((short)(slot.Position.X.Whole - EtecoonFirstTeachingX)) >= 0)
            return;

        state.FunctionTimer = NativeCountdown.Start(11);
        state.Function = EtecoonAiFunction.IdleBetweenTeachingJumps;
        InstallEtecoonInstruction(slot, state, EtecoonProgram.HoppingFacingLeft);
    }

    /// <summary>Runs to the right teaching marker before starting an idle hopping interval.</summary>
    private void RunEtecoonMoveRightToTeachingStart(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        if (unchecked((short)(slot.Position.X.Whole - EtecoonRunStartX)) < 0)
            return;

        state.FunctionTimer = NativeCountdown.Start(11);
        state.Function = EtecoonAiFunction.IdleBetweenTeachingJumps;
        InstallEtecoonInstruction(slot, state, EtecoonProgram.HoppingFacingLeft);
    }

    /// <summary>Runs to X=600 and launches the stronger rightward demonstration jump.</summary>
    private void RunEtecoonRightToLongJump(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        if (unchecked((short)(slot.Position.X.Whole - EtecoonRunStartX)) < 0)
            return;

        state.Function = EtecoonAiFunction.LongJump;
        SetEtecoonVerticalVelocity(state, EtecoonLongJumpYVelocity, EtecoonLongJumpYSubvelocity);
        InstallEtecoonInstruction(slot, state, EtecoonProgram.JumpingRight);
    }

    /// <summary>Advances the long jump on both axes and returns to running art at X=680.</summary>
    private void RunEtecoonLongJump(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        MoveEtecoonVertically(slot, state, samus, level);
        if (unchecked((short)(slot.Position.X.Whole - EtecoonLongJumpMidpointX)) < 0)
            return;

        InstallEtecoonInstruction(slot, state, EtecoonProgram.RunningRight);
        state.Function = EtecoonAiFunction.RunRightAfterLongJump;
    }

    /// <summary>Continues the long route to X=840 before installing its landing hop.</summary>
    private void RunEtecoonRightAfterLongJump(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        if (unchecked((short)(slot.Position.X.Whole - EtecoonLongJumpEndX)) < 0)
            return;

        InstallEtecoonInstruction(slot, state, EtecoonProgram.JumpingRight);
        state.Function = EtecoonAiFunction.LongJumpLanding;
        SetEtecoonVerticalVelocity(state, unchecked((ushort)-1), EtecoonLongJumpYSubvelocity);
    }

    /// <summary>Waits for floor contact after the long route, then resumes the teaching-hop pause.</summary>
    private void RunEtecoonLongJumpLanding(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        if (!MoveEtecoonVertically(slot, state, samus, level))
            return;

        state.FunctionTimer = NativeCountdown.Start(11);
        InstallEtecoonInstruction(slot, state, EtecoonProgram.HoppingFacingLeft);
        state.Function = EtecoonAiFunction.IdleBetweenTeachingJumps;
    }

    /// <summary>Handles ceiling and floor contact for a demonstration hop, selecting another hop or Samus wait.</summary>
    private void RunEtecoonTeachingJump(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        PauseEtecoonForEarthquake(slot);
        if (!MoveEtecoonVertically(slot, state, samus, level))
            return;

        if (state.Velocity.Y.WholePixels >= 0)
        {
            state.FunctionTimer = NativeCountdown.Start(11);
            InstallEtecoonInstruction(slot, state, EtecoonProgram.HoppingFacingLeft);
            state.Function = (slot.Parameter2 & 2) != 0 &&
                unchecked((short)(slot.Position.X.Whole - EtecoonLongRunMaximumX)) < 0
                    ? EtecoonAiFunction.WaitForSamusBeforeLongRun
                    : EtecoonAiFunction.IdleBetweenTeachingJumps;
            return;
        }

        SetEtecoonVerticalVelocity(state, 0, 0);
        slot.InstructionTimer = NativeCountdown.Start(3);
        state.Program = AnimationProgramCursor<EtecoonProgram>.Start(EtecoonProgram.HitCeiling);
    }

    /// <summary>Counts teaching hops in parameter one's high byte and takes the return run after four on nonlooping routes.</summary>
    private void RunEtecoonIdleBetweenTeachingJumps(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus)
    {
        PauseEtecoonForEarthquake(slot);
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        // Parameter one's high byte counts four teaching hops. Native then clears only
        // that byte, preserving the low-byte facing selector used by the airborne states.
        slot.Parameter1 = unchecked((ushort)(slot.Parameter1 + 256));
        bool fewerThanFourTeachingJumps =
            unchecked((short)((SnesLittleEndian.HighByteInPlace(slot.Parameter1)) - 1024)) < 0;
        if (!fewerThanFourTeachingJumps)
        {
            // `$A7:EDFD` clears the completed high-byte counter before testing route two.
            // Route two therefore loops in groups of four rather than overflowing forever.
            slot.Parameter1 = (byte)slot.Parameter1;
        }
        if (fewerThanFourTeachingJumps || (slot.Parameter2 & 2) != 0)
        {
            state.Function = EtecoonAiFunction.TeachingJump;
            state.Program = AnimationProgramCursor<EtecoonProgram>.Start(EtecoonAnimations.AfterSleep(state.Program.Program));
        }
        else
        {
            state.Function = EtecoonAiFunction.RunRightToReturnJump;
            state.Program = AnimationProgramCursor<EtecoonProgram>.Start(EtecoonProgram.RunningRight);
            SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity, EtecoonRightSubvelocity);
        }
        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity, EtecoonJumpYSubvelocity);
        slot.InstructionTimer = NativeCountdown.Start(1);
        if (unchecked((short)(samus.Position.X.Whole - 256)) >= 0)
            LastEtecoonSoundEffect = EnemySoundEffectDefinitions.EtecoonJumpSound;
    }

    /// <summary>Starts the long run when Samus approaches; otherwise schedules another teaching hop.</summary>
    private void RunEtecoonWaitForSamusBeforeLongRun(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus)
    {
        PauseEtecoonForEarthquake(slot);
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        if (EtecoonIsWithinY(slot, samus, EtecoonNearbyDistance) &&
            EtecoonIsWithinX(slot, samus, EtecoonLongRunTriggerDistance))
        {
            state.Program = AnimationProgramCursor<EtecoonProgram>.Start(EtecoonProgram.RunningRight);
            state.Function = EtecoonAiFunction.RunRightToLongJump;
        }
        else
        {
            SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity, EtecoonJumpYSubvelocity);
            state.Program = AnimationProgramCursor<EtecoonProgram>.Start(EtecoonAnimations.AfterSleep(state.Program.Program));
            state.Function = EtecoonAiFunction.TeachingJump;
            if (unchecked((short)(samus.Position.X.Whole - 256)) >= 0)
                LastEtecoonSoundEffect = EnemySoundEffectDefinitions.EtecoonJumpSound;
        }
        slot.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>Runs to the return marker and installs the rightward jump back toward the wall route.</summary>
    private void RunEtecoonRightToReturnJump(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        if (unchecked((short)(slot.Position.X.Whole - EtecoonRunStartX)) < 0)
            return;

        state.Function = EtecoonAiFunction.ReturnJump;
        InstallEtecoonInstruction(slot, state, EtecoonProgram.JumpingRight);
    }

    /// <summary>Completes the return jump and restores leftward wall-running speed on landing.</summary>
    private void RunEtecoonReturnJump(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        if (!MoveEtecoonVertically(slot, state, samus, level))
            return;

        SetEtecoonHorizontalVelocity(state, EtecoonLeftVelocity, EtecoonLeftSubvelocity);
        state.Function = EtecoonAiFunction.RunLeftToWall;
        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity, EtecoonJumpYSubvelocity);
        InstallEtecoonInstruction(slot, state, EtecoonProgram.RunningLeft);
    }

    /// <summary>Ports <c>Etecoon_Func_1</c>'s quake-owned parameter and timer writes.</summary>
    private void PauseEtecoonForEarthquake(RoomEnemySlot slot)
    {
        if (_earthquake.EarthquakeTimer.IsZero)
            return;

        slot.Parameter2 = unchecked((ushort)((byte)slot.Parameter2 | 0x8000));
        slot.InstructionTimer = NativeCountdown.FromRaw(unchecked((ushort)(slot.InstructionTimer.RawValue + 128)));
    }

    /// <summary>Ports <c>Etecoon_Func_2</c>'s signed 16.16 horizontal mover.</summary>
    private bool MoveEtecoonHorizontally(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level) =>
        _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level,
            slot,
            state.Velocity.X.RawValue);

    /// <summary>Ports <c>Etecoon_Func_3</c>, including its five-pixel gravity cap.</summary>
    private bool MoveEtecoonVertically(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        int displacement = state.Velocity.Y.RawValue;
        if (unchecked((short)(state.Velocity.Y.WholeWord - 5)) < 0)
        {
            AddEtecoonFixed(
                state,
                samus.Kinematics.YAcceleration,
                samus.Kinematics.YSubacceleration);
        }
        return _terrain.MoveEnemyVertically(level, slot, displacement);
    }

    /// <summary>Adds a signed 16.16 whole/fraction increment to the Etecoon's vertical speed.</summary>
    private static void AddEtecoonFixed(
        EtecoonEnemyState state,
        ushort wholeIncrement,
        ushort fractionIncrement)
    {
        SnesSignedSixteenSixteen increment =
            SnesSignedSixteenSixteen.FromParts(unchecked((short)wholeIncrement), fractionIncrement);
        state.Velocity = state.Velocity.WithY(state.Velocity.Y + increment);
    }

    /// <summary>Installs a native signed whole/fraction Y speed for the next movement phase.</summary>
    private static void SetEtecoonVerticalVelocity(
        EtecoonEnemyState state,
        ushort whole,
        ushort fraction)
    {
        state.Velocity = state.Velocity.WithY(SnesSignedSixteenSixteen.FromParts(unchecked((short)whole), fraction));
    }

    /// <summary>Installs a native signed whole/fraction X speed for the next movement phase.</summary>
    private static void SetEtecoonHorizontalVelocity(
        EtecoonEnemyState state,
        ushort whole,
        ushort fraction)
    {
        state.Velocity = state.Velocity.WithX(SnesSignedSixteenSixteen.FromParts(unchecked((short)whole), fraction));
    }

    /// <summary>Uses native post-decrement semantics, treating a one-to-zero or signed underflow as expiry.</summary>
    private static bool DecrementEtecoonTimerAndTestExpired(EtecoonEnemyState state)
    {
        bool wasOne = state.FunctionTimer.RawValue == 1;
        state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
        return wasOne || state.FunctionTimer.IsNegative;
    }

    /// <summary>Checks strict wrapped horizontal distance from an Etecoon to Samus.</summary>
    private static bool EtecoonIsWithinX(
        RoomEnemySlot slot,
        SamusState samus,
        int distance) =>
        Math.Abs(unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole))) < distance;

    /// <summary>Checks strict wrapped vertical distance from an Etecoon to Samus.</summary>
    private static bool EtecoonIsWithinY(
        RoomEnemySlot slot,
        SamusState samus,
        int distance) =>
        Math.Abs(unchecked((short)(slot.Position.Y.Whole - samus.Position.Y.Whole))) < distance;

    /// <summary>Ports <c>DetermineDirectionOfSamusFromEnemy</c> for the facing branch.</summary>
    private static ushort DetermineEtecoonDirectionToSamus(
        RoomEnemySlot slot,
        SamusState samus)
    {
        short deltaX = unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole));
        short deltaY = unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole));
        if (Math.Abs(deltaY) < EtecoonDirectionBand)
            return deltaX < 0 ? (ushort)7 : (ushort)2;
        if (Math.Abs(deltaX) < EtecoonDirectionBand)
            return deltaY < 0 ? (ushort)0 : (ushort)4;
        if (deltaX < 0)
            return deltaY < 0 ? (ushort)8 : (ushort)6;
        return deltaY < 0 ? (ushort)1 : (ushort)3;
    }

    /// <summary>Restarts an authored Etecoon animation at timer one.</summary>
    private static void InstallEtecoonInstruction(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        EtecoonProgram program)
    {
        state.Program = AnimationProgramCursor<EtecoonProgram>.Start(program);
        slot.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>Finds initialized state for a live Etecoon or rejects an unrelated slot callback.</summary>
    private EtecoonEnemyState RequireEtecoonState(RoomEnemySlot slot) =>
        _etecoonStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Etecoon state.");

    /// <summary>Runs the Etecoon's animations as typed programs (#641).</summary>
    internal void RegisterEtecoonTypedAnimations(EnemyAiTable table) =>
        table.AddTypedAnimation(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Etecoon, (slot, _) =>
        {
            if (!EnemyProgramAnimation.Expire(slot))
                return;
            var state = RequireEtecoonState(slot);
            AnimationProgramCursor<EtecoonProgram> cursor = state.Program;
            EnemyProgramAnimation.Run(slot, EtecoonAnimations.Programs, ref cursor, EtecoonFrameDefinitions.Spritemap);
            state.Program = cursor;
        });

    /// <summary>Registers the bank-$A7 Etecoon initializer for its enemy definition.</summary>
    internal void RegisterEtecoonInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Etecoon, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Etecoon, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeEtecoon(slot);
        });
    }

    /// <summary>Registers the Etecoon function dispatcher under its native main AI identity.</summary>
    internal void RegisterEtecoonMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Etecoon, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Etecoon, (slot, frame, nmiFrameCounter8) =>
        {
            RunEtecoonMain(slot, RequireEtecoonState(slot), frame.Samus, frame.Level);
        });
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        LastEtecoonSoundEffect = null;
        Array.Clear(_etecoonStates);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastEtecoonSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastEtecoonSoundEffect, library: SoundEffectLibrary.Library2,
            maximumQueued: LastEtecoonSoundEffect == EnemySoundEffectDefinitions.EtecoonWakeSound ? (byte)15 : (byte)6);
    }
}
