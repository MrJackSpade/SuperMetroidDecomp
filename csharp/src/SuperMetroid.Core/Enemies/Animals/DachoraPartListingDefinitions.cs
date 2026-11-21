namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>How the room lists the Dachora and its echoes (#2556): its four speed-boost echoes right after it.</summary>
internal static partial class DachoraPartListingDefinitions
{
    /// <summary>The Dachora, then its four echoes.</summary>
    internal static EnemyComposite<DachoraRole> Listing { get; } = new([DachoraRole.Runner, .. DefinitionRun.Repeated(DachoraRole.SpeedBoostEcho, EchoCount)]);

    /// <summary>The speed-boost echoes that trail a Dachora, listed right after it (#2557).</summary>
    internal const int EchoCount = 4;
}
