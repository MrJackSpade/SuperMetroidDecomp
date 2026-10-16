namespace SuperMetroid.Core.Enemies;

/// <summary>The AI-handler states as the native word the cartridge stores and copies.</summary>
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
