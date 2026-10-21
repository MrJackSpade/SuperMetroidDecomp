using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>
/// Shared visually empty extended frame at bank-local $804F. The supported
/// enemy banks carry the same one-component, point-hitbox record. Its native
/// callbacks are the common $A0:8023 touch and $A0:802D shot routines.
/// </summary>
internal static class CommonEnemyEmptyExtendedFrameDefinitions
{
    /// <summary>Bank-local extended frame root at $804F.</summary>
    internal const ushort Frame = 0x804f;
    /// <summary>
    /// The empty spritemap at $804D, at the same offset in every enemy bank (<c>Spritemap_Common_Nothing</c>): an actor
    /// showing it draws nothing, and the shared collision handlers skip it.
    /// </summary>
    internal const ushort EmptySpritemap = 0x804d;

    /// <summary>Limits the shared empty records to the enemy banks that carry native copies.</summary>
    private static bool IsSupportedBank(byte bank) => bank is 0xa0 or >= 0xa2 and <= 0xaa or 0xb2 or 0xb3;
    /// <summary>
    /// The frames that show the shared empty spritemap: the common empty frame, and the Norfair lava jumper frame
    /// catalogued at the same record.
    /// </summary>
    private static readonly HashSet<EnemyFrameId> EmptySpritemapFrames = [EnemyFrameId.Empty, EnemyFrameId.NorfairLavaJumper804d];
    /// <summary>Whether an actor showing <paramref name="frame"/> draws nothing: no frame yet, or the shared empty spritemap.</summary>
    internal static bool DrawsNothing(EnemyFrameId? frame) => frame is not { } shown || EmptySpritemapFrames.Contains(shown);
    /// <summary>Whether <paramref name="frame"/> is the shared point-hitbox extended frame.</summary>
    internal static bool IsFrame(EnemyFrameId? frame) => frame == EnemyFrameId.EmptyExtended;
    /// <summary>Recognizes the point-hitbox extended frame record only in banks with the shared record.</summary>
    internal static bool HasFrame(byte bank, ushort pointer) =>
        pointer == Frame && IsSupportedBank(bank);

    /// <summary>
    /// The ordinary $804D OAM record is a zero-part frame in every supported
    /// enemy bank. It is also selected directly by enemies that have not yet
    /// entered a visible animation, independently of the $804F extended frame.
    /// </summary>
    internal static bool HasEmptySpritemap(byte bank, ushort pointer) =>
        pointer == EmptySpritemap && IsSupportedBank(bank);
}
