using System.Collections.Frozen;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// Decodes a sound library's request number from cartridge data or the installed clips into the sound or stop it names,
/// and encodes a sound or stop back to its request for the sound program and the clip and override identities (#1410,
/// #2580). This is the only place the game's sounds and stops meet their cartridge numbering.
/// </summary>
internal static class SoundEffectCommands
{
    /// <summary>Each library's requests: the sound or stop of each request number.</summary>
    private static readonly FrozenDictionary<SoundEffectLibrary, FrozenDictionary<int, SoundEffectId>> Requests =
        new Dictionary<SoundEffectLibrary, FrozenDictionary<int, SoundEffectId>>
        {
            [SoundEffectLibrary.Samus] = Index(
                SoundEffectCommandDefinitions.Library1, static sound => sound,
                SoundEffectCommandDefinitions.Library1Stops, static stop => stop),
            [SoundEffectLibrary.Enemies] = Index(
                SoundEffectCommandDefinitions.Library2, static sound => sound,
                SoundEffectCommandDefinitions.Library2Stops, static stop => stop),
            [SoundEffectLibrary.Room] = Index(
                SoundEffectCommandDefinitions.Library3, static sound => sound,
                SoundEffectCommandDefinitions.Library3Stops, static stop => stop),
        }.ToFrozenDictionary();

    /// <summary>Each sound's and stop's request number in its own library.</summary>
    private static readonly FrozenDictionary<SoundEffectId, int> Commands =
        Requests.Values.SelectMany(library => library)
            .ToFrozenDictionary(request => request.Value, request => request.Key);

    /// <summary>The request number, 1 through 255, that asks the sound's library for <paramref name="sound"/>.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="sound"/> is the default identity.</exception>
    internal static int Command(SoundEffectId sound) => Commands.TryGetValue(sound, out int command)
        ? command : throw new InvalidOperationException("The default sound identity names no sound.");

    /// <summary>Whether <paramref name="library"/>'s request <paramref name="command"/> names a sound or stop the game asks for.</summary>
    /// <exception cref="InvalidDataException">The library is no sound library.</exception>
    internal static bool Names(SoundEffectLibrary library, int command) => RequestsOf(library).ContainsKey(command);

    /// <summary>The sound or stop <paramref name="library"/>'s request <paramref name="command"/> names.</summary>
    /// <exception cref="InvalidDataException">The library has no such request.</exception>
    internal static SoundEffectId Decode(SoundEffectLibrary library, int command) =>
        RequestsOf(library).TryGetValue(command, out SoundEffectId sound)
            ? sound
            : throw new InvalidDataException($"{library} has no sound request ${command:X2}.");

    /// <summary>The requests of <paramref name="library"/>.</summary>
    /// <exception cref="InvalidDataException">The library is no sound library.</exception>
    private static FrozenDictionary<int, SoundEffectId> RequestsOf(SoundEffectLibrary library) =>
        Requests.TryGetValue(library, out FrozenDictionary<int, SoundEffectId>? requests)
            ? requests
            : throw new InvalidDataException($"Unknown sound-effect library {library}.");

    /// <summary>
    /// Indexes a library's sounds and stops by request number, proving every sound and every stop has one and every
    /// request is distinct and nonzero.
    /// </summary>
    /// <param name="sounds">Each of the library's sounds' request number.</param>
    /// <param name="sound">The library-qualified identity of one of its sounds.</param>
    /// <param name="stops">Each of the library's stops' request number.</param>
    /// <param name="stop">The library-qualified identity of one of its stops.</param>
    private static FrozenDictionary<int, SoundEffectId> Index<TSound, TStop>(
        FrozenDictionary<TSound, byte> sounds, Func<TSound, SoundEffectId> sound,
        FrozenDictionary<TStop, byte> stops, Func<TStop, SoundEffectId> stop)
        where TSound : struct, Enum
        where TStop : struct, Enum
    {
        RequireEveryMember(sounds);
        RequireEveryMember(stops);
        var requests = new Dictionary<int, SoundEffectId>();
        foreach (KeyValuePair<SoundEffectId, byte> entry in Requested(sounds, sound).Concat(Requested(stops, stop)))
        {
            int command = entry.Value;
            if (command == 0 || !requests.TryAdd(command, entry.Key))
                throw new InvalidDataException($"{entry.Key} has request ${command:X2}, zero or another request's.");
        }
        return requests.ToFrozenDictionary();
    }

    /// <summary>Proves the table gives every member of <typeparamref name="TRequest"/> a request number.</summary>
    /// <exception cref="InvalidDataException">A member has no request number.</exception>
    private static void RequireEveryMember<TRequest>(FrozenDictionary<TRequest, byte> commands)
        where TRequest : struct, Enum
    {
        if (commands.Count != Enum.GetValues<TRequest>().Length)
            throw new InvalidDataException($"{typeof(TRequest).Name} gives {commands.Count} of its members a request number.");
    }

    /// <summary>The table's requests by their library-qualified <paramref name="identity"/>.</summary>
    private static IEnumerable<KeyValuePair<SoundEffectId, byte>> Requested<TRequest>(
        FrozenDictionary<TRequest, byte> commands, Func<TRequest, SoundEffectId> identity)
        where TRequest : struct, Enum =>
        commands.Select(entry => KeyValuePair.Create(identity(entry.Key), entry.Value));
}
