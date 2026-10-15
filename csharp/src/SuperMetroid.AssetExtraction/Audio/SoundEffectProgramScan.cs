using SuperMetroid.Core;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// Static scan of one sound-effect channel program for the driver state it can inherit instead
/// of setting itself (#1305 Findings 3(b)-(e)). A program with none of these renders the same
/// clip whatever played before it in its bank context.
/// </summary>
/// <param name="ProgramId">The decoded program's stable identifier.</param>
/// <param name="RetainsPreviousNote">(b) Its first note packet keeps the channel's previous note ($F6).</param>
/// <param name="RepeatsWithoutStart">(c) A forever or counted repeat runs before any repeat start, using a stale repeat point.</param>
/// <param name="EnablesNoise">It enables noise ($FC), whose rate is global; whether a context depends on another sound's rate is in <see cref="NoiseInstruments"/>.</param>
/// <param name="NoiseInstruments">The instrument bytes of the packets that follow a noise enable.</param>
/// <param name="PercussionInstruments">(e) Instrument bytes with the percussion marker, biased by the music's percussion base.</param>
/// <param name="RepeatsForever">The program ends in a forever repeat and never ends by itself.</param>
/// <param name="Instruments">Every distinct instrument byte its note packets select.</param>
internal sealed record SoundEffectProgramScan(
    string ProgramId,
    bool RetainsPreviousNote,
    bool RepeatsWithoutStart,
    bool EnablesNoise,
    IReadOnlyList<byte> NoiseInstruments,
    IReadOnlyList<byte> PercussionInstruments,
    bool RepeatsForever,
    IReadOnlyList<byte> Instruments)
{
    /// <summary>Index of the note byte in a decoded play-note instruction's arguments (instrument, volume, pan, note, length).</summary>
    private const int NoteArgument = 3;

    /// <summary>Scans <paramref name="program"/>'s instructions in stream order.</summary>
    internal static SoundEffectProgramScan Scan(AudioSoundProgramMetadata program)
    {
        Ensure.NotNull(program);
        bool sawNote = false;
        bool retainsPreviousNote = false;
        bool sawRepeatStart = false;
        bool repeatsWithoutStart = false;
        bool noisePending = false;
        bool enablesNoise = false;
        bool repeatsForever = false;
        var noiseInstruments = new List<byte>();
        var percussionInstruments = new List<byte>();
        var instruments = new SortedSet<byte>();
        foreach (AudioSoundInstructionMetadata instruction in program.Instructions)
        {
            switch (instruction.Operation)
            {
                case AudioSoundInstructionOperations.PlayNote:
                    byte instrument = instruction.Arguments[0];
                    instruments.Add(instrument);
                    if (!sawNote && instruction.Arguments[NoteArgument] == SpcDriverDefinitions.SoundEffects.RetainedNote)
                        retainsPreviousNote = true;
                    sawNote = true;
                    if (noisePending)
                        noiseInstruments.Add(instrument);
                    noisePending = false;
                    if ((instrument & SpcDriverDefinitions.Instruments.PercussionMarker) != 0)
                        percussionInstruments.Add(instrument);
                    break;
                case AudioSoundInstructionOperations.BeginRepeat:
                    sawRepeatStart = true;
                    break;
                case AudioSoundInstructionOperations.EndRepeat:
                    repeatsWithoutStart |= !sawRepeatStart;
                    break;
                case AudioSoundInstructionOperations.RepeatForever:
                    repeatsWithoutStart |= !sawRepeatStart;
                    repeatsForever = true;
                    break;
                case AudioSoundInstructionOperations.EnableNoise:
                    enablesNoise = true;
                    noisePending = true;
                    break;
                case AudioSoundInstructionOperations.SetAdsr:
                case AudioSoundInstructionOperations.PitchSlide:
                case AudioSoundInstructionOperations.PitchSlideLegato:
                case AudioSoundInstructionOperations.End:
                    break;
                default:
                    throw new InvalidDataException($"Sound program '{program.Id}' uses unknown operation '{instruction.Operation}'.");
            }
        }
        return new SoundEffectProgramScan(
            program.Id, retainsPreviousNote, repeatsWithoutStart, enablesNoise,
            noiseInstruments, percussionInstruments, repeatsForever, [.. instruments]);
    }

    /// <summary>
    /// (d) Whether a noise packet in <paramref name="instruments"/>'s bank context selects an
    /// instrument without a noise rate, so it plays at whatever rate another sound or the music set.
    /// </summary>
    internal bool UsesInheritedNoiseRate(IReadOnlyList<AudioInstrumentMetadata> instruments)
    {
        Ensure.NotNull(instruments);
        foreach (byte instrument in NoiseInstruments)
        {
            if ((instrument & SpcDriverDefinitions.Instruments.PercussionMarker) != 0 || !instruments[instrument].UsesNoise)
                return true;
        }
        return false;
    }

    /// <summary>
    /// The BRR sources this program's instruments key that <paramref name="samples"/> does not
    /// map, with percussion at the power-on percussion base. A noise instrument keys source zero.
    /// An instrument index past the table is reported as its index with the marker bit clear.
    /// </summary>
    internal IEnumerable<string> UnmappedSources(IReadOnlyList<AudioInstrumentMetadata> table, ManagedPcmSampleBank samples)
    {
        Ensure.NotNull(table);
        Ensure.NotNull(samples);
        foreach (byte instrument in Instruments)
        {
            int index = (instrument & SpcDriverDefinitions.Instruments.PercussionMarker) != 0
                ? unchecked((byte)(instrument + SpcDriverDefinitions.Instruments.PercussionBias))
                : instrument;
            if (index >= table.Count)
            {
                yield return $"instrument ${instrument:X2} past the {table.Count}-entry table";
                continue;
            }
            byte source = table[index].UsesNoise ? (byte)0 : table[index].SourceOrNoiseRate;
            if (!samples.Samples.ContainsKey(source))
                yield return $"instrument ${instrument:X2} source ${source:X2}";
        }
    }

    /// <summary>Whether any of (b), (c) or (e) applies; (d) depends on the bank context.</summary>
    internal bool DependsOnPriorStateInAnyContext =>
        RetainsPreviousNote || RepeatsWithoutStart || PercussionInstruments.Count != 0;
}
