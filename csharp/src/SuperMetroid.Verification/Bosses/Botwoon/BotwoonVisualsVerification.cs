using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Botwoon.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.VerificationOracles;

namespace SuperMetroid.Verification.Bosses.Botwoon;

/// <summary>Checks installed Botwoon head frames and compatibility of older enemy artwork overrides.</summary>
internal static class BotwoonVisualsVerification
{
    /// <summary>JSON options: Camel-case property names.</summary>
    private static readonly JsonSerializerOptions CamelCaseJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>Compares sixteen installed head frames to native OAM and confirms pre-Botwoon edits survive schema upgrade.</summary>
    internal static void VerifyInstalledBotwoonVisuals(
        SuperMetroidAddressSpace rom, EnemyTileSourceArtwork stock)
    {
        // The head animations' frames are checked against the cartridge lists by VerifyBotwoonHeadAnimations;
        // here every visible frame's installed artwork must draw the cartridge's OAM.
        var selected = new HashSet<ushort>();
        foreach (NativeEnemySpritemapDefinition frame in BotwoonVisualDefinitionsConstants.Frames())
        {
            ushort installedPointer = frame.Pointer;
            ushort native = installedPointer;
            selected.Add(installedPointer);
            AssertTrue((stock.Spritemaps ?? throw new InvalidOperationException("Verification requires stock.Spritemaps.")).TryGetDisplay(
                    BotwoonVisualDefinitionsConstants.Bank, installedPointer,
                    out EnemySpritemapParts installedParts),
                $"Botwoon head frame $B3:{installedPointer:X4} is editable");
            var nativeOam = new OamBuffer();
            var installedOam = new OamBuffer();
            DrawImportedEnemySpritemap(rom, nativeOam, new NativeEnemySpritemapDraw { Bank = BotwoonVisualDefinitionsConstants.Bank, Pointer = native, Origin = new(128, 128), PaletteBits = 0, BaseTileIndex = 0 });
            installedOam.AddEnemySpritemap(installedParts, new Position16<SpritePixels>(128, 128), 0, 0);
            AssertTrue(nativeOam.ImageEquals(installedOam) &&
                       nativeOam.NextByteOffset == installedOam.NextByteOffset,
                $"Botwoon head frame $B3:{installedPointer:X4} matches cartridge OAM");
        }
        AssertEqual(16, selected.Count,
            "Botwoon head selects sixteen distinct visible OAM frames");

        // Existing version-59 edits must survive the newly extracted head frames.
        byte[] current = EnemySpritemapFiles.Extract(rom);
        EnemySpritemapDocument document = RequiredJson.Deserialize<EnemySpritemapDocument>(
            current, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        // Version 59 ends before Botwoon; removing only Botwoon from today's
        // catalog incorrectly retains every family added in subsequent versions.
        // Version 59 named frames as they were named before frames were named for their meaning (#1465).
        EnemySpritemapDefinition[] previous = EnemySpritemapDefinitions.Frames
            .Take(EnemySpritemapDefinitions.PreBotwoonFrameCount).ToArray();
        Dictionary<string, string> legacyNames = EnemySpritemapDefinitions.Frames
            .ToDictionary(frame => frame.Name, frame => frame.LegacyName, StringComparer.Ordinal);
        var previousFrames = previous
            .ToDictionary(frame => frame.LegacyName, frame => document.Frames[frame.Name], StringComparer.Ordinal);
        Dictionary<string, string> bindings = document.DisplayFrames ?? throw new InvalidOperationException("Verification requires document.DisplayFrames.");
        var previousBindings = previous
            .ToDictionary(frame => frame.LegacyName, frame => legacyNames[bindings[frame.Name]], StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreBotwoonFrameCount,
            previousFrames.Count, "version-fifty-nine composition schema count");
        using var previousJson = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapSchema.PreBotwoon.Revision,
                Frames = previousFrames,
                DisplayFrames = previousBindings,
            }, CamelCaseJson));
        EnemySpritemapCatalog upgraded = EnemySpritemapDocuments.Load(
            previousJson, stock.Spritemaps);
        foreach (NativeEnemySpritemapDefinition frame in BotwoonVisualDefinitionsConstants.Frames())
            AssertTrue(upgraded.TryGetDisplay(frame.Bank, frame.Pointer, out _),
                $"version-fifty-nine override inherits {frame.Name}");
        Console.WriteLine(
            "  Botwoon visuals: 16 typed head frames draw the cartridge OAM, and legacy override migration passes.");
    }
}
