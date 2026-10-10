using System.Reflection;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

/// <summary>
/// Finite production-owner tests of the source-identified color/lifetime coupling in
/// #542. The input is an existing extracted installation, never a cartridge or replay.
/// </summary>
internal static partial class InstalledPowerBombIsolationTests
{
    internal static void Run(string installationRoot)
    {
        string stockDirectory = Path.Combine(Path.GetFullPath(installationRoot), "game", "maps");
        string path = Path.Combine(stockDirectory, PowerBombFixedColorFormat.FileName);
        byte[] original = File.ReadAllBytes(path);
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var document = JsonSerializer.Deserialize<PowerBombFixedColorDocument>(original, options)
            ?? throw new InvalidDataException($"Null color document: {path}");
        AreaMapPresentationCatalog stock = AreaMapPresentationCatalog.Load(stockDirectory, null);
        CheckCompiledLifetimeOperands(stock.PowerBombFixedColors);
        CheckStateRestoration(stock.PowerBombFixedColors);
        var framesToRender = new List<(RenderFrameSnapshot Frame, string Context)>();
        string overrides = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "installed-power-bomb-isolation-" + Guid.NewGuid().ToString("N")));
        int comparisons = 0, changedColors = 0;
        try
        {
            Directory.CreateDirectory(overrides);
            foreach (string edit in new[] { "black", "white", "red", "blue", "gradient" })
            {
                PaletteRgb5[] Replace(PaletteRgb5[] source) => source.Select((_, index) => edit switch
                {
                    "black" => new PaletteRgb5 { Red = 0, Green = 0, Blue = 0 },
                    "white" => new PaletteRgb5 { Red = 31, Green = 31, Blue = 31 },
                    "red" => new PaletteRgb5 { Red = 31, Green = 0, Blue = 0 },
                    "blue" => new PaletteRgb5 { Red = 0, Green = 0, Blue = 31 },
                    _ => new PaletteRgb5 { Red = index % 32, Green = (index * 3) % 32, Blue = (index * 7) % 32 },
                }).ToArray();
                File.WriteAllBytes(Path.Combine(overrides, PowerBombFixedColorFormat.FileName),
                    PowerBombFixedColorCatalog.Write(document with
                    { PreExplosion = Replace(document.PreExplosion), Explosion = Replace(document.Explosion) }));
                AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stockDirectory, overrides);
                Require(stock.ContentIdentity != edited.ContentIdentity, edit + ": override identity did not change");
                foreach (bool crystalFlash in new[] { false, true })
                {
                    var baseline = Create(stock.PowerBombFixedColors, crystalFlash);
                    var replacement = Create(edited.PowerBombFixedColors, crystalFlash);
                    var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
                    int frames = 0;
                    do
                    {
                        frames++;
                        Require(frames < 300, "The bounded stock explosion failed to clean up.");
                        bool expected = baseline.StepFrame(memory), actual = replacement.StepFrame(memory);
                        string context = $"{edit}, {(crystalFlash ? "Crystal Flash" : "Power Bomb")}, frame {frames}";
                        RequireSameMechanics(baseline, replacement, context);
                        Require(expected == actual, context + ": cleanup result differs");
                        if (replacement.Phase == PowerBombExplosionPhase.CrystalFlashAfterglow)
                            CheckEditedRestore(replacement, stock.PowerBombFixedColors, context);
                        var first = SnesGameplayFrameRenderer.CapturePowerBombColorMath(memory, baseline, 0, 0);
                        var second = SnesGameplayFrameRenderer.CapturePowerBombColorMath(memory, replacement, 0, 0);
                        RequireSameGeometry(first, second, context);
                        if (Colors(baseline) != Colors(replacement)) changedColors++;
                        if (second is not null)
                        {
                            var memoryImage = new PpuMemorySnapshot(new byte[SnesPpuLayout.VramByteCount],
                                new Bgr555[SnesPpuLayout.CgramColorCount], new byte[SnesPpuLayout.OamUploadByteCount], 0);
                            var packet = new RenderFrameSnapshot(new(framesToRender.Count + 1, 1, 0),
                                new LayeredRenderSnapshot(memoryImage, new RenderLayer[] { second }, 0, 15));
                            framesToRender.Add((packet, context));
                            // With a black backdrop this center pixel must be exactly the
                            // selected expanded RGB, not merely different from stock.
                            ColorAddWindow center = second.Windows[112];
                            if (center.Left <= 128 && center.Right >= 128)
                                Require(SoftwareFrameSnapshotRenderer.Render(packet)[112 * packet.Width + 128] ==
                                    new Rgba32(center.Red, center.Green, center.Blue), context + ": edited RGB did not reach pixels");
                        }
                        comparisons++;
                    } while (baseline.IsActive);
                    if (crystalFlash)
                        Require(frames == 36, context: "Stock Crystal Flash must expand for 18 frames, fade for 17, then clean up.");
                    if (crystalFlash)
                    {
                        Require(!replacement.IsActive && !replacement.IsArmed,
                            edit + ": Crystal Flash retained a lock after cleanup");
                        Require(Colors(replacement) == (0, 0, 0), edit + ": inactive Crystal Flash retained display colors");
                    }
                    baseline.Reset(); replacement.Reset();
                    RequireSameMechanics(baseline, replacement, edit + ": reset retains simulation state");
                    Require(Colors(replacement) == (0, 0, 0), edit + ": reset retains display colors");
                    Console.WriteLine($"{edit}: {(crystalFlash ? "Crystal Flash" : "Power Bomb")} cleanup at frame {frames}.");
                }
            }
            Require(changedColors > 0, "The edits never reached live fixed colors.");
            Require(File.ReadAllBytes(path).AsSpan().SequenceEqual(original), "Stock colors were modified.");
            File.Delete(Path.Combine(overrides, PowerBombFixedColorFormat.FileName));
            Require(AreaMapPresentationCatalog.Load(stockDirectory, overrides).ContentIdentity == stock.ContentIdentity,
                "Removing the override did not restore stock content identity.");
            foreach (D3D11DeviceKind kind in Enum.GetValues<D3D11DeviceKind>())
            {
                using var device = new D3D11RenderDevice(kind);
                using var renderer = new D3D11FrameRenderer(device);
                foreach (var sample in framesToRender)
                {
                    var restored = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotCodec.Serialize(sample.Frame));
                    PixelComparison.Verify(restored, SoftwareFrameSnapshotRenderer.Render(restored),
                        renderer.RenderForReadback(restored), $"{kind}: {sample.Context}");
                }
                Console.WriteLine($"{kind}: {framesToRender.Count} edited explosion frames match software after packet round trip.");
            }
            Console.WriteLine($"Power Bomb/Crystal Flash isolation: {comparisons} full mutable-state/window comparisons, " +
                $"{changedColors} changed color observations; disk selection/removal verified. No ROM or player data used.");
        }
        finally
        {
            string owner = Path.GetFullPath(Path.Combine("csharp", "test-temp")) + Path.DirectorySeparatorChar;
            if (!overrides.StartsWith(owner, StringComparison.OrdinalIgnoreCase) ||
                !Path.GetFileName(overrides).StartsWith("installed-power-bomb-isolation-", StringComparison.Ordinal))
                throw new InvalidOperationException("Refusing to remove a non-fixture override directory.");
            if (Directory.Exists(overrides)) Directory.Delete(overrides, true);
        }
    }

    internal static SamusPowerBombExplosionState Create(PowerBombFixedColorCatalog colors, bool crystalFlash)
    {
        var state = new SamusPowerBombExplosionState { PresentationColors = colors };
        if (crystalFlash) state.BeginCrystalFlash(128, 112);
        else { state.Arm(); state.Spawn(128, 112); }
        return state;
    }

    internal static void RequireSameMechanics(SamusPowerBombExplosionState first,
        SamusPowerBombExplosionState second, string context)
    {
        // Include private phase timers and every future serialized field. Only the
        // three COLDATA presentation outputs and nonserialized host binding may differ.
        foreach (FieldInfo field in typeof(SamusPowerBombExplosionState).GetFields(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (field.IsDefined(typeof(NonSerializedAttribute)) || field.Name is
                "<FixedColorRed>k__BackingField" or "<FixedColorGreen>k__BackingField" or
                "<FixedColorBlue>k__BackingField") continue;
            Require(Equals(field.GetValue(first), field.GetValue(second)),
                $"{context}: {field.Name} differs: stock={field.GetValue(first)}, edited={field.GetValue(second)}");
        }
    }

    private static void RequireSameGeometry(ScanlineColorAddRenderLayer? first,
        ScanlineColorAddRenderLayer? second, string context)
    {
        Require((first is null) == (second is null), context + ": window lifetime differs");
        if (first is null || second is null) return;
        for (int row = 0; row < first.Windows.Length; row++)
            Require(first.Windows[row].Left == second.Windows[row].Left &&
                first.Windows[row].Right == second.Windows[row].Right, context + $": window row {row} differs");
    }

    internal static (byte Red, byte Green, byte Blue) Colors(SamusPowerBombExplosionState state) =>
        (state.FixedColorRed, state.FixedColorGreen, state.FixedColorBlue);

    internal static void Require(bool condition, string context)
    {
        if (!condition) throw new InvalidOperationException(context);
    }
}
