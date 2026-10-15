
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
}
