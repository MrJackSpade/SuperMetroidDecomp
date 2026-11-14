using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// One 16x16 block of a room plane as a frame shows it (#1403, #1431): the block or collectible picture it shows (#1764)
/// and its mirroring.
/// </summary>
/// <param name="Picture">The block or collectible picture shown.</param>
/// <param name="Flips">How the picture is mirrored.</param>
public readonly record struct RoomBlockCell(RoomCellPicture Picture, BlockMirroring Flips);
