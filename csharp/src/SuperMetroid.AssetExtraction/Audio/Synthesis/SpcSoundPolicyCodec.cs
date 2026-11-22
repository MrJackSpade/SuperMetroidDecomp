using SuperMetroid.Core.Audio;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// The sound libraries' voice-allocation policies as the cartridge numbers them and as the installed audio manifest names
/// them (#2556): each policy's number in its library's handler table, and the written-out installed key the manifest
/// spells it by. The policies themselves are identities whose backing values nothing reads.
/// </summary>
[EncodingOwner("the sound driver's voice-allocation policy numbers and the installed keys that name each policy")]
internal static partial class SpcSoundPolicyCodec
{
    /// <summary>Each library-1 policy's installed key, which the audio manifest names it by.</summary>
    internal static InstalledKeys<SpcLibrary1Policy> Library1Keys { get; } = new(new Dictionary<SpcLibrary1Policy, string>
    {
        [SpcLibrary1Policy.OneVoiceLowPriority] = "OneVoiceLowPriority",
        [SpcLibrary1Policy.OneVoiceHighPriority] = "OneVoiceHighPriority",
        [SpcLibrary1Policy.TwoVoicesLowPriority] = "TwoVoicesLowPriority",
        [SpcLibrary1Policy.ThreeVoicesHighPriority] = "ThreeVoicesHighPriority",
        [SpcLibrary1Policy.FourVoicesLowPriority] = "FourVoicesLowPriority",
        [SpcLibrary1Policy.PowerBombFourVoices] = "PowerBombFourVoices",
    });

    /// <summary>Each library-2 policy's installed key, which the audio manifest names it by.</summary>
    internal static InstalledKeys<SpcLibrary2Policy> Library2Keys { get; } = new(new Dictionary<SpcLibrary2Policy, string>
    {
        [SpcLibrary2Policy.OneVoiceLowPriority] = "OneVoiceLowPriority",
        [SpcLibrary2Policy.OneVoiceHighPriority] = "OneVoiceHighPriority",
        [SpcLibrary2Policy.TwoVoicesLowPriority] = "TwoVoicesLowPriority",
        [SpcLibrary2Policy.TwoVoicesHighPriority] = "TwoVoicesHighPriority",
    });

    /// <summary>Each library-3 policy's installed key, which the audio manifest names it by.</summary>
    internal static InstalledKeys<SpcLibrary3Policy> Library3Keys { get; } = new(new Dictionary<SpcLibrary3Policy, string>
    {
        [SpcLibrary3Policy.CancelAndClearLowHealthMode] = "CancelAndClearLowHealthMode",
        [SpcLibrary3Policy.LowHealthModePreservePriority] = "LowHealthModePreservePriority",
        [SpcLibrary3Policy.OneVoiceLowPriority] = "OneVoiceLowPriority",
        [SpcLibrary3Policy.TwoVoicesLowPriority] = "TwoVoicesLowPriority",
        [SpcLibrary3Policy.TwoVoicesHighPriority] = "TwoVoicesHighPriority",
        [SpcLibrary3Policy.OneVoiceHighPriority] = "OneVoiceHighPriority",
    });

    /// <summary>The policy <paramref name="key"/> names among <paramref name="keys"/>.</summary>
    /// <exception cref="InvalidDataException">The key names no policy of the library.</exception>
    internal static TPolicy Parse<TPolicy>(InstalledKeys<TPolicy> keys, string key, SoundEffectLibrary library)
        where TPolicy : struct, Enum =>
        Ensure.NotNull(keys).TryParse(key, out TPolicy policy) ? policy
            : throw new InvalidDataException($"Unknown {library} sound policy '{key}'.");
}
