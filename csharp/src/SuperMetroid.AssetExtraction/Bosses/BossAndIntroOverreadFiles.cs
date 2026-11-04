using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// Extracts what Kraid's fingernails, Crocomire's volley and the opening cinematic's egg shell read past their data,
/// decoded from the player's cartridge (#1500), and loads it as the boss and intro overread services.
/// </summary>
public static class BossAndIntroOverreadFiles
{
    /// <summary>Decodes every read and writes the results as JSON with a hashed manifest.</summary>
    /// <param name="bus">Cartridge source for the reads.</param>
    /// <param name="directory">Destination directory, created if absent; an existing file and manifest are replaced.</param>
    /// <param name="sourceCartridgeSha256">Identity written into the manifest; loading requires the supported cartridge.</param>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        Directory.CreateDirectory(directory);
        BossAndIntroOverreadDocument document = BossAndIntroOverreadDecoding.Decode(cartridge);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, BossAndIntroOverreadFormat.JsonOptions);
        File.WriteAllBytes(Path.Combine(directory, BossAndIntroOverreadFormat.FileName), bytes);
        File.WriteAllBytes(Path.Combine(directory, BossAndIntroOverreadFormat.ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(
                new Manifest(BossAndIntroOverreadFormat.Version, sourceCartridgeSha256, Convert.ToHexString(SHA256.HashData(bytes))),
                BossAndIntroOverreadFormat.JsonOptions));
    }

    /// <summary>Validates the installed file's provenance and digest, then loads it.</summary>
    /// <param name="directory">Installed overread directory holding the file and its manifest.</param>
    /// <exception cref="InvalidDataException">The manifest, digest, provenance or file shape is invalid.</exception>
    public static BossAndIntroOverreadServices Load(string directory)
    {
        Manifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<Manifest>(
                File.ReadAllBytes(Path.Combine(directory, BossAndIntroOverreadFormat.ManifestFileName)), BossAndIntroOverreadFormat.JsonOptions) ??
                throw new InvalidDataException("Boss and intro overread manifest is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid boss and intro overread manifest.", error);
        }
        if (manifest.Version != BossAndIntroOverreadFormat.Version ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Boss and intro overread manifest does not match the pinned cartridge.");
        byte[] bytes = File.ReadAllBytes(Path.Combine(directory, BossAndIntroOverreadFormat.FileName));
        if (!string.Equals(manifest.FileSha256, Convert.ToHexString(SHA256.HashData(bytes)), StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Installed boss and intro overread file failed its manifest hash.");
        return Read(new MemoryStream(bytes, writable: false));
    }

    /// <summary>Checks the installed file's provenance, digest and shape.</summary>
    /// <param name="directory">Installed overread directory.</param>
    public static void ValidateStock(string directory) => _ = Load(directory);

    /// <summary>Reads and validates an overread document and builds its services.</summary>
    private static BossAndIntroOverreadServices Read(Stream json) =>
        Services(JsonAssetDocument.Read<BossAndIntroOverreadDocument>(json, BossAndIntroOverreadFormat.JsonOptions, "boss and intro overread"));

    /// <summary>Builds every service from a validated document.</summary>
    /// <exception cref="InvalidDataException">The document has an unsupported version or a value out of its range.</exception>
    private static BossAndIntroOverreadServices Services(BossAndIntroOverreadDocument document)
    {
        Ensure.NotNull(document);
        if (document.Version != BossAndIntroOverreadFormat.Version)
            throw new InvalidDataException("Boss and intro overread file has an unsupported version.");
        BossAndIntroOverreadDocument.ContourRecord contour = Require(document.KraidNailPastLedges, "Kraid nail contour record");
        IReadOnlyList<BossAndIntroOverreadDocument.FallStep> steps = Require(document.ShellFragmentFallPastCurve, "shell fragment steps");
        if (!IsSignedWord(contour.LeftOffset) || !IsSignedWord(contour.Bound) || !IsSignedWord(document.CrocomireTenthShotGradient))
            throw new InvalidDataException("Boss and intro overread file holds a word outside its range.");
        var fall = new List<SignedSixteenSixteen>();
        foreach (BossAndIntroOverreadDocument.FallStep? step in steps)
        {
            BossAndIntroOverreadDocument.FallStep present = Require(step, "shell fragment step");
            if (!IsSignedWord(present.WholePixels) || present.Fraction is < 0 or > ushort.MaxValue)
                throw new InvalidDataException("Boss and intro overread file holds a shell fragment step outside its range.");
            fall.Add(SignedSixteenSixteen.FromParts(present.WholePixels, present.Fraction));
        }
        if (fall.Count == 0)
            throw new InvalidDataException("Boss and intro overread file holds no shell fragment steps.");
        return new BossAndIntroOverreadServices(
            new KraidNailPastLedges(contour.LeftOffset, contour.Bound),
            new CrocomireTenthShotGradient(document.CrocomireTenthShotGradient),
            new ShellFragmentFallPastCurve(fall));
    }

    /// <summary>Requires a value the file must hold.</summary>
    private static T Require<T>(T? value, string name) where T : class =>
        value ?? throw new InvalidDataException($"Boss and intro overread file has no {name}.");

    /// <summary>Whether a decoded value fits a signed word.</summary>
    private static bool IsSignedWord(int value) => value is >= short.MinValue and <= short.MaxValue;

    /// <summary>Records the schema, cartridge identity and file digest checked on load.</summary>
    private sealed record Manifest(int Version, string SourceCartridgeSha256, string FileSha256);
}
