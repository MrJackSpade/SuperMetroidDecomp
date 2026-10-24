using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;

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
        SamusProjectileListLayout lists = SamusProjectileListEntries.Read(cartridge);
        var document = new SamusOverreadDocument(
            SamusOverreadFormat.Version,
            Range(cartridge, SamusOverreadSourceDefinitions.SpacetimeCopySource, SamusOverreadDefinitions.SpacetimeCopySourceByteCount),
            SamusOverreadSourceDefinitions.SpacetimeCopySource & ushort.MaxValue,
            Range(cartridge, SamusOverreadSourceDefinitions.SpeedBoosterGrappleCode,
                SamusOverreadDefinitions.SpeedBoosterGrappleCodeColorCount * sizeof(ushort)),
            Range(cartridge, SamusOverreadSourceDefinitions.HudPrecedingPostureBytes, SamusOverreadDefinitions.HudPrecedingPostureByteCount),
            Range(cartridge, SamusOverreadSourceDefinitions.HudFollowingPostureBytes, SamusOverreadDefinitions.HudFollowingPostureByteCount),
            TrailOverruns(cartridge, lists),
            lists.Positions.ToDictionary(position => position.Key.ToString(), position => position.Value),
            new SamusAnimationWindows(SamusSourceDefinitions.ChargeFlarePrograms.Offset,
                SamusSourceDefinitions.PoseDelayPrograms.Offset, SamusSourceDefinitions.RunningCadences.Offset));
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

    /// <summary>
    /// Reads what a trail finds past each placement list's records at every frame it can read there, as base-64 entries
    /// in frame order. Native indexes the list four bytes per frame within bank $9B's sixteen-bit offsets and takes the
    /// record's four bytes from there; a record below the cartridge half of the bank, or running into the next bank, lies
    /// in direct-page scratch and draws no trail (#1318).
    /// </summary>
    private static Dictionary<string, string> TrailOverruns(IImportCartridgeSource cartridge, SamusProjectileListLayout layout)
    {
        var lists = new Dictionary<string, string>();
        foreach (ProjectileTrailPlacementList list in Enum.GetValues<ProjectileTrailPlacementList>())
        {
            IReadOnlyList<int> frames = ProjectileTrailPlacements.OverrunFrames(list, layout);
            SnesAddress start = ProjectileTrailPlacementSources.Start(cartridge, list);
            var bytes = new byte[frames.Count * SamusOverreadDefinitions.TrailOverrunEntryByteCount];
            for (int index = 0; index < frames.Count; index++)
            {
                Span<byte> entry = bytes.AsSpan(index * SamusOverreadDefinitions.TrailOverrunEntryByteCount,
                    SamusOverreadDefinitions.TrailOverrunEntryByteCount);
                SnesAddress record = start.AddWithinBank(frames[index] * ProjectileTrailPlacementSources.RecordByteCount);
                SnesAddress last = record.AddWithinBank(ProjectileTrailPlacementSources.RecordByteCount - 1);
                if (!record.IsUpperLoRomWindow || last.Offset < record.Offset)
                {
                    entry[0] = (byte)TrailOverrunEntryKind.Scratch;
                    continue;
                }
                entry[0] = (byte)TrailOverrunEntryKind.Placement;
                RomDataReader.ReadFixedBank(cartridge, record, ProjectileTrailPlacementSources.RecordByteCount).CopyTo(entry[1..]);
            }
            lists.Add(list.ToString(), Convert.ToBase64String(bytes));
        }
        return lists;
    }

    /// <summary>Records the schema, cartridge identity and file digest checked on load.</summary>
    private sealed record Manifest(int Version, string SourceCartridgeSha256, string FileSha256);
}
