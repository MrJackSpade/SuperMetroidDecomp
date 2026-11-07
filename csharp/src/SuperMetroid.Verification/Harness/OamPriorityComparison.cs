using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Harness;

/// <summary>
/// Compares a native OAM image with the port's apart from each record's priority tier, which the draw depth replaces
/// (#1514): positions, characters, flips, palette rows and sizes must all be equal.
/// </summary>
internal static class OamPriorityComparison
{
    /// <summary>The priority bits of a record's attribute byte.</summary>
    private const byte PriorityBits = 0x30;

    /// <summary>Whether the two images hold the same records apart from each record's priority tier.</summary>
    internal static bool SameApartFromPriority(OamBuffer native, OamBuffer installed)
    {
        if (native.NextByteOffset != installed.NextByteOffset || !native.HighTableBytes.AsSpan().SequenceEqual(installed.HighTableBytes))
            return false;
        for (int offset = 0; offset < native.NextByteOffset; offset++)
        {
            byte mask = offset % 4 == 3 ? unchecked((byte)~PriorityBits) : byte.MaxValue;
            if ((native.LowTableBytes[offset] & mask) != (installed.LowTableBytes[offset] & mask))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Whether the two whole OAM tables are equal apart from each record's priority tier, whatever each buffer's append
    /// cursor: for comparing a finalized frame with a native buffer that was only appended to.
    /// </summary>
    internal static bool ImagesEqualApartFromPriority(OamBuffer native, OamBuffer installed)
    {
        if (!native.HighTableBytes.AsSpan().SequenceEqual(installed.HighTableBytes))
            return false;
        for (int offset = 0; offset < native.LowTableBytes.Length; offset++)
        {
            byte mask = offset % 4 == 3 ? unchecked((byte)~PriorityBits) : byte.MaxValue;
            if ((native.LowTableBytes[offset] & mask) != (installed.LowTableBytes[offset] & mask))
                return false;
        }
        return true;
    }
}
