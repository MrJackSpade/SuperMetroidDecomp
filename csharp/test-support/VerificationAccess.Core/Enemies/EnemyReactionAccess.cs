using SuperMetroid.AssetExtraction.Enemies.Callbacks;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Verification's decoding of native touch and shot callback words into the reactions the import decides, so checks
/// compare the cartridge's words with typed reactions through the import's own codec.
/// </summary>
internal static class EnemyReactionAccess
{
    /// <summary>The reaction a native touch or shot word names in <paramref name="bank"/>'s code, or null for a zero word.</summary>
    /// <param name="bank">The enemy bank whose code the word names.</param>
    /// <param name="word">The native callback word.</param>
    /// <param name="shot">Whether the word is a shot word rather than a touch word.</param>
    internal static EnemyReaction? Of(int bank, int word, bool shot) =>
        shot ? EnemyInteractionCallbacks.DecodeShot(bank, word) : EnemyInteractionCallbacks.DecodeTouch(bank, word);
}
