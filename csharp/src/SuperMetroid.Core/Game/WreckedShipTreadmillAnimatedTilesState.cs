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
    }

    /// <summary>
    /// Mirrors <c>Spawn_AnimatedTilesObject</c> for object $8275 or $827B. Both lists begin
    /// on the wait-for-area-boss instruction with an instruction timer of one.
    /// </summary>
    public void Start(WreckedShipTreadmillDirection direction)
    {
        IsActive = true;
        Direction = direction;
        NextFrameIndex = 0;
        LastSourceAddress = null;
    }

    /// <summary>
    /// Runs one bank-$87 handler pass and publishes the source selected for the following
    /// NMI. While Phantoon's area-boss bit is clear, $87:81BA rewinds onto itself and no
    /// source exists; after the bit is set, the four one-frame entries loop forever.
    /// </summary>
    public void Step(bool areaBossDefeated, VramWriteQueue writes)
    {
        ArgumentNullException.ThrowIfNull(writes);
        LastSourceAddress = null;
        if (!IsActive || !areaBossDefeated)
            return;

        ReadOnlySpan<int> sources = Direction switch
        {
            WreckedShipTreadmillDirection.Rightwards =>
            [
                WreckedShipTreadmillRomData.Frame0Source,
                WreckedShipTreadmillRomData.Frame1Source,
                WreckedShipTreadmillRomData.Frame2Source,
                WreckedShipTreadmillRomData.Frame3Source,
            ],
            WreckedShipTreadmillDirection.Leftwards =>
            [
                WreckedShipTreadmillRomData.Frame3Source,
                WreckedShipTreadmillRomData.Frame2Source,
                WreckedShipTreadmillRomData.Frame1Source,
                WreckedShipTreadmillRomData.Frame0Source,
            ],
            _ => throw new InvalidDataException(
                $"Unknown Wrecked Ship treadmill direction {Direction}."),
        };

        int source = sources[NextFrameIndex];
        LastSourceAddress = source;
        writes.Enqueue(
            WreckedShipTreadmillRomData.TransferByteCount,
            source,
            WreckedShipTreadmillRomData.EncodedVramDestination);
        NextFrameIndex = (NextFrameIndex + 1) & 3;
    }
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
