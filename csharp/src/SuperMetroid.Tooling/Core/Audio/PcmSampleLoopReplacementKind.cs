namespace SuperMetroid.Core.Audio;

/// <summary>Mutually exclusive policies for resolving a replacement sample's intrinsic PCM loop point.</summary>
public enum PcmSampleLoopReplacementKind
{
    /// <summary>Rescales the existing loop index to preserve its time at the replacement sample rate; an unlooped sample remains unlooped.</summary>
    PreserveTime,
    /// <summary>Removes the intrinsic loop point from the replacement metadata.</summary>
    Disabled,
    /// <summary>Uses the supplied decoded PCM frame index without sample-rate conversion.</summary>
    ExplicitSampleIndex,
}
