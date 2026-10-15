
namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Cartridge values of <see cref="GrappleFiringDefinitions"/> that only verification reads.</summary>
internal static class GrappleFiringDefinitionsConstants
{
    /// <summary>$9B:C104 GrappleBeamFireAngles, ten full-turn unsigned angle words.</summary>
    public const int AngleReferenceAddress = 0x9bc104;
    /// <summary>$9B:C122, GrappleBeamFireOffsets_NotRunning_OriginX: physical hand X offsets.</summary>
    public const int OriginXReferenceAddress = 0x9bc122;
    /// <summary>$9B:C136, GrappleBeamFireOffsets_NotRunning_OriginY: physical hand Y offsets.</summary>
    public const int OriginYReferenceAddress = 0x9bc136;
    /// <summary>$9B:C172, GrappleBeamFireOffsets_Running_OriginX: identical X-offset alias.</summary>
    public const int RunningOriginXReferenceAddress = 0x9bc172;
    /// <summary>$9B:C186, GrappleBeamFireOffsets_Running_OriginY: running physical hand Y offsets.</summary>
    public const int RunningOriginYReferenceAddress = 0x9bc186;
    /// <summary>$9B:C0DB GrappleBeamFireVelocityTable.X, ten signed 8.8 velocity words.</summary>
    public const int XVelocityReferenceAddress = 0x9bc0db;
    /// <summary>$9B:C0EF GrappleBeamFireVelocityTable.Y, ten signed 8.8 velocity words.</summary>
    public const int YVelocityReferenceAddress = 0x9bc0ef;

    /// <summary>Provides the native grapple origin, running offset, angle, and velocity reference addresses for firing checks.</summary>
    extension(GrappleFiringDefinitions)
    {
        /// <inheritdoc cref="GrappleFiringDefinitionsConstants.AngleReferenceAddress"/>
        internal static int AngleReferenceAddress => GrappleFiringDefinitionsConstants.AngleReferenceAddress;
        /// <inheritdoc cref="GrappleFiringDefinitionsConstants.OriginXReferenceAddress"/>
        internal static int OriginXReferenceAddress => GrappleFiringDefinitionsConstants.OriginXReferenceAddress;
        /// <inheritdoc cref="GrappleFiringDefinitionsConstants.OriginYReferenceAddress"/>
        internal static int OriginYReferenceAddress => GrappleFiringDefinitionsConstants.OriginYReferenceAddress;
        /// <inheritdoc cref="GrappleFiringDefinitionsConstants.RunningOriginXReferenceAddress"/>
        internal static int RunningOriginXReferenceAddress => GrappleFiringDefinitionsConstants.RunningOriginXReferenceAddress;
        /// <inheritdoc cref="GrappleFiringDefinitionsConstants.RunningOriginYReferenceAddress"/>
        internal static int RunningOriginYReferenceAddress => GrappleFiringDefinitionsConstants.RunningOriginYReferenceAddress;
        /// <inheritdoc cref="GrappleFiringDefinitionsConstants.XVelocityReferenceAddress"/>
        internal static int XVelocityReferenceAddress => GrappleFiringDefinitionsConstants.XVelocityReferenceAddress;
        /// <inheritdoc cref="GrappleFiringDefinitionsConstants.YVelocityReferenceAddress"/>
        internal static int YVelocityReferenceAddress => GrappleFiringDefinitionsConstants.YVelocityReferenceAddress;
    }
}
