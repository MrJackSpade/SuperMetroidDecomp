using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>
/// Verification access: runs the enemy-format draw passes as gameplay does and appends the OAM image their whole-frame
/// draws stand for to a native-style OAM buffer, so OAM oracles keep comparing the same records.
/// </summary>
internal static class EnemyFormatDrawAccess
{
    /// <summary>Appends the OAM image of a draw's published entries, in emission order, after the buffer's records.</summary>
    extension(OamBuffer target)
    {
        /// <summary>
        /// Lets <paramref name="draw"/> publish into a fresh frame and appends the OAM records its draws and OAM sprites
        /// stand for, wrapping at the 128th record as the native enemy writer does.
        /// </summary>
        internal void AppendDrawn(Action<SpriteFrameBuffer> draw)
        {
            OamBuffer image = SpriteFrameBuffer.DrawnOam(draw);
            for (int index = 0; index < image.LastFinalizedSpriteCount; index++)
            {
                int spriteIndex = target.NextByteOffset >> 2;
                image.LowTable.Slice(index * 4, 4).CopyTo(target.WritableLowTable()[target.NextByteOffset..]);
                OamEntry entry = image.GetEntry(index);
                target.SetHighTablePair(spriteIndex, SnesOamHighTablePair.FromSprite(unchecked(entry.Position.X), entry.IsLarge));
                target.NextByteOffset = (target.NextByteOffset + 4) % OamBuffer.LowTableByteCount;
            }
        }
    }

    /// <summary>Runs one enemy layer range's advance and publish passes.</summary>
    extension(EnemyDrawList drawing)
    {
        /// <summary>Advances and draws an inclusive layer range, appending its OAM image to <paramref name="oam"/>.</summary>
        internal void DrawLayersAsOam(OamBuffer oam, Position16<RoomPixels> camera, int firstLayer, int lastLayer)
        {
            drawing.AdvanceLayers(camera, firstLayer, lastLayer);
            oam.AppendDrawn(frame => drawing.DrawLayers(frame, camera, firstLayer, lastLayer));
        }
    }

    /// <summary>Draws room sprite objects into an OAM image.</summary>
    extension(RoomSpriteObjectPool pool)
    {
        /// <summary>Publishes the room sprite objects, appending their OAM image to <paramref name="oam"/>.</summary>
        internal void DrawRoomSpriteObjectsAsOam(OamBuffer oam, Position16<RoomPixels> camera) =>
            oam.AppendDrawn(frame => pool.DrawRoomSpriteObjects(frame, camera));
    }

    /// <summary>Draws enemy projectiles into an OAM image.</summary>
    extension(EnemyProjectileEngine engine)
    {
        /// <summary>Publishes the projectiles at one depth relative to Samus, appending their OAM image to <paramref name="oam"/>.</summary>
        internal void DrawEnemyProjectilesAtAsOam(OamBuffer oam, Position16<RoomPixels> camera,
            EnemyProjectileDepth depth, bool timeIsFrozen) =>
            oam.AppendDrawn(frame => engine.DrawEnemyProjectilesAt(frame, camera, depth, timeIsFrozen));

        /// <summary>Publishes the room sprite objects and the projectiles in front of Samus, appending their OAM image to <paramref name="oam"/>.</summary>
        internal void DrawEnemyProjectilesInFrontOfSamusAsOam(OamBuffer oam, Position16<RoomPixels> camera, bool timeIsFrozen = false) =>
            oam.AppendDrawn(frame => engine.DrawEnemyProjectilesInFrontOfSamus(frame, camera, timeIsFrozen));

        /// <summary>Publishes the projectiles behind Samus, appending their OAM image to <paramref name="oam"/>.</summary>
        internal void DrawEnemyProjectilesBehindSamusAsOam(OamBuffer oam, Position16<RoomPixels> camera, bool timeIsFrozen = false) =>
            oam.AppendDrawn(frame => engine.DrawEnemyProjectilesBehindSamus(frame, camera, timeIsFrozen));
    }

    /// <summary>Draws the Ceres elevator arrival actors into an OAM image.</summary>
    extension(CeresElevatorArrivalState arrival)
    {
        /// <summary>
        /// Publishes both arrival actors through <paramref name="presentation"/>, appending their OAM image to
        /// <paramref name="oam"/>.
        /// </summary>
        internal void DrawAsOam(OamBuffer oam, EnemyPresentation presentation, Position16<RoomPixels> camera) =>
            oam.AppendDrawn(frame => arrival.Draw(frame, presentation, camera));
    }
}
