namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A native word and its address.</summary>
/// <param name="Address">The address.</param>
/// <param name="Value">The value.</param>
internal readonly record struct NativeWord(ushort Address, ushort Value);
