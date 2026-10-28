
namespace SuperMetroid.Core.Fx;

/// <summary>Cartridge values of <see cref="RoomShakes"/> that only verification reads.</summary>
internal static class RoomShakeDefinitionsConstants
{
    /// <summary>$A0:872D BGShakeDisplacements.BG1X/BG1Y, at eight-byte record stride.</summary>
    public const int Bg1ReferenceAddress = 0xa0872d;
    /// <summary>$A0:8731 BGShakeDisplacements.BG2X/BG2Y, at eight-byte record stride.</summary>
    public const int Bg2ReferenceAddress = 0xa08731;
    /// <summary>$86:846B Get_Values_for_Screen_Shaking.horizontalX/Y, at four-byte stride.</summary>
    public const int ProjectileReferenceAddress = 0x86846b;

    /// <summary>Provides native BG1, BG2, and projectile room-shake reference addresses.</summary>
    extension(RoomShakes)
    {
        /// <inheritdoc cref="RoomShakeDefinitionsConstants.Bg1ReferenceAddress"/>
        internal static int Bg1ReferenceAddress => RoomShakeDefinitionsConstants.Bg1ReferenceAddress;
        /// <inheritdoc cref="RoomShakeDefinitionsConstants.Bg2ReferenceAddress"/>
        internal static int Bg2ReferenceAddress => RoomShakeDefinitionsConstants.Bg2ReferenceAddress;
        /// <inheritdoc cref="RoomShakeDefinitionsConstants.ProjectileReferenceAddress"/>
        internal static int ProjectileReferenceAddress => RoomShakeDefinitionsConstants.ProjectileReferenceAddress;
    }
}
