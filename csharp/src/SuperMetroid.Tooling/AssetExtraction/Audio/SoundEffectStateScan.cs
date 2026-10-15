using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Audio.Synthesis;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// Static scan of one sound-effect channel program for the driver state it can inherit instead
/// of setting itself (#1305 Findings 3(b)-(e)): the stage-1 gate's evidence that every sound
/// renders as one clip per bank context whatever played before it.
/// </summary>
/// <param name="ProgramId">The decoded program's stable identifier.</param>
/// <param name="RetainsPreviousNote">(b) Its first note packet keeps the channel's previous note ($F6).</param>
/// <param name="RepeatsWithoutStart">(c) A forever or counted repeat runs before any repeat start, using a stale repeat point.</param>
/// <param name="NoiseInstruments">The instrument bytes of the packets that follow a noise enable.</param>
/// <param name="PercussionInstruments">(e) Instrument bytes with the percussion marker, biased by the music's percussion base.</param>
internal sealed record SoundEffectStateScan(
    string ProgramId,
    bool RetainsPreviousNote,
    bool RepeatsWithoutStart,
    IReadOnlyList<byte> NoiseInstruments,
    IReadOnlyList<byte> PercussionInstruments)
{
    /// <summary>Index of the note byte in a decoded play-note instruction's arguments (instrument, volume, pan, note, length).</summary>
    private const int NoteArgument = 3;

    /// <summary>Whether any of (b), (c) or (e) applies; (d) depends on the bank context.</summary>
    internal bool DependsOnPriorStateInAnyContext =>
        RetainsPreviousNote || RepeatsWithoutStart || PercussionInstruments.Count != 0;

    /// <summary>Scans <paramref name="program"/>'s instructions in stream order.</summary>
    internal static SoundEffectStateScan Scan(AudioSoundProgramMetadata program)
    {
        Ensure.NotNull(program);
        bool sawNote = false;
        bool retainsPreviousNote = false;
        bool sawRepeatStart = false;
        bool repeatsWithoutStart = false;
        bool noisePending = false;
        var noiseInstruments = new List<byte>();
        var percussionInstruments = new List<byte>();
        foreach (AudioSoundInstructionMetadata instruction in program.Instructions)
        {
            switch (instruction.Operation)
            {
                case AudioSoundInstructionOperations.PlayNote:
                    byte instrument = instruction.Arguments[0];
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
                case AudioSoundInstructionOperations.RepeatForever:
                    repeatsWithoutStart |= !sawRepeatStart;
                    break;
                case AudioSoundInstructionOperations.EnableNoise:
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
        return new SoundEffectStateScan(program.Id, retainsPreviousNote, repeatsWithoutStart, noiseInstruments, percussionInstruments);
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
}
