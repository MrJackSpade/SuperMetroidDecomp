using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>The Dachora species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
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
    /// <summary>The bird's run speed cap: eight pixels per update.</summary>
    private static readonly SnesSignedSixteenSixteen DachoraMaximumSpeed = SnesSignedSixteenSixteen.FromParts(8, 0);
    /// <summary>Dachora's running acceleration: a sixteenth of a pixel per update, every update.</summary>
    private static readonly SnesSignedSixteenSixteen DachoraAcceleration = SnesSignedSixteenSixteen.FromParts(0, 4096);
    /// <summary>The shinespark's upward speed cap in whole pixels per update; the fraction is kept.</summary>
    private const short DachoraShinesparkMaximumWholeSpeed = 15;
    /// <summary>The fall's speed cap: ten pixels per update.</summary>
    private static readonly SnesSignedSixteenSixteen DachoraMaximumFallSpeed = SnesSignedSixteenSixteen.FromParts(10, 0);
    /// <summary>Scripted room X where Dachora reverses its leftward run.</summary>
    private const ushort DachoraLeftTurnX = 96;
    /// <summary>Scripted room X where Dachora begins its stored-shine demonstration.</summary>
    private const ushort DachoraShinesparkX = 1152;

    /// <summary>Library-two sound queued when Samus wakes the tutorial Dachora.</summary>
    private static readonly SoundEffectId DachoraActivationSound =
        EnemySoundEffectDefinitions.DachoraActivationSound;
    /// <summary>Library-two sound queued at the scripted wall turn.</summary>
    private static readonly SoundEffectId DachoraWallImpactSound =
        EnemySoundEffectDefinitions.DachoraWallImpactSound;
    /// <summary>Library-two sound queued while Dachora stores its shinespark.</summary>
    private static readonly SoundEffectId DachoraChargeSound =
        EnemySoundEffectDefinitions.DachoraChargeSound;
    /// <summary>Library-two sound accompanying Dachora's Speed Booster run.</summary>
    private static readonly SoundEffectId DachoraSpeedBoosterSound =
        EnemySoundEffectDefinitions.DachoraSpeedBoosterSound;
    /// <summary>Library-two sound queued when Dachora launches upward.</summary>
    private static readonly SoundEffectId DachoraLaunchSound =
        EnemySoundEffectDefinitions.DachoraLaunchSound;
    /// <summary>Library-two sound queued on Dachora's ceiling contact.</summary>
    private static readonly SoundEffectId DachoraCeilingImpactSound =
        EnemySoundEffectDefinitions.DachoraCeilingImpactSound;

    /// <summary>Most recent library-two Dachora sound request during this frame.</summary>
    public SoundEffectId? LastDachoraSoundEffect { get; private set; }

    /// <summary>Ports <c>InitAI_Dachora</c> at <c>$A7:F4DD</c>.</summary>
    internal static void InitializeDachora(RoomEnemySlot slot)
    {
        var state = new DachoraEnemyState
        {
            FacingRight = slot.Spawn.Population.Parameter1 != 0,
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);

        // Native property $2000 both disables Samus collision and admits enemy bytecode.
        // Population property $0400 already excludes these actors from host interaction.
        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.Frame = EnemyFrameId.Empty;
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);

        short direction = unchecked((short)slot.Spawn.Population.Parameter1);
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
        int frameCounterByte)
    {
        // Echo AI needs only the NMI parity bit. Every body state needs both the active
        // Samus gravity words and authentic room collision, so fail loudly if a caller
        // attempts to run the family without the state that native code reads globally.
        if (state.Function == DachoraAiFunction.Echo)
        {
            RunDachoraEcho(slot, state, frameCounterByte);
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
            case DachoraAiFunction.Echo:
            default:
                throw new InvalidDataException(
                    $"Dachora function {state.Function?.ToString() ?? "none"} is not translated.");
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
            state.FacingRight
                ? DachoraProgram.BlinkRight
                : DachoraProgram.BlinkLeft);
        state.Function = DachoraAiFunction.BlinkingBeforeRun;
        state.PhaseTimer = WrappingCountdown.Start(DachoraBlinkDuration);
        LastDachoraSoundEffect = DachoraActivationSound;
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
            state.FacingRight
                ? DachoraProgram.RunningRight
                : DachoraProgram.RunningLeft);
        state.Function = state.FacingRight
            ? DachoraAiFunction.RunningRight
            : DachoraAiFunction.RunningLeft;
        state.SetPaletteCycle(frame: 0, ticks: 1);
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
        state.SetPaletteCycle(frame: 0, ticks: 1);
        state.FacingRight = true;
        state.Speed = SnesSignedSixteenSixteen.Zero;
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
            LastDachoraSoundEffect = DachoraWallImpactSound;
            InstallDachoraInstruction(slot, state, DachoraProgram.RunningLeft);
            state.Function = DachoraAiFunction.RunningLeft;
            state.FacingRight = false;
            state.Speed = SnesSignedSixteenSixteen.Zero;
            state.SetPaletteCycle(frame: 0, ticks: 0);
            LoadDachoraPalette(slot, DachoraPalettePhase.Default, frame: 0);
            return;
        }

        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, slot);
        if (unchecked((short)(slot.Position.X.Whole - DachoraShinesparkX)) < 0)
            return;

        InstallDachoraInstruction(slot, state, DachoraProgram.ChargeRight);
        state.Function = DachoraAiFunction.ChargingShinespark;
        state.PhaseTimer = WrappingCountdown.Start(DachoraChargeDuration);
        // Variable A now holds the charge countdown; only the speed's fraction word is cleared.
        state.Speed = state.Speed.WithFraction(0);
        state.SetPaletteCycle(frame: 0, ticks: 0);
        slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole + 8)));
        LastDachoraSoundEffect = DachoraChargeSound;
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
        if (unchecked((short)(state.Speed.WholePixels - DachoraMaximumSpeed.WholePixels)) >= 0)
        {
            if (state.PaletteFrame == 0 && state.PaletteTicks == 1)
                LastDachoraSoundEffect = DachoraSpeedBoosterSound;

            if (state.TickPaletteCycle())
            {
                int paletteIndex = state.PaletteFrame;
                if ((uint)paletteIndex >= 4)
                {
                    throw new InvalidDataException(
                        $"Dachora speed palette index {paletteIndex} exceeds the retail table.");
                }
                LoadDachoraPalette(slot, DachoraPalettePhase.Speed, paletteIndex);
                state.AdvanceSpeedPaletteCycle();
            }
        }

        // Native presses the body down one pixel every running frame before changing speed.
        // The collision result is discarded; the probe keeps Dachora glued to room slopes.
        _terrain.MoveEnemyVertically(level, slot, 1 << 16);

        // The cartridge compares the whole and fraction words separately, each by the sign of its difference
        // from the cap's; a fraction with its top bit set therefore also counts as below the cap.
        SnesSignedSixteenSixteen speed = state.Speed;
        bool belowMaximum = unchecked((short)(speed.WholePixels - DachoraMaximumSpeed.WholePixels)) < 0 ||
            unchecked((short)(speed.Fraction - DachoraMaximumSpeed.Fraction)) < 0;
        if (belowMaximum)
        {
            speed += DachoraAcceleration;
            state.Speed = speed;

            // Native's 28-byte cursor jump lands at the matching pose in the faster clone of the
            // current six-pose run loop. The animation keeps its phase and timing.
            if ((speed.WholePixels is 4 or 8) && speed.Fraction == 0)
                state.Program = state.Program with { Program = FasterDachoraRun(state.Program.Program) };
        }
        else
        {
            state.Speed = DachoraMaximumSpeed;
        }

        return state.Speed.RawValue;
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
        slot.InstructionTimer = WrappingCountdown.Start(1);
        state.Function = DachoraAiFunction.Shinesparking;

        RoomEnemySlot[] echoes = RequireDachoraEchoSlots(slot);
        echoes[0].RequireSpeciesState<DachoraEnemyState>().EchoPositionTimer = WrappingCountdown.Start(0);
        foreach (RoomEnemySlot echo in echoes)
            echo.RequireSpeciesState<DachoraEnemyState>().EchoLifetime = WrappingCountdown.Start(0);
        state.VerticalAcceleration = SnesSignedSixteenSixteen.Zero;
        slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole - 8)));
        LastDachoraSoundEffect = DachoraLaunchSound;

        DachoraProgram echoList = state.FacingRight
            ? DachoraProgram.EchoRight
            : DachoraProgram.EchoLeft;
        foreach (RoomEnemySlot echo in echoes)
            InstallDachoraInstruction(echo, echo.RequireSpeciesState<DachoraEnemyState>(), echoList);
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

        state.VerticalAcceleration += SamusGravity(samus);
        state.Speed += state.VerticalAcceleration;

        // The cap replaces only the whole word, compared by the sign of its word difference; the fraction is kept.
        SnesSignedSixteenSixteen speed = state.Speed;
        SnesSignedSixteenSixteen capped = unchecked((short)(speed.WholePixels - DachoraShinesparkMaximumWholeSpeed)) >= 0
            ? speed.WithWholePixels(DachoraShinesparkMaximumWholeSpeed)
            : speed;
        int upwardDisplacement = unchecked(-capped.RawValue);
        if (!_terrain.MoveEnemyVertically(level, slot, upwardDisplacement))
            return;

        // Ceiling impact reverses the facing bit before the fall, a visible retail quirk.
        if (!state.FacingRight)
        {
            InstallDachoraInstruction(slot, state, DachoraProgram.FallingRight);
            state.FacingRight = true;
        }
        else
        {
            InstallDachoraInstruction(slot, state, DachoraProgram.FallingLeft);
            state.FacingRight = false;
        }
        state.Function = DachoraAiFunction.Falling;
        state.Speed = SnesSignedSixteenSixteen.Zero;
        state.SetPaletteCycle(frame: 0, ticks: 0);
        LoadDachoraPalette(slot, DachoraPalettePhase.Default, frame: 0);
        LastDachoraSoundEffect = DachoraCeilingImpactSound;
    }

    /// <summary>Ports the gravity-capped return to the ground at $A7:F935.</summary>
    private void RunDachoraFalling(
        RoomEnemySlot slot,
        DachoraEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        state.Speed += SamusGravity(samus);
        if (unchecked((short)(state.Speed.WholePixels - DachoraMaximumFallSpeed.WholePixels)) >= 0)
            state.Speed = DachoraMaximumFallSpeed;

        if (!_terrain.MoveEnemyVertically(
                level,
                slot,
                state.Speed.RawValue))
        {
            return;
        }

        InstallDachoraInstruction(
            slot,
            state,
            state.FacingRight
                ? DachoraProgram.RunningRight
                : DachoraProgram.RunningLeft);
        state.Function = state.FacingRight
            ? DachoraAiFunction.RunningRight
            : DachoraAiFunction.RunningLeft;
        state.Speed = SnesSignedSixteenSixteen.Zero;
    }

    /// <summary>Ports the four-slot, first-free echo pipeline at $A7:F89A.</summary>
    private void UpdateDachoraEchoPositions(RoomEnemySlot body)
    {
        RoomEnemySlot[] echoes = RequireDachoraEchoSlots(body);
        DachoraEnemyState first = echoes[0].RequireSpeciesState<DachoraEnemyState>();
        if (!first.EchoPositionTimer.IsZero)
        {
            first.EchoPositionTimer = first.EchoPositionTimer.Decrement().Remaining;
            return;
        }

        first.EchoPositionTimer = WrappingCountdown.Start(DachoraEchoPositionInterval);
        foreach (RoomEnemySlot echo in echoes)
        {
            DachoraEnemyState echoState = echo.RequireSpeciesState<DachoraEnemyState>();
            if (!echoState.EchoLifetime.IsZero)
                continue;

            echo.Position = echo.Position.WithWhole(body.Position.Whole);
            echoState.EchoLifetime = WrappingCountdown.Start(DachoraEchoLifetime);
            return;
        }
    }

    /// <summary>Ports the alternating, lifetime-owned echo visibility at $A7:F98C.</summary>
    private static void RunDachoraEcho(
        RoomEnemySlot slot,
        DachoraEnemyState state,
        int frameCounterByte)
    {
        if (!state.EchoLifetime.IsZero)
        {
            state.EchoLifetime = state.EchoLifetime.DecrementToZero();
            bool oddNativeSlot = (slot.SlotIndex & 1) != 0;
            bool visible = oddNativeSlot
                ? (frameCounterByte & 1) != 0
                : (frameCounterByte & 1) == 0;
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
        int paletteIndex = state.PaletteFrame;
        if ((uint)paletteIndex >= 4)
        {
            throw new InvalidDataException(
                $"Dachora shine palette index {paletteIndex} exceeds the retail table.");
        }
        LoadDachoraPalette(slot, DachoraPalettePhase.Shine, paletteIndex);
        state.AdvanceShinePaletteCycle();
    }

    /// <summary>Copies one full native OBJ palette into the actor-selected sprite palette.</summary>
    internal void LoadDachoraPalette(
        RoomEnemySlot slot, DachoraPalettePhase phase, int frame)
    {
        ColourRun destination = _presentation.ColourSetOf(slot);
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
        if (body.SlotIndex + 4 >= EnemySlotTable.MaximumEnemyCount)
            throw new InvalidDataException("Dachora body does not have four following physical echo slots.");

        var echoes = new RoomEnemySlot[4];
        for (int echoIndex = 0; echoIndex < echoes.Length; echoIndex++)
        {
            RoomEnemySlot echo = _slotTable.Slots[body.SlotIndex + echoIndex + 1];
            if (echo.DefinitionId != EnemyDefinitionId.Dachora)
            {
                throw new InvalidDataException(
                    $"Dachora echo {echoIndex} is enemy {echo.DefinitionId}, expected Dachora.");
            }
            echoes[echoIndex] = echo;
        }
        return echoes;
    }

    /// <summary>Starts an authored Dachora animation at its first tick.</summary>
    private static void InstallDachoraInstruction(RoomEnemySlot slot, DachoraEnemyState state, DachoraProgram program)
    {
        state.Program = AnimationProgramCursor<DachoraProgram>.Start(program);
        slot.InstructionTimer = WrappingCountdown.Start(1);
    }

    /// <summary>The faster clone of a run loop, where native's 28-byte speed-up jump lands at the same pose.</summary>
    private static DachoraProgram FasterDachoraRun(DachoraProgram program) => program switch
    {
        DachoraProgram.RunningLeft
            or DachoraProgram.RunningLeftFast
            or DachoraProgram.RunningRight
            or DachoraProgram.RunningRightFast => EnumOrder.Next(program),
        DachoraProgram.RunningLeftVeryFast or
            DachoraProgram.IdleLeft or
            DachoraProgram.BlinkLeft or
            DachoraProgram.EchoLeft or
            DachoraProgram.FallingLeft or
            DachoraProgram.RunningRightVeryFast or
            DachoraProgram.IdleRight or
            DachoraProgram.BlinkRight or
            DachoraProgram.ChargeRight or
            DachoraProgram.EchoRight or
            DachoraProgram.FallingRight => throw new InvalidOperationException($"Dachora speed-up from {program}, which has no faster run."),
        _ => throw new InvalidOperationException($"Dachora speed-up from {program}, which has no faster run."),
    };

    /// <summary>Uses wrapped signed positions and a strict native proximity threshold on one axis.</summary>
    private static bool DachoraWithinAxis(ushort actor, ushort samus, int distance) =>
        Math.Abs(unchecked((short)(actor - samus))) < distance;

    /// <summary>Samus's current gravity per update, which the shinespark and fall accumulate.</summary>
    private static SnesSignedSixteenSixteen SamusGravity(SamusState samus) =>
        SnesSignedSixteenSixteen.FromParts(
            unchecked((short)samus.Kinematics.YAcceleration.Whole),
            samus.Kinematics.YAcceleration.Fraction);

    /// <summary>Advances Dachora's typed animation.</summary>
    internal static void AdvanceDachoraTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, DachoraAnimations.Programs, DachoraFrameDefinitions.ShownFrames.Of,
            static (DachoraEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor);

    /// <summary>Ports Dachora's main AI.</summary>
    internal void RunDachoraMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunDachoraMain(
            slot,
            slot.RequireSpeciesState<DachoraEnemyState>(),
            frame.Samus,
            frame.Level,
            frame.Clock.Counter8);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastDachoraSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (LastDachoraSoundEffect is SoundEffectId sound)
            audio.QueueEnemySound(sound, maximumQueued: SoundQueueLimit.Max6);
    }
}
