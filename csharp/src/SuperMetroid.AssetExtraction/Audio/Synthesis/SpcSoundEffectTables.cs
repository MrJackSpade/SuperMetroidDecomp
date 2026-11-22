using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// Sound-effect stream pointers and voice-allocation classes from the resident SPC driver, looked up in
/// <see cref="SpcSoundEffectDefinitions"/> by library and one-based CPU command.
/// </summary>
internal static partial class SpcSoundEffectTables
{
    /// <summary>Number of defined one-based commands in <paramref name="library"/>.</summary>
    internal static int CommandCount(SoundEffectLibrary library) => library switch
    {
        SoundEffectLibrary.Samus => SpcSoundEffectDefinitions.Library1.Length,
        SoundEffectLibrary.Enemies => SpcSoundEffectDefinitions.Library2.Length,
        SoundEffectLibrary.Room => SpcSoundEffectDefinitions.Library3.Length,
        _ => throw new ArgumentOutOfRangeException(nameof(library), library, "Undefined sound library."),
    };

    /// <summary>Selects the native instruction-list set for a one-based sound command.</summary>
    internal static int StreamPointer(SoundEffectLibrary library, int command) => library switch
    {
        SoundEffectLibrary.Samus => Effect(SpcSoundEffectDefinitions.Library1, command).Stream,
        SoundEffectLibrary.Enemies => Effect(SpcSoundEffectDefinitions.Library2, command).Stream,
        SoundEffectLibrary.Room => Effect(SpcSoundEffectDefinitions.Library3, command).Stream,
        _ => throw new ArgumentOutOfRangeException(nameof(library), library, "Undefined sound library."),
    };

    /// <summary>
    /// The installed key of the voice-allocation policy a one-based <paramref name="command"/> takes in
    /// <paramref name="library"/>, as the extracted audio manifest names it (<see cref="SpcSoundPolicyCodec"/>).
    /// </summary>
    internal static string PolicyKey(SoundEffectLibrary library, int command) => library switch
    {
        SoundEffectLibrary.Samus => SpcSoundPolicyCodec.Library1Keys.Of(Effect(SpcSoundEffectDefinitions.Library1, command).Policy),
        SoundEffectLibrary.Enemies => SpcSoundPolicyCodec.Library2Keys.Of(Effect(SpcSoundEffectDefinitions.Library2, command).Policy),
        SoundEffectLibrary.Room => SpcSoundPolicyCodec.Library3Keys.Of(Effect(SpcSoundEffectDefinitions.Library3, command).Policy),
        _ => throw new ArgumentOutOfRangeException(nameof(library), library, "Undefined sound library."),
    };

    /// <summary>How <paramref name="library"/>'s handler admits a changed request for one-based <paramref name="command"/>.</summary>
    internal static SpcSoundRequestAdmission Admission(SoundEffectLibrary library, int command) => library switch
    {
        SoundEffectLibrary.Samus => AdmissionIn(SpcSoundEffectDefinitions.Library1, command),
        SoundEffectLibrary.Enemies => AdmissionIn(SpcSoundEffectDefinitions.Library2, command),
        SoundEffectLibrary.Room => AdmissionIn(SpcSoundEffectDefinitions.Library3, command),
        _ => throw new ArgumentOutOfRangeException(nameof(library), library, "Undefined sound library."),
    };

    /// <summary>Whether <paramref name="library"/> keeps the low-health mode, which refuses its requests but the clearing one.</summary>
    internal static bool HasLowHealthMode(SoundEffectLibrary library) => library switch
    {
        SoundEffectLibrary.Samus or SoundEffectLibrary.Enemies => false,
        SoundEffectLibrary.Room => true,
        _ => throw new ArgumentOutOfRangeException(nameof(library), library, "Undefined sound library."),
    };

    /// <summary>
    /// How the request rule admits one-based <paramref name="command"/> in <paramref name="library"/>: as its effect states,
    /// and by priority for a command beyond the library's effects, as the driver admits every command it does not exempt;
    /// such a command's start then finds no effect.
    /// </summary>
    private static SpcSoundRequestAdmission AdmissionIn<TPolicy>(SpcSoundEffectDefinition<TPolicy>[] library, int command)
        where TPolicy : struct, Enum =>
        (uint)(command - 1) < (uint)library.Length ? library[command - 1].Admission : SpcSoundRequestAdmission.ByPriority;

