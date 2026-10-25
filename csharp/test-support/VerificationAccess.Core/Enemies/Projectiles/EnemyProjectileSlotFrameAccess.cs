using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// Verification's native view of the frame an enemy projectile shows: the installed program-frame key and the bank-$8D
/// spritemap word the cartridge's slot held, read through the presentation boundary, and the frame a native word names.
/// </summary>
internal static class EnemyProjectileSlotFrameAccess
{
    /// <summary>The composition frame that draws bank-$8D spritemap <paramref name="spritemap"/>.</summary>
    internal static EnemyProjectileFrameId CompositionAt(ushort spritemap)
    {
        foreach (EnemyProjectileFrameId frame in Enum.GetValues<EnemyProjectileFrameId>())
        {
            if (EnemyProjectileFrames.IsComposition(frame) && EnemyProjectileFrames.Composition(frame) == spritemap)
                return frame;
        }
        throw new ArgumentOutOfRangeException(nameof(spritemap), spritemap, $"No projectile composition frame draws $8D:{spritemap:X4}.");
    }

    /// <summary>Native views of a slot.</summary>
    extension(RoomEnemyProjectileSlot self)
    {
        /// <summary>The bank-$8D spritemap word the native slot holds: zero once freed, the blank map under a program frame.</summary>
        internal ushort SpritemapPointer => self.Frame is not { } frame ? (ushort)0
            : EnemyProjectileFrames.IsComposition(frame) ? EnemyProjectileFrames.Composition(frame)
            : EnemyProjectileSpritemapDefinitions.BlankSpritemap;

        /// <summary>The bank-$86 operand storing the program frame the slot shows, zero when it shows a composition or nothing.</summary>
        internal ushort PresentationOperandAddress => self.Frame is { } frame && !EnemyProjectileFrames.IsComposition(frame)
            ? EnemyProjectileProgramFrameSources.Operand(frame)
            : (ushort)0;
    }
}
