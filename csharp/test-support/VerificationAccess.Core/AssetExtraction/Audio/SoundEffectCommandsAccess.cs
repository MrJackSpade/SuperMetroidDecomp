using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>Verification access to <see cref="SoundEffectCommands"/> decodes production does not use.</summary>
internal static class SoundEffectCommandsAccess
{
    /// <summary>Decodes optional sound words.</summary>
    extension(SoundEffectCommands)
    {
        /// <summary>
        /// The sound a cartridge operand or header word queues through <paramref name="library"/>: the request in its byte,
        /// or none for zero.
        /// </summary>
        /// <exception cref="InvalidDataException">The word does not fit a request byte, or names no request of the library.</exception>
        internal static SoundEffectId? DecodeOptional(SoundEffectLibrary library, int word)
        {
            if ((uint)word > byte.MaxValue)
                throw new InvalidDataException($"Sound word ${word:X4} for {library} does not fit a request byte.");
            return word == 0 ? null : SoundEffectCommands.Decode(library, word);
        }
    }
}
