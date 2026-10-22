using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Areas;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.World;

/// <summary>Verification access to <see cref="RuntimeArtwork"/> members production does not use.</summary>
internal static class RuntimeArtworkAccess
{
    /// <summary>Exposes <see cref="RuntimeArtwork"/> state that only verification reads.</summary>
    extension(RuntimeArtwork self)
    {
        /// <summary>The gameplay OBJ character page enemy-format sprites draw from: the table's uploads over current artwork.</summary>
        /// <param name="uploads">The OBJ character table's uploads.</param>
        internal SpriteArtwork GameplayObjectPage(ObjCharacterUploads uploads) =>
            new ObjCharacterTablePage().Page(uploads, self);
    }
}
