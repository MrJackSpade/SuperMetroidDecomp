using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// A catalog's authored sound-effect programs compiled once (#1490): every program's encoded bytes
/// for its fixed slot, and the program each effect channel's uploaded routing pointer must name.
/// Every bank load installs the same bytes and checks the same routes, so the catalog compiles them
/// when it loads and every player shares them read-only.
/// </summary>
internal sealed class SpcSoundEffectImage
{
    /// <summary>One program and the bytes written at its fixed slot.</summary>
    /// <param name="Program">The authored program.</param>
    /// <param name="Bytes">Its encoding, exactly its slot's capacity.</param>
    internal sealed record EncodedProgram(AudioSoundProgramMetadata Program, ReadOnlyMemory<byte> Bytes);

    /// <summary>One effect channel and the program its uploaded routing pointer must name.</summary>
    /// <param name="Effect">The effect whose routing table holds the pointer.</param>
    /// <param name="Channel">The channel's index in the effect's routing table.</param>
    /// <param name="Program">The program the channel plays.</param>
    internal sealed record Route(AudioSoundEffectMetadata Effect, int Channel, AudioSoundProgramMetadata Program);

    /// <summary>Encodes every program in order and resolves every effect channel's program.</summary>
    /// <exception cref="InvalidDataException">Programs repeat a stable ID, a program does not encode into its fixed slot, or an effect names an unknown program.</exception>
    internal SpcSoundEffectImage(
        IReadOnlyList<AudioSoundProgramMetadata> programs,
        IReadOnlyList<AudioSoundLibraryMetadata> libraries)
    {
        Ensure.NotNull(programs);
        Ensure.NotNull(libraries);
        Dictionary<string, AudioSoundProgramMetadata> byId = new(StringComparer.Ordinal);
        var encoded = new EncodedProgram[programs.Count];
        for (int index = 0; index < programs.Count; index++)
        {
            AudioSoundProgramMetadata program = programs[index];
            if (!byId.TryAdd(program.Id, program))
                throw new InvalidDataException($"Decoded SFX programs repeat stable ID '{program.Id}'.");
            encoded[index] = new EncodedProgram(program, SpcSoundEffectProgramCodec.Encode(program));
        }

        var routes = new List<Route>();
        foreach (AudioSoundLibraryMetadata library in libraries)
        {
            foreach (AudioSoundEffectMetadata effect in library.Effects)
            {
                for (int channel = 0; channel < effect.ChannelPrograms.Count; channel++)
                {
                    if (!byId.TryGetValue(effect.ChannelPrograms[channel], out AudioSoundProgramMetadata? program))
                    {
                        throw new InvalidDataException(
                            $"SFX effect '{effect.Id}' references unknown program " +
                            $"'{effect.ChannelPrograms[channel]}'.");
                    }
                    routes.Add(new Route(effect, channel, program));
                }
            }
        }
        Programs = encoded;
        Routes = [.. routes];
    }

    /// <summary>Every program's bytes, in catalog order, which is the order a bank load writes them.</summary>
    internal IReadOnlyList<EncodedProgram> Programs { get; }

    /// <summary>Every effect channel's route, in library, effect and channel order.</summary>
    internal IReadOnlyList<Route> Routes { get; }
}
