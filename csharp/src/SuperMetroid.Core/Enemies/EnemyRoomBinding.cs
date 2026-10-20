using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The loaded room's bindings that <see cref="RoomEnemySystem"/> shares with the owners it composes: the room's
/// enemy definition source, the cartridge-state services its AI calls (including the enemy random source), and the
/// host-installed enemy artwork.
/// </summary>
internal sealed class EnemyRoomBinding
{
    /// <summary>Binds the room to its enemy definitions and services and to the session whose enemy words outlive it.</summary>
    /// <param name="session">The enemy words and installed artwork that outlive each room.</param>
    /// <param name="definitions">The room's enemy definition source, or null for an enemy system with no population.</param>
    /// <param name="services">The cartridge-state services the room's AI calls, or null when none is supplied.</param>
    internal EnemyRoomBinding(EnemySession session, IRoomEnemyDefinitionSource? definitions, IRoomEnemyServices? services)
    {
        Session = Ensure.NotNull(session);
        Definitions = definitions;
        InstalledServices = services;
    }

    /// <summary>The enemy words and installed artwork that outlive each room.</summary>
    internal EnemySession Session { get; }

    /// <summary>The room's enemy definition source; null for an enemy system built without one.</summary>
    internal IRoomEnemyDefinitionSource? Definitions { get; }

    /// <summary>Whether a room population (or a verification fixture source) is bound.</summary>
    internal bool IsLoaded => Definitions is not null;

    /// <summary>
    /// The cartridge-state services the room's AI calls, which the room's enemy system was built with. A host without
    /// a service supplies one that fails at the exact dependency.
    /// </summary>
    internal IRoomEnemyServices Services => InstalledServices ?? throw new InvalidOperationException(
        "Room enemy AI ran in an enemy system built without services.");

    /// <summary>The services <see cref="Services"/> returns; null for an enemy system built without them.</summary>
    internal IRoomEnemyServices? InstalledServices { get; }

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
