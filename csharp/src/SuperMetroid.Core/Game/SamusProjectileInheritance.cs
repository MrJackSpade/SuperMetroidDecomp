using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Owns the native projectile-inheritance WRAM records. They are deliberately not
/// reconstructed from net frame displacement: individual movement calls overwrite
/// one direction, and the velocity reader crosses adjacent field boundaries.
/// </summary>
internal static class SamusProjectileInheritance
{
    /// <summary>Combines the projectile's base speed with the native direction-dependent WRAM inheritance records.</summary>
    /// <param name="memory">Live SNES memory containing the movement and camera subspeed words.</param>
    /// <param name="directionWord">Projectile direction word; its low nibble selects the inherited axes.</param>
    /// <param name="baseSpeed">Unsigned-magnitude launch speed applied along the selected direction.</param>
    /// <returns>Signed horizontal and vertical projectile velocities.</returns>
    /// <exception cref="InvalidDataException">The low direction nibble is outside the supported native range 0 through 9.</exception>
    internal static (short X, short Y) ReadVelocity(ISnesMutableMemory memory, ushort directionWord, short baseSpeed)
    {
        int direction = directionWord & 15;
        if (direction > 9)
            throw new InvalidDataException($"Projectile velocity initialization received invalid direction ${direction:X2}.");
        ushort upward = SnesWorkRam.ReadWord(memory, SamusProjectileInheritanceAddresses.Up - 1);
        int upContribution = (upward & 0xff00) == 0 ? 0 : (upward >> 2) | 0xc000;
        short x = unchecked((short)(direction switch
        {
            1 or 2 or 3 => baseSpeed + SnesWorkRam.ReadWord(memory, SamusProjectileInheritanceAddresses.Right - 1),
            6 or 7 or 8 => -baseSpeed + SnesWorkRam.ReadWord(memory, SamusProjectileInheritanceAddresses.Left - 1),
            _ => 0,
        }));
        short y = unchecked((short)(direction switch
        {
            0 or 1 or 8 or 9 => -baseSpeed + upContribution,
            3 or 4 or 5 or 6 => baseSpeed + SnesWorkRam.ReadWord(memory, SamusProjectileInheritanceAddresses.Down - 1),
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

    /// <summary>Publishes the camera's Y fractional movement word, whose adjacent byte participates in native projectile inheritance.</summary>
    /// <param name="bus">Address space receiving the WRAM update.</param>
    /// <param name="value">Current camera Y subspeed word.</param>
    internal static void PublishCameraYSubspeed(ISnesAddressSpace bus, ushort value) =>
        WriteWord(bus, SamusProjectileInheritanceAddresses.CameraYSubspeed, value);

    /// <summary>Stores one accepted directional movement displacement in the native projectile-inheritance record.</summary>
    /// <param name="bus">Address space receiving the high and fractional words.</param>
    /// <param name="direction">Axis direction whose latest movement record is replaced.</param>
    /// <param name="signedDisplacement">Signed fixed-point displacement accepted by that movement call.</param>
    /// <exception cref="ArgumentOutOfRangeException">The direction is not a directional movement axis.</exception>
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

    /// <summary>Writes a 16-bit value to adjacent WRAM bytes in SNES little-endian order.</summary>
    /// <param name="bus">Address space receiving the bytes.</param>
    /// <param name="address">WRAM address of the low byte.</param>
    /// <param name="value">Word to store.</param>
    private static void WriteWord(ISnesAddressSpace bus, int address, ushort value)
    {
        bus.WriteByte(address, unchecked((byte)value));
        bus.WriteByte(address + 1, (byte)(value >> 8));
    }
}
