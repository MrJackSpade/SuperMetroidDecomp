using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The Etecoon species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class EtecoonBehavior : IEnemySpecies
{
    private readonly EnemyTerrain _terrain;
    private readonly RoomEarthquake _earthquake;

    private readonly ElevatorBehavior _elevatorBehavior;

    internal EtecoonBehavior(EnemyTerrain terrain, RoomEarthquake earthquake, ElevatorBehavior elevatorBehavior)
    {
        _elevatorBehavior = elevatorBehavior;
        _terrain = terrain;
        _earthquake = earthquake;
    }

    internal const ushort EtecoonWakeSound = 0x0035;
    private const ushort EtecoonJumpSound = 0x0033;
    private const ushort EtecoonWallSound = 0x0032;
    private const int EtecoonActivationYDistance = 0x80;
    private const int EtecoonNearbyDistance = 0x40;
    private const int EtecoonDirectionBand = 0x20;
    private const int EtecoonLongRunTriggerDistance = 0x30;
    private const int EtecoonRightWallProbe = 32 << 16;
    private const ushort EtecoonFirstTeachingX = 537;
    private const ushort EtecoonRunStartX = 600;
    private const ushort EtecoonLongJumpMidpointX = 680;
    private const ushort EtecoonLongJumpEndX = 840;
    private const ushort EtecoonLongRunMaximumX = 832;

    // These are the literal eight ROM words at $A7:E900-$E90F. Keeping them named beside
    // their consumers avoids replacing authored fixed-point values with host-side tuning.
    private const ushort EtecoonJumpYVelocity = 0xfffd;
    private const ushort EtecoonJumpYSubvelocity = 0x0000;
    private const ushort EtecoonLongJumpYVelocity = 0xfffc;
    private const ushort EtecoonLongJumpYSubvelocity = 0x0000;
    private const ushort EtecoonRightVelocity = 0x0002;
    private const ushort EtecoonRightSubvelocity = 0x0000;
    private const ushort EtecoonLeftVelocity = 0xfffe;
    private const ushort EtecoonLeftSubvelocity = 0x0000;

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
        slot.SpritemapPointer = 0x804d;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.CurrentInstruction = EtecoonInstructionProgramDefinitions.Initial;
        state.Function = EtecoonAiFunction.WaitingForSamus;
        state.FunctionTimer = 0xffff;
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
            slot.Parameter2 = unchecked((ushort)(slot.Parameter2 - 0x0100));
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
        if ((state.FunctionTimer & 0x8000) == 0)
        {
            if (DecrementEtecoonTimerAndTestExpired(state))
            {
                InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.HoppingFacingLeft);
                state.Function = EtecoonAiFunction.PreJumpCountdown;
                state.FunctionTimer = 11;
            }
            return;
        }

        if (!EtecoonIsWithinY(slot, samus, EtecoonActivationYDistance))
            return;

        if ((slot.Parameter2 & 3) == 0)
            LastEtecoonSoundEffect = EtecoonWakeSound;
        InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.Flexing);
        state.FunctionTimer = 256;
    }

    private void RunEtecoonPreJumpCountdown(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus)
    {
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity, EtecoonJumpYSubvelocity);
        slot.CurrentInstruction = unchecked((ushort)(slot.CurrentInstruction + 2));
        slot.InstructionTimer = 1;
        state.Function = EtecoonAiFunction.InitialJump;
        if (unchecked((short)(samus.Position.X.Whole - 256)) >= 0)
            LastEtecoonSoundEffect = EtecoonJumpSound;
    }

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
                    InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.LookRightAtSamusAndRunLeft);
                    slot.Parameter1 = 0;
                }
                else
                {
                    InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.LookLeftAtSamusAndRunRight);
                    slot.Parameter1 = 1;
                }
                state.FunctionTimer = 32;
                state.Function = EtecoonAiFunction.FaceSamusPause;
            }
            else
            {
                state.FunctionTimer = 11;
                state.Function = EtecoonAiFunction.PreJumpCountdown;
                InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.HoppingFacingLeft);
            }
            return;
        }

        SetEtecoonVerticalVelocity(state, 0, 0);
        slot.InstructionTimer = 3;
        slot.CurrentInstruction = EtecoonInstructionProgramDefinitions.HitCeiling;
    }

    private static void RunEtecoonFaceSamusPause(
        RoomEnemySlot slot,
        EtecoonEnemyState state)
    {
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        slot.CurrentInstruction = unchecked((ushort)(slot.CurrentInstruction + 2));
        slot.InstructionTimer = 1;
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

    private void RunEtecoonLeftToWall(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        if (!MoveEtecoonHorizontally(slot, state, level))
            return;

        SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity, EtecoonRightSubvelocity);
        state.Function = EtecoonAiFunction.RunRightToWall;
        InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.RunningRight);
        slot.Parameter1 = 1;
    }

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
            InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.JumpingRight);
            state.Function = EtecoonAiFunction.Airborne;
            return;
        }

        MoveEtecoonHorizontally(slot, state, level);
    }

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
                InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.WallJumpLeftEligible);
                slot.Parameter1 = 0;
            }
            else
            {
                InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.WallJumpRightEligible);
                slot.Parameter1 = 1;
            }
            state.Function = EtecoonAiFunction.WallPause;
            state.FunctionTimer = 8;
            if (unchecked((short)(samus.Position.X.Whole - 256)) >= 0)
                LastEtecoonSoundEffect = EtecoonWallSound;
            return;
        }

        if (!MoveEtecoonVertically(slot, state, samus, level))
            return;

        InstallEtecoonInstruction(
            slot,
            slot.Parameter1 != 0
                ? EtecoonInstructionProgramDefinitions.HoppingFacingLeft
                : EtecoonInstructionProgramDefinitions.HoppingFacingRight);
        state.FunctionTimer = 11;
        state.Function = EtecoonAiFunction.RouteAfterLanding;
        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity, EtecoonJumpYSubvelocity);
    }

    private void RunEtecoonWallPause(RoomEnemySlot slot, EtecoonEnemyState state)
    {
        PauseEtecoonForEarthquake(slot);
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        if (slot.Parameter1 != 0)
        {
            InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.WallJumpRight);
            SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity, EtecoonRightSubvelocity);
        }
        else
        {
            InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.WallJumpLeft);
            SetEtecoonHorizontalVelocity(state, EtecoonLeftVelocity, EtecoonLeftSubvelocity);
        }
        state.Function = EtecoonAiFunction.Airborne;
        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity, EtecoonJumpYSubvelocity);
    }

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
                slot.CurrentInstruction = EtecoonInstructionProgramDefinitions.RunningLeft;
                SetEtecoonHorizontalVelocity(state, EtecoonLeftVelocity, EtecoonLeftSubvelocity);
                break;
            case 1:
                state.Function = EtecoonAiFunction.MoveRightToTeachingStart;
                slot.CurrentInstruction = EtecoonInstructionProgramDefinitions.RunningRight;
                SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity, EtecoonRightSubvelocity);
                break;
            case 2:
                state.Function = EtecoonAiFunction.TeachingJump;
                slot.CurrentInstruction = unchecked((ushort)(slot.CurrentInstruction + 2));
                SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity, EtecoonRightSubvelocity);
                break;
            default:
                throw new InvalidDataException(
                    $"Etecoon route {(byte)slot.Parameter2} exceeds the retail three-entry table.");
        }
        slot.InstructionTimer = 1;
    }

    private void RunEtecoonMoveLeftToTeachingStart(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        if (unchecked((short)(slot.Position.X.Whole - EtecoonFirstTeachingX)) >= 0)
            return;

        state.FunctionTimer = 11;
        state.Function = EtecoonAiFunction.IdleBetweenTeachingJumps;
        InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.HoppingFacingLeft);
    }

    private void RunEtecoonMoveRightToTeachingStart(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        if (unchecked((short)(slot.Position.X.Whole - EtecoonRunStartX)) < 0)
            return;

        state.FunctionTimer = 11;
        state.Function = EtecoonAiFunction.IdleBetweenTeachingJumps;
        InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.HoppingFacingLeft);
    }

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
        InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.JumpingRight);
    }

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

        InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.RunningRight);
        state.Function = EtecoonAiFunction.RunRightAfterLongJump;
    }

    private void RunEtecoonRightAfterLongJump(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        if (unchecked((short)(slot.Position.X.Whole - EtecoonLongJumpEndX)) < 0)
            return;

        InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.JumpingRight);
        state.Function = EtecoonAiFunction.LongJumpLanding;
        SetEtecoonVerticalVelocity(state, 0xffff, EtecoonLongJumpYSubvelocity);
    }

    private void RunEtecoonLongJumpLanding(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        if (!MoveEtecoonVertically(slot, state, samus, level))
            return;

        state.FunctionTimer = 11;
        InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.HoppingFacingLeft);
        state.Function = EtecoonAiFunction.IdleBetweenTeachingJumps;
    }

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
            state.FunctionTimer = 11;
            InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.HoppingFacingLeft);
            state.Function = (slot.Parameter2 & 2) != 0 &&
                unchecked((short)(slot.Position.X.Whole - EtecoonLongRunMaximumX)) < 0
                    ? EtecoonAiFunction.WaitForSamusBeforeLongRun
                    : EtecoonAiFunction.IdleBetweenTeachingJumps;
            return;
        }

        SetEtecoonVerticalVelocity(state, 0, 0);
        slot.InstructionTimer = 3;
        slot.CurrentInstruction = EtecoonInstructionProgramDefinitions.HitCeiling;
    }

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
        slot.Parameter1 = unchecked((ushort)(slot.Parameter1 + 0x0100));
        bool fewerThanFourTeachingJumps =
            unchecked((short)((SnesLittleEndian.HighByteInPlace(slot.Parameter1)) - 0x0400)) < 0;
        if (!fewerThanFourTeachingJumps)
        {
            // `$A7:EDFD` clears the completed high-byte counter before testing route two.
            // Route two therefore loops in groups of four rather than overflowing forever.
            slot.Parameter1 = (byte)slot.Parameter1;
        }
        if (fewerThanFourTeachingJumps || (slot.Parameter2 & 2) != 0)
        {
            state.Function = EtecoonAiFunction.TeachingJump;
            slot.CurrentInstruction = unchecked((ushort)(slot.CurrentInstruction + 2));
        }
        else
        {
            state.Function = EtecoonAiFunction.RunRightToReturnJump;
            slot.CurrentInstruction = EtecoonInstructionProgramDefinitions.RunningRight;
            SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity, EtecoonRightSubvelocity);
        }
        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity, EtecoonJumpYSubvelocity);
        slot.InstructionTimer = 1;
        if (unchecked((short)(samus.Position.X.Whole - 256)) >= 0)
            LastEtecoonSoundEffect = EtecoonJumpSound;
    }

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
            slot.CurrentInstruction = EtecoonInstructionProgramDefinitions.RunningRight;
            state.Function = EtecoonAiFunction.RunRightToLongJump;
        }
        else
        {
            SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity, EtecoonJumpYSubvelocity);
            slot.CurrentInstruction = unchecked((ushort)(slot.CurrentInstruction + 2));
            state.Function = EtecoonAiFunction.TeachingJump;
            if (unchecked((short)(samus.Position.X.Whole - 256)) >= 0)
                LastEtecoonSoundEffect = EtecoonJumpSound;
        }
        slot.InstructionTimer = 1;
    }

    private void RunEtecoonRightToReturnJump(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        if (unchecked((short)(slot.Position.X.Whole - EtecoonRunStartX)) < 0)
            return;

        state.Function = EtecoonAiFunction.ReturnJump;
        InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.JumpingRight);
    }

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
        InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.RunningLeft);
    }

    /// <summary>Ports <c>Etecoon_Func_1</c>'s quake-owned parameter and timer writes.</summary>
    private void PauseEtecoonForEarthquake(RoomEnemySlot slot)
    {
        if (_earthquake.EarthquakeTimer == 0)
            return;

        slot.Parameter2 = unchecked((ushort)((byte)slot.Parameter2 | 0x8000));
        slot.InstructionTimer = unchecked((ushort)(slot.InstructionTimer + 128));
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

    private static void AddEtecoonFixed(
        EtecoonEnemyState state,
        ushort wholeIncrement,
        ushort fractionIncrement)
    {
        SnesSignedSixteenSixteen increment =
            SnesSignedSixteenSixteen.FromParts(unchecked((short)wholeIncrement), fractionIncrement);
        state.Velocity = state.Velocity.WithY(state.Velocity.Y + increment);
    }

    private static void SetEtecoonVerticalVelocity(
        EtecoonEnemyState state,
        ushort whole,
        ushort fraction)
    {
        state.Velocity = state.Velocity.WithY(SnesSignedSixteenSixteen.FromParts(unchecked((short)whole), fraction));
    }

    private static void SetEtecoonHorizontalVelocity(
        EtecoonEnemyState state,
        ushort whole,
        ushort fraction)
    {
        state.Velocity = state.Velocity.WithX(SnesSignedSixteenSixteen.FromParts(unchecked((short)whole), fraction));
    }

    private static bool DecrementEtecoonTimerAndTestExpired(EtecoonEnemyState state)
    {
        bool wasOne = state.FunctionTimer == 1;
        state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
        return wasOne || (state.FunctionTimer & 0x8000) != 0;
    }

    private static bool EtecoonIsWithinX(
        RoomEnemySlot slot,
        SamusState samus,
        int distance) =>
        Math.Abs(unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole))) < distance;

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

    private static void InstallEtecoonInstruction(
        RoomEnemySlot slot,
        ushort instructionPointer)
    {
        slot.CurrentInstruction = instructionPointer;
        slot.InstructionTimer = 1;
    }

    private EtecoonEnemyState RequireEtecoonState(RoomEnemySlot slot) =>
        _etecoonStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Etecoon state.");

    internal void RegisterEtecoonInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Etecoon, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Etecoon, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeEtecoon(slot);
        });
    }

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
            maximumQueued: LastEtecoonSoundEffect == EtecoonWakeSound ? (byte)15 : (byte)6);
    }
}
