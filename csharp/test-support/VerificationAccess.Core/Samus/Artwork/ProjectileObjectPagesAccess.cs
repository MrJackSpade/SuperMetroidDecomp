using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Verification access: the projectile OBJ page an equipped-beam word binds, as the sheets were composed over.</summary>
internal static class ProjectileObjectPagesAccess
{
    /// <summary>Selects a page by equipped-beam word.</summary>
    extension(ProjectileObjectPages pages)
    {
        /// <summary>The page bound for an equipped-beam word.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The word selects no beam combination (bits 4-11 set).</exception>
        internal SpriteArtwork For(SamusBeamFlags equippedBeams) =>
            pages.For(new SamusBeamLoadoutWord(equippedBeams).Combination ?? throw new ArgumentOutOfRangeException(
                nameof(equippedBeams), equippedBeams, "Equipped-beam index is outside the beam-combination tables."));
    }
}
