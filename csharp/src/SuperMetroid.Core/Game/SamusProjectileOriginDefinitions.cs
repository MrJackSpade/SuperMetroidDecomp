using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Physical beam/missile muzzle positions, independent of charge-flare artwork.</summary>
internal static class SamusProjectileOriginDefinitions
{
    /// <summary>$90:C204..C253 ProjectileOriginOffsetsByDirection: default X/Y, then running/moonwalk X/Y, ten signed words each.</summary>
    private static ReadOnlySpan<short> Offsets =>
    [
        2, 13, 11, 13, 2, -5, -14, -11, -19, -2,
        -8, -13, 1, 4, 13, 13, 4, 1, -19, -8,
        2, 15, 15, 13, 2, -5, -13, -13, -15, -2,
        -8, -16, -2, 1, 13, 13, 1, -2, -16, -8,
    ];

    internal static (short X, short Y) Read(ISnesAddressSpace bus, bool running, ushort direction)
    {
        int offset = (direction & 0x0f) * sizeof(ushort);
        int x = running ? SamusProjectileRomData.Origins.RunningX : SamusProjectileRomData.Origins.DefaultX;
        int y = running ? SamusProjectileRomData.Origins.RunningY : SamusProjectileRomData.Origins.DefaultY;
        return (ReadWord(bus, x + offset), ReadWord(bus, y + offset));
    }

    private static short ReadWord(ISnesAddressSpace bus, int address)
    {
        int offset = address - SamusProjectileRomData.Origins.DefaultX;
        if (offset >= 0 && offset < Offsets.Length * sizeof(ushort) && (offset & 1) == 0)
            return Offsets[offset / sizeof(ushort)];
        // Resolve addresses before table ownership: low-nibble directions ten through
        // fifteen cross into adjacent rows, and running Y eventually reaches cooldowns.
        return unchecked((short)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
    }
}
