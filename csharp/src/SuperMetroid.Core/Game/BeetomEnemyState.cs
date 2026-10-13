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
    /// <summary>The owning enemy slot that stores Beetom's common native variables.</summary>
    private readonly RoomEnemySlot _slot;
    /// <summary>Per-slot instruction-list pointers selected by Beetom setup states.</summary>
    private readonly ushort[] _installedInstructionLists;
    /// <summary>Per-slot starting quadratic-table indices for short hops.</summary>
    private readonly ushort[] _initialShortLeapYSpeedIndexes;
    /// <summary>Per-slot starting quadratic-table indices for long hops.</summary>
    private readonly ushort[] _initialLongLeapYSpeedIndexes;
    /// <summary>Per-slot starting quadratic-table indices for lunges toward Samus.</summary>
    private readonly ushort[] _initialLungeYSpeedIndexes;
    /// <summary>Per-slot flags selecting the falling half of an airborne arc.</summary>
    private readonly ushort[] _fallingFlags;
    /// <summary>Per-slot latches that prevent attachment from being initialized twice.</summary>
    private readonly ushort[] _attachedFlags;
    /// <summary>Per-slot native direction words used by movement and art selection.</summary>
    private readonly ushort[] _directions;
    /// <summary>Per-slot Samus-relative offsets captured on initial attachment.</summary>
    private readonly Offset16<RoomPixels>[] _initialAttachmentOffsets;

    /// <summary>Binds the native common words in a slot to the matching shared extra-WRAM arrays.</summary>
    internal BeetomEnemyState(RoomEnemySlot slot, BeetomSharedTables tables)
    {
        _slot = slot;
        _installedInstructionLists = tables.InstalledInstructionLists;
        _initialShortLeapYSpeedIndexes = tables.InitialShortLeapYSpeedIndexes;
        _initialLongLeapYSpeedIndexes = tables.InitialLongLeapYSpeedIndexes;
        _initialLungeYSpeedIndexes = tables.InitialLungeYSpeedIndexes;
        _fallingFlags = tables.FallingFlags;
        _attachedFlags = tables.AttachedFlags;
        _directions = tables.Directions;
        _initialAttachmentOffsets = tables.InitialAttachmentOffsets;
    }

    /// <summary>Native variable B: quadratic-speed record index, converted to a byte offset by multiplying by eight; falling increases it up to $40.</summary>
    public ushort YSpeedTableIndex { get => _slot.VariableB; internal set => _slot.VariableB = value; }
    /// <summary>Native variable C: bank-$A8 function address dispatched once per enemy gameplay update, including separate decision and setup frames.</summary>
    public BeetomEnemyFunction Function { get => (BeetomEnemyFunction)_slot.VariableC; internal set => _slot.VariableC = (ushort)value; }
    /// <summary>Native variable D: idle/crawl countdown measured in gameplay frames; expiration tests the signed decrement result, not equality with zero.</summary>
    public NativeCountdown FunctionTimer { get => NativeCountdown.FromRaw(_slot.VariableD); internal set => _slot.VariableD = value.RawValue; }
    /// <summary>Native variable E: remaining controller-word changes needed to escape attachment, reset to $40 by the initial touch.</summary>
    public NativeCountdown ButtonCounter { get => NativeCountdown.FromRaw(_slot.VariableE); internal set => _slot.VariableE = value.RawValue; }
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
    /// <summary>Native $7E:780C,x/$780E,x signed room-pixel Samus-minus-Beetom offset captured on initial attachment; retained although following snaps X directly to Samus and uses Samus Y minus four.</summary>
    public Offset16<RoomPixels> InitialAttachmentOffset { get => _initialAttachmentOffsets[_slot.SlotIndex]; internal set => _initialAttachmentOffsets[_slot.SlotIndex] = value; }
}
