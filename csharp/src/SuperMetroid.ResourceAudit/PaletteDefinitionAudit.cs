using SuperMetroid.Tooling;
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
    /// <summary>Resolves bounded palette color pointers and records their coverage.</summary>
    /// <param name="root">Repository root used to locate each catalog's source file.</param>
    /// <param name="exports">Installed color identities recognized by the real importers.</param>
    /// <param name="report">Audit report receiving references, gaps, and coverage.</param>
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
            foreach (MethodInfo method in ToolingTypes.WithAdapter(catalog)
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                .Where(IsColorMethod).OrderBy(method => method.Name, StringComparer.Ordinal))
            {
                string prefix = method.Name[..^"ColorPointer".Length];
                covered |= CheckColorMethod(catalog, null, method, prefix, catalog, source, exports, report);
            }
            if (ToolingTypes.WithAdapter(catalog).Select(type => type.GetProperty("All", BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic))
                    .SingleOrDefault(property => property is not null)?.GetValue(null) is IEnumerable rows)
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

    /// <summary>Installs color-pointer identities produced by the room and title palette importers.</summary>
    /// <param name="exports">Index receiving the identities exposed by the loaded presentations.</param>
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
        foreach (ushort pointer in room.ColorPointers.Concat(TitlePalettePresentationTooling.ColorPointers))
            exports.Add(ResourceDomains.PaletteFx, ResourceIndex.Address(ResourceBanks.ProjectileOamAndPaletteFx, pointer));
    }

    /// <summary>Identifies catalog methods whose two integer arguments select a color pointer.</summary>
    /// <param name="method">Reflected method to classify.</param>
    /// <returns><see langword="true"/> when the method has the bounded color-pointer signature.</returns>
    private static bool IsColorMethod(MethodInfo method) =>
        method.Name.EndsWith("ColorPointer", StringComparison.Ordinal) && method.ReturnType == typeof(ushort) &&
        method.GetParameters() is [{ ParameterType: var first }, { ParameterType: var second }] &&
        first == typeof(int) && second == typeof(int);

    /// <summary>Enumerates a finite color-pointer method and records each referenced resource.</summary>
    /// <param name="rowType">Type that declares frame and color bounds.</param>
    /// <param name="row">Optional row instance on which the pointer method is invoked.</param>
    /// <param name="method">Pointer-producing method to inspect.</param>
    /// <param name="prefix">Catalog prefix for static bound names.</param>
    /// <param name="catalog">Catalog owning the audited declaration.</param>
    /// <param name="source">Repository-relative source label for findings.</param>
    /// <param name="exports">Installed resources used to resolve each pointer.</param>
    /// <param name="report">Audit report receiving references or a gap.</param>
    /// <returns><see langword="true"/> if finite bounds were found and inspected.</returns>
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

    /// <summary>Reads a declared integer bound from a reflected field or property.</summary>
    /// <param name="type">Type containing the bound.</param>
    /// <param name="row">Instance for an instance member, or <see langword="null"/> for a static member.</param>
    /// <param name="name">Bound member name.</param>
    /// <returns>The bound when represented as an integer; otherwise <see langword="null"/>.</returns>
    private static int? ReadCount(Type type, object? row, string name)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        object? value = type.GetField(name, flags)?.GetValue(row) ?? type.GetProperty(name, flags)?.GetValue(row);
        return value is int count ? count : null;
    }

    /// <summary>Finds the source file for a palette catalog across current and moved source layouts.</summary>
    /// <param name="root">Repository root.</param>
    /// <param name="name">Catalog type name used as the source file name.</param>
    /// <returns>A repository-relative path, or the expected Core path when no file is present.</returns>
    private static string FindSource(string root, string name)
    {
        string path = Path.Combine(root, "csharp/src/SuperMetroid.Core/Game", name + ".cs");
        if (File.Exists(path)) return Path.GetRelativePath(root, path).Replace('\\', '/');
        string moved = Path.Combine(root, "csharp/src/SuperMetroid.Tooling/Core/Game", name + ".cs");
        if (File.Exists(moved)) return Path.GetRelativePath(root, moved).Replace('\\', '/');
        return "csharp/src/SuperMetroid.Core/Game/" + name;
    }

    /// <summary>Records importer reads while supplying inert bytes only within the color address ranges.</summary>
    private sealed class ColorAddressRecorder : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>CPU addresses read by the room and title palette importers, in access order.</summary>
        public List<int> Reads { get; } = [];
        /// <summary>Allows only palette-bank bytes and the title's initial palette region to be read.</summary>
        /// <param name="cpuAddress">CPU address requested by an importer.</param>
        /// <returns>Zero, since values are irrelevant to the identity inventory.</returns>
        public byte ReadCartridgeByte(int cpuAddress)
        {
            bool titleInitialColor = cpuAddress >= TitleSequenceRomData.Assets.PaletteAddress &&
                cpuAddress < TitleSequenceRomData.Assets.PaletteAddress + SnesCgram.ByteCount;
            if ((cpuAddress >> 16) != ResourceBanks.ProjectileOamAndPaletteFx && !titleInitialColor)
                throw new InvalidDataException("Palette audit unexpectedly requested non-color-bank data.");
            Reads.Add(cpuAddress);
            return 0;
        }
        /// <summary>Rejects writes because palette identity extraction must not mutate emulated state.</summary>
        /// <param name="address">Requested emulated address.</param>
        /// <param name="value">Requested byte value.</param>
        public void WriteByte(int address, byte value) =>
            throw new InvalidOperationException("Static color extraction must not mutate emulated state.");
    }
}
