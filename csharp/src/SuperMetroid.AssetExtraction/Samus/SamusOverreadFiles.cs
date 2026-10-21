using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Extracts the cartridge bytes Samus's glitch paths read as data (owner question 3).</summary>
[CpuBusBoundary("codec: cartridge bytes Samus's glitch paths read → SamusOverreadCatalog")]
public static class SamusOverreadFiles
{
    /// <summary>Writes every overread range as base-64 JSON with a hashed manifest, then validates the result.</summary>
    /// <param name="bus">Cartridge source for the ranges.</param>
    /// <param name="directory">Destination directory, created if absent; an existing file and manifest are replaced.</param>
    /// <param name="sourceCartridgeSha256">Identity written into the manifest; validation requires the supported cartridge.</param>
    /// <exception cref="InvalidDataException">The written file has an unexpected shape, digest or provenance.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        Directory.CreateDirectory(directory);
        var document = new SamusOverreadDocument(
            SamusOverreadFormat.Version,
            Range(cartridge, SamusOverreadSourceDefinitions.SpacetimeCopySource, SamusOverreadDefinitions.SpacetimeCopySourceByteCount),
            Range(cartridge, SamusOverreadSourceDefinitions.SpeedBoosterGrappleCode,
                SamusOverreadDefinitions.SpeedBoosterGrappleCodeColorCount * sizeof(ushort)),
            Range(cartridge, SamusOverreadSourceDefinitions.HudPrecedingPostureBytes, SamusOverreadDefinitions.HudPrecedingPostureByteCount),
            Range(cartridge, SamusOverreadSourceDefinitions.HudFollowingPostureBytes, SamusOverreadDefinitions.HudFollowingPostureByteCount),
            Range(cartridge, SamusOverreadSourceDefinitions.TrailCoordinateAdjacentCode,
                SamusOverreadDefinitions.TrailCoordinateAdjacentByteCount));
        byte[] bytes = SamusOverreadCatalog.Write(document);
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
    public static SamusOverreadCatalog Load(string directory)
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
        return SamusOverreadCatalog.Load(new MemoryStream(bytes, writable: false));
    }

    /// <summary>Checks the installed file's provenance, digest and shape.</summary>
    /// <param name="directory">Installed overread directory.</param>
    public static void ValidateStock(string directory) => _ = Load(directory);

    /// <summary>Reads <paramref name="length"/> cartridge bytes from <paramref name="address"/> as base-64 text.</summary>
    private static string Range(IImportCartridgeSource cartridge, int address, int length)
    {
        var bytes = new byte[length];
        for (int offset = 0; offset < length; offset++)
            bytes[offset] = cartridge.ReadCartridgeByte(address + offset);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>Records the schema, cartridge identity and file digest checked on load.</summary>
    private sealed record Manifest(int Version, string SourceCartridgeSha256, string FileSha256);
}
