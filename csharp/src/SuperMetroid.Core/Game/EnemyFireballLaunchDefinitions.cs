namespace SuperMetroid.Core.Game;

/// <summary>Fixed NTSC launch velocities for Alcoon and Fune/Namihe projectiles.</summary>
public static class EnemyFireballLaunchDefinitions
{
    /// <summary>$86:9EF9, Alcoon fireball Y velocities selected by byte offsets 0, 2 and 4.</summary>
    public const int AlcoonReferenceAddress = 0x869ef9;
    /// <summary>$86:DEB6, NamiFuneFireball_XVelocityTable: eight signed left/right pairs.</summary>
    public const int NamiFuneReferenceAddress = 0x86deb6;

    /// <summary>Maps the three even native selectors to upward, level and downward
    /// one-pixel/frame launches, preserving the signed8.8 word representation.</summary>
    public static ushort AlcoonYVelocity(ushort byteOffset)
    {
        if (byteOffset > 4 || (byteOffset & 1) != 0)
            throw new InvalidDataException($"Alcoon launch byte offset {byteOffset} is not 0, 2 or 4.");
        return unchecked((ushort)((byteOffset / 2 - 1) * 256));
    }

    /// <summary>Parameter two's low byte selects quarter-pixel/frame increments1..8,
    /// mirrored by direction. The native caller stores the left speed in YVelocity
    /// and the right speed in XVelocity; both are horizontal signed8.8 velocities.</summary>
    public static (ushort Left, ushort Right) NamiFuneVelocities(ushort parameter)
    {
        int index = (byte)parameter;
        if (index >= 8)
            throw new InvalidDataException($"Fune/Namihe launch index {index} exceeds eight authored records.");
        ushort magnitude = (ushort)((index + 1) * 0x40);
        return (unchecked((ushort)-magnitude), magnitude);
    }
}
