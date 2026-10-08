namespace SuperMetroid.Core.Game;

/// <summary>
/// Shared visually empty extended frame at bank-local $804F. The supported
/// enemy banks carry the same one-component, point-hitbox record. Its native
/// callbacks are the common $A0:8023 touch and $A0:802D shot routines.
/// </summary>
internal static class CommonEnemyEmptyExtendedFrameDefinitions
{
    /// <summary>Bank-local extended frame root at $804F.</summary>
    internal const ushort Frame = 0x804f;
    /// <summary>Bank-local empty ordinary spritemap at $804D.</summary>
    internal const ushort EmptySpritemap = 0x804d;

    private static bool IsSupportedBank(byte bank) => bank is 0xa0 or >= 0xa2 and <= 0xaa or 0xb2 or 0xb3;
    internal static bool HasFrame(byte bank, ushort pointer) =>
        pointer == Frame && IsSupportedBank(bank);

    /// <summary>
    /// The ordinary $804D OAM record is a zero-part frame in every supported
    /// enemy bank. It is also selected directly by enemies that have not yet
    /// entered a visible animation, independently of the $804F extended frame.
    /// </summary>
    internal static bool HasEmptySpritemap(byte bank, ushort pointer) =>
        pointer == EmptySpritemap && IsSupportedBank(bank);

    internal static ushort Callback(bool selectShot) => selectShot
        ? EnemyAiCodePointers.BankA0.NormalEnemyShot
        : EnemyAiCodePointers.BankA0.NormalEnemyTouch;
}
