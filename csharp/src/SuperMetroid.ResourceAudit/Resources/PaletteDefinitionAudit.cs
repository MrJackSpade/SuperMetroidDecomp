using SuperMetroid.Core;
using SuperMetroid.Tooling;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;

using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;
namespace SuperMetroid.ResourceAudit.Resources;

/// <summary>
/// Checks bounded palette color identities against the real importer and loader.
/// A recording source supplies zero color values: no ROM or gameplay is involved.
/// The values are irrelevant; the contract under audit is which identities exist.
/// </summary>
internal static class PaletteDefinitionAudit
{
    /// <summary>
    /// Checks every colour the palette-FX catalogs' programs write against the exported identities: each heat-pulse
    /// program's frames and the title screen's two ambient owners' frames (#2602: the catalogs are named, not discovered by
    /// their names through reflection).
    /// </summary>
    public static void Run(string root, ResourceIndex exports, AuditReport report)
    {
        InstallColorIdentities(exports);
        int before = report.ReferenceCount;
        string heatSource = FindSource(root, nameof(PaletteFxHeatPulseColorDefinitions));
        foreach (PaletteFxHeatProgramDefinition heat in PaletteFxHeatPulseColorDefinitions.All)
        {
            for (int frame = 0; frame < heat.ColorFrames.FrameCount; frame++)
            for (int color = 0; color < heat.ColorFrames.ColorsPerFrame; color++)
                report.Require(ResourceDomains.PaletteFx, nameof(PaletteFxHeatPulseColorDefinitions),
                    ResourceIndex.Address(ResourceBanks.ProjectileOamAndPaletteFx, PaletteFxRecordSources.ColorAddress(heat.ColorFrames.Color(frame, color))), heatSource, exports);
        }
        string ambientSource = FindSource(root, nameof(TitleScreenAmbientPaletteFxColorDefinitions));
        foreach (TitleScreenAmbientPaletteFxColorDefinition ambient in TitleScreenAmbientPaletteOwners.All)
        {
            for (int frame = 0; frame < ambient.FrameCount; frame++)
            for (int color = 0; color < ambient.ColorsPerFrame; color++)
                report.Require(ResourceDomains.PaletteFx, nameof(TitleScreenAmbientPaletteFxColorDefinitions),
                    ResourceIndex.Address(ResourceBanks.ProjectileOamAndPaletteFx, PaletteFxRecordSources.ColorAddress(ambient.Color(frame, color))), ambientSource, exports);
        }
        report.Coverage.Add(new(ResourceDomains.PaletteFx, report.ReferenceCount - before,
            exports.Count(ResourceDomains.PaletteFx)));
    }

    /// <summary>Imports palette colors through a recording source and installs all loader-selected pointer identities.</summary>
    internal static void InstallColorIdentities(ResourceIndex exports)
    {
        SyntheticCartridge recorder = ColorAddressRecorder();
        byte[] roomDocument = RoomPaletteFxPresentationExtractor.Extract(recorder);
        RoomPaletteFxPresentation room = RoomPaletteFxPresentationFile.Read(new MemoryStream(roomDocument));
        byte[] titleDocument = TitlePaletteExtractor.Extract(recorder);
        TitlePaletteFile.Read(new MemoryStream(titleDocument));
        // Import once / install at multiple identities is legitimate. Use the
        // loaders' actual key sets, including aliases and the separate title provider.
        for (int index = 0; index < recorder.Reads.Count; index += 2)
        {
            if (index + 1 == recorder.Reads.Count || recorder.Reads[index + 1] != recorder.Reads[index] + 1)
                throw new InvalidDataException("Palette extraction no longer consists of bounded color-word reads.");
        }
        foreach (ushort pointer in room.ColorPointers.Concat(TitlePalettePresentationTooling.ColorPointers))
            exports.Add(ResourceDomains.PaletteFx, ResourceIndex.Address(ResourceBanks.ProjectileOamAndPaletteFx, pointer));
    }

    /// <summary>Locates a palette catalog's file, named for its type, in the import, Core or Tooling.</summary>
    private static string FindSource(string root, string name)
    {
        string[] found = new[] { "csharp/src/SuperMetroid.AssetExtraction", "csharp/src/SuperMetroid.Core", "csharp/src/SuperMetroid.Tooling/Core" }
            .SelectMany(directory => Directory.EnumerateFiles(Path.Combine(root, directory), name + ".cs", SearchOption.AllDirectories))
            .ToArray();
        return found.Length == 1
            ? Path.GetRelativePath(root, found[0]).Replace('\\', '/')
            : throw new FileNotFoundException($"Palette catalog {name} must be declared in exactly one import, Core or Tooling file; found {found.Length}.", name + ".cs");
    }

    /// <summary>Serves zero for every palette-FX bank or title initial-palette byte and records the read order.</summary>
    private static SyntheticCartridge ColorAddressRecorder()
    {
        var recorder = new SyntheticCartridge(
            _ => new InvalidDataException("Palette audit unexpectedly requested non-color-bank data."));
        recorder.DeclareZero(address => (SnesAddressMath.BankNumber(address)) == ResourceBanks.ProjectileOamAndPaletteFx);
        recorder.DeclareZero(TitleSourceDefinitions.Palette, CgramPalettes.ByteCount);
        return recorder;
    }
}
