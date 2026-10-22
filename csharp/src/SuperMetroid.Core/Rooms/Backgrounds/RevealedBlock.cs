namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>One block the X-ray reveal shows: the block definition it draws and how it lays out its tiles (#1392).</summary>
/// <param name="Definition">The block definition.</param>
/// <param name="Arrangement">How its tiles are laid out.</param>
public readonly record struct RevealedBlock(int Definition, RevealedBlockArrangement Arrangement);
