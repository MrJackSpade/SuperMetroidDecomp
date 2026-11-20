using System.Text.Json.Serialization;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// One sound effect rendered dry once, as the clip manifest stores it (#1305, #2057): the entry the game plays, whichever
/// music data set is loaded. Frame zero is the request check that accepted the request. Its shape is one of three, each
/// holding what that shape needs and nothing else (#2554): an audible one-shot, a silent one-shot, or an effect that
/// repeats forever, one looping layer per channel.
/// </summary>
/// <param name="Sound">The sound effect, whose library plays it.</param>
/// <param name="Policy">How the library accepts and starts the request.</param>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "shape")]
[JsonDerivedType(typeof(OneShot), "oneShot")]
[JsonDerivedType(typeof(Silent), "silent")]
[JsonDerivedType(typeof(Repeating), "repeating")]
public abstract record InstalledSoundEffectClip(SoundEffectId Sound, SoundEffectClipPolicy Policy)
{
    /// <summary>The rendered clips the entry plays, whose files the installation holds.</summary>
    public abstract IEnumerable<AudioClipFile> Clips { get; }

    /// <summary>The entry the game plays; a method, so the manifest does not store it.</summary>
    public abstract SoundEffectClipEntry PlayedEntry();

    /// <summary>Checks the entry's own bounds: each clip has frames, and each loop starts within its clip.</summary>
    /// <exception cref="InvalidDataException">A clip has no frames, a loop starts outside its clip, or a repeating effect has no layer.</exception>
    public abstract void Validate();

    /// <summary>Checks a clip has frames.</summary>
    /// <exception cref="InvalidDataException">The clip has no frames.</exception>
    protected static void ValidateClip(AudioClipFile clip)
    {
        if (clip.Frames <= 0)
            throw new InvalidDataException($"Audio clip '{clip.Path}' has {clip.Frames} frames.");
    }

    /// <summary>An audible one-shot effect: one clip, played once, and the library's logical end.</summary>
    /// <param name="Sound">The sound effect, whose library plays it.</param>
    /// <param name="Policy">How the library accepts and starts the request.</param>
    /// <param name="ActiveFrames">Frames until the library's logical end, when its priority resets.</param>
    /// <param name="Clip">The rendered effect.</param>
    public sealed record OneShot(SoundEffectId Sound, SoundEffectClipPolicy Policy, long ActiveFrames, AudioClipFile Clip)
        : InstalledSoundEffectClip(Sound, Policy)
    {
        /// <inheritdoc/>
        public override IEnumerable<AudioClipFile> Clips => [Clip];

        /// <inheritdoc/>
        public override SoundEffectClipEntry PlayedEntry() => new(Sound, Policy, ActiveFrames, [new SoundEffectClipLayer(Clip, null)]);

        /// <inheritdoc/>
        public override void Validate() => ValidateClip(Clip);
    }

    /// <summary>A silent one-shot effect: it plays nothing, yet occupies its library until its logical end.</summary>
    /// <param name="Sound">The sound effect, whose library plays it.</param>
    /// <param name="Policy">How the library accepts and starts the request.</param>
    /// <param name="ActiveFrames">Frames until the library's logical end, when its priority resets.</param>
    public sealed record Silent(SoundEffectId Sound, SoundEffectClipPolicy Policy, long ActiveFrames)
        : InstalledSoundEffectClip(Sound, Policy)
    {
        /// <inheritdoc/>
        public override IEnumerable<AudioClipFile> Clips => [];

        /// <inheritdoc/>
        public override SoundEffectClipEntry PlayedEntry() => new(Sound, Policy, ActiveFrames, []);

        /// <inheritdoc/>
        public override void Validate()
        {
        }
    }

    /// <summary>An effect that never ends: one looping layer per channel, and no logical end.</summary>
    /// <param name="Sound">The sound effect, whose library plays it.</param>
    /// <param name="Policy">How the library accepts and starts the request.</param>
    /// <param name="Layers">One looping layer per channel.</param>
    public sealed record Repeating(SoundEffectId Sound, SoundEffectClipPolicy Policy, IReadOnlyList<LoopingLayer> Layers)
        : InstalledSoundEffectClip(Sound, Policy)
    {
        /// <inheritdoc/>
        public override IEnumerable<AudioClipFile> Clips => Layers.Select(layer => layer.Clip);

        /// <inheritdoc/>
        public override SoundEffectClipEntry PlayedEntry() =>
            new(Sound, Policy, null, [.. Layers.Select(layer => new SoundEffectClipLayer(layer.Clip, layer.LoopStart))]);

        /// <inheritdoc/>
        public override void Validate()
        {
            if (Layers.Count == 0)
                throw new InvalidDataException($"Never-ending {Sound} has no layers.");
            foreach (LoopingLayer layer in Layers)
            {
                ValidateClip(layer.Clip);
                if (layer.LoopStart < 0 || layer.LoopStart >= layer.Clip.Frames)
                    throw new InvalidDataException($"{Sound} loops at {layer.LoopStart} of {layer.Clip.Frames} frames.");
            }
        }
    }

    /// <summary>One channel of a repeating effect.</summary>
    /// <param name="Clip">The rendered channel.</param>
    /// <param name="LoopStart">The first frame of its loop, repeated to the clip's end.</param>
    public sealed record LoopingLayer(AudioClipFile Clip, long LoopStart);
}
