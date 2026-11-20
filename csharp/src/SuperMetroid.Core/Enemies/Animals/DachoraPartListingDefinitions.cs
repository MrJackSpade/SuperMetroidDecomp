namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>How the room lists the Dachora and its echoes (#2556): its four speed-boost echoes right after it.</summary>
internal static class DachoraPartListingDefinitions
{
    /// <summary>The Dachora, then its four echoes.</summary>
    internal static EnemyComposite<DachoraRole> Listing { get; } = new([DachoraRole.Runner, DachoraRole.SpeedBoostEcho, DachoraRole.SpeedBoostEcho, DachoraRole.SpeedBoostEcho, DachoraRole.SpeedBoostEcho]);
}
