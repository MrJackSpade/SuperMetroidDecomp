using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The enemies drawn under a graphics set that spawn each frame of its shared spawned-object sheet with the room's graphics
/// (#1513), by the frame's sheet name: each such frame is coloured in the colouring its spawners first show, over the rows
/// its parts ink, rather than the whole sheet in one merged colouring.
/// </summary>
internal sealed class SharedSpawnerFrames
{
    /// <summary>The spawners of each frame, in the order the plan lists them.</summary>
    private readonly Dictionary<string, List<EnemyDefinitionId>> spawnersOf;

    /// <summary>Holds the spawners <paramref name="spawnersOf"/> names for each frame.</summary>
    internal SharedSpawnerFrames(Dictionary<string, List<EnemyDefinitionId>> spawnersOf) => this.spawnersOf = Ensure.NotNull(spawnersOf);

    /// <summary>A sheet no enemy spawns onto with the room's graphics: every sheet but a set's shared one.</summary>
    internal static SharedSpawnerFrames None { get; } = new(new Dictionary<string, List<EnemyDefinitionId>>(StringComparer.Ordinal));

    /// <summary>The enemies that spawn the frame named <paramref name="frame"/> onto the sheet; none when no enemy does.</summary>
    internal List<EnemyDefinitionId> SpawnersOf(string frame) =>
        spawnersOf.TryGetValue(Ensure.NotNull(frame), out List<EnemyDefinitionId>? spawners) ? [.. spawners] : [];
}
