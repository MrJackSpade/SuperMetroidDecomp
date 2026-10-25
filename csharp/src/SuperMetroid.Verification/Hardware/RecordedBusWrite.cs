namespace SuperMetroid.Verification.Hardware;

/// <summary>One byte write recorded on the bus, with its address.</summary>
/// <param name="Address">The address.</param>
/// <param name="Value">The value.</param>
internal readonly record struct RecordedBusWrite(int Address, byte Value);
