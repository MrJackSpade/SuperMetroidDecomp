namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// A Dachora and the speed-boost echoes that trail it (#2283): a room places one Dachora, and the Dachora brings its
/// <see cref="DachoraTimingDefinitions.EchoCount"/> echoes, listed right after it where its behaviour finds them.
/// </summary>
internal static class DachoraTrail
{
    /// <summary>
    /// The placements of the Dachora <paramref name="runner"/> places, followed by its echoes: each starts hidden where the
    /// Dachora stands, facing the way it faces, until its run places it in the trail.
    /// </summary>
    /// <param name="runner">The Dachora's own placement.</param>
    internal static RoomEnemyPlacement[] Placed(RoomEnemyPlacement runner)
    {
        DachoraSpawnSettings settings = runner.SpawnSettingsOf<DachoraSpawnSettings>();
        RoomEnemyPlacement echo = new(runner.DefinitionId, runner.Placing, runner.Contact, runner.Processing, EnemyPresence.Hidden,
            runner.CollidesByFrameHitboxes, settings with { Role = DachoraRole.SpeedBoostEcho });
        return [runner, .. DefinitionRun.Repeated(echo, DachoraTimingDefinitions.EchoCount)];
    }
}
