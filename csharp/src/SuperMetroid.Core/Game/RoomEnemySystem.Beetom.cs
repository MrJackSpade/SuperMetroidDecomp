using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A8 function pointer stored in Beetom's common variable two. Setup functions
/// intentionally remain separate states: on the cartridge, selecting an action consumes one
/// frame and installing that action consumes another before movement begins.
/// </summary>
public enum BeetomEnemyFunction : ushort
{
    DecideAction = 0xb814,
    DecideActionSamusNotInProximity = 0xb82f,
    StartIdling = 0xb84f,
    StartCrawlingLeft = 0xb85f,
    StartCrawlingRight = 0xb873,
    StartShortHopLeft = 0xb887,
    StartShortHopRight = 0xb8a9,
    StartLongHopLeft = 0xb8cb,
    StartLongHopRight = 0xb8ed,
    DecideActionSamusInProximity = 0xb90f,
    StartDrainingLeft = 0xb952,
    StartDrainingRight = 0xb966,
    StartDropping = 0xb97a,
    StartBeingFlung = 0xb9a2,
    Idling = 0xb9b2,
    CrawlingLeft = 0xb9c1,
    CrawlingRight = 0xba24,
    ShortHopLeft = 0xba84,
    ShortHopRight = 0xbab7,
    LongHopLeft = 0xbb55,
    LongHopRight = 0xbb88,
    LungeLeft = 0xbc26,
    LungeRight = 0xbc5a,
    DrainingLeft = 0xbcf8,
    DrainingRight = 0xbd42,
    Dropping = 0xbd9d,
    BeingFlung = 0xbdc5,
}

/// <summary>
/// Typed debugger view of Beetom's six common enemy words and nine extra-WRAM words. Values
/// remain native unsigned words so signed wrap, direction toggles, and table-index underflow
/// are visible while stepping the translated state machine.
/// </summary>
public sealed class BeetomEnemyState
{
    private readonly RoomEnemySlot _slot;
    private readonly ushort[] _installedInstructionLists;
    private readonly ushort[] _initialShortLeapYSpeedIndexes;
    private readonly ushort[] _initialLongLeapYSpeedIndexes;
    private readonly ushort[] _initialLungeYSpeedIndexes;
    private readonly ushort[] _fallingFlags;
    private readonly ushort[] _attachedFlags;
    private readonly ushort[] _directions;
    private readonly ushort[] _initialAttachmentXOffsets;
    private readonly ushort[] _initialAttachmentYOffsets;

    internal BeetomEnemyState(
        RoomEnemySlot slot,
        ushort[] installedInstructionLists,
        ushort[] initialShortLeapYSpeedIndexes,
        ushort[] initialLongLeapYSpeedIndexes,
        ushort[] initialLungeYSpeedIndexes,
        ushort[] fallingFlags,
        ushort[] attachedFlags,
        ushort[] directions,
        ushort[] initialAttachmentXOffsets,
        ushort[] initialAttachmentYOffsets)
    {
        _slot = slot;
        _installedInstructionLists = installedInstructionLists;
        _initialShortLeapYSpeedIndexes = initialShortLeapYSpeedIndexes;
        _initialLongLeapYSpeedIndexes = initialLongLeapYSpeedIndexes;
        _initialLungeYSpeedIndexes = initialLungeYSpeedIndexes;
        _fallingFlags = fallingFlags;
        _attachedFlags = attachedFlags;
        _directions = directions;
        _initialAttachmentXOffsets = initialAttachmentXOffsets;
        _initialAttachmentYOffsets = initialAttachmentYOffsets;
    }

