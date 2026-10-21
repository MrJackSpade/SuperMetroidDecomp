namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>The pointer and words of one lookup eye door stock expected.</summary>
/// <param name="Pointer">The pointer.</param>
/// <param name="Words">The words.</param>
internal readonly record struct LookupEyeDoorStockExpected(ushort Pointer, ushort[] Words);
