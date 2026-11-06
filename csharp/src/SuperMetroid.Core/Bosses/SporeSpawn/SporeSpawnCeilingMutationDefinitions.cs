namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Where Spore Spawn's ceiling PLM is spawned, and the program each ceiling mutation runs.</summary>
internal static class SporeSpawnCeilingMutationDefinitions
{
    /// <summary>The room block both ceiling mutations are spawned at: the top-left block of the 2x2 ceiling.</summary>
    internal static readonly Position16<RoomBlocks> Block = new(7, 30);

    /// <summary>The ceiling program each mutation runs.</summary>
    internal static readonly IReadOnlyDictionary<SporeSpawnCeilingMutation, SporeSpawnCeilingProgram> Programs =
        new Dictionary<SporeSpawnCeilingMutation, SporeSpawnCeilingProgram>
        {
            [SporeSpawnCeilingMutation.Crumble] = SporeSpawnCeilingProgram.Crumble,
            [SporeSpawnCeilingMutation.Clear] = SporeSpawnCeilingProgram.Clear,
        };
}
