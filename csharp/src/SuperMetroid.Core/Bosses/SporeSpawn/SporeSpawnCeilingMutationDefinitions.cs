using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Where Spore Spawn's ceiling object is spawned, and the terrain change each ceiling mutation makes.</summary>
internal static class SporeSpawnCeilingMutationDefinitions
{
    /// <summary>The room block both ceiling mutations are spawned at: the top-left block of the 2x2 ceiling.</summary>
    internal static readonly Position16<RoomBlocks> Block = new(7, 30);

    /// <summary>The sound the crumble starts with.</summary>
    internal static readonly SoundEffectId CrumbleSoundId = EnemySound.BlockCrumble;

    /// <summary>How long each image of the ceiling shows.</summary>
    internal const int FrameDuration = 4;

    /// <summary>The cleared ceiling, the last image of both changes.</summary>
    private static readonly TerrainChangeFrame Cleared = new(RoomObjectDraw.Of(SporeSpawnCeilingDraw.Clear), FrameDuration);

    /// <summary>
    /// The change each mutation makes: the crumble plays its sound with the first of three crumble images and ends on
    /// the cleared ceiling; the clear of an already-defeated room shows only the cleared ceiling.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SporeSpawnCeilingMutation, TerrainChange> Changes =
        new Dictionary<SporeSpawnCeilingMutation, TerrainChange>
        {
            [SporeSpawnCeilingMutation.Crumble] = new(1,
            [
                new TerrainChangeFrame(RoomObjectDraw.Of(SporeSpawnCeilingDraw.CrumbleFirst), FrameDuration, SoundBefore: CrumbleSoundId),
                new TerrainChangeFrame(RoomObjectDraw.Of(SporeSpawnCeilingDraw.CrumbleSecond), FrameDuration),
                new TerrainChangeFrame(RoomObjectDraw.Of(SporeSpawnCeilingDraw.CrumbleThird), FrameDuration),
                Cleared,
            ]),
            [SporeSpawnCeilingMutation.Clear] = new(1, [Cleared]),
        };
}
