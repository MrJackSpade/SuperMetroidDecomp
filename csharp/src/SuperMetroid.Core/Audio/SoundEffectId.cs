namespace SuperMetroid.Core.Audio;

/// <summary>
/// Lossless identity of one cartridge sound effect, including the library in which its
/// byte-sized sequence number is meaningful.
/// </summary>
/// <remarks>
/// Sound numbers are not global: for example, <c>$02</c> in library one is unrelated to
/// <c>$02</c> in library two. Keeping both fields in one value prevents APIs from swapping
/// their order or accidentally comparing IDs that belong to different SPC queues. Unknown
/// cartridge sequence numbers remain representable because the numeric component is not an
/// enum.
/// </remarks>
public readonly record struct SoundEffectId
{
    /// <summary>Creates a library-qualified sound effect identity.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is outside 0 through 255.</exception>
    public SoundEffectId(SoundEffectLibrary library, int value)
    {
        // Enum casts can forge undefined values, so validate at construction even though
        // ordinary callers use one of the three named enum members.
        _ = SoundEffectLibraries.ToQueueIndex(library);
        Library = library;
        Value = (byte)Ensure.BetweenInclusive(0, byte.MaxValue, value);
    }

    /// <summary>The exclusive cartridge SFX library containing this sequence.</summary>
    public SoundEffectLibrary Library { get; }

    /// <summary>The library-relative byte written to the corresponding SPC input port.</summary>
    public byte Value { get; }

    /// <summary>
    /// Preserves a cartridge value after proving that it is a valid byte-sized SPC sequence
    /// number. Accepting the decoded word keeps ROM instruction interpreters free of
    /// scattered casts while still accepting byte-valued dynamic data and numeric literals.
    /// </summary>
    public static SoundEffectId FromCartridge(SoundEffectLibrary library, ushort value)
    {
        if (value > byte.MaxValue)
        {
            throw new InvalidDataException(
                $"Sound effect ${value:X4} for {library} does not fit the cartridge's byte-sized SPC request port.");
        }

        return new SoundEffectId(library, unchecked((byte)value));
    }

    /// <inheritdoc />
    public override string ToString() => $"{Library}:${Value:X2}";
}
