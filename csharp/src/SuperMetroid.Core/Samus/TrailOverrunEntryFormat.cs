namespace SuperMetroid.Core.Samus;

/// <summary>
/// The installed overread file's encoding of a <see cref="TrailOverrunEntryKind"/>: the byte that opens each trail
/// overrun entry. Extraction writes it and the overread catalog reads it, both through here.
/// </summary>
internal static class TrailOverrunEntryFormat
{
    /// <summary>The byte that records a kind.</summary>
    internal static byte ToByte(TrailOverrunEntryKind kind) => kind switch
    {
        TrailOverrunEntryKind.Scratch => 0,
        TrailOverrunEntryKind.Placement => 1,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Undefined trail overrun entry kind."),
    };

    /// <summary>The kind a byte records.</summary>
    /// <exception cref="InvalidDataException">The byte records no kind.</exception>
    internal static TrailOverrunEntryKind FromByte(int value) => value switch
    {
        0 => TrailOverrunEntryKind.Scratch,
        1 => TrailOverrunEntryKind.Placement,
        _ => throw new InvalidDataException($"Trail overrun entry kind byte {value} records no kind."),
    };
}
