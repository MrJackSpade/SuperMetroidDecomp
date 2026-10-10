using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A8 function pointer stored in Beetom's common variable two. Setup functions
/// intentionally remain separate states: on the cartridge, selecting an action consumes one
/// frame and installing that action consumes another before movement begins.
/// </summary>
public enum BeetomEnemyFunction : ushort
{
    /// <summary><c>$A8:B814 Function_Beetom_DecideAction</c>: selects the near or distant decision entry from a strict 96-pixel horizontal proximity check.</summary>
    DecideAction = 0xb814,
    /// <summary><c>$A8:B82F Function_Beetom_DecideAction_SamusNotInProximity</c>: advances the shared RNG and selects an idle or hop setup, storing the low bit as direction.</summary>
    DecideActionSamusNotInProximity = 0xb82f,
    /// <summary><c>$A8:B84F Function_Beetom_StartIdling</c>: loads timer $20 and enters the stationary countdown on the next gameplay frame.</summary>
    StartIdling = 0xb84f,
    /// <summary><c>$A8:B85F Function_Beetom_StartCrawlingLeft</c>: installs left-facing crawling art and selects movement without moving on this setup frame.</summary>
    StartCrawlingLeft = 0xb85f,
    /// <summary><c>$A8:B873 Function_Beetom_StartCrawlingRight</c>: installs right-facing crawling art and selects movement without moving on this setup frame.</summary>
    StartCrawlingRight = 0xb873,
    /// <summary><c>$A8:B887 Function_Beetom_StartShortHopRight</c>: despite the inverted native label, initializes the rising leftward short hop and its art.</summary>
    StartShortHopLeft = 0xb887,
    /// <summary><c>$A8:B8A9 Function_Beetom_StartShortHopLeft</c>: despite the inverted native label, initializes the rising rightward short hop and its art.</summary>
    StartShortHopRight = 0xb8a9,
    /// <summary><c>$A8:B8CB Function_Beetom_StartLongHopLeft</c>: loads the long-hop speed-table index, clears falling, and installs left-facing hop art.</summary>
    StartLongHopLeft = 0xb8cb,
    /// <summary><c>$A8:B8ED Function_Beetom_StartLongHopRight</c>: loads the long-hop speed-table index, clears falling, and installs right-facing hop art.</summary>
    StartLongHopRight = 0xb8ed,
    /// <summary><c>$A8:B90F Function_Beetom_DecideAction_SamusInProximity</c>: aims the rising lunge toward Samus and installs the corresponding hop list.</summary>
    DecideActionSamusInProximity = 0xb90f,
    /// <summary><c>$A8:B952 Function_Beetom_StartDrainingSamus_FacingLeft</c>: installs left-facing attached art before the drain-following routine begins.</summary>
    StartDrainingLeft = 0xb952,
    /// <summary><c>$A8:B966 Function_Beetom_StartDrainingSamus_FacingRight</c>: installs right-facing attached art before the drain-following routine begins.</summary>
    StartDrainingRight = 0xb966,
    /// <summary><c>$A8:B97A Function_Beetom_StartDropping</c>: restores crawl art from the direction word, clears falling, and selects the straight drop.</summary>
    StartDropping = 0xb97a,
    /// <summary><c>$A8:B9A2 Function_Beetom_StartBeingFlung</c>: resets the downward speed-table index to zero after Samus shakes the Beetom off.</summary>
    StartBeingFlung = 0xb9a2,
    /// <summary><c>$A8:B9B2 Function_Beetom_Idling</c>: decrements the native word timer until its signed result is negative, then requests a new action.</summary>
    Idling = 0xb9b2,
    /// <summary><c>$A8:B9C1 Function_Beetom_CrawlingLeft</c>: moves left one quarter pixel per frame, reversing at a wall or failed eight-pixel-ahead ledge probe.</summary>
    CrawlingLeft = 0xb9c1,
    /// <summary><c>$A8:BA24 Function_Beetom_CrawlingRight</c>: moves right one quarter pixel per frame, reversing at a wall or failed eight-pixel-ahead ledge probe.</summary>
    CrawlingRight = 0xba24,
    /// <summary><c>$A8:BA84 Function_Beetom_ShortHopLeft</c>: moves left one quarter pixel per frame while the quadratic vertical index decreases or increases by four.</summary>
    ShortHopLeft = 0xba84,
    /// <summary><c>$A8:BAB7 Function_Beetom_ShortHopRight</c>: moves right one quarter pixel per frame while the quadratic vertical index decreases or increases by four.</summary>
    ShortHopRight = 0xbab7,
    /// <summary><c>$A8:BB55 Function_Beetom_LongHopLeft</c>: moves left one quarter pixel per frame while the quadratic vertical index decreases or increases by five.</summary>
    LongHopLeft = 0xbb55,
    /// <summary><c>$A8:BB88 Function_Beetom_LongHopRight</c>: moves right one quarter pixel per frame while the quadratic vertical index decreases or increases by five.</summary>
    LongHopRight = 0xbb88,
    /// <summary><c>$A8:BC26 Function_Beetom_LungeLeft</c>: lunges left three pixels per frame with vertical-index steps of minus five rising and plus three falling.</summary>
    LungeLeft = 0xbc26,
    /// <summary><c>$A8:BC5A Function_Beetom_LungeRight</c>: lunges right three pixels per frame with vertical-index steps of minus five rising and plus three falling.</summary>
    LungeRight = 0xbc5a,
    /// <summary><c>$A8:BCF8 Function_Beetom_DrainingSamus_FacingLeft</c>: follows Samus until the input-change counter reaches zero, then throws the Beetom right unless blocked.</summary>
    DrainingLeft = 0xbcf8,
    /// <summary><c>$A8:BD42 Function_Beetom_DrainingSamus_FacingRight</c>: follows Samus until the input-change counter reaches zero, then throws the Beetom left unless blocked.</summary>
    DrainingRight = 0xbd42,
    /// <summary><c>$A8:BD9D Function_Beetom_Dropping</c>: falls straight down three pixels per frame until collision selects crawling in the stored direction.</summary>
    Dropping = 0xbd9d,
    /// <summary><c>$A8:BDC5 Function_Beetom_BeingFlung</c>: falls with an increasing quadratic speed index while moving two pixels per frame opposite its pre-escape facing.</summary>
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