    /// <summary>The effect a one-based <paramref name="command"/> selects in <paramref name="library"/>.</summary>
    internal static SpcSoundEffectDefinition<TPolicy> Effect<TPolicy>(SpcSoundEffectDefinition<TPolicy>[] library, int command)
        where TPolicy : struct, Enum =>
        (uint)(command - 1) < (uint)library.Length ? library[command - 1] : throw new ArgumentOutOfRangeException(nameof(command));

    /// <summary>
    /// The SPC RAM address of one library's allocation field: contiguous per-channel bitsets and masks, then one
    /// intervening byte before the per-channel voice indices.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="library"/> is not a sound library.</exception>
    internal static ushort AllocationStateAddress(SoundEffectLibrary library, SpcAllocationField field)
    {
        (int baseAddress, int channelCount) = Allocation(library);
        int offset = field switch
        {
            SpcAllocationField.VoiceBitset => 0,
            SpcAllocationField.ChannelMask => channelCount,
            SpcAllocationField.VoiceIndex => 2 * channelCount + 1,
            _ => throw new ArgumentOutOfRangeException(nameof(field)),
        };
        return (ushort)(baseAddress + offset);
    }

    /// <summary>Where <paramref name="library"/>'s allocation state starts in SPC RAM, and how many channels it has.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="library"/> is not a sound library.</exception>
    internal static SoundLibraryAllocation Allocation(SoundEffectLibrary library) =>
        SpcSoundEffectDefinitions.Allocations.TryGetValue(library, out SoundLibraryAllocation allocation)
            ? allocation
            : throw new ArgumentOutOfRangeException(nameof(library), library, "Undefined sound library.");

    /// <summary>Number of channel programs the policy <paramref name="policyKey"/> names in <paramref name="library"/> requires.</summary>
    /// <remarks>#1165: matches the voice-count writes in the handlers catalogued by
    /// SpcLibrary1Policy/SpcLibrary2Policy/SpcLibrary3Policy. Library-three cancellation
    /// and low-health mode both allocate one voice. Unknown installed keys are rejected.</remarks>
    internal static int VoiceCount(SoundEffectLibrary library, string policyKey) => library switch
    {
        SoundEffectLibrary.Samus => VoiceCount(SpcSoundPolicyCodec.Parse(SpcSoundPolicyCodec.Library1Keys, policyKey, library)),
        SoundEffectLibrary.Enemies => VoiceCount(SpcSoundPolicyCodec.Parse(SpcSoundPolicyCodec.Library2Keys, policyKey, library)),
        SoundEffectLibrary.Room => VoiceCount(SpcSoundPolicyCodec.Parse(SpcSoundPolicyCodec.Library3Keys, policyKey, library)),
        _ => throw new ArgumentOutOfRangeException(nameof(library), library, "Undefined sound library."),
    };

    /// <summary>Number of channel programs a library-one policy requires.</summary>
    internal static int VoiceCount(SpcLibrary1Policy policy) => policy switch
    {
        SpcLibrary1Policy.OneVoiceLowPriority or SpcLibrary1Policy.OneVoiceHighPriority => 1,
        SpcLibrary1Policy.TwoVoicesLowPriority => 2,
        SpcLibrary1Policy.ThreeVoicesHighPriority => 3,
        SpcLibrary1Policy.FourVoicesLowPriority or SpcLibrary1Policy.PowerBombFourVoices => 4,
        _ => throw new ArgumentOutOfRangeException(nameof(policy), policy, "Undefined library-one policy."),
    };

    /// <summary>Number of channel programs a library-two policy requires.</summary>
    internal static int VoiceCount(SpcLibrary2Policy policy) => policy switch
    {
        SpcLibrary2Policy.OneVoiceLowPriority or SpcLibrary2Policy.OneVoiceHighPriority => 1,
        SpcLibrary2Policy.TwoVoicesLowPriority or SpcLibrary2Policy.TwoVoicesHighPriority => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(policy), policy, "Undefined library-two policy."),
    };

    /// <summary>Number of channel programs a library-three policy requires.</summary>
    internal static int VoiceCount(SpcLibrary3Policy policy) => policy switch
    {
        SpcLibrary3Policy.CancelAndClearLowHealthMode or SpcLibrary3Policy.LowHealthModePreservePriority or
            SpcLibrary3Policy.OneVoiceLowPriority or SpcLibrary3Policy.OneVoiceHighPriority => 1,
        SpcLibrary3Policy.TwoVoicesLowPriority or SpcLibrary3Policy.TwoVoicesHighPriority => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(policy), policy, "Undefined library-three policy."),
    };
}
