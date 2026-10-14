using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="SamusGrappleRomData.Firing"/> that only verification reads.</summary>
internal static class SamusGrappleRomDataFiringConstants
{
    /// <summary>Initial angles for ten shot directions.</summary>
    public const int Angles = 0x9bc104;
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
    extension(SamusGrappleRomData.Firing)
    {
        /// <inheritdoc cref="SamusGrappleRomDataFiringConstants.Angles"/>
        internal static int Angles => SamusGrappleRomDataFiringConstants.Angles;
        /// <inheritdoc cref="SamusGrappleRomDataFiringConstants.DefaultOriginX"/>
        internal static int DefaultOriginX => SamusGrappleRomDataFiringConstants.DefaultOriginX;
        /// <inheritdoc cref="SamusGrappleRomDataFiringConstants.DefaultOriginY"/>
        internal static int DefaultOriginY => SamusGrappleRomDataFiringConstants.DefaultOriginY;
        /// <inheritdoc cref="SamusGrappleRomDataFiringConstants.LeftFlareSpritemapOffsets"/>
        internal static int LeftFlareSpritemapOffsets => SamusGrappleRomDataFiringConstants.LeftFlareSpritemapOffsets;
        /// <inheritdoc cref="SamusGrappleRomDataFiringConstants.RightFlareSpritemapOffsets"/>
        internal static int RightFlareSpritemapOffsets => SamusGrappleRomDataFiringConstants.RightFlareSpritemapOffsets;
        /// <inheritdoc cref="SamusGrappleRomDataFiringConstants.RunningOriginX"/>
        internal static int RunningOriginX => SamusGrappleRomDataFiringConstants.RunningOriginX;
        /// <inheritdoc cref="SamusGrappleRomDataFiringConstants.RunningOriginY"/>
        internal static int RunningOriginY => SamusGrappleRomDataFiringConstants.RunningOriginY;
        /// <inheritdoc cref="SamusGrappleRomDataFiringConstants.XVelocities"/>
        internal static int XVelocities => SamusGrappleRomDataFiringConstants.XVelocities;
        /// <inheritdoc cref="SamusGrappleRomDataFiringConstants.YVelocities"/>
        internal static int YVelocities => SamusGrappleRomDataFiringConstants.YVelocities;
    }
}
