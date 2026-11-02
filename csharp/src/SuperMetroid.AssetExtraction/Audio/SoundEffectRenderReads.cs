using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Audio.Synthesis;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// Everything one sound-effect render read after its start (#1503): the audio RAM addresses read by the renderers that
/// synthesized PCM and by those that only sequenced, and every PCM source looked up. A render runs several renderers
/// from the same start; the reads of all of them are collected here.
/// </summary>
internal sealed class SoundEffectRenderReads
{
    /// <summary>The players whose renderers synthesized PCM.</summary>
    private readonly List<ManagedSpcPlayer> synthesized = [];

    /// <summary>The players whose renderers only advanced the sequencer.</summary>
    private readonly List<ManagedSpcPlayer> sequenced = [];

    /// <summary>Starts recording <paramref name="player"/>'s reads, filed by whether it synthesizes PCM.</summary>
    internal void Attach(ManagedSpcPlayer player, bool synthesize)
    {
        Ensure.NotNull(player).RecordReads();
        (synthesize ? synthesized : sequenced).Add(player);
    }

    /// <summary>The audio RAM addresses the synthesizing renderers read.</summary>
    internal int[] SynthesizedRamReads() => RamReads(synthesized);

    /// <summary>The audio RAM addresses the sequencing renderers read.</summary>
    internal int[] SequencedRamReads() => RamReads(sequenced);

    /// <summary>Every PCM source any renderer looked up.</summary>
    internal int[] SampleSources() =>
        [.. synthesized.Concat(sequenced).SelectMany(player => player.dsp.RecordedSampleSources()).Distinct().Order()];

    /// <summary>The union of <paramref name="players"/>' recorded RAM reads, in address order.</summary>
    private static int[] RamReads(List<ManagedSpcPlayer> players) =>
        [.. players.SelectMany(player => player.ram.RecordedReads()).Distinct().Order()];
}
