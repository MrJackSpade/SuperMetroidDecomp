namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Distinct per-channel fields in the native SPC voice-allocation structures.</summary>
internal enum SpcAllocationField
{
    /// <summary>Bitset of SPC voices claimed by a library channel.</summary>
    VoiceBitset,
    /// <summary>Channel mask stored beside the voice bitsets.</summary>
    ChannelMask,
    /// <summary>Voice index stored after the allocation masks and their intervening byte.</summary>
    VoiceIndex,
}
