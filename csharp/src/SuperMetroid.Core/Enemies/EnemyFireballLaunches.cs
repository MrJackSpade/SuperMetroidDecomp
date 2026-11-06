using static SuperMetroid.Core.Enemies.EnemyFireballLaunchDefinitions;

namespace SuperMetroid.Core.Enemies;

/// <summary>The launch velocities of Fune/Namihe's fireballs.</summary>
public static class EnemyFireballLaunches
{
    /// <summary>Parameter two's low byte selects quarter-pixel/frame increments1..8,
    /// mirrored by direction: the left and right horizontal signed8.8 velocities.</summary>
    public static NamiFuneLaunchVelocities NamiFuneVelocities(int parameter)
    {
        int index = WordWrap.LowByte(parameter);
        if (index >= NamiFuneRecordCount)
            throw new InvalidDataException($"Fune/Namihe launch index {index} exceeds eight authored records.");
        int magnitude = WordWrap.Unsigned((index + 1) * NamiFuneSpeedStep);
        return new NamiFuneLaunchVelocities(WordWrap.Unsigned(-magnitude), magnitude);
    }
}
