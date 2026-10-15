namespace SuperMetroid.EnsureAnalyzer.Hardware;

/// <summary>How the high byte's address follows the low byte's in a reconstructed word.</summary>
public enum WordReadProgression
{
    /// <summary>The next 24-bit bus address: <c>address + 1</c>.</summary>
    Linear,
    /// <summary>The next address within the bank: <c>AddWithinBank(address, 1)</c> or a 16-bit wrapped offset.</summary>
    FixedBank,
    /// <summary>A bank OR'd with an unwrapped <c>offset + 1</c>: the boundary behaviour cannot be inferred.</summary>
    Ambiguous,
}
