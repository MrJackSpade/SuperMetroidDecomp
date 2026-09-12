using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Owns the native projectile-inheritance WRAM records. They are deliberately not
/// reconstructed from net frame displacement: individual movement calls overwrite
/// one direction, and the velocity reader crosses adjacent field boundaries.
/// </summary>
internal static class SamusProjectileInheritance
{
    internal static (short X, short Y) ReadVelocity(ISnesAddressSpace bus, ushort directionWord, short baseSpeed)
    {
        int direction = directionWord & 15;
        if (direction > 9)
            throw new InvalidDataException($"Projectile velocity initialization received invalid direction ${direction:X2}.");
        ushort upward = ReadWord(bus, SamusProjectileInheritanceAddresses.Up - 1);
        int upContribution = (upward & 0xff00) == 0 ? 0 : (upward >> 2) | 0xc000;
        short x = unchecked((short)(direction switch
        {
            1 or 2 or 3 => baseSpeed + ReadWord(bus, SamusProjectileInheritanceAddresses.Right - 1),
            6 or 7 or 8 => -baseSpeed + ReadWord(bus, SamusProjectileInheritanceAddresses.Left - 1),
            _ => 0,
        }));
        short y = unchecked((short)(direction switch
        {
            0 or 1 or 8 or 9 => -baseSpeed + upContribution,
            3 or 4 or 5 or 6 => baseSpeed + ReadWord(bus, SamusProjectileInheritanceAddresses.Down - 1),
            _ => 0,
        }));
        return (x, y);
    }

    /// <summary>Native alpha tail clears directional pairs, but not camera Y subspeed.</summary>
    internal static void ClearMovement(ISnesAddressSpace bus)
    {
        for (int address = SamusProjectileInheritanceAddresses.Left;
             address < SamusProjectileInheritanceAddresses.Down + 4; address++)
            bus.WriteByte(address, 0);
    }

    internal static void PublishCameraYSubspeed(ISnesAddressSpace bus, ushort value) =>
        WriteWord(bus, SamusProjectileInheritanceAddresses.CameraYSubspeed, value);

    internal static void Record(ISnesAddressSpace bus, SamusCollisionDirection direction, int signedDisplacement)
    {
        int address = direction switch
        {
            SamusCollisionDirection.Left => SamusProjectileInheritanceAddresses.Left,
            SamusCollisionDirection.Right => SamusProjectileInheritanceAddresses.Right,
            SamusCollisionDirection.Up => SamusProjectileInheritanceAddresses.Up,
            SamusCollisionDirection.Down => SamusProjectileInheritanceAddresses.Down,
            _ => throw new ArgumentOutOfRangeException(nameof(direction)),
        };
        WriteWord(bus, address, unchecked((ushort)(signedDisplacement >> 16)));
        WriteWord(bus, address + 2, unchecked((ushort)signedDisplacement));
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private static void WriteWord(ISnesAddressSpace bus, int address, ushort value)
    {
        bus.WriteByte(address, unchecked((byte)value));
        bus.WriteByte(address + 1, (byte)(value >> 8));
    }
}
