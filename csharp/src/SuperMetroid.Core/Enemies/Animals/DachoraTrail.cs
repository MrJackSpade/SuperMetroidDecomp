namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// A Dachora and the speed-boost echoes that trail it (#2283): a room places one Dachora, and the Dachora brings its
/// <see cref="EchoCount"/> echoes, listed right after it where its behaviour finds them.
/// </summary>
internal static class DachoraTrail
{
    /// <summary>The speed-boost echoes that trail a Dachora, listed right after it (#2557).</summary>
    internal const int EchoCount = 4;

    /// <summary>
    /// The placements of a Dachora placed at <paramref name="placing"/>, followed by its echoes: each starts hidden where
    /// the Dachora stands, facing the way it faces, until its run places it in the trail.
    /// </summary>
    /// <param name="placing">Where the Dachora stands.</param>
    /// <param name="contact">How the Dachora and its echoes meet Samus.</param>
    /// <param name="processing">How the Dachora and its echoes are processed.</param>
    /// <param name="facing">The way the Dachora faces.</param>
    internal static RoomEnemyPlacement[] Placed(EnemyPlacing placing, EnemyContact contact, EnemyProcessing processing, HorizontalDirection facing)
    {
        var runner = new RoomEnemyPlacement(EnemyDefinitionId.Dachora, placing, contact, processing, EnemyPresence.None,
            CollidesByFrameHitboxes: false, new DachoraSpawnSettings(Facing: facing, Role: DachoraRole.Runner));
        var echo = new RoomEnemyPlacement(EnemyDefinitionId.Dachora, placing, contact, processing, EnemyPresence.Hidden,
            CollidesByFrameHitboxes: false, new DachoraSpawnSettings(Facing: facing, Role: DachoraRole.SpeedBoostEcho));
        return [runner, .. DefinitionRun.Repeated(echo, EchoCount)];
    }
}
