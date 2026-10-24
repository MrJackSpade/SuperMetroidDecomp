
namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Cartridge values of <see cref="GrappleBeamDefinitions.Firing"/> that only verification reads.</summary>
internal static class GrappleBeamDefinitionsFiringConstants
{
    /// <summary>Initial angles for ten shot directions.</summary>
    public const int Angles = 0x9bc104;
    /// <summary>Main flare animation delays in bank $90.</summary>
    public const int MainFlareAnimationDelays = 0x90c487;
    /// <summary>Non-running grapple-point X origins.</summary>
    public const int DefaultOriginX = 0x9bc122;
    /// <summary>Non-running grapple-point Y origins.</summary>
    public const int DefaultOriginY = 0x9bc136;
    /// <summary>Left-facing flare spritemap offsets in bank $93.</summary>
    public const int LeftFlareSpritemapOffsets = 0x93a22b;
    /// <summary>Right-facing flare spritemap offsets in bank $93.</summary>
    public const int RightFlareSpritemapOffsets = 0x93a225;
    /// <summary>Running grapple-point X origins.</summary>
    public const int RunningOriginX = 0x9bc172;
    /// <summary>Running grapple-point Y origins.</summary>
    public const int RunningOriginY = 0x9bc186;
    /// <summary>Initial X velocities for ten shot directions.</summary>
    public const int XVelocities = 0x9bc0db;
    /// <summary>Initial Y velocities for ten shot directions.</summary>
    public const int YVelocities = 0x9bc0ef;

    /// <summary>Provides grapple firing angles, origins, flare offsets, and velocities for shot comparisons.</summary>
    extension(GrappleBeamDefinitions.Firing)
    {
        /// <inheritdoc cref="GrappleBeamDefinitionsFiringConstants.MainFlareAnimationDelays"/>
        internal static int MainFlareAnimationDelays => GrappleBeamDefinitionsFiringConstants.MainFlareAnimationDelays;
        /// <inheritdoc cref="GrappleBeamDefinitionsFiringConstants.Angles"/>
        internal static int Angles => GrappleBeamDefinitionsFiringConstants.Angles;
        /// <inheritdoc cref="GrappleBeamDefinitionsFiringConstants.DefaultOriginX"/>
        internal static int DefaultOriginX => GrappleBeamDefinitionsFiringConstants.DefaultOriginX;
        /// <inheritdoc cref="GrappleBeamDefinitionsFiringConstants.DefaultOriginY"/>
        internal static int DefaultOriginY => GrappleBeamDefinitionsFiringConstants.DefaultOriginY;
        /// <inheritdoc cref="GrappleBeamDefinitionsFiringConstants.LeftFlareSpritemapOffsets"/>
        internal static int LeftFlareSpritemapOffsets => GrappleBeamDefinitionsFiringConstants.LeftFlareSpritemapOffsets;
        /// <inheritdoc cref="GrappleBeamDefinitionsFiringConstants.RightFlareSpritemapOffsets"/>
        internal static int RightFlareSpritemapOffsets => GrappleBeamDefinitionsFiringConstants.RightFlareSpritemapOffsets;
        /// <inheritdoc cref="GrappleBeamDefinitionsFiringConstants.RunningOriginX"/>
        internal static int RunningOriginX => GrappleBeamDefinitionsFiringConstants.RunningOriginX;
        /// <inheritdoc cref="GrappleBeamDefinitionsFiringConstants.RunningOriginY"/>
        internal static int RunningOriginY => GrappleBeamDefinitionsFiringConstants.RunningOriginY;
        /// <inheritdoc cref="GrappleBeamDefinitionsFiringConstants.XVelocities"/>
        internal static int XVelocities => GrappleBeamDefinitionsFiringConstants.XVelocities;
        /// <inheritdoc cref="GrappleBeamDefinitionsFiringConstants.YVelocities"/>
        internal static int YVelocities => GrappleBeamDefinitionsFiringConstants.YVelocities;
    }
}
