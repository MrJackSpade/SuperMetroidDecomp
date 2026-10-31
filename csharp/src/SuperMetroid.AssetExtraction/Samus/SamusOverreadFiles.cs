using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Extracts what Samus's glitch paths read past their data, decoded from the player's cartridge into each read's domain
/// result (owner question 3, #1474), and loads it as the Samus overread services.
/// </summary>
public static class SamusOverreadFiles
{
    /// <summary>Decodes every glitch read, writes the results as JSON with a hashed manifest, then validates the result.</summary>
    /// <param name="bus">Cartridge source for the reads.</param>
    /// <param name="directory">Destination directory, created if absent; an existing file and manifest are replaced.</param>
    /// <param name="sourceCartridgeSha256">Identity written into the manifest; validation requires the supported cartridge.</param>
    /// <exception cref="InvalidDataException">The written file has an unexpected shape, digest or provenance.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        Directory.CreateDirectory(directory);
        SamusOverreadDocument document = SamusOverreadDecoding.Decode(cartridge);
        byte[] bytes = JsonAssetDocument.WriteValidated(document, SamusOverreadFormat.JsonOptions, json => _ = Read(json));
        File.WriteAllBytes(Path.Combine(directory, SamusOverreadFormat.FileName), bytes);
        File.WriteAllBytes(Path.Combine(directory, SamusOverreadFormat.ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(
                new Manifest(SamusOverreadFormat.Version, sourceCartridgeSha256, Convert.ToHexString(SHA256.HashData(bytes))),
                SamusOverreadFormat.JsonOptions));
        _ = Load(directory);
    }

    /// <summary>Validates the installed file's provenance and digest, then loads it.</summary>
    /// <param name="directory">Installed overread directory holding the file and its manifest.</param>
    /// <exception cref="InvalidDataException">The manifest, digest, provenance or file shape is invalid.</exception>
    public static SamusOverreadServices Load(string directory)
    {
        Manifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<Manifest>(
                File.ReadAllBytes(Path.Combine(directory, SamusOverreadFormat.ManifestFileName)), SamusOverreadFormat.JsonOptions) ??
                throw new InvalidDataException("Samus overread manifest is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Samus overread manifest.", error);
        }
        if (manifest.Version != SamusOverreadFormat.Version ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Samus overread manifest does not match the pinned cartridge.");
        byte[] bytes = File.ReadAllBytes(Path.Combine(directory, SamusOverreadFormat.FileName));
        if (!string.Equals(manifest.FileSha256, Convert.ToHexString(SHA256.HashData(bytes)), StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Installed Samus overread file failed its manifest hash.");
        return Read(new MemoryStream(bytes, writable: false));
    }

    /// <summary>Checks the installed file's provenance, digest and shape.</summary>
    /// <param name="directory">Installed overread directory.</param>
    public static void ValidateStock(string directory) => _ = Load(directory);

    /// <summary>Reads and validates an overread document and builds its services.</summary>
    private static SamusOverreadServices Read(Stream json) =>
        SamusOverreadServicesReader.Read(JsonAssetDocument.Read<SamusOverreadDocument>(json, SamusOverreadFormat.JsonOptions, "Samus overread"));

    /// <summary>Records the schema, cartridge identity and file digest checked on load.</summary>
    private sealed record Manifest(int Version, string SourceCartridgeSha256, string FileSha256);
}
