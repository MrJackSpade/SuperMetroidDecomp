using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// Reads a persistent audio override (#1305 Q3) and applies it to the stock clip manifest. Each replacement
/// names a stock song or sound effect by identity and supplies a finished stereo WAV at the native
/// rate. A song keeps its tempo timeline; a sound keeps its policy and logical end in every data set, and its
/// replacement loops exactly when the stock sound does. Anything unknown, duplicated, malformed or outside the override
/// directory fails loudly; no replacement silently falls back to stock.
/// </summary>
/// <remarks>
/// Clips are named by what they are (#1410): a song by its music data set and track,
/// <c>music-{MusicDataSet}-track-{nn}</c>, and a sound by its library's sound name,
/// <c>sfx-{Library}Sound.{Name}</c>. No cartridge data index or sound number appears in an identity; an override
/// naming a clip any other way, including the earlier hexadecimal identities, names no installed clip and fails.
/// </remarks>
internal static class ClipAudioOverrides
{
    /// <summary>A song identity: its music data set's name and its track number in decimal.</summary>
    private const string MusicPrefix = "music-";

    /// <summary>A sound-effect identity: its library's sound name.</summary>
    private const string SoundEffectPrefix = "sfx-";

    /// <summary>The selected manifest and where each replacement clip lives.</summary>
    /// <param name="Manifest">The stock manifest with the replacements applied.</param>
    /// <param name="Files">Each replacement clip's absolute path.</param>
    /// <param name="Identity">A digest of the override manifest and every replacement file.</param>
    internal sealed record Selection(AudioClipManifest Manifest, IReadOnlyDictionary<AudioClipFile, string> Files, string Identity);

    /// <summary>
    /// Loads the stock clips beneath <paramref name="audioDirectory"/>, validates them, then selects the persistent
    /// override in <paramref name="overrideDirectory"/> when that directory exists. Corrupt stock fails even with a valid
    /// override, and an invalid override never falls back to stock.
    /// </summary>
    /// <exception cref="FileNotFoundException">A manifest or a clip file is missing.</exception>
    /// <exception cref="InvalidDataException">The stock or the override is invalid.</exception>
    internal static ClipAudioLibrary Load(string audioDirectory, string? overrideDirectory)
    {
        ClipAudioLibrary.StockClips stock = ClipAudioLibrary.ReadStock(audioDirectory, InstalledAudioClips.ManifestOptions);
        if (overrideDirectory is null || !Directory.Exists(overrideDirectory))
            return ClipAudioLibrary.Select(stock, stock.Manifest, stock.Identity, new Dictionary<AudioClipFile, string>());
        Selection selection = Apply(stock.Manifest, overrideDirectory);
        // An override without replacements is audibly stock and keeps the stock identity.
        string identity = selection.Files.Count == 0
            ? stock.Identity
            : Convert.ToHexString(SHA256.HashData(Convert.FromHexString(stock.Identity + selection.Identity)));
        return ClipAudioLibrary.Select(stock, selection.Manifest, identity, selection.Files);
    }

    /// <summary>Reads the override in <paramref name="overrideDirectory"/> and applies it to <paramref name="stock"/>.</summary>
    /// <exception cref="FileNotFoundException">The override manifest or a replacement file is missing.</exception>
    /// <exception cref="InvalidDataException">The override is malformed, names an unknown or repeated clip, escapes its directory, or a replacement's format or loop is invalid.</exception>
    internal static Selection Apply(AudioClipManifest stock, string overrideDirectory)
    {
        string root = Path.GetFullPath(overrideDirectory);
        string manifestPath = Path.Combine(root, AudioClipOverrideManifest.FileName);
        if (!File.Exists(manifestPath))
            throw new FileNotFoundException($"Audio override '{root}' has no {AudioClipOverrideManifest.FileName}.", manifestPath);
        byte[] manifestBytes = File.ReadAllBytes(manifestPath);
        AudioClipOverrideManifest overrides = JsonSerializer.Deserialize<AudioClipOverrideManifest>(manifestBytes, ClipAudioLibrary.JsonOptions)
            ?? throw new InvalidDataException($"Audio override manifest '{manifestPath}' deserialized to null.");
        if (overrides.FormatVersion != AudioClipOverrideManifest.CurrentFormatVersion)
        {
            throw new InvalidDataException(
                $"Audio override manifest '{manifestPath}' uses format {overrides.FormatVersion}; expected {AudioClipOverrideManifest.CurrentFormatVersion}.");
        }

        List<MusicClipEntry> music = [.. stock.Music];
        List<SoundEffectClipEntry> soundEffects = [.. stock.SoundEffects];
        var files = new Dictionary<AudioClipFile, string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var identity = new List<byte>(manifestBytes);
        foreach (AudioClipOverride replacement in overrides.Clips)
        {
            if (!seen.Add(replacement.Id))
                throw new InvalidDataException($"Audio override repeats '{replacement.Id}'.");
            (AudioClipFile clip, string path) = ReadReplacement(root, replacement, stock.SampleRate);
            files[clip] = path;
            identity.AddRange(Convert.FromHexString(clip.Sha256));
            if (replacement.Id.StartsWith(MusicPrefix, StringComparison.Ordinal))
                ReplaceMusic(music, replacement, clip);
            else if (replacement.Id.StartsWith(SoundEffectPrefix, StringComparison.Ordinal))
                ReplaceSoundEffect(soundEffects, replacement, clip);
            else
                throw new InvalidDataException($"Audio override '{replacement.Id}' is neither a music nor a sound-effect identity.");
        }
        return new Selection(
            stock with { Music = music, SoundEffects = soundEffects },
            files,
            Convert.ToHexString(SHA256.HashData(identity.ToArray())));
    }

