namespace SuperMetroid.Core.Enemies;

/// <summary>Fixed NTSC launch velocities for Alcoon and Fune/Namihe projectiles.</summary>
public static class EnemyFireballLaunchDefinitions
{
    /// <summary>The quarter-pixel step, in 8.8, by which each Fune/Namihe launch record is faster than the last.</summary>
    internal const int NamiFuneSpeedStep = 64;

    /// <summary>The authored Fune/Namihe launch records.</summary>
    internal const int NamiFuneRecordCount = 8;
}
