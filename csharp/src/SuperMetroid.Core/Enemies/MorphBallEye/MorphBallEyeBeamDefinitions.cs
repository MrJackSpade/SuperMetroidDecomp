namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>The Morph Ball eye's beam colour and mount encoding, as its bank-$A3 routines write them.</summary>
internal static class MorphBallEyeBeamDefinitions
{
    /// <summary>Parameter two's low nibble: the wall the eye is mounted on.</summary>
    internal const int MountDirectionBits = 0x000f;

    /// <summary>The high nibble of the eye's table angle: the tracking pose it resumes at.</summary>
    internal const int PoseAngleBits = 0x00f0;

    /// <summary>The colour the beam adds when it appears: red and green at intensity 16, without blue.</summary>
    internal static Bgr555 InitialColor { get; } = new(16, 16, 0);

    /// <summary>The beam cycles through sixteen colours.</summary>
    internal const int ColorIndexBits = 0x000f;

    /// <summary>How much faster the widening beam's half-width grows each update: a quarter angle unit.</summary>
    internal static SignedSixteenSixteen WidthGrowthStep { get; } = SignedSixteenSixteen.FromParts(0, 0x4000);

    /// <summary>The whole angular half-width, in angle units, at which a widening beam is full.</summary>
    internal const short FullAngularWidth = 4;
}
