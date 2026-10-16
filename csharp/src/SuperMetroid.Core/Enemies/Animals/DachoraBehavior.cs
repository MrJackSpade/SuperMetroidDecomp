using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>The Dachora species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class DachoraBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Dachora];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new DachoraEntity(this, slot, common);

    /// <summary>Provides the room bus and active context for Dachora's native AI.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Probes floors, walls, and ceiling during the run and shinespark arc.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Provides the four physical echo slots immediately after Dachora's body.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Supplies installed palette art for speed and shine color cycles.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Binds the room, terrain, echo slots, and presentation used by Dachora's AI callbacks.</summary>
    internal DachoraBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemySlotTable slotTable, EnemyPresentation presentation)
    {
        _room = room;
        _terrain = terrain;
        _slotTable = slotTable;
        _presentation = presentation;
    }

    /// <summary>Native $A7:F4C9 horizontal proximity threshold for activating the bird.</summary>
    private const int DachoraActivationXDistance = 96;
    /// <summary>Native $A7:F4CB vertical proximity threshold for activating the bird.</summary>
    private const int DachoraActivationYDistance = 64;
    /// <summary>Native warning interval before Dachora starts its leftward run.</summary>
    private const ushort DachoraBlinkDuration = 120;
    /// <summary>Native stored-shine countdown before Dachora launches.</summary>
    private const ushort DachoraChargeDuration = 60;
    /// <summary>Accepted-frame interval between echo position captures.</summary>
    private const ushort DachoraEchoPositionInterval = 1;
    /// <summary>Accepted-frame lifetime of each Dachora shine echo.</summary>
    private const ushort DachoraEchoLifetime = 8;
    /// <summary>Whole half of the bird's native 16.16 run speed cap.</summary>
    private const ushort DachoraMaximumSpeedWhole = 8;
    /// <summary>Fraction half of the bird's native 16.16 run speed cap.</summary>
    private const ushort DachoraMaximumSpeedFraction = 0;
    /// <summary>Whole half of Dachora's native 16.16 running acceleration.</summary>
    private const ushort DachoraAccelerationWhole = 0;
    /// <summary>Fraction half of Dachora's native 16.16 running acceleration.</summary>
    private const ushort DachoraAccelerationFraction = 4096;
    /// <summary>Scripted room X where Dachora reverses its leftward run.</summary>
    private const ushort DachoraLeftTurnX = 96;
    /// <summary>Scripted room X where Dachora begins its stored-shine demonstration.</summary>
    private const ushort DachoraShinesparkX = 1152;

    /// <summary>Most recent library-two Dachora sound request during this frame.</summary>
    public ushort? LastDachoraSoundEffect { get; private set; }

    /// <summary>Ports <c>InitAI_Dachora</c> at <c>$A7:F4DD</c>.</summary>
    internal static void InitializeDachora(RoomEnemySlot slot)
    {
        var state = new DachoraEnemyState(slot);
        slot.SetSpeciesState(state);

        // Native property $2000 both disables Samus collision and admits enemy bytecode.
        // Population property $0400 already excludes these actors from host interaction.
        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.SpritemapPointer = CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap;
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);

        short direction = unchecked((short)slot.Parameter1);
        if (direction < 0)
        {
            // A negative direction identifies an echo. Its low bit selects the same facing
            // convention as the body, while variable D later becomes its eight-frame life.
            state.Program = AnimationProgramCursor<DachoraProgram>.Start((direction & 1) != 0
                ? DachoraProgram.EchoRight
                : DachoraProgram.EchoLeft);
            state.Function = DachoraAiFunction.Echo;
            return;
        }

        state.Program = AnimationProgramCursor<DachoraProgram>.Start(direction != 0
            ? DachoraProgram.IdleRight
            : DachoraProgram.IdleLeft);
        state.Function = DachoraAiFunction.WaitingForSamus;
    }

    /// <summary>Ports <c>MainAI_Dachora</c>'s bank-local indirect jump.</summary>
    private void RunDachoraMain(
        RoomEnemySlot slot,
        DachoraEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        byte nmiFrameCounter8)
    {
        // Echo AI needs only the NMI parity bit. Every body state needs both the active
        // Samus gravity words and authentic room collision, so fail loudly if a caller
        // attempts to run the family without the state that native code reads globally.
        if (state.Function == DachoraAiFunction.Echo)
        {
            RunDachoraEcho(slot, state, nmiFrameCounter8);
            return;
        }

        SamusState activeSamus = samus ?? throw new InvalidOperationException(
            "Dachora body AI requires the active Samus actor.");
        RoomLevelData activeLevel = level ?? throw new InvalidOperationException(
            "Dachora body AI requires active room collision data.");
        switch (state.Function)
        {
            case DachoraAiFunction.WaitingForSamus:
                RunDachoraWaiting(slot, state, activeSamus, activeLevel);
                break;
            case DachoraAiFunction.BlinkingBeforeRun:
                RunDachoraBlinking(slot, state);
                break;
            case DachoraAiFunction.RunningLeft:
                RunDachoraLeft(slot, state, activeLevel);
                break;
            case DachoraAiFunction.RunningRight:
                RunDachoraRight(slot, state, activeLevel);
                break;
            case DachoraAiFunction.ChargingShinespark:
                RunDachoraCharging(slot, state);
                break;
            case DachoraAiFunction.Shinesparking:
                RunDachoraShinespark(slot, state, activeSamus, activeLevel);
                break;
            case DachoraAiFunction.Falling:
                RunDachoraFalling(slot, state, activeSamus, activeLevel);
                break;
            default:
                throw new InvalidDataException(
                    $"Dachora function $A7:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports $A7:F570, including the one-pixel floor probe before proximity.</summary>
    private void RunDachoraWaiting(
        RoomEnemySlot slot,
        DachoraEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        _terrain.MoveEnemyVertically(level, slot, 1 << 16);
        if (!DachoraWithinAxis(slot.Position.Y.Whole, samus.Position.Y.Whole, DachoraActivationYDistance) ||
            !DachoraWithinAxis(slot.Position.X.Whole, samus.Position.X.Whole, DachoraActivationXDistance))
        {
            return;
        }

        InstallDachoraInstruction(
            slot,
            state,
            slot.Parameter1 != 0
                ? DachoraProgram.BlinkRight
                : DachoraProgram.BlinkLeft);
        state.Function = DachoraAiFunction.BlinkingBeforeRun;
        state.PhaseTimer = NativeCountdown.Start(DachoraBlinkDuration);
        LastDachoraSoundEffect = EnemySoundEffectDefinitions.DachoraActivationSound;
    }

    /// <summary>Ports the 120-frame warning countdown at $A7:F5BC.</summary>
    private static void RunDachoraBlinking(RoomEnemySlot slot, DachoraEnemyState state)
    {
        state.PhaseTimer = state.PhaseTimer.Decrement().Remaining;
        if (!state.PhaseTimer.IsZero)
            return;

        InstallDachoraInstruction(
            slot,
            state,
            slot.Parameter1 != 0
                ? DachoraProgram.RunningRight
                : DachoraProgram.RunningLeft);
        state.Function = slot.Parameter1 != 0
            ? DachoraAiFunction.RunningRight
            : DachoraAiFunction.RunningLeft;
        state.PaletteAnimationTimer = 1;
    }

    /// <summary>Ports $A7:F5ED. Leftward speed is the exact two's complement of A.B.</summary>
    private void RunDachoraLeft(
        RoomEnemySlot slot,
        DachoraEnemyState state,
        RoomLevelData level)
    {
        int displacement = unchecked(-AccelerateDachora(slot, state, level));
        bool hitWall = _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level,
            slot,
            displacement);
        if (!hitWall)
            EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, slot);

        if (!hitWall && unchecked((short)(slot.Position.X.Whole - DachoraLeftTurnX)) >= 0)
            return;

        InstallDachoraInstruction(slot, state, DachoraProgram.RunningRight);
        state.Function = DachoraAiFunction.RunningRight;
        state.PaletteAnimationTimer = 1;
        slot.Parameter1 = 1;
        state.Speed = 0;
        state.Subspeed = 0;
        LoadDachoraPalette(slot, DachoraPalettePhase.Default, frame: 0);
    }

    /// <summary>Ports $A7:F65E, including the hard-coded tutorial launch coordinate.</summary>
    private void RunDachoraRight(
        RoomEnemySlot slot,
        DachoraEnemyState state,
        RoomLevelData level)
    {
        int displacement = AccelerateDachora(slot, state, level);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, displacement))
        {
            LastDachoraSoundEffect = EnemySoundEffectDefinitions.DachoraWallImpactSound;
            InstallDachoraInstruction(slot, state, DachoraProgram.RunningLeft);
            state.Function = DachoraAiFunction.RunningLeft;
            slot.Parameter1 = 0;
            state.Speed = 0;
            state.Subspeed = 0;
            state.PaletteAnimationTimer = 0;
            LoadDachoraPalette(slot, DachoraPalettePhase.Default, frame: 0);
            return;
        }

        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, slot);
        if (unchecked((short)(slot.Position.X.Whole - DachoraShinesparkX)) < 0)
            return;

        InstallDachoraInstruction(slot, state, DachoraProgram.ChargeRight);
        state.Function = DachoraAiFunction.ChargingShinespark;
        state.PhaseTimer = NativeCountdown.Start(DachoraChargeDuration);
        state.Subspeed = 0;
        state.PaletteAnimationTimer = 0;
        slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole + 8)));
        LastDachoraSoundEffect = EnemySoundEffectDefinitions.DachoraChargeSound;
    }

    /// <summary>
    /// Ports <c>AccelerateRunningDachora</c> at $A7:F6D5. This intentionally retains the
    /// strange packed palette timer: after frame three is reached, native code reloads that
    /// brightest speed palette every sixteen frames instead of wrapping to frame zero.
    /// </summary>
    internal int AccelerateDachora(
        RoomEnemySlot slot,
        DachoraEnemyState state,
        RoomLevelData level)
    {
        if (unchecked((short)(state.Speed - DachoraMaximumSpeedWhole)) >= 0)
        {
            if (state.PaletteAnimationTimer == 1)
                LastDachoraSoundEffect = EnemySoundEffectDefinitions.DachoraSpeedBoosterSound;

            state.PaletteAnimationTimer = NativeWordCounter.Decrement(state.PaletteAnimationTimer).Value;
            if ((byte)state.PaletteAnimationTimer == 0)
            {
                int paletteIndex = SnesLittleEndian.HighByte(state.PaletteAnimationTimer);
                if ((uint)paletteIndex >= 4)
                {
                    throw new InvalidDataException(
                        $"Dachora speed palette index {paletteIndex} exceeds the retail table.");
                }
                LoadDachoraPalette(slot, DachoraPalettePhase.Speed, paletteIndex);
                state.PaletteAnimationTimer = unchecked((ushort)(
                    state.PaletteAnimationTimer + DachoraPaletteDefinitions.SpeedStep));
                if (unchecked((short)(state.PaletteAnimationTimer - DachoraPaletteDefinitions.SpeedEnd)) >= 0)
                    state.PaletteAnimationTimer = DachoraPaletteDefinitions.SpeedRestart;
            }
        }

        // Native presses the body down one pixel every running frame before changing speed.
        // The collision result is discarded; the probe keeps Dachora glued to room slopes.
        _terrain.MoveEnemyVertically(level, slot, 1 << 16);

        bool belowMaximum = unchecked((short)(state.Speed - DachoraMaximumSpeedWhole)) < 0 ||
            unchecked((short)(state.Subspeed - DachoraMaximumSpeedFraction)) < 0;
        if (belowMaximum)
        {
            (state.Speed, state.Subspeed) = AddDachoraFixed(
                state.Speed,
                state.Subspeed,
                DachoraAccelerationWhole,
                DachoraAccelerationFraction);

            // Native's 28-byte cursor jump lands at the matching pose in the faster clone of the
            // current six-pose run loop. The animation keeps its phase and timing.
            if ((state.Speed is 4 or 8) && state.Subspeed == 0)
                state.Program = state.Program with { Program = FasterDachoraRun(state.Program.Program) };
        }
        else
        {
            state.Speed = DachoraMaximumSpeedWhole;
            state.Subspeed = DachoraMaximumSpeedFraction;
        }

        return ComposeDachoraFixed(state.Speed, state.Subspeed);
    }

    /// <summary>Ports the stored-shine countdown and echo activation at $A7:F78F.</summary>
    private void RunDachoraCharging(RoomEnemySlot slot, DachoraEnemyState state)
    {
        StepDachoraShinePalette(slot, state);
        state.PhaseTimer = state.PhaseTimer.Decrement().Remaining;
        if (!state.PhaseTimer.IsZero)
            return;

        // The charge animation is asleep at $F4B7. Stepping past the sleep enters the same
        // right-facing echo animation used by the four trails, exactly as the cartridge does on launch.
        state.Program = AnimationProgramCursor<DachoraProgram>.Start(DachoraAnimations.AfterSleep(state.Program.Program));
        slot.InstructionTimer = NativeCountdown.Start(1);
        state.Function = DachoraAiFunction.Shinesparking;

        RoomEnemySlot[] echoes = RequireDachoraEchoSlots(slot);
        RequireDachoraState(echoes[0]).EchoPositionTimer = NativeCountdown.Start(0);
        foreach (RoomEnemySlot echo in echoes)
            RequireDachoraState(echo).VerticalSubaccelerationOrLifetime = 0;
        state.VerticalAcceleration = 0;
        state.VerticalSubaccelerationOrLifetime = 0;
        slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole - 8)));
        LastDachoraSoundEffect = EnemySoundEffectDefinitions.DachoraLaunchSound;

        DachoraProgram echoList = slot.Parameter1 != 0
            ? DachoraProgram.EchoRight
            : DachoraProgram.EchoLeft;
        foreach (RoomEnemySlot echo in echoes)
            InstallDachoraInstruction(echo, RequireDachoraState(echo), echoList);
    }

    /// <summary>Ports upward acceleration, trail placement, and ceiling collision at $A7:F806.</summary>
    private void RunDachoraShinespark(
        RoomEnemySlot slot,
        DachoraEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        StepDachoraShinePalette(slot, state);
        UpdateDachoraEchoPositions(slot);

        (state.VerticalAcceleration, state.VerticalSubaccelerationOrLifetime) =
            AddDachoraFixed(
                state.VerticalAcceleration,
                state.VerticalSubaccelerationOrLifetime,
                samus.Kinematics.YAcceleration,
                samus.Kinematics.YSubacceleration);
        (state.Speed, state.Subspeed) = AddDachoraFixed(
            state.Speed,
            state.Subspeed,
            state.VerticalAcceleration,
            state.VerticalSubaccelerationOrLifetime);

        ushort cappedWhole = unchecked((short)(state.Speed - 15)) >= 0
            ? (ushort)15
            : state.Speed;
        int upwardDisplacement = unchecked(-ComposeDachoraFixed(cappedWhole, state.Subspeed));
        if (!_terrain.MoveEnemyVertically(level, slot, upwardDisplacement))
            return;

        // Ceiling impact reverses the facing bit before the fall, a visible retail quirk.
        if (slot.Parameter1 == 0)
        {
            InstallDachoraInstruction(slot, state, DachoraProgram.FallingRight);
            slot.Parameter1 = 1;
        }
        else
        {
            InstallDachoraInstruction(slot, state, DachoraProgram.FallingLeft);
            slot.Parameter1 = 0;
        }
        state.Function = DachoraAiFunction.Falling;
        state.Speed = 0;
        state.Subspeed = 0;
        state.PaletteAnimationTimer = 0;
        LoadDachoraPalette(slot, DachoraPalettePhase.Default, frame: 0);
        LastDachoraSoundEffect = EnemySoundEffectDefinitions.DachoraCeilingImpactSound;
    }

    /// <summary>Ports the gravity-capped return to the ground at $A7:F935.</summary>
    private void RunDachoraFalling(
        RoomEnemySlot slot,
        DachoraEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        (state.Speed, state.Subspeed) = AddDachoraFixed(
            state.Speed,
            state.Subspeed,
            samus.Kinematics.YAcceleration,
            samus.Kinematics.YSubacceleration);
        if (unchecked((short)(state.Speed - 10)) >= 0)
        {
            state.Speed = 10;
            state.Subspeed = 0;
        }

        if (!_terrain.MoveEnemyVertically(
                level,
                slot,
                ComposeDachoraFixed(state.Speed, state.Subspeed)))
        {
            return;
        }

        InstallDachoraInstruction(
            slot,
            state,
            slot.Parameter1 != 0
                ? DachoraProgram.RunningRight
                : DachoraProgram.RunningLeft);
        state.Function = slot.Parameter1 != 0
            ? DachoraAiFunction.RunningRight
            : DachoraAiFunction.RunningLeft;
        state.Speed = 0;
        state.Subspeed = 0;
    }

    /// <summary>Ports the four-slot, first-free echo pipeline at $A7:F89A.</summary>
    private void UpdateDachoraEchoPositions(RoomEnemySlot body)
    {
        RoomEnemySlot[] echoes = RequireDachoraEchoSlots(body);
        DachoraEnemyState first = RequireDachoraState(echoes[0]);
        if (!first.EchoPositionTimer.IsZero)
        {
            first.EchoPositionTimer = first.EchoPositionTimer.Decrement().Remaining;
            return;
        }

        first.EchoPositionTimer = NativeCountdown.Start(DachoraEchoPositionInterval);
        foreach (RoomEnemySlot echo in echoes)
        {
            DachoraEnemyState echoState = RequireDachoraState(echo);
            if (echoState.VerticalSubaccelerationOrLifetime != 0)
                continue;

            echo.Position = echo.Position.WithWhole(body.Position.Whole);
            echoState.VerticalSubaccelerationOrLifetime = DachoraEchoLifetime;
            return;
        }
    }

    /// <summary>Ports the alternating, lifetime-owned echo visibility at $A7:F98C.</summary>
    private static void RunDachoraEcho(
        RoomEnemySlot slot,
        DachoraEnemyState state,
        byte nmiFrameCounter8)
    {
        ushort lifetime = state.VerticalSubaccelerationOrLifetime;
        if (lifetime != 0)
        {
            state.VerticalSubaccelerationOrLifetime = unchecked((ushort)(lifetime - 1));
            bool oddNativeSlot = (slot.SlotIndex & 1) != 0;
            bool visible = oddNativeSlot
                ? (nmiFrameCounter8 & 1) != 0
                : (nmiFrameCounter8 & 1) == 0;
            slot.Properties = visible
                ? slot.Properties.Without(EnemyProperties.Invisible)
                : slot.Properties.With(EnemyProperties.Invisible);
            return;
        }

        slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
    }

    /// <summary>Ports the four-frame shine palette loop at $A7:F90A.</summary>
    internal void StepDachoraShinePalette(RoomEnemySlot slot, DachoraEnemyState state)
    {
        int paletteIndex = SnesLittleEndian.HighByte(state.PaletteAnimationTimer);
        if ((uint)paletteIndex >= 4)
        {
            throw new InvalidDataException(
                $"Dachora shine palette index {paletteIndex} exceeds the retail table.");
        }
        LoadDachoraPalette(slot, DachoraPalettePhase.Shine, paletteIndex);
        state.PaletteAnimationTimer = unchecked((ushort)(state.PaletteAnimationTimer + DachoraPaletteDefinitions.ShineStep));
        if (unchecked((short)(state.PaletteAnimationTimer - DachoraPaletteDefinitions.ShineEnd)) >= 0)
            state.PaletteAnimationTimer = 0;
    }

    /// <summary>Copies one full native OBJ palette into the actor-selected sprite palette.</summary>
    internal void LoadDachoraPalette(
        RoomEnemySlot slot, DachoraPalettePhase phase, int frame)
    {
        CgramPalette destination = _presentation.CgramRows.ObjectRow((slot.PaletteIndex >> 9) & 7);
        if (_room.Artwork?.DachoraColors is { } colors)
        {
            for (int color = 0; color < DachoraColorRomData.ColorsPerFrame; color++)
                destination.Write(color, colors.Resolve(phase, frame, color));
            return;
        }
        throw new InvalidOperationException("Dachora requires installed palette-cycle colors.");
    }

    /// <summary>Finds the four consecutive Dachora echoes following the body and rejects a malformed formation.</summary>
    private RoomEnemySlot[] RequireDachoraEchoSlots(RoomEnemySlot body)
    {
        if (body.SlotIndex + 4 >= RoomEnemySystem.MaximumEnemyCount)
            throw new InvalidDataException("Dachora body does not have four following physical echo slots.");

        var echoes = new RoomEnemySlot[4];
        for (int echoIndex = 0; echoIndex < echoes.Length; echoIndex++)
        {
            RoomEnemySlot echo = _slotTable._slots[body.SlotIndex + echoIndex + 1];
            if (echo.EnemyDefinitionPointer != EnemyDefinitionId.Dachora)
            {
                throw new InvalidDataException(
                    $"Dachora echo {echoIndex} is enemy ${(int)echo.EnemyDefinitionPointer:X4}, expected $E5FF.");
            }
            echoes[echoIndex] = echo;
        }
        return echoes;
    }

    /// <summary>Finds initialized Dachora state for a body or echo callback.</summary>
    internal static DachoraEnemyState RequireDachoraState(RoomEnemySlot slot) =>
        slot.SpeciesState<DachoraEnemyState>() ?? throw new InvalidOperationException(
            $"Dachora slot {slot.SlotIndex} has no initialized native state.");

    /// <summary>Starts an authored Dachora animation at its first tick.</summary>
    private static void InstallDachoraInstruction(RoomEnemySlot slot, DachoraEnemyState state, DachoraProgram program)
    {
        state.Program = AnimationProgramCursor<DachoraProgram>.Start(program);
        slot.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>The faster clone of a run loop, where native's 28-byte speed-up jump lands at the same pose.</summary>
    private static DachoraProgram FasterDachoraRun(DachoraProgram program) => program switch
    {
        DachoraProgram.RunningLeft => DachoraProgram.RunningLeftFast,
        DachoraProgram.RunningLeftFast => DachoraProgram.RunningLeftVeryFast,
        DachoraProgram.RunningRight => DachoraProgram.RunningRightFast,
        DachoraProgram.RunningRightFast => DachoraProgram.RunningRightVeryFast,
        _ => throw new InvalidOperationException($"Dachora speed-up from {program}, which has no faster run."),
    };

    /// <summary>Uses wrapped signed positions and a strict native proximity threshold on one axis.</summary>
    private static bool DachoraWithinAxis(ushort actor, ushort samus, int distance) =>
        Math.Abs(unchecked((short)(actor - samus))) < distance;

    /// <summary>Combines signed whole and unsigned fraction words into Dachora's 16.16 motion value.</summary>
    private static int ComposeDachoraFixed(ushort whole, ushort fraction) =>
        unchecked((SnesSixteenSixteen.FromWhole((int)(short)whole)) | fraction);

    /// <summary>Adds paired native whole and fraction words with carry, preserving 32-bit wraparound.</summary>
    private static (ushort Whole, ushort Fraction) AddDachoraFixed(
        ushort whole,
        ushort fraction,
        ushort wholeIncrement,
        ushort fractionIncrement)
    {
        uint value = (SnesSixteenSixteen.FromWhole((uint)whole)) | fraction;
        uint increment = (SnesSixteenSixteen.FromWhole((uint)wholeIncrement)) | fractionIncrement;
        value = unchecked(value + increment);
        return (SnesFixedPosition.WholeOf(unchecked((int)value)), unchecked((ushort)value));
    }

    /// <summary>Advances Dachora's typed animation.</summary>
    internal static void AdvanceDachoraTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        var state = RequireDachoraState(slot);
        AnimationProgramCursor<DachoraProgram> cursor = state.Program;
        EnemyProgramAnimation.Run(slot, DachoraAnimations.Programs, ref cursor, DachoraFrameDefinitions.Spritemap);
        state.Program = cursor;
    }

    /// <summary>Ports Dachora's initialization AI.</summary>
    internal static void RunDachoraInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeDachora(slot);
    }

    /// <summary>Ports Dachora's main AI.</summary>
    internal void RunDachoraMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunDachoraMain(
            slot,
            RequireDachoraState(slot),
            frame.Samus,
            frame.Level,
            frame.Clock.Counter8);
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        LastDachoraSoundEffect = null;
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastDachoraSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastDachoraSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
    }
}
