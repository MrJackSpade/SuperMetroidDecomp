namespace SuperMetroid.Core;

/// <summary>A timer mechanics byte and its address.</summary>
/// <param name="Pointer">The pointer.</param>
/// <param name="Value">The value.</param>
internal readonly record struct TimerByte(ushort Pointer, byte Value);