    /// <summary>Native variable B: quadratic-speed record index, converted to a byte offset by multiplying by eight; falling increases it up to $40.</summary>
    public ushort YSpeedTableIndex { get => _slot.VariableB; internal set => _slot.VariableB = value; }
    /// <summary>Native variable C: bank-$A8 function address dispatched once per enemy gameplay update, including separate decision and setup frames.</summary>
    public BeetomEnemyFunction Function { get => (BeetomEnemyFunction)_slot.VariableC; internal set => _slot.VariableC = (ushort)value; }
    /// <summary>Native variable D: idle/crawl countdown measured in gameplay frames; expiration tests the signed decrement result, not equality with zero.</summary>
    public ushort FunctionTimer { get => _slot.VariableD; internal set => _slot.VariableD = value; }
    /// <summary>Native variable E: remaining controller-word changes needed to escape attachment, reset to $40 by the initial touch.</summary>
    public ushort ButtonCounter { get => _slot.VariableE; internal set => _slot.VariableE = value; }
    /// <summary>Native variable F: input word last observed by initialization or the attached counter; any differing word, including releases, decrements <see cref="ButtonCounter"/>.</summary>
    public ushort PreviousController1Input { get => _slot.VariableF; internal set => _slot.VariableF = value; }
    /// <summary>Native $7E:7800,x cached bank-$A8 list pointer selected by setup; installation also resets the common instruction timer and loop counter.</summary>
    public ushort InstalledInstructionList { get => _installedInstructionLists[_slot.SlotIndex]; internal set => _installedInstructionLists[_slot.SlotIndex] = value; }
    /// <summary>Native $7E:7804,x short-hop starting record index, calculated using unaligned quadratic-table words until the $3000 height accumulator target is reached in steps of four.</summary>
    public ushort InitialShortLeapYSpeedIndex { get => _initialShortLeapYSpeedIndexes[_slot.SlotIndex]; internal set => _initialShortLeapYSpeedIndexes[_slot.SlotIndex] = value; }
    /// <summary>Native $7E:7806,x long-hop starting record index, calculated using unaligned quadratic-table words until the $4000 height accumulator target is reached in steps of five.</summary>
    public ushort InitialLongLeapYSpeedIndex { get => _initialLongLeapYSpeedIndexes[_slot.SlotIndex]; internal set => _initialLongLeapYSpeedIndexes[_slot.SlotIndex] = value; }
    /// <summary>Native $7E:7808,x lunge starting record index, calculated using unaligned quadratic-table words until the $3000 height accumulator target is reached in steps of three.</summary>
    public ushort InitialLungeYSpeedIndex { get => _initialLungeYSpeedIndexes[_slot.SlotIndex]; internal set => _initialLungeYSpeedIndexes[_slot.SlotIndex] = value; }
    /// <summary>Native $7E:780A,x arc phase: false uses the rising speed-table half; signed index underflow switches to the falling half at index zero.</summary>
    public bool Falling { get => _fallingFlags[_slot.SlotIndex] != 0; internal set => _fallingFlags[_slot.SlotIndex] = value ? (ushort)1 : (ushort)0; }
    /// <summary>Native $7E:7810,x attachment latch set on first touch and cleared after escape or the common-shot callback; it prevents repeated attachment setup.</summary>
    public bool AttachedToSamus { get => _attachedFlags[_slot.SlotIndex] != 0; internal set => _attachedFlags[_slot.SlotIndex] = value ? (ushort)1 : (ushort)0; }
    /// <summary>Native $7E:7812,x direction word: zero left-facing, one right-facing; collision toggles it, while the flung state moves opposite this facing.</summary>
    public ushort Direction { get => _directions[_slot.SlotIndex]; internal set => _directions[_slot.SlotIndex] = value; }
    /// <summary>Native $7E:780C,x signed room-pixel Samus-minus-Beetom X captured on initial attachment; retained although following snaps directly to Samus.</summary>
    public ushort InitialAttachmentXOffset { get => _initialAttachmentXOffsets[_slot.SlotIndex]; internal set => _initialAttachmentXOffsets[_slot.SlotIndex] = value; }
    /// <summary>Native $7E:780E,x signed room-pixel Samus-minus-Beetom Y captured on initial attachment; retained although following uses Samus Y minus four.</summary>
    public ushort InitialAttachmentYOffset { get => _initialAttachmentYOffsets[_slot.SlotIndex]; internal set => _initialAttachmentYOffsets[_slot.SlotIndex] = value; }
}

