namespace SuperMetroid.Verification.Frontend.GameOver;

/// <summary>The pointer, duration, sprite, palette, sound, next and restart of one original.</summary>
/// <param name="Pointer">The pointer.</param>
/// <param name="Duration">The duration.</param>
/// <param name="Sprite">The sprite.</param>
/// <param name="Palette">The palette.</param>
/// <param name="Sound">The sound.</param>
/// <param name="Next">The next.</param>
/// <param name="Restart">The restart.</param>
internal readonly record struct Original(ushort Pointer, ushort Duration, ushort Sprite, ushort Palette, ushort Sound, ushort Next, bool Restart);
