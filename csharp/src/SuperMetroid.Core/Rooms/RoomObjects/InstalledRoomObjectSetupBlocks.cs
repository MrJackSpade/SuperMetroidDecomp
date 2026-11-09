using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the blocks room-object setups and block reactions write of their own accord (#1580): for
/// each <see cref="RoomObjectSetupBlock"/>, the block of the room's block sheet it shows and how that block is mirrored.
/// The import pictures every one; the game shows it as installed. How each collides stays with the setup that writes it.
/// </summary>
public sealed class InstalledRoomObjectSetupBlocks
{
    /// <summary>Holds the pictures, which the import has checked name every setup block once.</summary>
    /// <param name="blocks">What each setup block shows, owned by these pictures.</param>
    internal InstalledRoomObjectSetupBlocks(IReadOnlyDictionary<RoomObjectSetupBlock, RoomBlockAppearance> blocks) =>
        All = Ensure.NotNull(blocks);

    /// <summary>What every setup block shows.</summary>
    internal IReadOnlyDictionary<RoomObjectSetupBlock, RoomBlockAppearance> All { get; }

    /// <summary>What <paramref name="block"/> shows.</summary>
    /// <exception cref="InvalidDataException">The installation holds no picture of the block.</exception>
    internal RoomBlockAppearance Shown(RoomObjectSetupBlock block) =>
        All.TryGetValue(block, out RoomBlockAppearance shown)
            ? shown
            : throw new InvalidDataException($"No installed picture of room-object setup block {block}.");
}
