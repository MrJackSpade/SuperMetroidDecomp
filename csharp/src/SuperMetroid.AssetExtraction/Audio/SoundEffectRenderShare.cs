using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// One rendered sound effect and every input it read (#1503). The driver is deterministic, so a render from another
/// bank context that starts in the same state and would read the same bytes and the same PCM produces this render
/// exactly; such a context takes it instead of rendering again.
/// </summary>
internal sealed class SoundEffectRenderShare
{
    /// <summary>Where the render started.</summary>
    private readonly SoundEffectRenderStarts starts;

    /// <summary>The audio RAM addresses the synthesizing renderers read.</summary>
    private readonly int[] synthesizedReads;

    /// <summary>The audio RAM addresses the sequencing renderers read.</summary>
    private readonly int[] sequencedReads;

    /// <summary>Every PCM source the render looked up.</summary>
    private readonly int[] sampleSources;

    /// <summary>Keeps a render with what it read.</summary>
    /// <param name="starts">Where the render started.</param>
    /// <param name="reads">What the render's renderers read after the start.</param>
    /// <param name="rendering">The render: a <see cref="SoundEffectRendering.OneShot"/> or its looping layers.</param>
    internal SoundEffectRenderShare(SoundEffectRenderStarts starts, SoundEffectRenderReads reads, object rendering)
    {
        this.starts = Ensure.NotNull(starts);
        Ensure.NotNull(reads);
        synthesizedReads = reads.SynthesizedRamReads();
        sequencedReads = reads.SequencedRamReads();
        sampleSources = reads.SampleSources();
        Rendering = Ensure.NotNull(rendering);
    }

    /// <summary>The render: a <see cref="SoundEffectRendering.OneShot"/> or its looping layers.</summary>
    internal object Rendering { get; }

    /// <summary>Whether a render starting at <paramref name="other"/> reproduces this one exactly.</summary>
    internal bool Reproduces(SoundEffectRenderStarts other)
    {
        Ensure.NotNull(other);
        if (!starts.Synthesized.Agrees(other.Synthesized, synthesizedReads))
            return false;
        if (starts.Sequenced is null || other.Sequenced is null)
        {
            if (starts.Sequenced is not null || other.Sequenced is not null)
                return false;
        }
        else if (!starts.Sequenced.Agrees(other.Sequenced, sequencedReads))
            return false;
        return Array.TrueForAll(sampleSources, source => starts.Synthesized.Samples.ResolvesAlike(other.Synthesized.Samples, source));
    }
}
