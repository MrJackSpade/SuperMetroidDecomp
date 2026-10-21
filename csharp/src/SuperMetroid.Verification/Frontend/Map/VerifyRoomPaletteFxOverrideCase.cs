namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>The owner, definition pointer and color byte index of one verify room palette fx override case.</summary>
/// <param name="Owner">The owner.</param>
/// <param name="DefinitionPointer">The definition pointer.</param>
/// <param name="ColorByteIndex">The color byte index.</param>
internal readonly record struct VerifyRoomPaletteFxOverrideCase(string Owner, ushort DefinitionPointer, ushort ColorByteIndex);
