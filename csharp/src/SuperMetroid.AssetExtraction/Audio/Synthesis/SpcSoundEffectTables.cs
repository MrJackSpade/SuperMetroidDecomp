namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// Sound-effect stream pointers and voice-allocation classes from the resident SPC driver, looked up in
/// <see cref="SpcSoundEffectDefinitions"/> by library and one-based CPU command.
/// </summary>
internal static class SpcSoundEffectTables
{
    /// <summary>Number of defined one-based commands in each native SPC sound library.</summary>
    internal static int CommandCount(int libraryIndex) => libraryIndex switch
    {
        0 => SpcSoundEffectDefinitions.Library1.Length,
        1 => SpcSoundEffectDefinitions.Library2.Length,
        2 => SpcSoundEffectDefinitions.Library3.Length,
        _ => throw new ArgumentOutOfRangeException(nameof(libraryIndex)),
    };

    /// <summary>Selects the native instruction-list set for a one-based sound command.</summary>
    internal static ushort StreamPointer(int libraryIndex, int command) => libraryIndex switch
    {
        0 => Effect(SpcSoundEffectDefinitions.Library1, command).Stream,
        1 => Effect(SpcSoundEffectDefinitions.Library2, command).Stream,
        2 => Effect(SpcSoundEffectDefinitions.Library3, command).Stream,
        _ => throw new ArgumentOutOfRangeException(nameof(libraryIndex)),
    };

    /// <summary>
    /// Dispatches one-based CPU sound commands to their native voice-allocation policy.
    /// Native jump tables at SPC $1F4D/$31B1/$4776 choose handlers, not numeric samples.
    /// The byte result preserves the extracted audio manifest's library-relative policy identity.
    /// </summary>
    internal static byte Configuration(int libraryIndex, int command) => libraryIndex switch
    {
        0 => (byte)Effect(SpcSoundEffectDefinitions.Library1, command).Policy,
        1 => (byte)Effect(SpcSoundEffectDefinitions.Library2, command).Policy,
        2 => (byte)Effect(SpcSoundEffectDefinitions.Library3, command).Policy,
        _ => throw new ArgumentOutOfRangeException(nameof(libraryIndex)),
    };

    /// <summary>The effect a one-based <paramref name="command"/> selects in <paramref name="library"/>.</summary>
    private static SpcSoundEffectDefinition<TPolicy> Effect<TPolicy>(SpcSoundEffectDefinition<TPolicy>[] library, int command)
        where TPolicy : struct, Enum =>
        (uint)(command - 1) < (uint)library.Length ? library[command - 1] : throw new ArgumentOutOfRangeException(nameof(command));

    /// <summary>
    /// The SPC RAM address of one library's allocation field: contiguous per-channel bitsets and masks, then one
    /// intervening byte before the per-channel voice indices.
    /// </summary>
    internal static ushort AllocationStateAddress(int libraryIndex, SpcAllocationField field)
    {
        if ((uint)libraryIndex >= (uint)SpcSoundEffectDefinitions.Allocations.Length)
            throw new ArgumentOutOfRangeException(nameof(libraryIndex));
        (int baseAddress, int channelCount) = SpcSoundEffectDefinitions.Allocations[libraryIndex];
        int offset = field switch
        {
            SpcAllocationField.VoiceBitset => 0,
            SpcAllocationField.ChannelMask => channelCount,
            SpcAllocationField.VoiceIndex => 2 * channelCount + 1,
            _ => throw new ArgumentOutOfRangeException(nameof(field)),
        };
        return (ushort)(baseAddress + offset);
    }

    /// <summary>Number of channel programs required by a named native allocation policy.</summary>
    /// <remarks>#1165: matches the voice-count writes in the handlers catalogued by
    /// SpcLibrary1Policy/SpcLibrary2Policy/SpcLibrary3Policy. Library-three cancellation
    /// and low-health mode both allocate one voice. Unknown manifest bytes are rejected,
    /// and unsupported library indices preserve ArgumentOutOfRangeException.</remarks>
    internal static int GetVoiceCount(int libraryIndex, int configuration) => libraryIndex switch
    {
        0 => (SpcLibrary1Policy)configuration switch
        {
            SpcLibrary1Policy.OneVoiceLowPriority or SpcLibrary1Policy.OneVoiceHighPriority => 1,
            SpcLibrary1Policy.TwoVoicesLowPriority => 2,
            SpcLibrary1Policy.ThreeVoicesHighPriority => 3,
            SpcLibrary1Policy.FourVoicesLowPriority or SpcLibrary1Policy.PowerBombFourVoices => 4,
            _ => throw new InvalidDataException($"Unknown SPC SFX1 configuration {configuration}."),
        },
        1 => (SpcLibrary2Policy)configuration switch
        {
            SpcLibrary2Policy.OneVoiceLowPriority or SpcLibrary2Policy.OneVoiceHighPriority => 1,
            SpcLibrary2Policy.TwoVoicesLowPriority or SpcLibrary2Policy.TwoVoicesHighPriority => 2,
            _ => throw new InvalidDataException($"Unknown SPC SFX2 configuration {configuration}."),
        },
        2 => (SpcLibrary3Policy)configuration switch
        {
            SpcLibrary3Policy.CancelAndClearLowHealthMode or SpcLibrary3Policy.LowHealthModePreservePriority or
                SpcLibrary3Policy.OneVoiceLowPriority or SpcLibrary3Policy.OneVoiceHighPriority => 1,
            SpcLibrary3Policy.TwoVoicesLowPriority or SpcLibrary3Policy.TwoVoicesHighPriority => 2,
            _ => throw new InvalidDataException($"Unknown SPC SFX3 configuration {configuration}."),
        },
        _ => throw new ArgumentOutOfRangeException(nameof(libraryIndex)),
    };
}
