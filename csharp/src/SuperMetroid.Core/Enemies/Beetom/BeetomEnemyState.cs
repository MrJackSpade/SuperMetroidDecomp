using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>
/// Typed debugger view of Beetom's six common enemy words and nine extra-WRAM words. Values
/// remain native unsigned words so signed wrap, direction toggles, and table-index underflow
/// are visible while stepping the translated state machine.
/// </summary>
public sealed class BeetomEnemyState
{
    /// <summary>The owning enemy slot that stores Beetom's common native variables.</summary>
    private readonly RoomEnemySlot _slot;
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
    /// <summary>Where the typed animation (#641) resumes. Setup installs it in place of the native $7E:7800,x list pointer, which nothing reads back; installation also resets the common instruction timer and loop counter.</summary>
    internal AnimationProgramCursor<BeetomProgram> Program { get; set; }
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
