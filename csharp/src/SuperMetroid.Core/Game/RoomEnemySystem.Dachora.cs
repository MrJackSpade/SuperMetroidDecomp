using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$A7 function words stored in Dachora variable F. These are native addresses, not
/// host-only state IDs, so debugger watches can be compared directly with the cartridge.
/// </summary>
public enum DachoraAiFunction : ushort
{
    /// <summary><c>$A7:F570</c>: probes the floor and waits for Samus to enter the activation rectangle.</summary>
    WaitingForSamus = 0xf570,

    /// <summary><c>$A7:F5BC</c>: runs the 120-update warning countdown before installing the first run list.</summary>
    BlinkingBeforeRun = 0xf5bc,

    /// <summary><c>$A7:F5ED</c>: accelerates left along the floor until the turn coordinate or a wall.</summary>
    RunningLeft = 0xf5ed,

    /// <summary><c>$A7:F65E</c>: accelerates right until the tutorial launch coordinate or a wall.</summary>
    RunningRight = 0xf65e,

    /// <summary><c>$A7:F78F</c>: cycles the shine palette and counts down to the vertical launch.</summary>
    ChargingShinespark = 0xf78f,

    /// <summary><c>$A7:F806</c>: accelerates upward, publishes echo positions, and transitions on ceiling collision.</summary>
    Shinesparking = 0xf806,

    /// <summary><c>$A7:F935</c>: applies capped gravity until floor collision resumes the running phase.</summary>
    Falling = 0xf935,

    /// <summary><c>$A7:F98C</c>: alternates one physical echo slot's visibility for its eight-update lifetime.</summary>
    Echo = 0xf98c,
}

/// <summary>
/// Typed view of the six native Dachora variables. Their meanings intentionally change
/// between phases: A/B are horizontal speed while running and vertical speed after the
/// shinespark; D is vertical subacceleration on the body but echo lifetime on echo slots.
/// </summary>
public sealed class DachoraEnemyState
{
    /// <summary>Physical enemy record whose six variable words back this typed state view.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates a typed view over the native variable words of one Dachora body or echo slot.</summary>
    /// <param name="slot">Physical enemy record owned by this actor.</param>
    internal DachoraEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Variable A: whole speed, countdown, or echo-position interval.</summary>
    public ushort SpeedOrTimer
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Variable B: fractional speed.</summary>
    public ushort Subspeed
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Variable C: whole vertical acceleration during the shinespark.</summary>
    public ushort VerticalAcceleration
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Variable D: fractional vertical acceleration or echo visibility lifetime.</summary>
    public ushort VerticalSubaccelerationOrLifetime
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Variable E: packed palette-animation frame and low-byte countdown.</summary>
    public ushort PaletteAnimationTimer
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Variable F: bank-$A7 indirect main-AI address.</summary>
    public DachoraAiFunction Function
    {
        get => (DachoraAiFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }
}

/// <summary>
/// Literal translation of friendly Dachora enemy <c>$E5FF</c> from $A7:F225-$F9C1.
/// Retail represents the speed echoes as four consecutive full enemy records, so this
/// implementation preserves their individual bytecode, visibility, and physical slots.
/// Dachora has no authored attacks or damage reactions; every combat callback is a no-op.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Native enemy-definition word identifying Dachora bodies and their four echo actors.</summary>
    internal const ushort DachoraDefinition = 0xe5ff;

    /// <summary>Library-two sound requested when Samus enters the activation rectangle.</summary>
    private const ushort DachoraActivationSound = 0x001d;
    /// <summary>Library-two sound played when the running actor reaches maximum speed.</summary>
    private const ushort DachoraSpeedBoosterSound = 0x0039;
    /// <summary>Library-two sound played when Dachora begins charging its shinespark.</summary>
    private const ushort DachoraChargeSound = 0x003d;
    /// <summary>Library-two sound played when Dachora launches upward.</summary>
    private const ushort DachoraLaunchSound = 0x003b;
    /// <summary>Library-two sound played when the upward actor hits the ceiling.</summary>
    private const ushort DachoraCeilingImpactSound = 0x003c;
    /// <summary>Library-two sound played when the running actor hits a wall.</summary>
    private const ushort DachoraWallImpactSound = 0x0071;

