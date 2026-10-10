using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Owns the native projectile-inheritance WRAM records. They are deliberately not
/// reconstructed from net frame displacement: individual movement calls overwrite
/// one direction, and the velocity reader crosses adjacent field boundaries.
/// </summary>
internal static class SamusProjectileInheritance
{
    internal static (short X, short Y) ReadVelocity(ISnesMutableMemory memory, ushort directionWord, short baseSpeed)
    {
        var word = new SamusProjectileDirectionWord(directionWord);
        if (word.DirectionIndex > 9)
            throw new InvalidDataException($"Projectile velocity initialization received invalid direction ${word.DirectionIndex:X2}.");
        SamusProjectileDirection direction = word.Direction;
        ushort upward = SnesWorkRam.ReadWord(memory, SamusProjectileInheritanceAddresses.Up - 1);
        int upContribution = (upward & 0xff00) == 0 ? 0 : (upward >> 2) | 0xc000;
        short x = unchecked((short)(direction switch
        {
            SamusProjectileDirection.UpRight or SamusProjectileDirection.Right or SamusProjectileDirection.DownRight =>
                baseSpeed + SnesWorkRam.ReadWord(memory, SamusProjectileInheritanceAddresses.Right - 1),
            SamusProjectileDirection.DownLeft or SamusProjectileDirection.Left or SamusProjectileDirection.UpLeft =>
                -baseSpeed + SnesWorkRam.ReadWord(memory, SamusProjectileInheritanceAddresses.Left - 1),
            SamusProjectileDirection.UpFacingRight or SamusProjectileDirection.DownFacingRight or
                SamusProjectileDirection.DownFacingLeft or SamusProjectileDirection.UpFacingLeft => 0,
            _ => throw new InvalidOperationException($"Undefined projectile direction {direction}."),
        }));
        short y = unchecked((short)(direction switch
        {
            SamusProjectileDirection.UpFacingRight or SamusProjectileDirection.UpRight or
                SamusProjectileDirection.UpLeft or SamusProjectileDirection.UpFacingLeft => -baseSpeed + upContribution,
            SamusProjectileDirection.DownRight or SamusProjectileDirection.DownFacingRight or
                SamusProjectileDirection.DownFacingLeft or SamusProjectileDirection.DownLeft =>
                baseSpeed + SnesWorkRam.ReadWord(memory, SamusProjectileInheritanceAddresses.Down - 1),
            SamusProjectileDirection.Right or SamusProjectileDirection.Left => 0,
            _ => throw new InvalidOperationException($"Undefined projectile direction {direction}."),
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

    private static void WriteWord(ISnesAddressSpace bus, int address, ushort value)
    {
        bus.WriteByte(address, unchecked((byte)value));
        bus.WriteByte(address + 1, (byte)(value >> 8));
    }
}
