namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Which output channel a stereo volume register drives.</summary>
internal enum DspStereoSide
{
    /// <summary>The left output (VxVOLL, MVOLL, EVOLL).</summary>
    Left = 0,

    /// <summary>The right output (VxVOLR, MVOLR, EVOLR).</summary>
    Right = 1,
}