    // NTSC retail constants at $A7:F4C9-$F4DB. They are kept as paired native words so
    // fixed-point carry and exact threshold frames remain visible and auditable.
    /// <summary>Horizontal proximity threshold, in pixels, for beginning Dachora's warning phase.</summary>
    private const int DachoraActivationXDistance = 0x60;
    /// <summary>Vertical proximity threshold, in pixels, for beginning Dachora's warning phase.</summary>
    private const int DachoraActivationYDistance = 0x40;
    /// <summary>Warning countdown length before the first running instruction list begins.</summary>
    private const ushort DachoraBlinkDuration = 0x78;
    /// <summary>Charge countdown length before the shinespark launch.</summary>
    private const ushort DachoraChargeDuration = 0x3c;
    /// <summary>Running updates between checks that publish a new echo position.</summary>
    private const ushort DachoraEchoPositionInterval = 1;
    /// <summary>Visible lifetime in updates assigned to each newly positioned speed echo.</summary>
    private const ushort DachoraEchoLifetime = 8;
    /// <summary>Whole-number cap on horizontal running speed in 16.16 fixed-point units.</summary>
    private const ushort DachoraMaximumSpeedWhole = 8;
    /// <summary>Fractional component of the horizontal running speed cap.</summary>
    private const ushort DachoraMaximumSpeedFraction = 0;
    /// <summary>Whole-number horizontal acceleration increment per running update.</summary>
    private const ushort DachoraAccelerationWhole = 0;
    /// <summary>Fractional horizontal acceleration increment per running update.</summary>
    private const ushort DachoraAccelerationFraction = 0x1000;
    /// <summary>X coordinate where the left-running path turns back toward the tutorial launch point.</summary>
    private const ushort DachoraLeftTurnX = 0x0060;
    /// <summary>Hard-coded X coordinate that begins the shinespark charge on the right-running path.</summary>
    private const ushort DachoraShinesparkX = 0x0480;

    /// <summary>Per-slot typed Dachora state, populated when each body or echo is initialized.</summary>
    private readonly DachoraEnemyState?[] _dachoraStates =
        new DachoraEnemyState?[MaximumEnemyCount];

    /// <summary>Most recent library-two Dachora sound request during this frame.</summary>
    public ushort? LastDachoraSoundEffect { get; private set; }

    /// <summary>Ports <c>InitAI_Dachora</c> at <c>$A7:F4DD</c>.</summary>
    private void InitializeDachora(RoomEnemySlot slot)
    {
        var state = new DachoraEnemyState(slot);
        _dachoraStates[slot.SlotIndex] = state;

        // Native property $2000 both disables Samus collision and admits enemy bytecode.
        // Population property $0400 already excludes these actors from host interaction.
        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.SpritemapPointer = 0x804d;
        slot.InstructionTimer = 1;
        slot.Timer = 0;

        short direction = unchecked((short)slot.Parameter1);
        if (direction < 0)
        {
            // A negative direction identifies an echo. Its low bit selects the same facing
            // convention as the body, while variable D later becomes its eight-frame life.
            slot.CurrentInstruction = (direction & 1) != 0
                ? DachoraInstructionProgramDefinitions.EchoRight
                : DachoraInstructionProgramDefinitions.EchoLeft;
            state.Function = DachoraAiFunction.Echo;
            return;
        }

        slot.CurrentInstruction = direction != 0
            ? DachoraInstructionProgramDefinitions.IdleRight
            : DachoraInstructionProgramDefinitions.IdleLeft;
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
        MoveEnemyVertically(level, slot, 1 << 16);
        if (!DachoraWithinAxis(slot.YPosition, samus.YPosition, DachoraActivationYDistance) ||
            !DachoraWithinAxis(slot.XPosition, samus.XPosition, DachoraActivationXDistance))
        {
            return;
        }

        InstallDachoraInstruction(
            slot,
            slot.Parameter1 != 0
                ? DachoraInstructionProgramDefinitions.BlinkRight
                : DachoraInstructionProgramDefinitions.BlinkLeft);
        state.Function = DachoraAiFunction.BlinkingBeforeRun;
        state.SpeedOrTimer = DachoraBlinkDuration;
        LastDachoraSoundEffect = DachoraActivationSound;
    }

