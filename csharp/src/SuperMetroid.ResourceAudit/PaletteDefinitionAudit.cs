using System.Collections;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.ResourceAudit;

/// <summary>
/// Checks bounded palette color identities against the real importer and loader.
/// A recording source supplies zero color values: no ROM or gameplay is involved.
/// The values are irrelevant; the contract under audit is which identities exist.
/// </summary>
internal static class PaletteDefinitionAudit
{
    public static void Run(string root, ResourceIndex exports, AuditReport report)
    {
        InstallColorIdentities(exports);
        int before = report.ReferenceCount;
        foreach (Type catalog in typeof(RoomPaletteFxSystem).Assembly.GetTypes()
            .Where(type => type.Name.Contains("PaletteFx", StringComparison.Ordinal) &&
                type.Name.EndsWith("ProgramMechanicsDefinitions", StringComparison.Ordinal))
            .OrderBy(type => type.FullName, StringComparer.Ordinal))
        {
            if (catalog == typeof(RoomPaletteFxProgramMechanicsDefinitions)) continue;
            // Its single compiled instruction is Delete: no presentation words.
            if (catalog == typeof(PaletteFxDeleteProgramMechanicsDefinitions)) continue;
            string source = FindSource(root, catalog.Name);
            bool covered = false;
            foreach (MethodInfo method in catalog.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(IsColorMethod).OrderBy(method => method.Name, StringComparer.Ordinal))
            {
                string prefix = method.Name[..^"ColorPointer".Length];
                covered |= CheckColorMethod(catalog, null, method, prefix, catalog, source, exports, report);
            }
            if (catalog.GetProperty("All", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) is IEnumerable rows)
            {
                foreach (object row in rows)
                {
                    foreach (MethodInfo method in row.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .Where(IsColorMethod))
                        covered |= CheckColorMethod(row.GetType(), row, method, string.Empty, catalog, source, exports, report);
                    if (row is PaletteFxHeatProgramDefinition heat)
                    {
                        foreach (PaletteFxHeatProgramFrameDefinition frame in heat.Frames)
                        for (int color = 0; color < PaletteFxHeatProgramDefinition.ColorsPerFrame; color++)
                            report.Require(ResourceDomains.PaletteFx, catalog.Name,
                                ResourceIndex.Address(ResourceBanks.ProjectileOamAndPaletteFx, frame.FirstColorPointer + color * sizeof(ushort)), source, exports);
                        covered = true;
                    }
                    if (row is TitleScreenAmbientPaletteFxProgramDefinition ambient)
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

    internal static void InstallColorIdentities(ResourceIndex exports)
    {
        var recorder = new ColorAddressRecorder();
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
        foreach (ushort pointer in room.ColorPointers.Concat(title.ColorPointers))
            exports.Add(ResourceDomains.PaletteFx, ResourceIndex.Address(ResourceBanks.ProjectileOamAndPaletteFx, pointer));
    }

    private static bool IsColorMethod(MethodInfo method) =>
        method.Name.EndsWith("ColorPointer", StringComparison.Ordinal) && method.ReturnType == typeof(ushort) &&
        method.GetParameters() is [{ ParameterType: var first }, { ParameterType: var second }] &&
        first == typeof(int) && second == typeof(int);

    private static bool CheckColorMethod(Type rowType, object? row, MethodInfo method, string prefix,
        Type catalog, string source, ResourceIndex exports, AuditReport report)
    {
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
            ushort pointer = (ushort)method.Invoke(row, [frame, color])!;
            report.Require(ResourceDomains.PaletteFx, catalog.Name + "." + method.Name,
                ResourceIndex.Address(ResourceBanks.ProjectileOamAndPaletteFx, pointer), source, exports);
        }
        return true;
    }

    private static int? ReadCount(Type type, object? row, string name)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        object? value = type.GetField(name, flags)?.GetValue(row) ?? type.GetProperty(name, flags)?.GetValue(row);
        return value is int count ? count : null;
    }

    private static string FindSource(string root, string name)
    {
        string directory = Path.Combine(root, "csharp/src/SuperMetroid.Core/Game");
        string path = Path.Combine(directory, name + ".cs");
        if (File.Exists(path)) return Path.GetRelativePath(root, path).Replace('\\', '/');
        return "csharp/src/SuperMetroid.Core/Game/" + name;
    }

    private sealed class ColorAddressRecorder : ISnesAddressSpace, IImportCartridgeSource
    {
        public List<int> Reads { get; } = [];
        public byte ReadCartridgeByte(int cpuAddress)
        {
            bool titleInitialColor = cpuAddress >= TitleSequenceRomData.Assets.PaletteAddress &&
                cpuAddress < TitleSequenceRomData.Assets.PaletteAddress + SnesCgram.ByteCount;
            if ((cpuAddress >> 16) != ResourceBanks.ProjectileOamAndPaletteFx && !titleInitialColor)
                throw new InvalidDataException("Palette audit unexpectedly requested non-color-bank data.");
            Reads.Add(cpuAddress);
            return 0;
        }
        public void WriteByte(int address, byte value) =>
            throw new InvalidOperationException("Static color extraction must not mutate emulated state.");
    }
}
