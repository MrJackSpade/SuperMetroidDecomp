namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The codec between the typed <see cref="EnemyAiStates"/> flags and the AI-handler word the cartridge stores and copies.
/// The word stays exact because two places still read it as a plain word: the thaw path stores the remaining bits into
/// the frozen timer, and the slot's aliased enemy-RAM reads return the AI-handler field by offset. This type is the one
/// place the flags become that word; the destination is owned state that no longer reads the flags as a word, which
/// removes this codec.
/// </summary>
[SnesEncodingOwner]
public static class EnemyAiStatesWord
{
    /// <summary>The native AI-handler word for a set of states.</summary>
    extension(EnemyAiStates states)
    {
        /// <summary>
        /// The exact numeric value of the states as the native word, for the places the cartridge reuses it as a plain
        /// word: the thaw path stores the remaining bits into the frozen timer, and enemy-RAM aliases read the word.
        /// </summary>
        public ushort NativeWord => (ushort)states;
    }
}
