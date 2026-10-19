using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Enemies;

/// <summary>Verification's native view of the enemy renderer: a draw named by bank and word draws the frame shown there.</summary>
internal static class EnemyPresentationNativeDrawAccess
{
    /// <summary>Native views of the renderer.</summary>
    extension(EnemyPresentation self)
    {
        /// <summary>Draws the installed frame at the native bank and word of <paramref name="draw"/>.</summary>
        internal void DrawEnemySpritemap(OamBuffer oam, NativeEnemySpritemapDraw draw) =>
            self.DrawEnemySpritemap(oam, new EnemySpritemapDraw
            {
                Frame = RoomEnemySlotFrameAccess.FrameAt(draw.Bank, draw.Pointer)
                    ?? throw new ArgumentOutOfRangeException(nameof(draw), draw, "A native draw of word zero shows no frame."),
                Origin = draw.Origin,
                PaletteBits = draw.PaletteBits,
                BaseTileIndex = draw.BaseTileIndex,
                ClipVerticalWrap = draw.ClipVerticalWrap,
                OriginYIsOnScreen = draw.OriginYIsOnScreen,
            });
    }
}
