namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>The pointer, id and count of one kraid room visual selection definition.</summary>
/// <param name="Pointer">The pointer.</param>
/// <param name="Id">The id.</param>
/// <param name="Count">The count.</param>
internal readonly record struct KraidRoomVisualSelectionDefinition(ushort Pointer, string Id, int Count);
