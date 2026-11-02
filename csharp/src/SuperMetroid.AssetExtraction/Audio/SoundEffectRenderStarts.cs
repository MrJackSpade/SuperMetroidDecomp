namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// The starts of every renderer one sound-effect render uses in one bank context (#1503): the synthesizing start every
/// render uses, and for a never-ending effect the sequencing start that finds its channel loops.
/// </summary>
/// <param name="Synthesized">The start of the renderers that synthesize PCM.</param>
/// <param name="Sequenced">The start of the renderer that only sequences, or null for a one-shot effect.</param>
internal sealed record SoundEffectRenderStarts(SoundEffectRenderStart Synthesized, SoundEffectRenderStart? Sequenced);