/// <summary>Literal translation of Beetom enemy <c>$E87F</c> at <c>$A8:B696-$BED2</c>.</summary>
public sealed partial class RoomEnemySystem
{

    private const ushort BeetomProximityDistance = 0x0060;
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
                ? BeetomInstructionProgramDefinitions.CrawlingLeft
                : BeetomInstructionProgramDefinitions.CrawlingRight);
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
                NativeWordCounterStep timer = NativeWordCounter.Decrement(state.FunctionTimer);
                state.FunctionTimer = timer.Value;
                if (timer.IsNegative)
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
                throw new InvalidDataException($"Beetom function $A8:{(ushort)state.Function:X4} is not translated.");
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
        SetBeetomInstructionList(
            slot,
            state,
            left
                ? BeetomInstructionProgramDefinitions.CrawlingLeft
                : BeetomInstructionProgramDefinitions.CrawlingRight);
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
        SetBeetomInstructionList(
            slot,
            state,
            left
                ? BeetomInstructionProgramDefinitions.HopLeft
                : BeetomInstructionProgramDefinitions.HopRight);
    }

    private static void StartBeetomLunge(RoomEnemySlot slot, BeetomEnemyState state, SamusState samus)
    {
        bool left = unchecked((short)(samus.XPosition - slot.XPosition)) < 0;
        state.YSpeedTableIndex = state.InitialLungeYSpeedIndex;
        state.Direction = left ? (ushort)0 : (ushort)1;
        state.Function = left ? BeetomEnemyFunction.LungeLeft : BeetomEnemyFunction.LungeRight;
        state.Falling = false;
        SetBeetomInstructionList(
            slot,
            state,
            left
                ? BeetomInstructionProgramDefinitions.HopLeft
                : BeetomInstructionProgramDefinitions.HopRight);
    }

    private static void StartBeetomDrain(RoomEnemySlot slot, BeetomEnemyState state, bool left)
    {
        SetBeetomInstructionList(
            slot,
            state,
            left
                ? BeetomInstructionProgramDefinitions.DrainingLeft
                : BeetomInstructionProgramDefinitions.DrainingRight);
        state.Function = left ? BeetomEnemyFunction.DrainingLeft : BeetomEnemyFunction.DrainingRight;
    }

    private static void StartBeetomDrop(RoomEnemySlot slot, BeetomEnemyState state)
    {
        SetBeetomInstructionList(
            slot,
            state,
            state.Direction == 0
                ? BeetomInstructionProgramDefinitions.CrawlingLeft
                : BeetomInstructionProgramDefinitions.CrawlingRight);
        state.Falling = false;
        state.Function = BeetomEnemyFunction.Dropping;
    }

    private void RunBeetomCrawl(RoomEnemySlot slot, BeetomEnemyState state, RoomLevelData level, bool left)
    {
        NativeWordCounterStep timer = NativeWordCounter.Decrement(state.FunctionTimer);
        state.FunctionTimer = timer.Value;
        if (timer.IsNegative)
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

    private static ushort CalculateInitialBeetomYSpeedIndex(ushort targetHeight, ushort tableIndexDelta)
    {
        ushort tableIndex = 0;
        ushort accumulatedHeight = 0;
        for (int iteration = 0; iteration < ushort.MaxValue; iteration++)
        {
            tableIndex = unchecked((ushort)(tableIndex + tableIndexDelta));
            // The ROM reads at table+1, intentionally combining bytes from adjacent fixed-
            // point fields. An aligned host read changes every resulting jump arc.
            accumulatedHeight = unchecked((ushort)(accumulatedHeight +
                EnemyQuadraticSpeedDefinitions.ReadWord(tableIndex * 8 + 1)));
            if (unchecked((short)(accumulatedHeight - targetHeight)) >= 0)
                return tableIndex;
        }
        throw new InvalidDataException("Beetom initial-speed calculation did not reach its ROM target height.");
    }

    /// <summary>Ports Beetom's custom touch handler at $A8:BE2E.</summary>
    private void ResolveBeetomTouch(
        RoomEnemySlot slot,
        BeetomEnemyState state,
        SamusState samus)
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
            ResolveNormalEnemyTouch(slot, samus);
            samus.InvincibilityTimer = 0;
            samus.KnockbackTimer = 0;
            return;
        }

        if ((_randomEnemyCounter & 7) == 7 && samus.Health >= 30)
            QueueEnemySound(SoundEffectLibrary3Sounds.AttachedEnemyDrain, maximumQueued: 6);

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