    public ushort YSpeedTableIndex { get => _slot.VariableB; internal set => _slot.VariableB = value; }
    public BeetomEnemyFunction Function { get => (BeetomEnemyFunction)_slot.VariableC; internal set => _slot.VariableC = (ushort)value; }
    public ushort FunctionTimer { get => _slot.VariableD; internal set => _slot.VariableD = value; }
    public ushort ButtonCounter { get => _slot.VariableE; internal set => _slot.VariableE = value; }
    public ushort PreviousController1Input { get => _slot.VariableF; internal set => _slot.VariableF = value; }
    public ushort InstalledInstructionList { get => _installedInstructionLists[_slot.SlotIndex]; internal set => _installedInstructionLists[_slot.SlotIndex] = value; }
    public ushort InitialShortLeapYSpeedIndex { get => _initialShortLeapYSpeedIndexes[_slot.SlotIndex]; internal set => _initialShortLeapYSpeedIndexes[_slot.SlotIndex] = value; }
    public ushort InitialLongLeapYSpeedIndex { get => _initialLongLeapYSpeedIndexes[_slot.SlotIndex]; internal set => _initialLongLeapYSpeedIndexes[_slot.SlotIndex] = value; }
    public ushort InitialLungeYSpeedIndex { get => _initialLungeYSpeedIndexes[_slot.SlotIndex]; internal set => _initialLungeYSpeedIndexes[_slot.SlotIndex] = value; }
    public bool Falling { get => _fallingFlags[_slot.SlotIndex] != 0; internal set => _fallingFlags[_slot.SlotIndex] = value ? (ushort)1 : (ushort)0; }
    public bool AttachedToSamus { get => _attachedFlags[_slot.SlotIndex] != 0; internal set => _attachedFlags[_slot.SlotIndex] = value ? (ushort)1 : (ushort)0; }
    public ushort Direction { get => _directions[_slot.SlotIndex]; internal set => _directions[_slot.SlotIndex] = value; }
    public ushort InitialAttachmentXOffset { get => _initialAttachmentXOffsets[_slot.SlotIndex]; internal set => _initialAttachmentXOffsets[_slot.SlotIndex] = value; }
    public ushort InitialAttachmentYOffset { get => _initialAttachmentYOffsets[_slot.SlotIndex]; internal set => _initialAttachmentYOffsets[_slot.SlotIndex] = value; }
}

