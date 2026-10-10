using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyInstalledBotwoonVisuals(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        var selected = new HashSet<ushort>();
        for (int index = 0;
             index < BotwoonInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = BotwoonInstructionProgramDefinitions
                .PresentationWordAddress(index);
            int nativeAddress = (BotwoonVisualDefinitions.Bank << 16) | operand;
            ushort native = unchecked((ushort)(rom.ReadCartridgeByte(nativeAddress) |
                rom.ReadCartridgeByte(nativeAddress + 1) << 8));
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    EnemyDefinitionId.Botwoon, operand,
                    out ushort installedPointer),
                $"Botwoon head operand $B3:{operand:X4} is compiled");
            AssertEqual(native, installedPointer,
                $"Botwoon head selector $B3:{operand:X4} matches the cartridge");
            if (installedPointer == CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap)
                continue;
            selected.Add(installedPointer);
            AssertTrue(stock.Spritemaps!.TryGetDisplay(
                    BotwoonVisualDefinitions.Bank, installedPointer,
                    out EnemySpritemapParts installedParts),
                $"Botwoon head frame $B3:{installedPointer:X4} is editable");
            var nativeOam = new OamBuffer();
            var installedOam = new OamBuffer();
            DrawImportedEnemySpritemap(rom, nativeOam, BotwoonVisualDefinitions.Bank,
                native, 128, 128, 0, 0);
            installedOam.AddEnemySpritemap(installedParts, 128, 128, 0, 0);
            AssertTrue(nativeOam.LowTable.SequenceEqual(installedOam.LowTable) &&
                       nativeOam.HighTable.SequenceEqual(installedOam.HighTable) &&
                       nativeOam.NextByteOffset == installedOam.NextByteOffset,
                $"Botwoon head frame $B3:{installedPointer:X4} matches cartridge OAM");
        }
        AssertEqual(BotwoonVisualDefinitions.FrameCount, selected.Count,
            "Botwoon head selects sixteen distinct visible OAM frames");
        AssertThrows<InvalidDataException>(
            () => BotwoonVisualDefinitions.FrameAt(
                BotwoonInstructionProgramDefinitions.MovingUpLeft),
            "Botwoon visual catalog rejects its neighboring instruction control word");

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
                Version = EnemySpritemapDefinitions.PreBotwoonVersion,
                Frames = previousFrames,
                DisplayFrames = previousBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemySpritemapCatalog upgraded = EnemySpritemapCatalog.Load(
            previousJson, stock.Spritemaps);
        foreach (EnemySpritemapDefinition frame in BotwoonVisualDefinitions.Frames())
            AssertTrue(upgraded.TryGetDisplay(frame.Bank, frame.Pointer, out _),
                $"version-fifty-nine override inherits {frame.Name}");
        Console.WriteLine(
            "  Botwoon visuals: 25 cartridge-matched selectors, 16 OAM frames, and legacy override migration pass.");
    }
}
