using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Extracts what enemies read past their authored data, decoded from the player's cartridge into each read's domain
/// result (#1500), and loads it as the enemy overread services.
/// </summary>
public static class EnemyOverreadFiles
{
    /// <summary>Decodes every enemy over-read and writes the results as JSON with a hashed manifest.</summary>
    /// <param name="bus">Cartridge source for the reads.</param>
    /// <param name="directory">Destination directory, created if absent; an existing file and manifest are replaced.</param>
    /// <param name="sourceCartridgeSha256">Identity written into the manifest.</param>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        Directory.CreateDirectory(directory);
        EnemyOverreadDocument document = EnemyOverreadDecoding.Decode(cartridge);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, EnemyOverreadFormat.JsonOptions);
        File.WriteAllBytes(Path.Combine(directory, EnemyOverreadFormat.FileName), bytes);
        File.WriteAllBytes(Path.Combine(directory, EnemyOverreadFormat.ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(
                new Manifest(EnemyOverreadFormat.Version, sourceCartridgeSha256, Convert.ToHexString(SHA256.HashData(bytes))),
                EnemyOverreadFormat.JsonOptions));
    }

    /// <summary>Validates the installed file's provenance and digest, then loads it.</summary>
    /// <param name="directory">Installed overread directory holding the file and its manifest.</param>
    /// <exception cref="InvalidDataException">The manifest, digest, provenance or file shape is invalid.</exception>
    public static EnemyOverreadServices Load(string directory)
    {
        Manifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<Manifest>(
                File.ReadAllBytes(Path.Combine(directory, EnemyOverreadFormat.ManifestFileName)), EnemyOverreadFormat.JsonOptions) ??
                throw new InvalidDataException("Enemy overread manifest is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid enemy overread manifest.", error);
        }
        if (manifest.Version != EnemyOverreadFormat.Version ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Enemy overread manifest does not match the pinned cartridge.");
        byte[] bytes = File.ReadAllBytes(Path.Combine(directory, EnemyOverreadFormat.FileName));
        return Read(new MemoryStream(bytes, writable: false));
    }

    /// <summary>Reads and validates an overread document and builds its services.</summary>
    private static EnemyOverreadServices Read(Stream json) =>
        Build(JsonAssetDocument.Read<EnemyOverreadDocument>(json, EnemyOverreadFormat.JsonOptions, "Enemy overread"));

    /// <summary>Builds every service, requiring an answer for every situation each one covers.</summary>
    /// <exception cref="InvalidDataException">The document is incomplete or out of range.</exception>
    private static EnemyOverreadServices Build(EnemyOverreadDocument document)
    {
        Ensure.NotNull(document);
        if (document.Version != EnemyOverreadFormat.Version)
            throw new InvalidDataException("Enemy overread file has an unsupported version.");
        IReadOnlyList<bool> apexGrowth = Require(document.MagdolliteApexGrowthBySlot, nameof(document.MagdolliteApexGrowthBySlot));
        if (apexGrowth.Count != EnemySlotOrder.Capacity)
            throw new InvalidDataException(
                $"Enemy overread file holds Magdollite apex growth for {apexGrowth.Count} slots, not {EnemySlotOrder.Capacity}.");
        EnemyOverreadDocument.Drift drift = Require(document.FallingSparkUnauthoredLaunch, nameof(document.FallingSparkUnauthoredLaunch));
        if (drift.WholePixels is < short.MinValue or > short.MaxValue || drift.Fraction is < 0 or > ushort.MaxValue)
            throw new InvalidDataException("Enemy overread file's falling-spark unauthored launch is out of range.");
        return new EnemyOverreadServices(
            new MagdolliteApexGrowth([.. apexGrowth]),
            new FallingSparkUnauthoredLaunch(SignedSixteenSixteen.FromParts(drift.WholePixels, drift.Fraction)));
    }

    /// <summary>Requires a document member.</summary>
    private static T Require<T>(T? value, string name) where T : class =>
        value ?? throw new InvalidDataException($"Enemy overread file has no {name}.");

    /// <summary>Records the schema, cartridge identity and file digest checked on load.</summary>
    private sealed record Manifest(int Version, string SourceCartridgeSha256, string FileSha256);
}