    /// <summary>The music identity of <paramref name="dataSet"/>'s <paramref name="track"/>, as an override names it.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="dataSet"/> is not a defined data set.</exception>
    internal static string MusicId(MusicDataSet dataSet, int track) =>
        Enum.IsDefined(dataSet)
            ? $"{MusicPrefix}{dataSet}-track-{track:00}"
            : throw new ArgumentOutOfRangeException(nameof(dataSet), dataSet, "Not a defined music data set.");

    /// <summary>The identity an override names <paramref name="sound"/> by: its library's sound name.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="sound"/> is the default identity, which names no sound.</exception>
    internal static string SoundEffectId(Core.Audio.SoundEffectId sound) => $"{SoundEffectPrefix}{sound}";

    /// <summary>Reads and checks one replacement WAV beneath the override directory.</summary>
    private static ClipReplacement ReadReplacement(string root, AudioClipOverride replacement, int sampleRate)
    {
        if (string.IsNullOrWhiteSpace(replacement.File) || Path.IsPathRooted(replacement.File))
            throw new InvalidDataException($"Audio override '{replacement.Id}' file '{replacement.File}' must be relative.");
        string path = Path.GetFullPath(Path.Combine(root, replacement.File));
        string prefix = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;
        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Audio override '{replacement.Id}' file '{replacement.File}' escapes '{root}'.");
        if (!File.Exists(path))
            throw new FileNotFoundException($"Audio override '{replacement.Id}' file '{path}' is missing.", path);
        byte[] bytes = File.ReadAllBytes(path);
        (int rate, short[] samples) = PcmWaveFile.ReadStereoPcm16(bytes, path);
        if (rate != sampleRate)
            throw new InvalidDataException($"Audio override '{replacement.Id}' is {rate} Hz; clips are {sampleRate} Hz.");
        long frames = samples.Length / PcmSampleFormat.StereoChannelCount;
        if (frames == 0)
            throw new InvalidDataException($"Audio override '{replacement.Id}' is empty.");
        if (replacement.LoopStart is long loop && (loop < 0 || loop >= frames))
            throw new InvalidDataException($"Audio override '{replacement.Id}' loops at {loop} of {frames} frames.");
        return new ClipReplacement(new AudioClipFile(replacement.File, Convert.ToHexString(SHA256.HashData(bytes)), frames), path);
    }

    /// <summary>Replaces one song's clip and loop, keeping its tempo timeline.</summary>
    private static void ReplaceMusic(List<MusicClipEntry> music, AudioClipOverride replacement, AudioClipFile clip)
    {
        int index = music.FindIndex(entry => MusicId(entry.AudioSet, entry.Track) == replacement.Id);
        if (index < 0)
            throw new InvalidDataException($"Audio override '{replacement.Id}' names no installed song.");
        music[index] = music[index] with { Clip = clip, LoopStart = replacement.LoopStart };
    }

    /// <summary>Replaces one sound's layers in every data set with the replacement clip.</summary>
    private static void ReplaceSoundEffect(List<SoundEffectClipEntry> soundEffects, AudioClipOverride replacement, AudioClipFile clip)
    {
        bool found = false;
        for (int index = 0; index < soundEffects.Count; index++)
        {
            SoundEffectClipEntry entry = soundEffects[index];
            if (SoundEffectId(entry.Sound) != replacement.Id)
                continue;
            found = true;
            if ((entry.ActiveFrames is null) != (replacement.LoopStart is not null))
            {
                throw new InvalidDataException(entry.ActiveFrames is null
                    ? $"Audio override '{replacement.Id}' replaces a never-ending sound and needs a loop start."
                    : $"Audio override '{replacement.Id}' replaces a one-shot sound and cannot loop.");
            }
            SoundEffectVoices voices = SoundEffectVoices.None;
            foreach (SoundEffectClipLayer layer in entry.Layers)
                voices |= layer.Voices;
            soundEffects[index] = entry with { Layers = [new SoundEffectClipLayer(clip, replacement.LoopStart, voices, [0])] };
        }
        if (!found)
            throw new InvalidDataException($"Audio override '{replacement.Id}' names no installed sound effect.");
    }
}
