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
    private static readonly FrozenDictionary<int, SamusSound> Library1Sounds = Invert(SoundEffectCommandDefinitions.Library1);
    /// <summary>Each library-two request's sound.</summary>
    private static readonly FrozenDictionary<int, EnemySound> Library2Sounds = Invert(SoundEffectCommandDefinitions.Library2);
    /// <summary>Each library-three request's sound.</summary>
    private static readonly FrozenDictionary<int, RoomSound> Library3Sounds = Invert(SoundEffectCommandDefinitions.Library3);

    /// <summary>Each sound's request number in its own library.</summary>
    private static readonly FrozenDictionary<SoundEffectId, int> Commands =
        SoundEffectCommandDefinitions.Library1.Select(entry => KeyValuePair.Create((SoundEffectId)entry.Key, (int)entry.Value))
            .Concat(SoundEffectCommandDefinitions.Library2.Select(entry => KeyValuePair.Create((SoundEffectId)entry.Key, (int)entry.Value)))
            .Concat(SoundEffectCommandDefinitions.Library3.Select(entry => KeyValuePair.Create((SoundEffectId)entry.Key, (int)entry.Value)))
            .ToFrozenDictionary();

    /// <summary>The request number, 1 through 255, that asks the sound's library for <paramref name="sound"/>.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="sound"/> is the default identity.</exception>
    internal static int Command(SoundEffectId sound) => Commands.TryGetValue(sound, out int command)
        ? command : throw new InvalidOperationException("The default sound identity names no sound.");

    /// <summary>Whether <paramref name="library"/>'s request <paramref name="command"/> names a sound the game asks for.</summary>
    /// <exception cref="InvalidDataException">The library is no sound library.</exception>
    internal static bool Names(SoundEffectLibrary library, int command) => library switch
    {
        SoundEffectLibrary.Samus => Library1Sounds.ContainsKey(command),
        SoundEffectLibrary.Enemies => Library2Sounds.ContainsKey(command),
        SoundEffectLibrary.Room => Library3Sounds.ContainsKey(command),
        _ => throw new InvalidDataException($"Unknown sound-effect library {library}."),
    };

    /// <summary>The sound <paramref name="library"/>'s request <paramref name="command"/> names.</summary>
    /// <exception cref="InvalidDataException">The library has no such request.</exception>
    internal static SoundEffectId Decode(SoundEffectLibrary library, int command)
    {
        SoundEffectId? sound = library switch
        {
            SoundEffectLibrary.Samus => Library1Sounds.TryGetValue(command, out SamusSound one) ? (SoundEffectId)one : null,
            SoundEffectLibrary.Enemies => Library2Sounds.TryGetValue(command, out EnemySound two) ? (SoundEffectId)two : null,
            SoundEffectLibrary.Room => Library3Sounds.TryGetValue(command, out RoomSound three) ? (SoundEffectId)three : null,
            _ => throw new InvalidDataException($"Unknown sound-effect library {library}."),
        };
        return sound ?? throw new InvalidDataException($"{library} has no sound request ${command:X2}.");
    }

    /// <summary>Indexes a library's sounds by request number, proving every request is distinct and nonzero.</summary>
    private static FrozenDictionary<int, TSound> Invert<TSound>(FrozenDictionary<TSound, byte> commands)
        where TSound : struct, Enum
    {
        if (commands.Count != Enum.GetValues<TSound>().Length)
            throw new InvalidDataException($"{typeof(TSound).Name} gives {commands.Count} of its sounds a request number.");
        var sounds = new Dictionary<int, TSound>();
        foreach (KeyValuePair<TSound, byte> entry in commands)
        {
            TSound sound = entry.Key;
            int command = entry.Value;
            if (command == 0 || !sounds.TryAdd(command, sound))
                throw new InvalidDataException($"{typeof(TSound).Name}.{sound} has request ${command:X2}, zero or another sound's.");
        }
        return sounds.ToFrozenDictionary();
    }
}
