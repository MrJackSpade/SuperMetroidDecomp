namespace SuperMetroid.Core.Programs;

/// <summary>A mechanics value and the address it is stored at.</summary>
/// <param name="Address">The address.</param>
/// <param name="Value">The value.</param>
internal readonly record struct AddressedValue(ushort Address, ushort Value);
