using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

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
    private const int AnimatedTileBank = 0x870000;

    private ushort _objectPointer;
    private ushort _instructionPointer;
    private ushort _instructionTimer;
    private ushort _transferByteCount;
    private ushort _encodedVramDestination;

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
        _objectPointer = 0;
        _instructionPointer = 0;
        _instructionTimer = 0;
        _transferByteCount = 0;
        _encodedVramDestination = 0;
    }

    /// <summary>
    /// Mirrors <c>Spawn_AnimatedTilesObject</c> for object $8275 or $827B. Both lists begin
    /// on the wait-for-area-boss instruction with an instruction timer of one.
    /// </summary>
    public void Start(ISnesAddressSpace bus, WreckedShipTreadmillDirection direction)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ushort objectPointer = direction switch
        {
            WreckedShipTreadmillDirection.Rightwards =>
                AnimatedTileObjectPointers.WreckedShipTreadmillRightwards,
            WreckedShipTreadmillDirection.Leftwards =>
                AnimatedTileObjectPointers.WreckedShipTreadmillLeftwards,
            _ => throw new InvalidDataException(
                $"Unknown Wrecked Ship treadmill direction {direction}."),
        };

        IsActive = true;
        Direction = direction;
        NextFrameIndex = 0;
        LastSourceAddress = null;
        _objectPointer = objectPointer;
        _instructionPointer = ReadBank87Word(bus, objectPointer);
        _transferByteCount = ReadBank87Word(bus, unchecked((ushort)(objectPointer + 2)));
        _encodedVramDestination = ReadBank87Word(bus, unchecked((ushort)(objectPointer + 4)));
        _instructionTimer = 1;

        if (_transferByteCount == 0)
        {
            throw new InvalidDataException(
                $"Animated-tile object $87:{objectPointer:X4} has a zero transfer size.");
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
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(writes);
        LastSourceAddress = null;
        if (!IsActive)
            return;

        _instructionTimer = unchecked((ushort)(_instructionTimer - 1));
        if (_instructionTimer != 0)
            return;

        ushort cursor = _instructionPointer;
        for (int guard = 0; guard < 64; guard++)
        {
            ushort word = ReadBank87Word(bus, cursor);
            if ((word & 0x8000) == 0)
            {
                _instructionTimer = word;
                ushort sourcePointer = ReadBank87Word(
                    bus,
                    unchecked((ushort)(cursor + 2)));
                _instructionPointer = unchecked((ushort)(cursor + 4));
                LastSourceAddress = AnimatedTileBank | sourcePointer;
                writes.Enqueue(
                    _transferByteCount,
                    LastSourceAddress.Value,
                    _encodedVramDestination);
                NextFrameIndex = (NextFrameIndex + 1) & 3;
                return;
            }

            switch (word)
            {
                case AnimatedTileInstructionCodes.Delete:
                    Reset();
                    return;

                case AnimatedTileInstructionCodes.Goto:
                    cursor = ReadBank87Word(bus, unchecked((ushort)(cursor + 2)));
                    break;

                case AnimatedTileInstructionCodes.WaitUntilAreaBossIsDead:
                    if (!areaBossDefeated)
                    {
                        _instructionTimer = 1;
                        return;
                    }
                    cursor = unchecked((ushort)(cursor + 2));
                    break;

                default:
                    throw new NotSupportedException(
                        $"Animated-tile object $87:{_objectPointer:X4} instruction " +
                        $"$87:{word:X4} at $87:{cursor:X4} is not translated.");
            }
        }

        throw new InvalidDataException(
            $"Animated-tile object $87:{_objectPointer:X4} exceeded 64 leading " +
            $"instructions at $87:{cursor:X4}.");
    }

    private static ushort ReadBank87Word(ISnesAddressSpace bus, ushort pointer) =>
        RomDataReader.ReadWordFixedBank(bus, AnimatedTileBank | pointer);
}

/// <summary>Cartridge-owned constants for Wrecked Ship entrance treadmill animation.</summary>
internal static class WreckedShipTreadmillRomData
{
    /// <summary>First 32-byte graphics frame at $87:8E64.</summary>
    public const int Frame0Source = 0x878e64;

    /// <summary>Second 32-byte graphics frame at $87:8E84.</summary>
    public const int Frame1Source = 0x878e84;

    /// <summary>Third 32-byte graphics frame at $87:8EA4.</summary>
    public const int Frame2Source = 0x878ea4;

    /// <summary>Fourth 32-byte graphics frame at $87:8EC4.</summary>
    public const int Frame3Source = 0x878ec4;

    /// <summary>Size word in animated-tile objects $87:8275/$827B.</summary>
    public const ushort TransferByteCount = 0x0020;

    /// <summary>VRAM word address in animated-tile objects $87:8275/$827B.</summary>
    public const ushort EncodedVramDestination = 0x00e0;
}
