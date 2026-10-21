namespace SuperMetroid.Verification.Saves;

/// <summary>The slot, complement and expected of one game save json case.</summary>
/// <param name="Slot">The slot.</param>
/// <param name="Complement">The complement.</param>
/// <param name="Expected">The expected.</param>
internal readonly record struct GameSaveJsonCase(ushort Slot, ushort Complement, int Expected);
