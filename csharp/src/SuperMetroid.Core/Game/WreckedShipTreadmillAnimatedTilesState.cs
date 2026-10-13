using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The direction encoded by Wrecked Ship's two bank-$87 treadmill animated-tile objects.
/// </summary>
public enum WreckedShipTreadmillDirection
{
    /// <summary>Frames $8E64,$8E84,$8EA4,$8EC4, selected by object $8275.</summary>
    Rightwards,

    /// <summary>Frames $8EC4,$8EA4,$8E84,$8E64, selected by object $827B.</summary>
    Leftwards,
}

/// <summary>
/// Translates the two cartridge animated-tile objects spawned by Wrecked Ship entrance
/// door setup. This is an object/list interpreter for the shared bank-$87 behavior, not a
/// room-coordinate animation: the door chooses a direction and the object owns its VRAM
/// destination, boss-bit wait, one-frame cadence, and source-frame order.
/// </summary>
public sealed class WreckedShipTreadmillAnimatedTilesState
{

    /// <summary>Bank-87 object identity selected by the room's entrance-door setup.</summary>
    private AnimatedTileObject _objectPointer;
    /// <summary>Next compiled instruction word to interpret for this animated-tile object.</summary>
    private ushort _instructionPointer;
    /// <summary>Actor-pass countdown before interpreting another frame or command.</summary>
    private NativeCountdown _instructionTimer;
    /// <summary>Number of source bytes queued for each treadmill tile DMA transfer.</summary>
    private ushort _transferByteCount;
    /// <summary>Native VRAM destination word retained for queued DMA conversion.</summary>
    private ushort _encodedVramDestination;

    /// <summary>The queued-DMA destination the treadmill object's native word decodes to.</summary>
    [PpuLayoutBoundary("codec: Wrecked Ship treadmill animated-tile object's native VRAM destination")]
    private VramDmaDestination Destination => VramDmaDestination.FromNative(_encodedVramDestination);
    /// <summary>Resolved object/list mechanics used after the stock direction and pointer are checked.</summary>
    private WreckedShipTreadmillObjectDefinition? _compiledMechanics;

    /// <summary>Whether one of the two door-spawned objects occupies its native slot.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Direction selected by the object header passed to $87:8027.</summary>
    public WreckedShipTreadmillDirection Direction { get; private set; }

    /// <summary>Index of the next entry in the four-frame cartridge list.</summary>
    public int NextFrameIndex { get; private set; }

    /// <summary>Most recent 24-bit source published for the next NMI transfer.</summary>
    public int? LastSourceAddress { get; private set; }

    /// <summary>Clears the room-owned object arrays during destination-room teardown.</summary>
    public void Reset()
    {
        IsActive = false;
        Direction = default;
        NextFrameIndex = 0;
        LastSourceAddress = null;
        _objectPointer = AnimatedTileObject.None;
        _instructionPointer = 0;
        _instructionTimer = NativeCountdown.Start(0);
        _transferByteCount = 0;
        _encodedVramDestination = 0;
        _compiledMechanics = null;
    }

    /// <summary>
    /// Mirrors <c>Spawn_AnimatedTilesObject</c> for object $8275 or $827B. Both lists begin
    /// on the wait-for-area-boss instruction with an instruction timer of one.
    /// </summary>
    public void Start(ISnesAddressSpace bus, WreckedShipTreadmillDirection direction)
    {
        Ensure.NotNull(bus);
        WreckedShipTreadmillObjectDefinition definition =
            WreckedShipTreadmillMechanicsDefinitions.ForDirection(direction);
        StartDefinition(bus, direction, definition.ObjectPointer);
    }

