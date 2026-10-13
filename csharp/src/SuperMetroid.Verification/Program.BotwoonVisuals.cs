using SuperMetroid.Core.Rom;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static ImportedSpritemapsVerification;

/// <summary>Checks installed Botwoon head frames and compatibility of older enemy artwork overrides.</summary>
internal static class BotwoonVisualsVerification
{
    /// <summary>Compares sixteen installed head frames to native OAM and confirms pre-Botwoon edits survive schema upgrade.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyInstalledBotwoonVisuals(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        // The head animations' frames are checked against the cartridge lists by VerifyBotwoonHeadAnimations;
        // here every visible frame's installed artwork must draw the cartridge's OAM.
        var selected = new HashSet<ushort>();
        foreach (EnemySpritemapDefinition frame in BotwoonVisualDefinitions.Frames())
        {
            ushort installedPointer = frame.Pointer;
            ushort native = installedPointer;
            selected.Add(installedPointer);
            AssertTrue(stock.Spritemaps!.TryGetDisplay(
                    BotwoonVisualDefinitions.Bank, installedPointer,
                    out EnemySpritemapParts installedParts),
                $"Botwoon head frame $B3:{installedPointer:X4} is editable");
            var nativeOam = new OamBuffer();
            var installedOam = new OamBuffer();
            DrawImportedEnemySpritemap(rom, nativeOam, new EnemySpritemapDraw { Bank = BotwoonVisualDefinitions.Bank, Pointer = native, Origin = new(128, 128), PaletteBits = 0, BaseTileIndex = 0 });
            installedOam.AddEnemySpritemap(installedParts, new Position16<OamPixels>(128, 128), 0, 0);
            AssertTrue(nativeOam.ImageEquals(installedOam) &&
                       nativeOam.NextByteOffset == installedOam.NextByteOffset,
                $"Botwoon head frame $B3:{installedPointer:X4} matches cartridge OAM");
        }
        AssertEqual(16, selected.Count,
            "Botwoon head selects sixteen distinct visible OAM frames");

        // Existing version-59 edits must survive the newly extracted head frames.
        byte[] current = EnemySpritemapFiles.Extract(rom);
        EnemySpritemapDocument document = JsonSerializer.Deserialize<EnemySpritemapDocument>(
            current, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        // Version 59 ends before Botwoon; removing only Botwoon from today's
        // catalog incorrectly retains every family added in subsequent versions.
        HashSet<string> previousNames = EnemySpritemapDefinitions.Frames
            .ToArray().Take(EnemySpritemapDefinitions.PreBotwoonFrameCount)
            .Select(frame => frame.Name).ToHashSet(StringComparer.Ordinal);
        var previousFrames = document.Frames
            .Where(pair => previousNames.Contains(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var previousBindings = document.DisplayFrames!
            .Where(pair => previousFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreBotwoonFrameCount,
            previousFrames.Count, "version-fifty-nine composition schema count");
        using var previousJson = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreBotwoon,
                Frames = previousFrames,
                DisplayFrames = previousBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemySpritemapCatalog upgraded = EnemySpritemapCatalog.Load(
            previousJson, stock.Spritemaps);
        foreach (EnemySpritemapDefinition frame in BotwoonVisualDefinitions.Frames())
            AssertTrue(upgraded.TryGetDisplay(frame.Bank, frame.Pointer, out _),
                $"version-fifty-nine override inherits {frame.Name}");
        Console.WriteLine(
            "  Botwoon visuals: 16 typed head frames draw the cartridge OAM, and legacy override migration passes.");
    }
}
