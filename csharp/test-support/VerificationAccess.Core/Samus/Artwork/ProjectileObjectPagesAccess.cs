using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Verification access: the projectile OBJ page an equipped-beam word binds, as the sheets were composed over.</summary>
internal static class ProjectileObjectPagesAccess
{
    /// <summary>Selects a page by equipped-beam word.</summary>
    extension(ProjectileObjectPages pages)
    {
        /// <summary>The page bound for the equipped beams' combination.</summary>
        internal SpriteArtwork For(SamusBeamFlags equippedBeams) => pages.For(SamusBeamCombinations.Of(equippedBeams));
    }
}
