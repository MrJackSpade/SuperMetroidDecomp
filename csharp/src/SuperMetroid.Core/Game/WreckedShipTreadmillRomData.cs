namespace SuperMetroid.Core.Game;

/// <summary>Cartridge-owned constants for Wrecked Ship entrance treadmill animation.</summary>
internal static class WreckedShipTreadmillRomData
{
    /// <summary>First 32-byte graphics frame at $87:8E64.</summary>
    public const int Frame0Source = 0x878e64;

    /// <summary>Second 32-byte graphics frame at $87:8E84.</summary>
    public const int Frame1Source = Frame0Source + TransferByteCount;

    /// <summary>Third 32-byte graphics frame at $87:8EA4.</summary>
    public const int Frame2Source = Frame0Source + 2 * TransferByteCount;

    /// <summary>Fourth 32-byte graphics frame at $87:8EC4.</summary>
    public const int Frame3Source = Frame0Source + 3 * TransferByteCount;

    /// <summary>Size word in animated-tile objects $87:8275/$827B.</summary>
    public const ushort TransferByteCount = 0x0020;

    /// <summary>VRAM word address in animated-tile objects $87:8275/$827B.</summary>
    public const ushort EncodedVramDestination = 0x00e0;

    /// <summary>Presentation source identity for one of the four native 32-byte frames.</summary>
    public static int FrameSource(int frameIndex)
    {
        if ((uint)frameIndex >= 4) throw new ArgumentOutOfRangeException(nameof(frameIndex));
        return Frame0Source + frameIndex * TransferByteCount;
    }
}
