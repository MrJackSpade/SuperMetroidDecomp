using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.World;

/// <summary>Verification access to <see cref="RuntimeArtwork"/> members production does not use.</summary>
internal static class RuntimeArtworkAccess
{
    /// <summary>Exposes <see cref="RuntimeArtwork"/> state that only verification reads.</summary>
    extension(RuntimeArtwork self)
    {
        /// <summary>The gameplay OBJ character page enemy-format sprites draw from: the table's uploads over current artwork.</summary>
        /// <param name="uploads">The OBJ character table's uploads.</param>
        internal SpriteArtwork GameplayObjectPage(SpriteArtworkPlacements uploads) =>
            new ObjCharacterTablePage().Page(uploads, self);
    }
}
