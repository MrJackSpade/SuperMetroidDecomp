namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>Which of the two patrolling platforms an enemy is (#2375): the two look different.</summary>
internal enum PlatformKind
{
    /// <summary>The Tripper, whose art shows whether it moves up or down.</summary>
    Tripper,

    /// <summary>The suspensor platform, which looks the same moving or still.</summary>
    SuspensorPlatform,
}
