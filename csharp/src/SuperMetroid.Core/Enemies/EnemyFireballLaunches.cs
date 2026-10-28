using static SuperMetroid.Core.Enemies.EnemyFireballLaunchDefinitions;

namespace SuperMetroid.Core.Enemies;

/// <summary>The launch velocities of Alcoon's and Fune/Namihe's fireballs.</summary>
public static class EnemyFireballLaunches
{
    /// <summary>Maps the three even native selectors to upward, level and downward
    /// one-pixel/frame launches, preserving the signed8.8 word representation.</summary>
    public static ushort AlcoonYVelocity(int byteOffset)
    {
        if (byteOffset > 4 || (byteOffset & 1) != 0)
            throw new InvalidDataException($"Alcoon launch byte offset {byteOffset} is not 0, 2 or 4.");
        return unchecked((ushort)((byteOffset / 2 - 1) * 256));
    }

    /// <summary>Parameter two's low byte selects quarter-pixel/frame increments1..8,
    /// mirrored by direction. The native caller stores the left speed in YVelocity
    /// and the right speed in XVelocity; both are horizontal signed8.8 velocities.</summary>
    public static NamiFuneLaunchVelocities NamiFuneVelocities(int parameter)
    {
        int index = (byte)parameter;
        if (index >= NamiFuneRecordCount)
            throw new InvalidDataException($"Fune/Namihe launch index {index} exceeds eight authored records.");
        int magnitude = (ushort)((index + 1) * NamiFuneSpeedStep);
        return new NamiFuneLaunchVelocities(unchecked((ushort)-magnitude), magnitude);
    }
}
