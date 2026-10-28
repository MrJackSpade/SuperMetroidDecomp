using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Audio.Synthesis;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// What clip export needs from one sound-effect channel program (#1305): whether it repeats
/// forever, so its sound renders as looping per-channel layers, and which instruments its note
/// packets select, so a bank context that does not upload their samples gets no clip.
/// </summary>
/// <param name="RepeatsForever">The program ends in a forever repeat and never ends by itself.</param>
/// <param name="Instruments">Every distinct instrument byte its note packets select.</param>
internal sealed record SoundEffectProgramScan(bool RepeatsForever, IReadOnlyList<byte> Instruments)
{
    /// <summary>Scans <paramref name="program"/>'s instructions.</summary>
    internal static SoundEffectProgramScan Scan(AudioSoundProgramMetadata program)
    {
        Ensure.NotNull(program);
        var instruments = new SortedSet<byte>();
        foreach (AudioSoundInstructionMetadata instruction in program.Instructions)
        {
            if (instruction.Operation == AudioSoundInstructionOperations.PlayNote)
                instruments.Add(instruction.Arguments[0]);
        }
        return new SoundEffectProgramScan(
            program.Instructions.Any(instruction => instruction.Operation == AudioSoundInstructionOperations.RepeatForever),
            [.. instruments]);
    }

    /// <summary>
    /// The BRR sources this program's instruments key that <paramref name="samples"/> does not
    /// map, with percussion at the power-on percussion base. A noise instrument keys source zero.
    /// An instrument index past the table is reported as such.
    /// </summary>
    internal IEnumerable<string> UnmappedSources(IReadOnlyList<AudioInstrumentMetadata> table, ManagedPcmSampleBank samples)
    {
        Ensure.NotNull(table);
        Ensure.NotNull(samples);
        foreach (int instrument in Instruments)
        {
            int index = (instrument & SpcDriverDefinitions.Instruments.PercussionMarker) != 0
                ? unchecked((byte)(instrument + SpcDriverDefinitions.Instruments.PercussionBias))
                : instrument;
            if (index >= table.Count)
            {
                yield return $"instrument ${instrument:X2} past the {table.Count}-entry table";
                continue;
            }
            int source = table[index].UsesNoise ? 0 : table[index].SourceOrNoiseRate;
            if (!samples.Samples.ContainsKey(source))
                yield return $"instrument ${instrument:X2} source ${source:X2}";
        }
    }
}