    /// <summary>Ports the 120-frame warning countdown at $A7:F5BC.</summary>
    private static void RunDachoraBlinking(RoomEnemySlot slot, DachoraEnemyState state)
    {
        state.SpeedOrTimer = unchecked((ushort)(state.SpeedOrTimer - 1));
        if (state.SpeedOrTimer != 0)
            return;

        InstallDachoraInstruction(
            slot,
            slot.Parameter1 != 0
                ? DachoraInstructionProgramDefinitions.RunningRight
                : DachoraInstructionProgramDefinitions.RunningLeft);
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
        bool hitWall = MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level,
            slot,
            displacement);
        if (!hitWall)
            AlignEnemyYWithNonSquareSlope(level, slot);

        if (!hitWall && unchecked((short)(slot.XPosition - DachoraLeftTurnX)) >= 0)
            return;

        InstallDachoraInstruction(slot, DachoraInstructionProgramDefinitions.RunningRight);
        state.Function = DachoraAiFunction.RunningRight;
        state.PaletteAnimationTimer = 1;
        slot.Parameter1 = 1;
        state.SpeedOrTimer = 0;
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
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, displacement))
        {
            LastDachoraSoundEffect = DachoraWallImpactSound;
            InstallDachoraInstruction(slot, DachoraInstructionProgramDefinitions.RunningLeft);
            state.Function = DachoraAiFunction.RunningLeft;
            slot.Parameter1 = 0;
            state.SpeedOrTimer = 0;
            state.Subspeed = 0;
            state.PaletteAnimationTimer = 0;
            LoadDachoraPalette(slot, DachoraPalettePhase.Default, frame: 0);
            return;
        }

        AlignEnemyYWithNonSquareSlope(level, slot);
        if (unchecked((short)(slot.XPosition - DachoraShinesparkX)) < 0)
            return;

        InstallDachoraInstruction(slot, DachoraInstructionProgramDefinitions.ChargeRight);
        state.Function = DachoraAiFunction.ChargingShinespark;
        state.SpeedOrTimer = DachoraChargeDuration;
        state.Subspeed = 0;
        state.PaletteAnimationTimer = 0;
        slot.YPosition = unchecked((ushort)(slot.YPosition + 8));
        LastDachoraSoundEffect = DachoraChargeSound;
    }

    /// <summary>
    /// Ports <c>AccelerateRunningDachora</c> at $A7:F6D5. This intentionally retains the
    /// strange packed palette timer: after frame three is reached, native code reloads that
    /// brightest speed palette every sixteen frames instead of wrapping to frame zero.
    /// </summary>
    private int AccelerateDachora(
        RoomEnemySlot slot,
        DachoraEnemyState state,
        RoomLevelData level)
    {
        if (unchecked((short)(state.SpeedOrTimer - DachoraMaximumSpeedWhole)) >= 0)
        {
            if (state.PaletteAnimationTimer == 1)
                LastDachoraSoundEffect = DachoraSpeedBoosterSound;

            state.PaletteAnimationTimer = unchecked((ushort)(state.PaletteAnimationTimer - 1));
            if ((byte)state.PaletteAnimationTimer == 0)
            {
                int paletteIndex = state.PaletteAnimationTimer >> 8;
                if ((uint)paletteIndex >= 4)
                {
                    throw new InvalidDataException(
                        $"Dachora speed palette index {paletteIndex} exceeds the retail table.");
                }
                LoadDachoraPalette(slot, DachoraPalettePhase.Speed, paletteIndex);
                state.PaletteAnimationTimer = unchecked((ushort)(
                    state.PaletteAnimationTimer + 0x0110));
                if (unchecked((short)(state.PaletteAnimationTimer - 0x0410)) >= 0)
                    state.PaletteAnimationTimer = 0x0310;
            }
        }

        // Native presses the body down one pixel every running frame before changing speed.
        // The collision result is discarded; the probe keeps Dachora glued to room slopes.
        MoveEnemyVertically(level, slot, 1 << 16);

        bool belowMaximum = unchecked((short)(state.SpeedOrTimer - DachoraMaximumSpeedWhole)) < 0 ||
            unchecked((short)(state.Subspeed - DachoraMaximumSpeedFraction)) < 0;
        if (belowMaximum)
        {
            (state.SpeedOrTimer, state.Subspeed) = AddDachoraFixed(
                state.SpeedOrTimer,
                state.Subspeed,
                DachoraAccelerationWhole,
                DachoraAccelerationFraction);

            // Each 28-byte jump lands at the matching frame in the faster clone of the
            // current six-frame run list. The instruction interpreter owns phase/timing.
            if ((state.SpeedOrTimer is 4 or 8) && state.Subspeed == 0)
                slot.CurrentInstruction = unchecked((ushort)(slot.CurrentInstruction + 28));
        }
        else
        {
            state.SpeedOrTimer = DachoraMaximumSpeedWhole;
            state.Subspeed = DachoraMaximumSpeedFraction;
        }

        return ComposeDachoraFixed(state.SpeedOrTimer, state.Subspeed);
    }

    /// <summary>Ports the stored-shine countdown and echo activation at $A7:F78F.</summary>
    private void RunDachoraCharging(RoomEnemySlot slot, DachoraEnemyState state)
    {
        StepDachoraShinePalette(slot, state);
        state.SpeedOrTimer = unchecked((ushort)(state.SpeedOrTimer - 1));
        if (state.SpeedOrTimer != 0)
            return;

        // The body list is asleep at $F4B7. Adding two enters the same right-facing echo
        // list used by the four trails, exactly as the cartridge does on launch.
        slot.CurrentInstruction = unchecked((ushort)(slot.CurrentInstruction + 2));
        slot.InstructionTimer = 1;
        state.Function = DachoraAiFunction.Shinesparking;

        RoomEnemySlot[] echoes = RequireDachoraEchoSlots(slot);
        RequireDachoraState(echoes[0]).SpeedOrTimer = 0;
        foreach (RoomEnemySlot echo in echoes)
            RequireDachoraState(echo).VerticalSubaccelerationOrLifetime = 0;
        state.VerticalAcceleration = 0;
        state.VerticalSubaccelerationOrLifetime = 0;
        slot.YPosition = unchecked((ushort)(slot.YPosition - 8));
        LastDachoraSoundEffect = DachoraLaunchSound;

        ushort echoList = slot.Parameter1 != 0
            ? DachoraInstructionProgramDefinitions.EchoRight
            : DachoraInstructionProgramDefinitions.EchoLeft;
        foreach (RoomEnemySlot echo in echoes)
            InstallDachoraInstruction(echo, echoList);
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
        (state.SpeedOrTimer, state.Subspeed) = AddDachoraFixed(
            state.SpeedOrTimer,
            state.Subspeed,
            state.VerticalAcceleration,
            state.VerticalSubaccelerationOrLifetime);

        ushort cappedWhole = unchecked((short)(state.SpeedOrTimer - 15)) >= 0
            ? (ushort)15
            : state.SpeedOrTimer;
        int upwardDisplacement = unchecked(-ComposeDachoraFixed(cappedWhole, state.Subspeed));
        if (!MoveEnemyVertically(level, slot, upwardDisplacement))
            return;

        // Ceiling impact reverses the facing bit before the fall, a visible retail quirk.
        if (slot.Parameter1 == 0)
        {
            InstallDachoraInstruction(slot, DachoraInstructionProgramDefinitions.FallingRight);
            slot.Parameter1 = 1;
        }
        else
        {
            InstallDachoraInstruction(slot, DachoraInstructionProgramDefinitions.FallingLeft);
            slot.Parameter1 = 0;
        }
        state.Function = DachoraAiFunction.Falling;
        state.SpeedOrTimer = 0;
        state.Subspeed = 0;
        state.PaletteAnimationTimer = 0;
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
        (state.SpeedOrTimer, state.Subspeed) = AddDachoraFixed(
            state.SpeedOrTimer,
            state.Subspeed,
            samus.Kinematics.YAcceleration,
            samus.Kinematics.YSubacceleration);
        if (unchecked((short)(state.SpeedOrTimer - 10)) >= 0)
        {
            state.SpeedOrTimer = 10;
            state.Subspeed = 0;
        }

        if (!MoveEnemyVertically(
                level,
                slot,
                ComposeDachoraFixed(state.SpeedOrTimer, state.Subspeed)))
        {
            return;
        }

        InstallDachoraInstruction(
            slot,
            slot.Parameter1 != 0
                ? DachoraInstructionProgramDefinitions.RunningRight
                : DachoraInstructionProgramDefinitions.RunningLeft);
        state.Function = slot.Parameter1 != 0
            ? DachoraAiFunction.RunningRight
            : DachoraAiFunction.RunningLeft;
        state.SpeedOrTimer = 0;
        state.Subspeed = 0;
    }

    /// <summary>Ports the four-slot, first-free echo pipeline at $A7:F89A.</summary>
    private void UpdateDachoraEchoPositions(RoomEnemySlot body)
    {
        RoomEnemySlot[] echoes = RequireDachoraEchoSlots(body);
        DachoraEnemyState first = RequireDachoraState(echoes[0]);
        if (first.SpeedOrTimer != 0)
        {
            first.SpeedOrTimer = unchecked((ushort)(first.SpeedOrTimer - 1));
            return;
        }

        first.SpeedOrTimer = DachoraEchoPositionInterval;
        foreach (RoomEnemySlot echo in echoes)
        {
            DachoraEnemyState echoState = RequireDachoraState(echo);
            if (echoState.VerticalSubaccelerationOrLifetime != 0)
                continue;

            echo.XPosition = body.XPosition;
            echo.YPosition = body.YPosition;
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
            bool oddNativeSlot = (slot.NativeIndex & 0x0040) != 0;
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
    private void StepDachoraShinePalette(RoomEnemySlot slot, DachoraEnemyState state)
    {
        int paletteIndex = state.PaletteAnimationTimer >> 8;
        if ((uint)paletteIndex >= 4)
        {
            throw new InvalidDataException(
                $"Dachora shine palette index {paletteIndex} exceeds the retail table.");
        }
        LoadDachoraPalette(slot, DachoraPalettePhase.Shine, paletteIndex);
        state.PaletteAnimationTimer = unchecked((ushort)(state.PaletteAnimationTimer + 0x0100));
        if (unchecked((short)(state.PaletteAnimationTimer - 0x0400)) >= 0)
            state.PaletteAnimationTimer = 0;
    }

    /// <summary>Copies one full native OBJ palette into the actor-selected sprite palette.</summary>
    private void LoadDachoraPalette(
        RoomEnemySlot slot, DachoraPalettePhase phase, int frame)
    {
        int objectPalette = (slot.PaletteIndex >> 9) & 7;
        int destinationColor = 128 + objectPalette * 16;
        if (TileArtwork?.DachoraColors is { } colors)
        {
            for (int color = 0; color < DachoraColorRomData.ColorsPerFrame; color++)
                _cgram!.SetColor(destinationColor + color,
                    colors.Resolve(phase, frame, color));
            return;
        }
        throw new InvalidOperationException("Dachora requires installed palette-cycle colors.");
    }

    /// <summary>Resolves the four consecutive physical enemy slots used as the body's speed echoes.</summary>
    /// <param name="body">Initialized Dachora body whose following slots hold its echoes.</param>
    /// <returns>The four following slots in native echo order.</returns>
    /// <exception cref="InvalidDataException">The body lacks four available following slots or any slot is not a Dachora actor.</exception>
    private RoomEnemySlot[] RequireDachoraEchoSlots(RoomEnemySlot body)
    {
        if (body.SlotIndex + 4 >= MaximumEnemyCount)
            throw new InvalidDataException("Dachora body does not have four following physical echo slots.");

        var echoes = new RoomEnemySlot[4];
        for (int echoIndex = 0; echoIndex < echoes.Length; echoIndex++)
        {
            RoomEnemySlot echo = _slots[body.SlotIndex + echoIndex + 1];
            if (echo.EnemyDefinitionPointer != DachoraDefinition)
            {
                throw new InvalidDataException(
                    $"Dachora echo {echoIndex} is enemy ${echo.EnemyDefinitionPointer:X4}, expected $E5FF.");
            }
            echoes[echoIndex] = echo;
        }
        return echoes;
    }

    /// <summary>Gets the typed variable-word view previously installed for a physical Dachora slot.</summary>
    /// <param name="slot">Body or echo slot whose state is required.</param>
    /// <exception cref="InvalidOperationException">The slot has not been initialized as Dachora.</exception>
    private DachoraEnemyState RequireDachoraState(RoomEnemySlot slot) =>
        _dachoraStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Dachora slot {slot.SlotIndex} has no initialized native state.");

    /// <summary>Switches an actor to an instruction list and primes its first instruction for the next interpreter step.</summary>
    /// <param name="slot">Dachora body or echo receiving the list.</param>
    /// <param name="instructionList">Native instruction-list address to install.</param>
    private static void InstallDachoraInstruction(RoomEnemySlot slot, ushort instructionList)
    {
        slot.CurrentInstruction = instructionList;
        slot.InstructionTimer = 1;
    }

    /// <summary>Checks signed wrapped coordinate separation against a strict per-axis activation threshold.</summary>
    /// <param name="actor">Dachora coordinate on one axis.</param>
    /// <param name="samus">Samus coordinate on the same axis.</param>
    /// <param name="distance">Exclusive proximity limit in pixels.</param>
    /// <returns>Whether the absolute signed separation is less than the limit.</returns>
    private static bool DachoraWithinAxis(ushort actor, ushort samus, int distance) =>
        Math.Abs(unchecked((short)(actor - samus))) < distance;

    /// <summary>Combines signed whole and unsigned fractional words into the 16.16 displacement representation.</summary>
    /// <param name="whole">Signed integer component stored in a 16-bit word.</param>
    /// <param name="fraction">Fractional low word.</param>
    /// <returns>The packed fixed-point value used by enemy movement.</returns>
    private static int ComposeDachoraFixed(ushort whole, ushort fraction) =>
        unchecked(((int)(short)whole << 16) | fraction);

    /// <summary>Adds two 16.16 values with native 32-bit wraparound and returns their component words.</summary>
    /// <param name="whole">Current signed whole component.</param>
    /// <param name="fraction">Current fractional component.</param>
    /// <param name="wholeIncrement">Whole component to add.</param>
    /// <param name="fractionIncrement">Fractional component to add, including carry into the whole word.</param>
    /// <returns>The wrapped whole and fractional result words.</returns>
    private static (ushort Whole, ushort Fraction) AddDachoraFixed(
        ushort whole,
        ushort fraction,
        ushort wholeIncrement,
        ushort fractionIncrement)
    {
        uint value = ((uint)whole << 16) | fraction;
        uint increment = ((uint)wholeIncrement << 16) | fractionIncrement;
        value = unchecked(value + increment);
        return (unchecked((ushort)(value >> 16)), unchecked((ushort)value));
    }
}
