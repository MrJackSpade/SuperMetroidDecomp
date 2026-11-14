namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// One whole 16x16 block the X-ray reveal shows (#1431): the block or collectible picture it draws (#1764) and how it lays
/// the picture out.
/// </summary>
/// <param name="Picture">The block or collectible picture drawn.</param>
/// <param name="Arrangement">How its picture is laid out.</param>
public readonly record struct RevealedBlock(RoomCellPicture Picture, RevealedBlockArrangement Arrangement);
