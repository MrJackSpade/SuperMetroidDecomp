namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>The Morph Ball eye's beam colour levels and mount encoding, as its bank-$A3 routines write them.</summary>
internal static class MorphBallEyeBeamDefinitions
{
    /// <summary>Parameter two's low nibble: the wall the eye is mounted on.</summary>
    internal const int MountDirectionBits = 0x000f;

    /// <summary>The high nibble of the eye's table angle: the tracking pose it resumes at.</summary>
    internal const int PoseAngleBits = 0x00f0;

    /// <summary>The beam's red level when it appears.</summary>
    internal const byte InitialRed = 0x30;

    /// <summary>The beam's green level when it appears.</summary>
    internal const byte InitialGreen = 0x50;

    /// <summary>The beam's blue level when it appears and while it cycles.</summary>
    internal const byte Blue = 0x80;

    /// <summary>The red level a fading beam settles at.</summary>
    internal const byte FadedRed = 0x20;

    /// <summary>The green level at which a fading beam is removed.</summary>
    internal const byte FadedGreen = 0x40;

    /// <summary>The beam cycles through sixteen colours.</summary>
    internal const int ColorIndexBits = 0x000f;

    /// <summary>How much faster the widening beam's half-width grows each update: a quarter angle unit.</summary>
    internal static SnesSignedSixteenSixteen WidthGrowthStep { get; } = SnesSignedSixteenSixteen.FromParts(0, 0x4000);

    /// <summary>The whole angular half-width, in angle units, at which a widening beam is full.</summary>
    internal const short FullAngularWidth = 4;
}
