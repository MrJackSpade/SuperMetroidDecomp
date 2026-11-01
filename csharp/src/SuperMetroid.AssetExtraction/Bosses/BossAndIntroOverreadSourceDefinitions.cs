using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// Where on the cartridge, and how, Kraid's fingernails, Crocomire's volley and the opening cinematic's egg shell read past
/// their data: extraction's private knowledge, which it decodes once into the boss and intro overread services (#1500).
/// </summary>
[EncodingOwner("cartridge tables Kraid's nails, Crocomire's volley and the egg shell read past")]
internal static class BossAndIntroOverreadSourceDefinitions
{
    /// <summary>$A7:BF1D: Kraid's nail contour, a left-offset word then a bound word per record, the ledges first.</summary>
    internal const int KraidNailContour = 0xa7bf1d;

    /// <summary>The bytes of one nail contour record: its left offset and its bound.</summary>
    internal const int KraidNailContourRecordBytes = 4;

    /// <summary>The bound's place within a nail contour record, after the left offset.</summary>
    internal const int KraidNailContourBoundOffset = 2;

    /// <summary>$86:9059: the gradient word of each place in Crocomire's volley, the first place first.</summary>
    internal const int CrocomireVolleyGradients = 0x869059;

    /// <summary>$8B:AA02: the egg-shell fragments' gravity steps, a whole word then a fraction word per frame fallen.</summary>
    internal const int ShellFragmentGravity = 0x8baa02;

    /// <summary>The bytes of one gravity step: its whole word and its fraction word.</summary>
    internal const int ShellFragmentGravityStepBytes = 4;

    /// <summary>The fraction word's place within a gravity step, after the whole word.</summary>
    internal const int ShellFragmentGravityFractionOffset = 2;

    /// <summary>
    /// The frames a fragment can index the gravity steps by: the fall count is kept in eight bits, so a fragment still
    /// airborne after this many would read the first step again.
    /// </summary>
    internal const int ShellFragmentFrameLimit = 256;
}
