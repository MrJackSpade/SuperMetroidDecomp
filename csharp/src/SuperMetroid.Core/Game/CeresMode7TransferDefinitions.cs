using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>One $A6 Mode 7 low-byte tilemap DMA descriptor and its authored source bytes.</summary>
public readonly record struct CeresMode7Transfer(
    int SourceAddress, ushort DestinationWord, ReadOnlyMemory<byte> TileNumbers);

/// <summary>
/// The seven fixed transfer lists selected by the Ceres door/elevator and Ridley
/// animation routines. These are tilemap instructions, not character pixels: $2118
/// replaces only the low byte of each VRAM word and leaves its high byte untouched.
/// </summary>
public static class CeresMode7TransferDefinitions
{
    /// <summary>$A6:F904, Ceres elevator landing-platform light frame.</summary>
    public const ushort ElevatorLight = 0xf904;
    /// <summary>$A6:F90E, Ceres elevator landing-platform dark frame.</summary>
    public const ushort ElevatorDark = 0xf90e;
    /// <summary>$A6:ACE2, baby-capsule tilemap frame zero.</summary>
    public const ushort BabyFrame0 = 0xace2;
    /// <summary>$A6:ACF5, baby-capsule tilemap frame one (also frame three).</summary>
    public const ushort BabyFrame1 = 0xacf5;
    /// <summary>$A6:AD08, baby-capsule tilemap frame two.</summary>
    public const ushort BabyFrame2 = 0xad08;
    /// <summary>$A6:AD49, Ridley wing tilemap frame zero.</summary>
    public const ushort WingFrame0 = 0xad49;
    /// <summary>$A6:AD80, Ridley wing tilemap frame one.</summary>
    public const ushort WingFrame1 = 0xad80;

    private static readonly CeresMode7Transfer[] PlatformLight =
    [
        new(0xa6f918, 0x060e, new byte[] { 0x68, 0x69, 0x69, 0x78 }),
    ];
    private static readonly CeresMode7Transfer[] PlatformDark =
    [
        new(0xa6f91c, 0x060e, new byte[] { 0x8d, 0x8e, 0x8e, 0x79 }),
    ];
    private static readonly CeresMode7Transfer[] Baby0 =
    [
        new(0xa6ad1b, 0x0504, new byte[] { 0x59, 0x5a }),
        new(0xa6ad1d, 0x0584, new byte[] { 0x69, 0x6a }),
    ];
    private static readonly CeresMode7Transfer[] Baby1 =
    [
        new(0xa6ad1f, 0x0504, new byte[] { 0x8a, 0x8b }),
        new(0xa6ad21, 0x0584, new byte[] { 0x8c, 0x8d }),
    ];
    private static readonly CeresMode7Transfer[] Baby2 =
    [
        new(0xa6ad23, 0x0504, new byte[] { 0x8e, 0x8f }),
        new(0xa6ad25, 0x0584, new byte[] { 0x9d, 0x9e }),
    ];
    private static readonly CeresMode7Transfer[] Wing0 =
    [
        new(0xa6adb7, 0x000b, new byte[] { 0x00, 0x01, 0x02, 0x03 }),
        new(0xa6adbf, 0x0080, new byte[] { 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0xff, 0xff, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f }),
        new(0xa6addb, 0x0100, new byte[] { 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0xa8 }),
        new(0xa6adf7, 0x0181, new byte[] { 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2a, 0x2b, 0x2c }),
        new(0xa6ae0f, 0x0201, new byte[] { 0xff, 0xff, 0x1d, 0x1e, 0x1f, 0x30, 0x31, 0x32, 0x33, 0x34, 0xff, 0xff, 0xff, 0xff, 0xff }),
        new(0xa6ae2d, 0x0280, new byte[] { 0xff, 0xff, 0xff, 0xff, 0x2e, 0x2f, 0x40, 0x41, 0x42, 0x43, 0x44, 0xff, 0xff, 0xff, 0xff, 0xff }),
    ];
    private static readonly CeresMode7Transfer[] Wing1 =
    [
        new(0xa6adbb, 0x000b, new byte[] { 0xff, 0xff, 0xff, 0xff }),
        new(0xa6adcd, 0x0080, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff }),
        new(0xa6ade9, 0x0100, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x20, 0x17, 0xaa, 0xff, 0xff, 0xff, 0xff, 0xff }),
        new(0xa6ae03, 0x0181, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0x26, 0x27, 0x28, 0xff, 0xff, 0xff, 0xff }),
        new(0xa6ae1e, 0x0201, new byte[] { 0x91, 0x92, 0x93, 0x94, 0x95, 0x30, 0x31, 0x32, 0x96, 0x97, 0x98, 0x99, 0x9a, 0x98, 0x9c }),
        new(0xa6ae3d, 0x0280, new byte[] { 0x90, 0x9f, 0xa0, 0xa1, 0xa2, 0xa3, 0x40, 0x41, 0x42, 0xa4, 0xa5, 0xa6, 0xa7, 0x7d, 0x83, 0x2d }),
    ];

    /// <summary>Resolves a native list pointer without reading cartridge memory.</summary>
    public static ReadOnlySpan<CeresMode7Transfer> Get(ushort pointer) => pointer switch
    {
        ElevatorLight => PlatformLight,
        ElevatorDark => PlatformDark,
        BabyFrame0 => Baby0,
        BabyFrame1 => Baby1,
        BabyFrame2 => Baby2,
        WingFrame0 => Wing0,
        WingFrame1 => Wing1,
        _ => throw new InvalidDataException($"Unknown Ceres Mode 7 transfer list $A6:{pointer:X4}."),
    };

    /// <summary>Replays the native $2118 low-byte writes in list order.</summary>
    public static void ApplyTo(SnesVram vram, ushort pointer)
    {
        ArgumentNullException.ThrowIfNull(vram);
        foreach (CeresMode7Transfer transfer in Get(pointer))
            vram.LoadMode7MapBytes(transfer.TileNumbers.Span, transfer.DestinationWord);
    }
}
