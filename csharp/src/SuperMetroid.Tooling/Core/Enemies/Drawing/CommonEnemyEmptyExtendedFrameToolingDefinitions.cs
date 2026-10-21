
namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>Development-tool members of <see cref="CommonEnemyEmptyExtendedFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class CommonEnemyEmptyExtendedFrameToolingDefinitions
{
    /// <summary>
    /// The empty spritemap at $804D, at the same offset in every enemy bank (<c>Spritemap_Common_Nothing</c>): an actor
    /// showing it draws nothing, and the shared collision handlers skip it.
    /// </summary>
    internal const ushort EmptySpritemap = 0x804d;
    /// <summary>Limits the shared empty records to the enemy banks that carry native copies.</summary>
    internal static bool IsSupportedBank(byte bank) => bank is 0xa0 or >= 0xa2 and <= 0xaa or 0xb2 or 0xb3;
    /// <summary>
    /// The ordinary $804D OAM record is a zero-part frame in every supported
    /// enemy bank. It is also selected directly by enemies that have not yet
    /// entered a visible animation, independently of the $804F extended frame.
    /// </summary>
    internal static bool HasEmptySpritemap(byte bank, ushort pointer) =>
        pointer == EmptySpritemap && IsSupportedBank(bank);
}
