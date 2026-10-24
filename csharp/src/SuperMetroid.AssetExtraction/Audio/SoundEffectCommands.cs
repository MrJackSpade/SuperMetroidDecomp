using System.Collections.Frozen;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// Decodes a sound library's request number from cartridge data or the installed clips into the sound it names, and
/// encodes a sound back to its request for the sound program and the clip and override identities (#1410). This is the
/// only place the game's sounds meet their cartridge numbering.
/// </summary>
internal static class SoundEffectCommands
{
    /// <summary>Each library-one request's sound.</summary>
    private static readonly FrozenDictionary<int, Library1Sound> Library1Sounds = Invert(SoundEffectCommandDefinitions.Library1);
    /// <summary>Each library-two request's sound.</summary>
    private static readonly FrozenDictionary<int, Library2Sound> Library2Sounds = Invert(SoundEffectCommandDefinitions.Library2);
    /// <summary>Each library-three request's sound.</summary>
    private static readonly FrozenDictionary<int, Library3Sound> Library3Sounds = Invert(SoundEffectCommandDefinitions.Library3);

    /// <summary>The request number, 1 through 255, that asks the sound's library for <paramref name="sound"/>.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="sound"/> is the default identity.</exception>
    internal static int Command(SoundEffectId sound) => sound.Match<int>(
        library1 => SoundEffectCommandDefinitions.Library1[library1],
        library2 => SoundEffectCommandDefinitions.Library2[library2],
        library3 => SoundEffectCommandDefinitions.Library3[library3]);

    /// <summary>The sound <paramref name="library"/>'s request <paramref name="command"/> names.</summary>
    /// <exception cref="InvalidDataException">The library has no such request.</exception>
    internal static SoundEffectId Decode(SoundEffectLibrary library, int command)
    {
        SoundEffectId? sound = library switch
        {
            SoundEffectLibrary.Library1 => Library1Sounds.TryGetValue(command, out Library1Sound one) ? (SoundEffectId)one : null,
            SoundEffectLibrary.Library2 => Library2Sounds.TryGetValue(command, out Library2Sound two) ? (SoundEffectId)two : null,
            SoundEffectLibrary.Library3 => Library3Sounds.TryGetValue(command, out Library3Sound three) ? (SoundEffectId)three : null,
            _ => throw new InvalidDataException($"Unknown sound-effect library {library}."),
        };
        return sound ?? throw new InvalidDataException($"{library} has no sound request ${command:X2}.");
    }

    /// <summary>
    /// The sound a cartridge operand or header word queues through <paramref name="library"/>: the request in its byte,
    /// or none for zero.
    /// </summary>
    /// <exception cref="InvalidDataException">The word does not fit a request byte, or names no request of the library.</exception>
    internal static SoundEffectId? DecodeOptional(SoundEffectLibrary library, ushort word)
    {
        if (word > byte.MaxValue)
            throw new InvalidDataException($"Sound word ${word:X4} for {library} does not fit a request byte.");
        return word == 0 ? null : Decode(library, word);
    }

    /// <summary>Indexes a library's sounds by request number, proving every request is distinct and nonzero.</summary>
    private static FrozenDictionary<int, TSound> Invert<TSound>(FrozenDictionary<TSound, byte> commands)
        where TSound : struct, Enum
    {
        if (commands.Count != Enum.GetValues<TSound>().Length)
            throw new InvalidDataException($"{typeof(TSound).Name} gives {commands.Count} of its sounds a request number.");
        var sounds = new Dictionary<int, TSound>();
        foreach ((TSound sound, byte command) in commands)
        {
            if (command == 0 || !sounds.TryAdd(command, sound))
                throw new InvalidDataException($"{typeof(TSound).Name}.{sound} has request ${command:X2}, zero or another sound's.");
        }
        return sounds.ToFrozenDictionary();
    }
}
