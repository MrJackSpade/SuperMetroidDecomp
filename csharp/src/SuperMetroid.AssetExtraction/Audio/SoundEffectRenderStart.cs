using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Audio.Synthesis;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// The driver at the moment a sound-effect render stops depending on its bank context (#1503): the common and context
/// uploads and frame 0 have run and the request has not been processed. Everything the render does from here is a
/// function of this state alone.
/// </summary>
internal sealed class SoundEffectRenderStart
{
    /// <summary>The sequencer, sound-effect and DSP state bytes.</summary>
    private readonly byte[] state;

    /// <summary>A copy of audio RAM.</summary>
    private readonly byte[] ram;

    /// <summary>Captures <paramref name="player"/>'s state.</summary>
    private SoundEffectRenderStart(ManagedSpcPlayer player)
    {
        var bytes = new List<byte>();
        player.WriteSequencerState(bytes, includeTempoPhase: true);
        player.dsp.WriteState(bytes);
        state = [.. bytes];
        ram = player.ram.Snapshot();
        Samples = player.dsp.SampleBank;
        HoldsSamples = player.dsp.HoldsSamples;
    }

    /// <summary>The PCM sample bank the DSP resolves sources from.</summary>
    internal ManagedPcmSampleBank Samples { get; }

    /// <summary>Whether a voice already holds PCM, whose content recording could not follow.</summary>
    internal bool HoldsSamples { get; }

    /// <summary>Captures the start of the render <paramref name="renderer"/> is about to perform.</summary>
    internal static SoundEffectRenderStart Capture(SpcAudioClipRenderer renderer) => new(Ensure.NotNull(renderer).Player);

    /// <summary>
    /// Whether a render starting here behaves exactly as one starting at <paramref name="other"/> that read only
    /// <paramref name="ramReads"/>: neither holds PCM, every non-RAM value agrees, and every read address holds the same
    /// byte.
    /// </summary>
    internal bool Agrees(SoundEffectRenderStart other, IReadOnlyList<int> ramReads)
    {
        Ensure.NotNull(other);
        Ensure.NotNull(ramReads);
        if (HoldsSamples || other.HoldsSamples || !state.SequenceEqual(other.state))
            return false;
        foreach (int address in ramReads)
        {
            if (ram[address] != other.ram[address])
                return false;
        }
        return true;
    }
}
