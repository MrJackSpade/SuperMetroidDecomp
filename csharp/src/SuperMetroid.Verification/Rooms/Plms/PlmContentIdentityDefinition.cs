namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>The pointer and word of one plm content identity definition.</summary>
/// <param name="Pointer">The pointer.</param>
/// <param name="Word">The word.</param>
internal readonly record struct PlmContentIdentityDefinition(ushort Pointer, ushort Word);
