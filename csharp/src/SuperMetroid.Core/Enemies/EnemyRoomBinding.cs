using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The loaded room's bindings that <see cref="RoomEnemySystem"/> shares with the owners it composes: the room's
/// enemy definition source, the cartridge-state services its AI calls (including the enemy random source), and the
/// host-installed enemy artwork.
/// </summary>
internal sealed class EnemyRoomBinding
{
    /// <summary>Binds the room to the session whose enemy words outlive it.</summary>
    /// <param name="session">The enemy words and installed artwork that outlive each room.</param>
    internal EnemyRoomBinding(EnemySession session) => Session = Ensure.NotNull(session);

    /// <summary>The enemy words and installed artwork that outlive each room.</summary>
    internal EnemySession Session { get; }

    /// <summary>The enemy definition source <see cref="RoomEnemySystem.Load"/> bound; null until a population loads.</summary>
    internal IRoomEnemyDefinitionSource? Definitions { get; set; }

    /// <summary>Whether a room population (or a verification fixture source) is bound.</summary>
    internal bool IsLoaded => Definitions is not null;

    /// <summary>
    /// The cartridge-state services the current room's AI calls; installed by <see cref="RoomEnemySystem.Load"/>. A
    /// host without a service supplies one that fails at the exact dependency.
    /// </summary>
    internal IRoomEnemyServices Services
    {
        get => installedServices ?? throw new InvalidOperationException(
            "Room enemy AI ran before RoomEnemySystem.Load installed its services.");
        set => installedServices = Ensure.NotNull(value);
    }

    /// <summary>The services <see cref="Services"/> holds; null until <see cref="RoomEnemySystem.Load"/> installs them.</summary>
    internal IRoomEnemyServices? installedServices;

    /// <summary>The cartridge random source of the current room's services.</summary>
    internal IEnemyRandomNumbers Random => Services.Random;

    /// <summary>The session's installed enemy presentation; drawing and named artwork transfers require it.</summary>
    internal EnemyTileArtworkCatalog? Artwork => Session.TileArtwork;

    /// <summary>The installed enemy presentation; drawing and named artwork transfers require it.</summary>
    internal EnemyTileArtworkCatalog InstalledArtwork => Artwork ??
        throw new InvalidDataException("Enemy artwork is not installed; the host installs it with the room.");

    /// <summary>The installed Mother Brain corpse tiles; the bound artwork must provide them.</summary>
    internal RoomCharacterAtlas MotherBrainCorpseArtwork =>
        Artwork?.MotherBrainCorpse ?? throw new InvalidDataException(
            "Installed enemy artwork is missing mother-brain-corpse-tiles.png.");

    /// <summary>The session's enemy staging surfaces; they outlive each room, as the cartridge's WRAM does.</summary>
    internal EnemyStagingSurfaces Staging => Session.Staging;

    /// <summary>The session's Mode 7 transfer queue residue, which Ceres Ridley's getaway writes.</summary>
    internal SuperMetroid.Core.Bosses.Ridley.Mode7TransferQueueResidue? Mode7QueueResidue => Session.Mode7QueueResidue;

    /// <summary>The bound enemy definition source; enemy work requires a loaded room population.</summary>
    internal IRoomEnemyDefinitionSource LoadedDefinitions =>
        Definitions ?? throw new InvalidOperationException("A room enemy population must be loaded first.");

    /// <summary>Rejects enemy work before a population is bound.</summary>
    internal void EnsureLoaded() => _ = LoadedDefinitions;
}
