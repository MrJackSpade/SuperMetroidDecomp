using SuperMetroid.Core;
using SuperMetroid.Tooling;
using System.Collections;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.ResourceAudit.Resources;

/// <summary>
/// Checks bounded palette color identities against the real importer and loader.
/// A recording source supplies zero color values: no ROM or gameplay is involved.
/// The values are irrelevant; the contract under audit is which identities exist.
/// </summary>
internal static class PaletteDefinitionAudit
{
    /// <summary>Checks every recognized palette-FX color pointer against exported identities and records coverage gaps.</summary>
    [ReflectionAudit("coverage audit: catalogs are discovered by their color-pointer method and row-table naming conventions")]
    public static void Run(string root, ResourceIndex exports, AuditReport report)
    {
        InstallColorIdentities(exports);
        int before = report.ReferenceCount;
        foreach (Type catalog in typeof(RoomPaletteFxSystem).Assembly.GetTypes()
            .Where(type => type.Name.Contains("PaletteFx", StringComparison.Ordinal) &&
                type.Name.EndsWith("ProgramMechanicsDefinitions", StringComparison.Ordinal))
            .OrderBy(type => type.FullName, StringComparer.Ordinal))
        {
            string source = FindSource(root, catalog.Name);
            var audit = new PaletteCatalogAudit(catalog, source, exports, report);
            bool covered = false;
            foreach (MethodInfo method in ToolingTypes.WithAdapter(catalog)
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                .Where(IsColorMethod).OrderBy(method => method.Name, StringComparer.Ordinal))
            {
                string prefix = method.Name[..^"ColorPointer".Length];
                covered |= CheckColorMethod(catalog, null, method, prefix, audit);
            }
            if (ToolingTypes.WithAdapter(catalog).Select(type => type.GetProperty("All", BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic))
                    .SingleOrDefault(property => property is not null)?.GetValue(null) is IEnumerable rows)
            {
                foreach (object row in rows)
                {
                    foreach (MethodInfo method in row.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .Where(IsColorMethod))
                        covered |= CheckColorMethod(row.GetType(), row, method, string.Empty, audit);
                    if (row is PaletteFxHeatProgramDefinition heat)
                    {
                        foreach (PaletteFxHeatProgramFrameDefinition frame in heat.Frames)
                        for (int color = 0; color < PaletteFxHeatProgramDefinition.ColorsPerFrame; color++)
                            report.Require(ResourceDomains.PaletteFx, catalog.Name,
                                ResourceIndex.Address(ResourceBanks.ProjectileOamAndPaletteFx, frame.FirstColorPointer + color * sizeof(ushort)), source, exports);
                        covered = true;
                    }
                    if (row is TitleScreenAmbientPaletteFxColorDefinition ambient)
                    {
                        for (int frame = 0; frame < ambient.FrameCount; frame++)
                        for (int color = 0; color < ambient.ColorsPerFrame; color++)
                            report.Require(ResourceDomains.PaletteFx, catalog.Name,
                                ResourceIndex.Address(ResourceBanks.ProjectileOamAndPaletteFx, ambient.FramePointer(frame) +
                                    sizeof(ushort) + color * sizeof(ushort)), source, exports);
                        covered = true;
                    }
                }
            }
            if (!covered)
                report.Gap(ResourceDomains.PaletteFx, catalog.Name, source,
                    "No bounded color-pointer declaration recognized; this program is not certified by the palette adapter.");
        }
        report.Coverage.Add(new(ResourceDomains.PaletteFx, report.ReferenceCount - before,
            exports.Count(ResourceDomains.PaletteFx)));
    }