    /// <summary>
    /// Starts one bank-$87 definition. Stock pointers bind compiled mechanics; this seam
    /// retains strict generic-dispatch tests for deliberately constructed definitions.
    /// </summary>
    internal void StartDefinition(
        ISnesAddressSpace bus,
        WreckedShipTreadmillDirection direction,
        AnimatedTileObject objectPointer)
    {
        Ensure.NotNull(bus);

        IsActive = true;
        Direction = direction;
        NextFrameIndex = 0;
        LastSourceAddress = null;
        _objectPointer = objectPointer;
        if (!WreckedShipTreadmillMechanicsDefinitions.TryResolve(objectPointer, out _compiledMechanics))
            throw new InvalidDataException($"Wrecked Ship treadmill $87:{(int)objectPointer:X4} is not compiled.");
        if (_compiledMechanics.Direction != direction)
            throw new InvalidDataException(
                $"Wrecked Ship treadmill $87:{(int)objectPointer:X4} belongs to {_compiledMechanics.Direction}, not {direction}.");
        _instructionPointer = _compiledMechanics.WaitInstructionPointer;
        _transferByteCount = WreckedShipTreadmillRomData.TransferByteCount;
        _encodedVramDestination = WreckedShipTreadmillRomData.EncodedVramDestination;
        _instructionTimer = NativeCountdown.Start(1);

        if (_transferByteCount == 0)
        {
            throw new InvalidDataException(
                $"Animated-tile object $87:{(int)objectPointer:X4} has a zero transfer size.");
        }
    }

    /// <summary>
    /// Runs one bank-$87 handler pass and publishes the source selected for the following
    /// NMI. While Phantoon's area-boss bit is clear, $87:81BA rewinds onto itself and no
    /// source exists; after the bit is set, the four one-frame entries loop forever.
    /// </summary>
    public void Step(
        ISnesAddressSpace bus,
        bool areaBossDefeated,
        VramWriteQueue writes)
    {
        Ensure.NotNull(bus);
        Ensure.NotNull(writes);
        LastSourceAddress = null;
        if (!IsActive)
            return;

        _instructionTimer = _instructionTimer.Decrement().Remaining;
        if (!_instructionTimer.IsZero)
            return;

        ushort cursor = _instructionPointer;
        for (int guard = 0; guard < 64; guard++)
        {
            ushort word = ReadMechanicsWord(cursor);
            if ((word & 0x8000) == 0)
            {
                if (word == 0)
                {
                    throw new InvalidDataException(
                        $"Animated-tile object $87:{(int)_objectPointer:X4} has a zero-duration " +
                        $"frame at $87:{cursor:X4}.");
                }
                _instructionTimer = NativeCountdown.FromRaw(word);
                int sourceAddress = (_compiledMechanics ?? throw new InvalidOperationException(
                    "Wrecked Ship treadmill lost its compiled mechanics.")).FrameSourceAddress(cursor);
                _instructionPointer = unchecked((ushort)(cursor + 4));
                LastSourceAddress = sourceAddress;
                writes.Enqueue(
                    _transferByteCount,
                    LastSourceAddress.Value,
                    Destination);
                NextFrameIndex = (NextFrameIndex + 1) & 3;
                return;
            }

            switch (ClosedNativeWords.Decode<AnimatedTileInstruction>(word, "animated-tile instruction"))
            {
                case AnimatedTileInstruction.Delete:
                    Reset();
                    return;

                case AnimatedTileInstruction.Goto:
                    cursor = ReadMechanicsWord(unchecked((ushort)(cursor + 2)));
                    break;

                case AnimatedTileInstruction.WaitUntilAreaBossIsDead:
                    if (!areaBossDefeated)
                    {
                        _instructionTimer = NativeCountdown.Start(1);
                        return;
                    }
                    cursor = unchecked((ushort)(cursor + 2));
                    break;

                default:
                    throw new NotSupportedException(
                        $"Animated-tile object $87:{(int)_objectPointer:X4} instruction " +
                        $"$87:{word:X4} at $87:{cursor:X4} is not translated.");
            }
        }

        throw new InvalidDataException(
            $"Animated-tile object $87:{(int)_objectPointer:X4} exceeded 64 leading " +
            $"instructions at $87:{cursor:X4}.");
    }

    /// <summary>Reads a word from the selected compiled list and rejects pointers outside its authored mechanics.</summary>
    private ushort ReadMechanicsWord(ushort pointer)
    {
        if (_compiledMechanics is null)
            throw new InvalidOperationException("Wrecked Ship treadmill requires compiled animation mechanics.");
        if (_compiledMechanics.TryReadMechanicsWord(pointer, out ushort value))
            return value;

        throw new InvalidDataException(
            $"Wrecked Ship treadmill object $87:{(int)_objectPointer:X4} reached non-catalog " +
            $"mechanics word $87:{pointer:X4}.");
    }

}
