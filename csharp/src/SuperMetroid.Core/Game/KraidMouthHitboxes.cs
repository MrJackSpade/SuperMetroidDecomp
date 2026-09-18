using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Compiled collision geometry, independent of Kraid's editable head artwork.</summary>
internal static class KraidMouthHitboxes
{
    private const int Bank = 0xa70000;

    /// <summary>Aligned fixed records at $A7:9788..97C7.</summary>
    public static bool IsDefined(ushort pointer) => pointer >= 0x9788 && pointer <= 0x97c0 && ((pointer - 0x9788) & 7) == 0;

    /// <summary>
    /// $A7:9788..97C7, Hitbox_KraidMouth_0..7. The native projectile test uses
    /// left/top/bottom only; the authored right edge is retained as definition data.
    /// Entry four is unused but remains a defined all-zero rectangle.
    /// </summary>
    public static (short Left, short Top, short Right, short Bottom) Resolve(ushort pointer) => pointer switch
    {
        0x9788 => (16, -120, 40, -88),
        0x9790 => (16, -120, 40, -104),
        0x9798 => (16, -128, 40, -112),
        0x97a0 => (16, -128, 40, -120),
        0x97a8 => (0, 0, 0, 0),
        0x97b0 => (6, -96, 32, -80),
        0x97b8 => (0, -104, 32, -80),
        0x97c0 => (0, -112, 32, -80),
        _ => throw new InvalidDataException($"Undefined Kraid mouth hitbox $A7:{pointer:X4}."),
    };

    /// <summary>
    /// Resolves compiled cartridge geometry or a genuine bank-$A7 low-half live-memory,
    /// hardware, or open-bus alias. The seven upper-window bytes reachable when a low-half
    /// record crosses $7FFF are retained explicitly; unrelated cartridge addresses are not
    /// accepted as hitboxes.
    /// </summary>
    public static (short Left, short Top, short Bottom) ResolveCollision(
        ISnesAddressSpace bus,
        ushort pointer)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (IsDefined(pointer))
        {
            (short left, short top, _, short bottom) = Resolve(pointer);
            return (left, top, bottom);
        }
        if (pointer >= 0x8000)
        {
            throw new InvalidDataException(
                $"Kraid mouth hitbox $A7:{pointer:X4} is outside the compiled geometry catalog " +
                "and is not a live bank-$A7 low-half alias.");
        }

        return (
            unchecked((short)ReadLiveWord(bus, pointer)),
            unchecked((short)ReadLiveWord(bus, AddWithinBank(pointer, 2))),
            unchecked((short)ReadLiveWord(bus, AddWithinBank(pointer, 6))));
    }

    /// <summary>
    /// Exact bytes at <c>$A7:8000-$8006</c>, reachable only when a mutable low-half
    /// record beginning at <c>$A7:7FF9-$7FFF</c> crosses into the LoROM window.
    /// </summary>
    public static ReadOnlySpan<byte> LowHalfBoundaryBytes => [0x22, 0x6d, 0x9f, 0xa0, 0x6b, 0x22, 0x7d];

    private static ushort ReadLiveWord(ISnesAddressSpace bus, ushort pointer)
    {
        byte low = ReadLiveByte(bus, pointer);
        byte high = ReadLiveByte(bus, AddWithinBank(pointer, 1));
        return (ushort)(low | high << 8);
    }

    private static byte ReadLiveByte(ISnesAddressSpace bus, ushort pointer)
    {
        if (pointer < 0x8000)
            return bus.ReadByte(Bank | pointer);

        int boundaryIndex = pointer - 0x8000;
        ReadOnlySpan<byte> boundary = LowHalfBoundaryBytes;
        if ((uint)boundaryIndex < (uint)boundary.Length)
            return boundary[boundaryIndex];

        throw new InvalidDataException(
            $"Kraid low-half mouth geometry crossed into uncompiled cartridge address $A7:{pointer:X4}.");
    }

    private static ushort AddWithinBank(ushort pointer, int offset) =>
        unchecked((ushort)(pointer + offset));
}
