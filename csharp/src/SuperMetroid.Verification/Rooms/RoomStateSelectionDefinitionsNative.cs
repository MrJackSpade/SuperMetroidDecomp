namespace SuperMetroid.Verification.Rooms;

/// <summary>The code, operand and target of one room state selection definitions native.</summary>
/// <param name="Code">The code.</param>
/// <param name="Operand">The operand.</param>
/// <param name="Target">The target.</param>
internal readonly record struct RoomStateSelectionDefinitionsNative(ushort Code, byte Operand, ushort Target);
