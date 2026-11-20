using System.Collections.Frozen;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Cartridge values and helpers of <see cref="SpcSoundPolicyCodec"/> that only verification reads.</summary>
internal static class SpcSoundPolicyCodecConstants
{
    /// <summary>Each library-1 policy's number in the driver's handler table, which the cartridge's effect table holds.</summary>
    internal static readonly FrozenDictionary<SpcLibrary1Policy, byte> Library1Numbers = new Dictionary<SpcLibrary1Policy, byte>
    {
        [SpcLibrary1Policy.OneVoiceLowPriority] = 0,
        [SpcLibrary1Policy.OneVoiceHighPriority] = 1,
        [SpcLibrary1Policy.TwoVoicesLowPriority] = 2,
        [SpcLibrary1Policy.ThreeVoicesHighPriority] = 3,
        [SpcLibrary1Policy.FourVoicesLowPriority] = 4,
        [SpcLibrary1Policy.PowerBombFourVoices] = 5,
    }.ToFrozenDictionary();
    /// <summary>The driver's number for <paramref name="policy"/>.</summary>
    internal static byte NativeNumber(SpcLibrary1Policy policy) => Ensure.Listed(Library1Numbers, policy);
    /// <summary>Each library-2 policy's number in the driver's handler table, which the cartridge's effect table holds.</summary>
    internal static readonly FrozenDictionary<SpcLibrary2Policy, byte> Library2Numbers = new Dictionary<SpcLibrary2Policy, byte>
    {
        [SpcLibrary2Policy.OneVoiceLowPriority] = 0,
        [SpcLibrary2Policy.OneVoiceHighPriority] = 1,
        [SpcLibrary2Policy.TwoVoicesLowPriority] = 2,
        [SpcLibrary2Policy.TwoVoicesHighPriority] = 3,
    }.ToFrozenDictionary();
    /// <summary>The driver's number for <paramref name="policy"/>.</summary>
    internal static byte NativeNumber(SpcLibrary2Policy policy) => Ensure.Listed(Library2Numbers, policy);
    /// <summary>Each library-3 policy's number in the driver's handler table, which the cartridge's effect table holds.</summary>
    internal static readonly FrozenDictionary<SpcLibrary3Policy, byte> Library3Numbers = new Dictionary<SpcLibrary3Policy, byte>
    {
        [SpcLibrary3Policy.CancelAndClearLowHealthMode] = 0,
        [SpcLibrary3Policy.LowHealthModePreservePriority] = 1,
        [SpcLibrary3Policy.OneVoiceLowPriority] = 2,
        [SpcLibrary3Policy.TwoVoicesLowPriority] = 3,
        [SpcLibrary3Policy.TwoVoicesHighPriority] = 4,
        [SpcLibrary3Policy.OneVoiceHighPriority] = 5,
    }.ToFrozenDictionary();
    /// <summary>The driver's number for <paramref name="policy"/>.</summary>
    internal static byte NativeNumber(SpcLibrary3Policy policy) => Ensure.Listed(Library3Numbers, policy);
}
