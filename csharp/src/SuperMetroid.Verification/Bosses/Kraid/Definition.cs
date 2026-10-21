namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>The pointer, count and id of one definition.</summary>
/// <param name="Pointer">The pointer.</param>
/// <param name="Count">The count.</param>
/// <param name="Id">The id.</param>
internal readonly record struct Definition(ushort Pointer, int Count, string Id);
