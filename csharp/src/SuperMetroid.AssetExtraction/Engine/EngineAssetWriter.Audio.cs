using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// The game's sounds and music. The Godot game plays one file for each name it asks for: a sound's is the old
/// sound's name after the library it is in, and a piece of music's the old track's name. A request that only stops
/// sounds has a name too, and its file is of nothing: played where its library sounds, it ends what was sounding.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The folders of the asset tree the Godot game's definition names for its sounds and its music.</summary>
    private const string SoundFolder = "sounds", MusicFolder = "music";

    /// <summary>
    /// The file that says, for each sound some set of music plays differently, which sets those are: each has the
    /// sound's file again in a folder of the sounds named for the set.
    /// </summary>
    private const string SoundVersionsFile = "audio/sound_versions.json";

    /// <summary>The data file of the tracks a set of music keeps differently: for each, the sets that have a file of it of their own.</summary>
    private const string MusicVersionsFile = "audio/music_versions.json";

    /// <summary>Writes every sound, every stop of sounds, and every piece of music.</summary>
    private void WriteAudio()
    {
        InstalledClipAudioLibrary audio = _installation.LoadAudioClips();
        AudioClipManifest manifest = audio.Manifest;
        string root = _installation.AudioDirectory;
        short[] Samples(AudioClipFile clip)
        {
            string path = Path.Combine(root, clip.Path);
            PcmSamples read = PcmWaveFile.ReadStereoPcm16(File.ReadAllBytes(path), path);
            if (read.SampleRate != manifest.SampleRate)
                throw new InvalidDataException($"The audio clip {path} is at {read.SampleRate} frames a second, not {manifest.SampleRate}.");
            if (read.Samples.Length != clip.Frames * PcmSampleFormat.StereoChannelCount)
                throw new InvalidDataException($"The audio clip {path} has {read.Samples.Length / PcmSampleFormat.StereoChannelCount} frames, not {clip.Frames}.");
            return read.Samples;
        }

        void WriteShape(string file, SoundEffectId sound, InstalledSoundEffectShape shape)
        {
            switch (shape)
            {
                case InstalledOneShotSoundEffect { Clip: { } clip }:
                    _tree.WriteSound(file, manifest.SampleRate, Samples(clip), null);
                    break;
                case InstalledOneShotSoundEffect:
                    // A sound the old game plays with nothing to hear.
                    _tree.WriteSound(file, manifest.SampleRate, new short[PcmSampleFormat.StereoChannelCount], null);
                    break;
                case InstalledLoopingSoundEffect looping:
                    (short[] mixed, long loopStart) = Looped(sound, [.. looping.Layers.Select(layer => (Samples(layer.Clip), layer.LoopStart))]);
                    _tree.WriteSound(file, manifest.SampleRate, mixed, loopStart);
                    break;
                default:
                    throw new InvalidDataException($"The sound {sound} is of a shape this does not know.");
            }
        }

        // The clips a shape plays, which tell one shape from another: the installation names a clip by what is in it.
        static string ClipsOf(InstalledSoundEffectShape shape) => shape switch
        {
            InstalledOneShotSoundEffect oneShot => oneShot.Clip?.Path ?? "",
            InstalledLoopingSoundEffect looping => string.Join('|', looping.Layers.Select(layer => $"{layer.Clip.Path}@{layer.LoopStart}")),
            _ => throw new InvalidDataException("A sound is of a shape this does not know."),
        };

        // One file for each sound, as the old game plays it where the set of music loaded changes nothing of it; and,
        // for a sound some set plays differently because it redefines an instrument the sound plays, one more for
        // each such set, in a folder named for the set. The game is told which sounds have a file for which sets.
        var written = new HashSet<string>(StringComparer.Ordinal);
        var versions = new JsonObject();
        foreach (InstalledSoundEffectClip entry in manifest.SoundEffects)
        {
            // A request that only stops sounds has its own file of nothing, below.
            if (EngineNames.Sound(entry.Sound) is not { } name)
                continue;
            if (!written.Add(name))
                throw new InvalidDataException($"The installation keeps the sound {entry.Sound} twice.");
            WriteShape($"{SoundFolder}/{name}.wav", entry.Sound, entry.Shape);
            var sets = new JsonArray();
            foreach (InstalledSoundEffectSetShape set in entry.ByDataSet)
            {
                if (ClipsOf(set.Shape) == ClipsOf(entry.Shape))
                    continue;
                WriteShape($"{SoundFolder}/{MusicSetName(set.DataSet)}/{name}.wav", entry.Sound, set.Shape);
                sets.Add(MusicSetName(set.DataSet));
            }

            if (sets.Count > 0)
                versions[name] = sets;
        }

        _tree.WriteData(SoundVersionsFile, versions);

        // Every sound the old game can ask for has its file.
        foreach (SoundEffectId sound in EverySound())
        {
            if (!written.Contains(EngineNames.Sound(sound)!))
                throw new InvalidDataException($"The installation keeps no clip for the sound {sound}.");
        }

        foreach (SoundEffectId stop in EveryStop())
            _tree.WriteSound($"{SoundFolder}/{EngineNames.Stop(stop)}.wav", manifest.SampleRate, new short[PcmSampleFormat.StereoChannelCount], null);

        // One file for each track, as the first set listed with it keeps it; and, where another set keeps the same
        // track differently, one more for that set, in a folder named for the set. The game is told which tracks have
        // a file for which sets.
        var musicVersions = new JsonObject();
        foreach (MusicTrack track in Enum.GetValues<MusicTrack>())
        {
            MusicClipEntry[] kept = [.. manifest.Music.Where(listed => listed.Track == track)];
            if (kept.Length == 0)
                throw new InvalidDataException($"The installation keeps no clip for the music {track}.");
            MusicClipEntry entry = kept[0];
            _tree.WriteSound($"{MusicFolder}/{MusicName(track)}.wav", manifest.SampleRate, Samples(entry.Clip), entry.LoopStart);
            var sets = new JsonArray();
            foreach (MusicClipEntry other in kept.Skip(1))
            {
                if (other.Clip.Path == entry.Clip.Path && other.LoopStart == entry.LoopStart)
                    continue;
                _tree.WriteSound($"{MusicFolder}/{MusicSetName(other.AudioSet)}/{MusicName(track)}.wav", manifest.SampleRate, Samples(other.Clip), other.LoopStart);
                sets.Add(MusicSetName(other.AudioSet));
            }

            if (sets.Count > 0)
                musicVersions[MusicName(track)] = sets;
        }

        _tree.WriteData(MusicVersionsFile, musicVersions);
    }

    /// <summary>Every sound of the old game's three libraries.</summary>
    private static IEnumerable<SoundEffectId> EverySound()
    {
        foreach (SamusSound sound in Enum.GetValues<SamusSound>())
            yield return sound;
        foreach (EnemySound sound in Enum.GetValues<EnemySound>())
            yield return sound;
        foreach (RoomSound sound in Enum.GetValues<RoomSound>())
            yield return sound;
    }

    /// <summary>Every request of the old game's three libraries that only stops sounds.</summary>
    private static IEnumerable<SoundEffectId> EveryStop()
    {
        foreach (SamusSoundStop stop in Enum.GetValues<SamusSoundStop>())
            yield return stop;
        foreach (EnemySoundStop stop in Enum.GetValues<EnemySoundStop>())
            yield return stop;
        foreach (RoomSoundStop stop in Enum.GetValues<RoomSoundStop>())
            yield return stop;
    }

    /// <summary>
    /// A sound that goes on and on, as one run of samples that loops. The old game plays such a sound as layers, each
    /// played through once and then round its own loop for good. Together they repeat once every layer has come
    /// round to where it was, so they are mixed up to the latest loop start and on through the least stretch that is
    /// a whole number of every layer's loops, and the mix loops from that start.
    /// </summary>
    private static (short[] Samples, long LoopStart) Looped(SoundEffectId sound, IReadOnlyList<(short[] Samples, long LoopStart)> layers)
    {
        const int channels = PcmSampleFormat.StereoChannelCount;
        long start = layers.Max(layer => layer.LoopStart);
        long loop = 1;
        foreach ((short[] samples, long loopStart) in layers)
        {
            long length = samples.Length / channels - loopStart;
            if (length <= 0)
                throw new InvalidDataException($"A layer of the sound {sound} loops from its end.");
            loop = checked(loop / Gcd(loop, length) * length);
        }

        long frames = checked(start + loop);
        var mixed = new short[checked(frames * channels)];
        for (long frame = 0; frame < frames; frame++)
        {
            for (int channel = 0; channel < channels; channel++)
            {
                int sum = 0;
                foreach ((short[] samples, long loopStart) in layers)
                {
                    long length = samples.Length / channels;
                    long at = frame < length ? frame : loopStart + (frame - loopStart) % (length - loopStart);
                    sum += samples[at * channels + channel];
                }

                // Voices sounding together add, and what is too loud to hold is held at the loudest there is.
                mixed[frame * channels + channel] = (short)Math.Clamp(sum, short.MinValue, short.MaxValue);
            }
        }

        return (mixed, start);

        static long Gcd(long a, long b)
        {
            while (b != 0)
                (a, b) = (b, a % b);
            return a;
        }
    }
}
