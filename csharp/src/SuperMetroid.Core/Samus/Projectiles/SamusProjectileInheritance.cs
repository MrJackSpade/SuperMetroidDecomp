using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Owns the native projectile-inheritance WRAM records. They are deliberately not
/// reconstructed from net frame displacement: individual movement calls overwrite
/// one direction, and the velocity reader crosses adjacent field boundaries.
/// </summary>
internal static class SamusProjectileInheritance
{
    /// <summary>Combines a shot's base speed with direction-specific overlapping WRAM words using native signed arithmetic.</summary>
    internal static EightEightVector<RoomPixels> ReadVelocity(ISnesAddressSpace bus, ushort directionWord, short baseSpeed)
    {
        var word = new SamusProjectileDirectionWord(directionWord);
        if (word.DirectionIndex > 9)
            throw new InvalidDataException($"Projectile velocity initialization received invalid direction ${word.DirectionIndex:X2}.");
        SamusProjectileDirection direction = word.Direction;
        ProjectileInheritanceMemory memory = Memory(bus);
        ushort upward = memory.LaunchVelocityWord(SamusCollisionDirection.Up);
        int upContribution = (SnesLittleEndian.HighByteInPlace(upward)) == 0 ? 0 : (upward >> 2) | 0xc000;
        short x = unchecked((short)(direction switch
        {
            SamusProjectileDirection.UpRight or SamusProjectileDirection.Right or SamusProjectileDirection.DownRight =>
                baseSpeed + memory.LaunchVelocityWord(SamusCollisionDirection.Right),
            SamusProjectileDirection.DownLeft or SamusProjectileDirection.Left or SamusProjectileDirection.UpLeft =>
                -baseSpeed + memory.LaunchVelocityWord(SamusCollisionDirection.Left),
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
                baseSpeed + memory.LaunchVelocityWord(SamusCollisionDirection.Down),
            SamusProjectileDirection.Right or SamusProjectileDirection.Left => 0,
            _ => throw new InvalidOperationException($"Undefined projectile direction {direction}."),
        }));
        return new(x, y);
    }

    /// <summary>Native alpha tail clears directional pairs, but not camera Y subspeed.</summary>
    internal static void ClearMovement(ISnesAddressSpace bus) => Memory(bus).ClearMovement();

    /// <summary>Writes camera Y subspeed, whose high byte influences leftward launch inheritance.</summary>
    internal static void PublishCameraYSubspeed(ISnesAddressSpace bus, ushort value)
    {
        ProjectileInheritanceMemory memory = Memory(bus);
        memory.CameraYSubspeed = value;
    }

    /// <summary>Publishes one movement call's signed displacement to the matching direction record.</summary>
    internal static void Record(ISnesAddressSpace bus, SamusCollisionDirection direction, int signedDisplacement) =>
        Memory(bus).Record(direction, signedDisplacement);

    /// <summary>Requires a live WRAM-capable bus for the projectile inheritance view.</summary>
    private static ProjectileInheritanceMemory Memory(ISnesAddressSpace bus) =>
        new(SnesMemoryBusRequirement.Require(bus, "Projectile inheritance requires live WRAM."));
}
