using SuperMetroid.AssetExtraction.Enemies;
namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Verification's native view of a Ceres elevator arrival actor's spritemap word: the cartridge runs the pad and the
/// platform as enemy projectiles, whose spritemap word names the picture each shows.
/// </summary>
internal static class CeresElevatorProjectileAccess
{
    /// <summary>$8000, the word the cartridge's projectile holds before its first frame.</summary>
    internal const ushort NoSpritemap = 0x8000;

    /// <summary>Native views of an actor.</summary>
    extension(CeresElevatorArrivalState.CeresElevatorArrivalActor self)
    {
        /// <summary>The bank-$8D spritemap the actor shows, or <see cref="NoSpritemap"/> before its first frame.</summary>
        internal ushort SpritemapPointer => self.Pose is { } pose ? EnemyProjectileCompositionSources.ElevatorArrivalSpritemap(pose) : NoSpritemap;
    }
}
