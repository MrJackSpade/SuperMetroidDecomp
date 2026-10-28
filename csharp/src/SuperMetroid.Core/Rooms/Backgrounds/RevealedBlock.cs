namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>One whole 16x16 block the X-ray reveal shows (#1431): the block it draws and how it lays the block's picture out.</summary>
/// <param name="Definition">The block, by its visual block index.</param>
/// <param name="Arrangement">How its picture is laid out.</param>
public readonly record struct RevealedBlock(int Definition, RevealedBlockArrangement Arrangement);
