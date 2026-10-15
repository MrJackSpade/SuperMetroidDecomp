using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Cryptography;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>The extracted audio streams, sample banks, metadata and identity an audio asset catalog is built from (#633).</summary>
internal sealed record ExtractedAudioAssetParts
{
    /// <summary>Streams.</summary>
    public required IReadOnlyDictionary<AudioBank, byte[]> Streams { get; init; }

    /// <summary>Sample banks.</summary>
    public required IReadOnlyDictionary<AudioBank, ManagedPcmSampleBank> SampleBanks { get; init; }

    /// <summary>Instrument banks.</summary>
    public required IReadOnlyDictionary<AudioBank, IReadOnlyList<AudioInstrumentMetadata>> InstrumentBanks { get; init; }

    /// <summary>Music banks.</summary>
    public required IReadOnlyDictionary<AudioBank, AudioBankMetadata> MusicBanks { get; init; }

    /// <summary>Decoded, address-stable authored SFX channel programs.</summary>
    public required IReadOnlyList<AudioSoundProgramMetadata> SoundPrograms { get; init; }

    /// <summary>Stable SFX command IDs and their compiled routing/allocation metadata.</summary>
    public required IReadOnlyList<AudioSoundLibraryMetadata> SoundLibraries { get; init; }

    /// <summary>Canonical sample count.</summary>
    public required int CanonicalSampleCount { get; init; }

    /// <summary>Stable SHA-256 identity of the validated selected manifest. The canonical JSON contains every upload/WAV hash plus all editable routing, instrument, music, and SFX definitions, so formatting-only changes do not alter this value while any audible edit does.</summary>
    public required string ContentIdentity { get; init; }
}
