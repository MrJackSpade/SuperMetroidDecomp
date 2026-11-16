namespace SuperMetroid.Verification.Memory;

/// <summary>The recorded memory the checks compare.</summary>
internal static class MemoryChecks
{
    /// <summary>The first byte of a recording.</summary>
    internal static byte Head => new RecordedMemory([7, 9]).Head;
}
