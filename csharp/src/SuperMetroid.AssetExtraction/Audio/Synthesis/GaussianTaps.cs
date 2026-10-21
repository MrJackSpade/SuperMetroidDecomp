using System.Runtime.CompilerServices;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// The four Gaussian interpolation weights of one phase, indexed by the neighbouring decoded sample each one weights,
/// oldest first. Each weight is in 1/2048 units, and the four sum to 2048.
/// </summary>
[InlineArray(4)]
internal struct GaussianTaps
{
    /// <summary>The first weight; the others follow it in place.</summary>
    private int weight;
}
