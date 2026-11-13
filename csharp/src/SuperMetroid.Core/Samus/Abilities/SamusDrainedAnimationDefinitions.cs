namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The animation frames the drained-Samus handlers and commands publish directly, outside the drained poses' own
/// animations (#1513). They are frames of the asymmetrical drained animations as the game numbers them, so several name an
/// cue's place or a number between two runs of the pose they land in, which the game draws as it stands.
/// </summary>
internal static class SamusDrainedAnimationDefinitions
{
    /// <summary>The first frame of the left-facing drained crouch from which the able-to-stand handler accepts Up.</summary>
    internal const int AbleToStandFirstFrame = 8;

    /// <summary>The frame the able-to-stand handler moves the left-facing drained crouch to on Up, starting its stand-up.</summary>
    internal const int AbleToStandFrame = 13;

    /// <summary>The first frame from which the unable-to-stand handler accepts Up, whatever pose Samus holds.</summary>
    internal const int UnableToStandFirstFrame = 8;

    /// <summary>The last frame from which the unable-to-stand handler accepts Up.</summary>
    internal const int UnableToStandLastFrame = 11;

    /// <summary>The frame the unable-to-stand handler moves the animation to on Up, its failed attempt to stand.</summary>
    internal const int UnableToStandFrame = 18;

    /// <summary>The frame the Hyper Beam acquisition freezes the drained animation at.</summary>
    internal const int HyperBeamAcquisitionFrame = 28;

    /// <summary>The frame the rainbow-stop command starts the standing part of the drained animation at.</summary>
    internal const int RainbowStopFrame = 13;

    /// <summary>The frame a falling drained crouch lands on.</summary>
    internal const int LandingFrame = 7;

    /// <summary>How many ticks the landing frame is held.</summary>
    internal const int LandingTimer = 8;

    /// <summary>The frame the release moves a drained crouch to.</summary>
    internal const int ReleaseCrouchingFrame = 13;

    /// <summary>The frame the release moves a drained standing pose to.</summary>
    internal const int ReleaseStandingFrame = 4;

    /// <summary>The frame the drained-fall controller initializes the drained crouch on.</summary>
    internal const int FallInitialFrame = 2;

    /// <summary>The frame the crouching-or-falling controller writes the drained crouch on, the first of its held crouch.</summary>
    internal const int PutCrouchingFrame = 8;

    /// <summary>How many ticks the crouching-or-falling controller holds its first frame.</summary>
    internal const int PutCrouchingTimer = 16;
}
