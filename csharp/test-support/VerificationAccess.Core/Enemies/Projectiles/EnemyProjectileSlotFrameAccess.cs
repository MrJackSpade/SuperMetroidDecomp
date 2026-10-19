using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// Verification's native view of the frame an enemy projectile shows: the installed program-frame key and the bank-$8D
/// spritemap word the cartridge's slot held, read through the presentation boundary, and the frame a native word names.
/// </summary>
internal static class EnemyProjectileSlotFrameAccess
{
    /// <summary>Each installed program frame's identity, by its bank-$86 operand.</summary>
    private static readonly Lazy<Dictionary<ushort, EnemyProjectileFrameId>> ProgramFrames = new(() =>
        EnemyProjectilePresentationFrameSets.All.ToArray().ToDictionary(frame => frame.OperandAddress, frame => EnemyProjectileFrames.Identity(frame.Name)));

    /// <summary>The program frame whose installed artwork key is bank-$86 operand <paramref name="operandAddress"/>.</summary>
    internal static EnemyProjectileFrameId ProgramFrameAt(ushort operandAddress) =>
        ProgramFrames.Value.TryGetValue(operandAddress, out EnemyProjectileFrameId frame)
            ? frame
            : throw new ArgumentOutOfRangeException(nameof(operandAddress), operandAddress, $"No installed program frame at $86:{operandAddress:X4}.");

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
        internal ushort SpritemapPointer => self.Frame switch
        {
            null => 0,
            { } frame when EnemyProjectileFrames.IsComposition(frame) => EnemyProjectileFrames.Composition(frame),
            _ => EnemyProjectileSpritemapDefinitions.BlankSpritemap,
        };

        /// <summary>The installed program-frame key the slot shows, zero when it shows a composition or nothing.</summary>
        internal ushort PresentationOperandAddress => self.Frame is { } frame && !EnemyProjectileFrames.IsComposition(frame)
            ? EnemyProjectileFrames.ProgramFrameKey(frame)
            : (ushort)0;
    }
}
