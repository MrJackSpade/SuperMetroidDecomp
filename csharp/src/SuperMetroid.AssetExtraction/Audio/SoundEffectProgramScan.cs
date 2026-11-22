using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Audio.Synthesis;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// What clip export needs from one sound-effect channel program (#1305): whether it repeats
/// forever, so its sound renders as looping per-channel layers, and which instruments its note
/// packets select, so a sound whose render context cannot play them fails its export.
/// </summary>
/// <param name="RepeatsForever">The program ends in a forever repeat and never ends by itself.</param>
/// <param name="Instruments">Every distinct instrument byte its note packets select.</param>
internal sealed partial record SoundEffectProgramScan(bool RepeatsForever, IReadOnlyList<byte> Instruments)
{
    /// <summary>Scans <paramref name="program"/>'s instructions.</summary>
    internal static SoundEffectProgramScan Scan(AudioSoundProgramMetadata program)
    {
        Ensure.NotNull(program);
        var instruments = new SortedSet<byte>();
        foreach (AudioSoundInstructionMetadata instruction in program.Instructions)
        {
            if (instruction.Operation == AudioSoundOperation.PlayNote)
                instruments.Add(instruction.Arguments[0]);
        }
        return new SoundEffectProgramScan(
            program.Instructions.Any(instruction => instruction.Operation == AudioSoundOperation.RepeatForever),
            [.. instruments]);
    }

    /// <summary>
    /// The instruments this program keys that <paramref name="table"/> and <paramref name="samples"/> cannot play, with
    /// percussion at the power-on percussion base: an index past the table, an entry the upload leaves empty (#2603), whose
    /// zero pitch and envelope sound nothing, or a BRR source <paramref name="samples"/> does not map. A noise instrument
    /// keys source zero.
    /// </summary>
    internal IEnumerable<string> UnplayableInstruments(IReadOnlyList<AudioInstrumentMetadata> table, ManagedPcmSampleBank samples)
    {
        Ensure.NotNull(table);
        Ensure.NotNull(samples);
        foreach (int instrument in Instruments)
        {
            int index = SpcDriverBytes.IsPercussion(instrument)
                ? unchecked((byte)(instrument + SpcDriverDefinitions.Instruments.PercussionBias))
                : instrument;
            if (index >= table.Count)
            {
                yield return $"instrument ${instrument:X2} past the {table.Count}-entry table";
                continue;
            }
            if (table[index] is { UsesNoise: false, SourceOrNoiseRate: 0, Adsr1: 0, Adsr2: 0, Gain: 0, PitchBase: 0 })
            {
                yield return $"instrument ${instrument:X2} left empty";
                continue;
            }
            int source = table[index].UsesNoise ? 0 : table[index].SourceOrNoiseRate;
            if (!samples.Samples.ContainsKey(source))
                yield return $"instrument ${instrument:X2} source ${source:X2}";
        }
    }
}
