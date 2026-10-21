namespace SuperMetroid.Verification.Fx.Artwork;

/// <summary>The pointer and colors of one room palette fx presentation row.</summary>
/// <param name="Pointer">The pointer.</param>
/// <param name="Colors">The colors.</param>
internal readonly record struct RoomPaletteFxPresentationRow(int Pointer, ushort[] Colors);
