namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Cartridge values of <see cref="KraidArmCollisionDefinitions"/> that only verification reads.</summary>
internal static class KraidArmCollisionDefinitionsConstants
{
    /// <summary>Number of arm collision frames in Kraid's extracted animation data.</summary>
    public const int FrameCount = 22;

    /// <summary>Provides Kraid arm collision frame count for hitbox data verification.</summary>
    extension(KraidArmCollisionDefinitions)
    {
        /// <inheritdoc cref="KraidArmCollisionDefinitionsConstants.FrameCount"/>
        internal static int FrameCount => KraidArmCollisionDefinitionsConstants.FrameCount;
    }
    /// <summary>$A7:92A1, ExtendedSpritemap_KraidArm_Dying_PreparingToLungeForward_0.</summary>
    internal const ushort FirstSingleComponentFrame = 0x92a1;
    /// <summary>$A7:8F59, ExtendedSpritemap_KraidArm_General_0; twenty five-component frames follow.</summary>
    internal const ushort FirstGeneralFrame = 0x8f59;
    /// <summary>Native byte stride of a five-component extended frame, including its count word.</summary>
    internal const int GeneralFrameBytes = 2 + 5 * 8;
}
