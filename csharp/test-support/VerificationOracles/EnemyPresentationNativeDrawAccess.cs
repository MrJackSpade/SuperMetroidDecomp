using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.TestSupport.VerificationOracles;

/// <summary>
/// Verification's native view of the enemy renderer: a draw named by bank and word writes the installed frame shown there
/// into a native OAM buffer, with the native writer's wrap and clip, to compare with cartridge OAM oracles.
/// </summary>
internal static class EnemyPresentationNativeDrawAccess
{
    /// <summary>Native views of the renderer.</summary>
    extension(RoomEnemySystem self)
    {
        /// <summary>Writes the installed frame at the native bank and word of <paramref name="draw"/> into <paramref name="oam"/>.</summary>
        [PpuLayoutBoundary("exact hardware test: writes native OAM records of an installed enemy frame")]
        internal void DrawEnemySpritemap(OamBuffer oam, NativeEnemySpritemapDraw draw)
        {
            EnemyFrameId frame = RoomEnemySlotFrameAccess.FrameAt(draw.Bank, draw.Pointer)
                ?? throw new ArgumentOutOfRangeException(nameof(draw), draw, "A native draw of word zero shows no frame.");
            // The common empty frame is compiled definition data, not missing artwork.
            if (frame == EnemyFrameId.Empty)
                return;
            if (self.TileArtwork?.Spritemaps?.TryGetDisplay(EnemyFrames.Bank(frame), EnemyFrames.Spritemap(frame),
                    out EnemySpritemapParts installed) != true)
                throw new InvalidDataException($"Enemy frame {frame} requires an installed display composition.");
            oam.AddEnemySpritemap(installed, new Position16<OamPixels>(draw.Origin.X, draw.Origin.Y),
                checked((ushort)draw.PaletteBits), checked((ushort)draw.BaseTileIndex), draw.ClipVerticalWrap, draw.OriginYIsOnScreen);
        }
    }
}
