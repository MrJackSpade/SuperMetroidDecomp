using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The loaded room's bindings that <see cref="RoomEnemySystem"/> shares with the owners it composes: the room's
/// address space, the cartridge-state services its AI calls (including the enemy random source), and the
/// host-installed enemy artwork.
/// </summary>
internal sealed class EnemyRoomBinding
{
    /// <summary>The address space <see cref="RoomEnemySystem.Load"/> bound; null until a population loads.</summary>
    internal ISnesAddressSpace? Bus { get; set; }

    /// <summary>Whether a room population (or a verification fixture bus) is bound.</summary>
    internal bool IsLoaded => Bus is not null;

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

    /// <summary>
    /// Installed enemy presentation. Drawing or a named artwork transfer requires its corresponding catalog; null
    /// does not enable a cartridge-backed fallback. The host reinstalls it after a debugger restore.
    /// </summary>
    [field: NonSerialized]
    internal EnemyTileArtworkCatalog? Artwork { get; set; }

    /// <summary>The installed enemy presentation; drawing and named artwork transfers require it.</summary>
    internal EnemyTileArtworkCatalog InstalledArtwork => Artwork ??
        throw new InvalidDataException("Enemy artwork is not installed; the host installs it with the room.");

    /// <summary>The installed Mother Brain corpse tiles; the bound artwork must provide them.</summary>
    internal RoomCharacterAtlas MotherBrainCorpseArtwork =>
        Artwork?.MotherBrainCorpse ?? throw new InvalidDataException(
            "Installed enemy artwork is missing mother-brain-corpse-tiles.png.");

    /// <summary>The room's live WRAM, required for enemy staging.</summary>
    internal ISnesMemoryBus MemoryBus =>
        SnesMemoryBusRequirement.Require(LoadedBus, "Enemy staging requires live WRAM.");

    /// <summary>The enemy staging surfaces in live WRAM.</summary>
    internal EnemyScratchMemory Scratch => new(MemoryBus);

    /// <summary>The bound address space; enemy work requires a loaded room population.</summary>
    internal ISnesAddressSpace LoadedBus =>
        Bus ?? throw new InvalidOperationException("A room enemy population must be loaded first.");

    /// <summary>Rejects enemy work before a population is bound.</summary>
    internal void EnsureLoaded() => _ = LoadedBus;
}
