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
    /// Installs the palette-FX colour identities the importer and loaders select, and records their coverage. No catalog
    /// declares colour pointers of its own any more: the colour layouts moved to the import (#1583), so every referenced
    /// colour is one the loaders select.
    /// </summary>
    public static void Run(ResourceIndex exports, AuditReport report)
    {
        InstallColorIdentities(exports);
        report.Coverage.Add(new(ResourceDomains.PaletteFx, 0, exports.Count(ResourceDomains.PaletteFx)));
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
