namespace SuperMetroid.Verification.Hardware;

/// <summary>The address and value of one cpu bus boundary write.</summary>
/// <param name="Address">The address.</param>
/// <param name="Value">The value.</param>
internal readonly record struct CpuBusBoundaryWrite(int Address, byte Value);