    /// <summary>Imports palette colors through a recording source and installs all loader-selected pointer identities.</summary>
    internal static void InstallColorIdentities(ResourceIndex exports)
    {
        SyntheticCartridge recorder = ColorAddressRecorder();
        byte[] roomDocument = RoomPaletteFxPresentationExtractor.Extract(recorder);
        RoomPaletteFxPresentation room = RoomPaletteFxPresentation.Load(new MemoryStream(roomDocument));
        byte[] titleDocument = TitlePaletteExtractor.Extract(recorder);
        TitlePalettePresentation title = TitlePalettePresentation.Load(new MemoryStream(titleDocument));
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

    /// <summary>Recognizes two-index color-pointer methods returning a bank-local word address.</summary>
    private static bool IsColorMethod(MethodInfo method) =>
        method.Name.EndsWith("ColorPointer", StringComparison.Ordinal) && method.ReturnType == typeof(ushort) &&
        method.GetParameters() is [{ ParameterType: var first }, { ParameterType: var second }] &&
        first == typeof(int) && second == typeof(int);

    /// <summary>Enumerates finite frame and color bounds for one reflected palette catalog method.</summary>
    private static bool CheckColorMethod(Type rowType, object? row, MethodInfo method, string prefix,
        PaletteCatalogAudit audit)
    {
        Type catalog = audit.Catalog;
        string source = audit.Source;
        ResourceIndex exports = audit.Exports;
        AuditReport report = audit.Report;
        int? frames = ReadCount(rowType, row, "FrameCount") ?? ReadCount(catalog, null, prefix + "FrameCount");
        int? colors = ReadCount(rowType, row, "ColorsPerFrame") ?? ReadCount(catalog, null, prefix + "ColorsPerFrame");
        if (frames is null && rowType.GetProperty("Frames")?.GetValue(row) is IEnumerable frameRows)
            frames = frameRows.Cast<object>().Count();
        if (frames is null or <= 0 || colors is null or <= 0)
        {
            report.Gap(ResourceDomains.PaletteFx, catalog.Name + "." + method.Name, source,
                "Color-pointer function has no recognized finite frame/color bounds.");
            return false;
        }
        for (int frame = 0; frame < frames; frame++)
        for (int color = 0; color < colors; color++)
        {
            ushort pointer = (ushort)(method.Invoke(row, [frame, color]) ?? throw new InvalidOperationException("Verification requires method.Invoke(row, [frame, color])."));
            report.Require(ResourceDomains.PaletteFx, catalog.Name + "." + method.Name,
                ResourceIndex.Address(ResourceBanks.ProjectileOamAndPaletteFx, pointer), source, exports);
        }
        return true;
    }

    /// <summary>Finds a reflected frame or color count on the row or its catalog type.</summary>
    [ReflectionAudit("coverage audit: a discovered row declares its count under a conventional member name")]
    private static int? ReadCount(Type type, object? row, string name)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        object? value = type.GetField(name, flags)?.GetValue(row) ?? type.GetProperty(name, flags)?.GetValue(row);
        return value is int count ? count : null;
    }

    /// <summary>Locates a palette catalog's file, named for its type, in its Core or Tooling domain directory.</summary>
    private static string FindSource(string root, string name)
    {
        string[] found = new[] { "csharp/src/SuperMetroid.Core", "csharp/src/SuperMetroid.Tooling/Core" }
            .SelectMany(directory => Directory.EnumerateFiles(Path.Combine(root, directory), name + ".cs", SearchOption.AllDirectories))
            .ToArray();
        return found.Length == 1
            ? Path.GetRelativePath(root, found[0]).Replace('\\', '/')
            : throw new FileNotFoundException($"Palette catalog {name} must be declared in exactly one Core or Tooling file; found {found.Length}.", name + ".cs");
    }

    /// <summary>Serves zero for every palette-FX bank or title initial-palette byte and records the read order.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: records which palette-FX bank and title palette bytes the color importers read")]
    private static SyntheticCartridge ColorAddressRecorder()
    {
        var recorder = new SyntheticCartridge(
            _ => new InvalidDataException("Palette audit unexpectedly requested non-color-bank data."),
            _ => new InvalidOperationException("Static color extraction must not mutate emulated state."));
        recorder.DeclareZero(address => (SnesAddressMath.BankNumber(address)) == ResourceBanks.ProjectileOamAndPaletteFx);
        recorder.DeclareZero(TitleSequenceRomData.Assets.PaletteAddress, CgramPalettes.ByteCount);
        return recorder;
    }

    /// <summary>One palette-FX catalog under audit, with where its findings and exported identities go (#633).</summary>
    private readonly record struct PaletteCatalogAudit(Type Catalog, string Source, ResourceIndex Exports, AuditReport Report);
}