/// <summary>Literal translation of Beetom enemy <c>$E87F</c> at <c>$A8:B696-$BED2</c>.</summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort BeetomDefinition = 0xe87f;

    private const ushort BeetomCrawlingLeftInstruction = 0xb696;
    private const ushort BeetomHopLeftInstruction = 0xb6ac;
    private const ushort BeetomDrainingLeftInstruction = 0xb6cc;
    private const ushort BeetomCrawlingRightInstruction = 0xb6f2;
    private const ushort BeetomHopRightInstruction = 0xb708;
    private const ushort BeetomDrainingRightInstruction = 0xb728;
    private const ushort BeetomProximityDistance = 0x0060;
    private const ushort BeetomDrainSound = 0x002d;
    private const ushort BeetomMashCount = 0x0040;
    private const ushort BeetomMaximumYSpeedIndex = 0x0040;

    private readonly ushort[] _beetomInstalledInstructionLists = new ushort[MaximumEnemyCount];
    private readonly ushort[] _beetomInitialShortLeapYSpeedIndexes = new ushort[MaximumEnemyCount];
    private readonly ushort[] _beetomInitialLongLeapYSpeedIndexes = new ushort[MaximumEnemyCount];
    private readonly ushort[] _beetomInitialLungeYSpeedIndexes = new ushort[MaximumEnemyCount];
    private readonly ushort[] _beetomFallingFlags = new ushort[MaximumEnemyCount];
    private readonly ushort[] _beetomAttachedToSamusFlags = new ushort[MaximumEnemyCount];
    private readonly ushort[] _beetomDirections = new ushort[MaximumEnemyCount];
    private readonly ushort[] _beetomInitialAttachmentXOffsets = new ushort[MaximumEnemyCount];
    private readonly ushort[] _beetomInitialAttachmentYOffsets = new ushort[MaximumEnemyCount];
    private readonly BeetomEnemyState?[] _beetomStates = new BeetomEnemyState?[MaximumEnemyCount];

    /// <summary>Typed Beetom state for all 32 physical enemy slots.</summary>
    public IReadOnlyList<BeetomEnemyState?> BeetomStates => _beetomStates;

    /// <summary>Ports <c>InitAI_Beetom</c> at $A8:B776.</summary>
    private void InitializeBeetom(RoomEnemySlot slot, SamusState? samus, ushort controllerInput)
    {
        if (samus is null)
            throw new InvalidOperationException("Beetom initialization requires the active Samus actor.");

        var state = new BeetomEnemyState(
            slot,
            _beetomInstalledInstructionLists,
            _beetomInitialShortLeapYSpeedIndexes,
            _beetomInitialLongLeapYSpeedIndexes,
            _beetomInitialLungeYSpeedIndexes,
            _beetomFallingFlags,
            _beetomAttachedToSamusFlags,
            _beetomDirections,
            _beetomInitialAttachmentXOffsets,
            _beetomInitialAttachmentYOffsets);
        _beetomStates[slot.SlotIndex] = state;

        state.FunctionTimer = 0;
        state.Function = 0;
        state.Falling = false;
        state.AttachedToSamus = false;
        slot.VariableA = 0;
        state.ButtonCounter = BeetomMashCount;
        state.PreviousController1Input = controllerInput;

        // Every Beetom initializer writes the global seed, even when several Beetoms share
        // a room. This is observable cartridge behavior, not a per-actor random stream.
        Action<ushort> setRandomNumber = _setRandomNumber ?? throw new InvalidOperationException(
            "Beetom initialization requires the runtime random-seed writer.");
        setRandomNumber(0x0017);

        state.InitialShortLeapYSpeedIndex = CalculateInitialBeetomYSpeedIndex(0x3000, 4);
        state.InitialLongLeapYSpeedIndex = CalculateInitialBeetomYSpeedIndex(0x4000, 5);
        state.InitialLungeYSpeedIndex = CalculateInitialBeetomYSpeedIndex(0x3000, 3);

        // The source label's branch name is reversed. A negative Samus-minus-enemy delta
        // selects the left-facing list; nonnegative selects the right-facing list.
        SetBeetomInstructionList(
            slot,
            state,
            unchecked((short)(samus.XPosition - slot.XPosition)) < 0
                ? BeetomCrawlingLeftInstruction
                : BeetomCrawlingRightInstruction);
        state.Function = BeetomEnemyFunction.DecideAction;
    }

    /// <summary>Ports <c>MainAI_Beetom</c> and all indirect function targets.</summary>
    private void RunBeetomMain(
        RoomEnemySlot slot,
        BeetomEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        ushort controllerInput)
    {
        switch (state.Function)
        {
            case BeetomEnemyFunction.DecideAction:
                RequireBeetomSamus(samus);
                state.Function = WrappedMagnitude(unchecked((ushort)(samus!.XPosition - slot.XPosition))) < BeetomProximityDistance
                    ? BeetomEnemyFunction.DecideActionSamusInProximity
                    : BeetomEnemyFunction.DecideActionSamusNotInProximity;
                return;
            case BeetomEnemyFunction.DecideActionSamusNotInProximity:
                ChooseDistantBeetomAction(state);
                return;
            case BeetomEnemyFunction.StartIdling:
                state.FunctionTimer = 0x0020;
                state.Function = BeetomEnemyFunction.Idling;
                return;
            case BeetomEnemyFunction.StartCrawlingLeft:
                StartBeetomCrawling(slot, state, left: true);
                return;
            case BeetomEnemyFunction.StartCrawlingRight:
                StartBeetomCrawling(slot, state, left: false);
                return;
            case BeetomEnemyFunction.StartShortHopLeft:
                StartBeetomHop(slot, state, state.InitialShortLeapYSpeedIndex, left: true, longHop: false);
                return;
            case BeetomEnemyFunction.StartShortHopRight:
                StartBeetomHop(slot, state, state.InitialShortLeapYSpeedIndex, left: false, longHop: false);
                return;
            case BeetomEnemyFunction.StartLongHopLeft:
                StartBeetomHop(slot, state, state.InitialLongLeapYSpeedIndex, left: true, longHop: true);
                return;
            case BeetomEnemyFunction.StartLongHopRight:
                StartBeetomHop(slot, state, state.InitialLongLeapYSpeedIndex, left: false, longHop: true);
                return;
            case BeetomEnemyFunction.DecideActionSamusInProximity:
                RequireBeetomSamus(samus);
                StartBeetomLunge(slot, state, samus!);
                return;
            case BeetomEnemyFunction.StartDrainingLeft:
                StartBeetomDrain(slot, state, left: true);
                return;
            case BeetomEnemyFunction.StartDrainingRight:
                StartBeetomDrain(slot, state, left: false);
                return;
            case BeetomEnemyFunction.StartDropping:
                StartBeetomDrop(slot, state);
                return;
            case BeetomEnemyFunction.StartBeingFlung:
                state.YSpeedTableIndex = 0;
                state.Function = BeetomEnemyFunction.BeingFlung;
                return;
            case BeetomEnemyFunction.Idling:
                state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
                if (unchecked((short)state.FunctionTimer) < 0)
                    state.Function = BeetomEnemyFunction.DecideAction;
                return;
            case BeetomEnemyFunction.CrawlingLeft:
                RequireBeetomLevel(level);
                RunBeetomCrawl(slot, state, level!, left: true);
                return;
            case BeetomEnemyFunction.CrawlingRight:
                RequireBeetomLevel(level);
                RunBeetomCrawl(slot, state, level!, left: false);
                return;
            case BeetomEnemyFunction.ShortHopLeft:
                RequireBeetomLevel(level);
                RunBeetomArc(slot, state, level!, left: true, lunge: false, risingDelta: 4, fallingDelta: 4);
                return;
            case BeetomEnemyFunction.ShortHopRight:
                RequireBeetomLevel(level);
                RunBeetomArc(slot, state, level!, left: false, lunge: false, risingDelta: 4, fallingDelta: 4);
                return;
            case BeetomEnemyFunction.LongHopLeft:
                RequireBeetomLevel(level);
                RunBeetomArc(slot, state, level!, left: true, lunge: false, risingDelta: 5, fallingDelta: 5);
                return;
            case BeetomEnemyFunction.LongHopRight:
                RequireBeetomLevel(level);
                RunBeetomArc(slot, state, level!, left: false, lunge: false, risingDelta: 5, fallingDelta: 5);
                return;
            case BeetomEnemyFunction.LungeLeft:
                RequireBeetomLevel(level);
                RunBeetomArc(slot, state, level!, left: true, lunge: true, risingDelta: 5, fallingDelta: 3);
                return;
            case BeetomEnemyFunction.LungeRight:
                RequireBeetomLevel(level);
                RunBeetomArc(slot, state, level!, left: false, lunge: true, risingDelta: 5, fallingDelta: 3);
                return;
            case BeetomEnemyFunction.DrainingLeft:
                RequireBeetomSamus(samus);
                RequireBeetomLevel(level);
                RunBeetomDrain(slot, state, samus!, level!, controllerInput, left: true);
                return;
            case BeetomEnemyFunction.DrainingRight:
                RequireBeetomSamus(samus);
                RequireBeetomLevel(level);
                RunBeetomDrain(slot, state, samus!, level!, controllerInput, left: false);
                return;
            case BeetomEnemyFunction.Dropping:
                RequireBeetomLevel(level);
                RunBeetomDrop(slot, state, level!);
                return;
            case BeetomEnemyFunction.BeingFlung:
                RequireBeetomLevel(level);
                RunFlungBeetom(slot, state, level!);
                return;
            default:
                throw new NotSupportedException($"Beetom function $A8:{(ushort)state.Function:X4} is not translated.");
        }
    }

    private void ChooseDistantBeetomAction(BeetomEnemyState state)
    {
        ushort random = _nextRandom!();
        state.Function = (random & 7) switch
        {
            0 or 1 => BeetomEnemyFunction.StartIdling,
            2 or 4 => BeetomEnemyFunction.StartShortHopLeft,
            3 or 7 => BeetomEnemyFunction.StartLongHopRight,
            5 => BeetomEnemyFunction.StartShortHopRight,
            _ => BeetomEnemyFunction.StartLongHopLeft,
        };
        state.Direction = unchecked((ushort)(random & 1));
    }

    private static void StartBeetomCrawling(RoomEnemySlot slot, BeetomEnemyState state, bool left)
    {
        state.Function = left ? BeetomEnemyFunction.CrawlingLeft : BeetomEnemyFunction.CrawlingRight;
        SetBeetomInstructionList(slot, state, left ? BeetomCrawlingLeftInstruction : BeetomCrawlingRightInstruction);
    }

    private static void StartBeetomHop(
        RoomEnemySlot slot,
        BeetomEnemyState state,
        ushort initialYSpeedIndex,
        bool left,
        bool longHop)
    {
        state.YSpeedTableIndex = initialYSpeedIndex;
        state.Function = (left, longHop) switch
        {
            (true, false) => BeetomEnemyFunction.ShortHopLeft,
            (false, false) => BeetomEnemyFunction.ShortHopRight,
            (true, true) => BeetomEnemyFunction.LongHopLeft,
            _ => BeetomEnemyFunction.LongHopRight,
        };
        state.Falling = false;
        SetBeetomInstructionList(slot, state, left ? BeetomHopLeftInstruction : BeetomHopRightInstruction);
    }

    private static void StartBeetomLunge(RoomEnemySlot slot, BeetomEnemyState state, SamusState samus)
    {
        bool left = unchecked((short)(samus.XPosition - slot.XPosition)) < 0;
        state.YSpeedTableIndex = state.InitialLungeYSpeedIndex;
        state.Direction = left ? (ushort)0 : (ushort)1;
        state.Function = left ? BeetomEnemyFunction.LungeLeft : BeetomEnemyFunction.LungeRight;
        state.Falling = false;
        SetBeetomInstructionList(slot, state, left ? BeetomHopLeftInstruction : BeetomHopRightInstruction);
    }

    private static void StartBeetomDrain(RoomEnemySlot slot, BeetomEnemyState state, bool left)
    {
        SetBeetomInstructionList(slot, state, left ? BeetomDrainingLeftInstruction : BeetomDrainingRightInstruction);
        state.Function = left ? BeetomEnemyFunction.DrainingLeft : BeetomEnemyFunction.DrainingRight;
    }

    private static void StartBeetomDrop(RoomEnemySlot slot, BeetomEnemyState state)
    {
        SetBeetomInstructionList(
            slot,
            state,
            state.Direction == 0 ? BeetomCrawlingLeftInstruction : BeetomCrawlingRightInstruction);
        state.Falling = false;
        state.Function = BeetomEnemyFunction.Dropping;
    }

    private void RunBeetomCrawl(RoomEnemySlot slot, BeetomEnemyState state, RoomLevelData level, bool left)
    {
        state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
        if (unchecked((short)state.FunctionTimer) < 0)
        {
            state.FunctionTimer = 0x0040;
            state.Function = BeetomEnemyFunction.DecideAction;
            return;
        }

        // The native ledge probe temporarily moves the center eight pixels ahead, then
        // performs a real one-pixel downward collision move. Restore only the whole words,
        // preserving collision-produced subposition just as bank $A8 does.
        slot.XPosition = unchecked((ushort)(slot.XPosition + (left ? -8 : 8)));
        if (!MoveEnemyVertically(level, slot, 1 << 16))
        {
            state.Function = left ? BeetomEnemyFunction.StartCrawlingRight : BeetomEnemyFunction.StartCrawlingLeft;
            slot.YPosition = unchecked((ushort)(slot.YPosition - 1));
            slot.XPosition = unchecked((ushort)(slot.XPosition + (left ? 8 : -8)));
            return;
        }

        slot.XPosition = unchecked((ushort)(slot.XPosition + (left ? 8 : -8)));
        int displacement = left ? unchecked((int)0xffffc000) : 0x00004000;
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, displacement))
            state.Function = left ? BeetomEnemyFunction.StartCrawlingRight : BeetomEnemyFunction.StartCrawlingLeft;
    }

    private void RunBeetomArc(
        RoomEnemySlot slot,
        BeetomEnemyState state,
        RoomLevelData level,
        bool left,
        bool lunge,
        ushort risingDelta,
        ushort fallingDelta)
    {
        RunBeetomVerticalArc(slot, state, level, risingDelta, fallingDelta);

        // The horizontal half still runs on the exact frame that vertical collision changes
        // the function pointer. That ordering is easy to lose when expressing the two axes
        // as a single host movement vector.
        int displacement = lunge
            ? (left ? unchecked((int)0xfffd0000) : 0x00030000)
            : (left ? unchecked((int)0xffffc000) : 0x00004000);
        if (!MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, displacement))
            return;

        state.Direction ^= 1;
        state.Function = BeetomEnemyFunction.StartDropping;
    }

    private void RunBeetomVerticalArc(
        RoomEnemySlot slot,
        BeetomEnemyState state,
        RoomLevelData level,
        ushort risingDelta,
        ushort fallingDelta)
    {
        if (!state.Falling)
        {
            if (MoveEnemyVertically(level, slot, ReadQuadraticEnemySpeed(state.YSpeedTableIndex, negative: true)))
            {
                state.Function = BeetomEnemyFunction.StartDropping;
                return;
            }

            state.YSpeedTableIndex = unchecked((ushort)(state.YSpeedTableIndex - risingDelta));
            if (unchecked((short)state.YSpeedTableIndex) >= 0)
                return;

            state.YSpeedTableIndex = 0;
            state.Falling = true;
            return;
        }

        if (MoveEnemyVertically(level, slot, ReadQuadraticEnemySpeed(state.YSpeedTableIndex, negative: false)))
        {
            state.Function = BeetomEnemyFunction.DecideAction;
            return;
        }

        state.YSpeedTableIndex = unchecked((ushort)Math.Min(
            BeetomMaximumYSpeedIndex,
            state.YSpeedTableIndex + fallingDelta));
    }

    private void RunBeetomDrain(
        RoomEnemySlot slot,
        BeetomEnemyState state,
        SamusState samus,
        RoomLevelData level,
        ushort controllerInput,
        bool left)
    {
        if (state.ButtonCounter != 0)
        {
            slot.XPosition = samus.XPosition;
            slot.YPosition = unchecked((ushort)(samus.YPosition - 4));
            if (controllerInput != state.PreviousController1Input)
            {
                state.PreviousController1Input = controllerInput;
                state.ButtonCounter = unchecked((ushort)(state.ButtonCounter - 1));
            }
            return;
        }

        // Escape throws the Beetom sixteen pixels away. A wall reverses the direction and
        // applies an additional 32-pixel move, producing the native net sixteen-pixel throw
        // to the other side rather than merely cancelling the first displacement.
        int initialDisplacement = left ? 16 << 16 : unchecked((int)0xfff00000);
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, initialDisplacement))
        {
            state.Direction = left ? (ushort)1 : (ushort)0;
            int reflectedDisplacement = left ? unchecked((int)0xffe00000) : 32 << 16;
            MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, reflectedDisplacement);
        }
        state.AttachedToSamus = false;
        state.Function = BeetomEnemyFunction.StartBeingFlung;
    }

    private void RunBeetomDrop(RoomEnemySlot slot, BeetomEnemyState state, RoomLevelData level)
    {
        if (!MoveEnemyVertically(level, slot, 3 << 16))
            return;
        state.Function = state.Direction == 0
            ? BeetomEnemyFunction.StartCrawlingLeft
            : BeetomEnemyFunction.StartCrawlingRight;
    }

    private void RunFlungBeetom(RoomEnemySlot slot, BeetomEnemyState state, RoomLevelData level)
    {
        if (MoveEnemyVertically(level, slot, ReadQuadraticEnemySpeed(state.YSpeedTableIndex, negative: false)))
            state.Function = BeetomEnemyFunction.DecideAction;
        else
            state.YSpeedTableIndex = unchecked((ushort)Math.Min(BeetomMaximumYSpeedIndex, state.YSpeedTableIndex + 1));

        // Direction describes the pre-escape facing. The throw deliberately travels in the
        // opposite direction: zero moves right and one moves left.
        int horizontal = state.Direction == 0 ? 2 << 16 : unchecked((int)0xfffe0000);
        if (!MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, horizontal))
            return;
        state.Direction ^= 1;
        state.Function = BeetomEnemyFunction.StartDropping;
    }

    private ushort CalculateInitialBeetomYSpeedIndex(ushort targetHeight, ushort tableIndexDelta)
    {
        ushort tableIndex = 0;
        ushort accumulatedHeight = 0;
        for (int iteration = 0; iteration < ushort.MaxValue; iteration++)
        {
            tableIndex = unchecked((ushort)(tableIndex + tableIndexDelta));
            // The ROM reads at table+1, intentionally combining bytes from adjacent fixed-
            // point fields. An aligned host read changes every resulting jump arc.
            accumulatedHeight = unchecked((ushort)(accumulatedHeight + ReadWord(
                _bus!,
                QuadraticEnemySpeedTable + tableIndex * 8 + 1)));
            if (unchecked((short)(accumulatedHeight - targetHeight)) >= 0)
                return tableIndex;
        }
        throw new InvalidDataException("Beetom initial-speed calculation did not reach its ROM target height.");
    }

    /// <summary>Ports Beetom's custom touch handler at $A8:BE2E.</summary>
    private void ResolveBeetomTouch(
        RoomEnemySlot slot,
        BeetomEnemyState state,
        SamusState samus,
        ushort controllerInput)
    {
        if (!state.AttachedToSamus)
        {
            state.Function = state.Direction == 0
                ? BeetomEnemyFunction.StartDrainingLeft
                : BeetomEnemyFunction.StartDrainingRight;
            state.AttachedToSamus = true;
            state.ButtonCounter = BeetomMashCount;
            slot.Layer = 2;
            state.InitialAttachmentXOffset = unchecked((ushort)(samus.XPosition - slot.XPosition));
            state.InitialAttachmentYOffset = unchecked((ushort)(samus.YPosition - slot.YPosition));
        }

        if (samus.HorizontalSpeed.ContactDamageIndex != 0)
        {
            ResolveNormalEnemyTouch(slot, samus, controllerInput);
            samus.InvincibilityTimer = 0;
            samus.KnockbackTimer = 0;
            return;
        }

        if ((_randomEnemyCounter & 7) == 7 && samus.Health >= 30)
            LastBeetomSoundEffect = BeetomDrainSound;

        // Unlike ordinary touch damage, the once-per-64-frame drain explicitly cancels the
        // invincibility and knockback installed by common AI. Apply the suit-scaled health
        // subtraction directly, then reproduce those two clearing stores.
        if ((slot.FrameCounter & 0x003f) != 0x003f)
            return;
        ushort damage = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            ? unchecked((ushort)(slot.Definition.Damage >> 2))
            : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                ? unchecked((ushort)(slot.Definition.Damage >> 1))
                : slot.Definition.Damage;
        samus.Health = samus.Health <= damage
            ? (ushort)0
            : unchecked((ushort)(samus.Health - damage));
        samus.InvincibilityTimer = 0;
        samus.KnockbackTimer = 0;
    }

    /// <summary>Ports the private post-common-shot tail at $A8:BEAC.</summary>
    private static void ResolveBeetomShotAfterCommon(RoomEnemySlot slot, BeetomEnemyState state)
    {
        if (slot.FrozenTimer != 0 && state.Function is
            BeetomEnemyFunction.DrainingLeft or BeetomEnemyFunction.DrainingRight)
        {
            state.Function = BeetomEnemyFunction.StartDropping;
        }
        state.AttachedToSamus = false;
    }

    private static void SetBeetomInstructionList(
        RoomEnemySlot slot,
        BeetomEnemyState state,
        ushort instructionList)
    {
        state.InstalledInstructionList = instructionList;
        slot.CurrentInstruction = instructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private static void RequireBeetomSamus(SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Beetom AI requires the active Samus actor.");
    }

    private static void RequireBeetomLevel(RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Beetom movement requires room level data.");
    }

    private BeetomEnemyState RequireBeetomState(RoomEnemySlot slot) =>
        _beetomStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Beetom state.");
}
