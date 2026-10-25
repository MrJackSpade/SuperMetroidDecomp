using System.Reflection;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification.Samus;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>
/// Finite production-owner tests of the source-identified color/lifetime coupling in
/// #542. The input is an existing extracted installation, never a cartridge or replay.
/// </summary>
internal static class InstalledPowerBombIsolationTests
{
    /// <summary>JSON options: Camel-case property names.</summary>
    private static readonly JsonSerializerOptions CamelCaseJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>Runs five installed palette edits through Power Bomb and Crystal Flash, comparing mechanics, color windows, cleanup, and WARP/D3D pixels.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native OAM tables, native PPU geometry, native VRAM words")]
    internal static void Run(string installationRoot)
    {
        string stockDirectory = Path.Combine(Path.GetFullPath(installationRoot), "game", "maps");
        string path = Path.Combine(stockDirectory, PowerBombFixedColorFormat.FileName);
        byte[] original = File.ReadAllBytes(path);
        var options = CamelCaseJson;
        var document = JsonSerializer.Deserialize<PowerBombFixedColorDocument>(original, options)
            ?? throw new InvalidDataException($"Null color document: {path}");
        AreaMapPresentationCatalog stock = MapPresentationFiles.Load(stockDirectory, null);
        CheckCompiledLifetimeOperands(stock.PowerBombFixedColors);
        CheckStateRestoration(stock.PowerBombFixedColors);
        var framesToRender = new List<ContextFrame>();
        using var overridesScratch = new TestScratchDirectory("installed-power-bomb-isolation");
        string overrides = overridesScratch.Root;
        int comparisons = 0, changedColors = 0;
        try
        {
            Directory.CreateDirectory(overrides);
            foreach (string edit in new[] { "black", "white", "red", "blue", "gradient" })
            {
                // Build one complete fixed-color row for the selected black, white, primary, or gradient edit.
                PaletteRgb5[] Replace(PaletteRgb5[] source) => source.Select((_, index) => edit switch
                {
                    "black" => new PaletteRgb5 { Red = 0, Green = 0, Blue = 0 },
                    "white" => new PaletteRgb5 { Red = 31, Green = 31, Blue = 31 },
                    "red" => new PaletteRgb5 { Red = 31, Green = 0, Blue = 0 },
                    "blue" => new PaletteRgb5 { Red = 0, Green = 0, Blue = 31 },
                    _ => new PaletteRgb5 { Red = index % 32, Green = (index * 3) % 32, Blue = (index * 7) % 32 },
                }).ToArray();
                File.WriteAllBytes(Path.Combine(overrides, PowerBombFixedColorFormat.FileName),
                    PowerBombFixedColorFile.Write(document with
                    { PreExplosion = Replace(document.PreExplosion), Explosion = Replace(document.Explosion) }));
                AreaMapPresentationCatalog edited = MapPresentationFiles.Load(stockDirectory, overrides);
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
                        bool expected = baseline.StepFrame(), actual = replacement.StepFrame();
                        string context = $"{edit}, {(crystalFlash ? "Crystal Flash" : "Power Bomb")}, frame {frames}";
                        RequireSameMechanics(baseline, replacement, context);
                        Require(expected == actual, context + ": cleanup result differs");
                        if (replacement.Phase == PowerBombExplosionPhase.CrystalFlashAfterglow)
                            CheckEditedRestore(replacement, stock.PowerBombFixedColors, context);
                        var first = GameplayEffectCapture.CapturePowerBombColorMath(PowerBombRenderSnapshot.Capture(baseline), new Position16<RoomPixels>(0, 0));
                        var second = GameplayEffectCapture.CapturePowerBombColorMath(PowerBombRenderSnapshot.Capture(replacement), new Position16<RoomPixels>(0, 0));
                        RequireSameGeometry(first, second, context);
                        if (Colors(baseline) != Colors(replacement)) changedColors++;
                        if (second is not null)
                        {
                            var memoryImage = new FramePaint(new Bgr555[SnesPpuLayout.CgramColorCount], []);
                            var packet = new DisplayPacket(new(framesToRender.Count + 1, 1, 0),
                                new LayeredScene(memoryImage, new RenderLayer[] { second }, 15));
                            framesToRender.Add(new ContextFrame(packet, context));
                            // With a black backdrop this center pixel must be exactly the
                            // selected expanded RGB, not merely different from stock.
                            ColorAddWindow center = second.Windows[112];
                            if (center.Left <= 128 && center.Right >= 128)
                                Require(DisplayRendering.Render(packet)[112 * packet.Size.X + 128] ==
                                    new Rgba32(checked((byte)center.Red), checked((byte)center.Green), checked((byte)center.Blue)), context + ": edited RGB did not reach pixels");
                        }
                        comparisons++;
                    } while (baseline.IsActive);
                    if (crystalFlash)
                        Require(frames == 36, context: "Stock Crystal Flash must expand for 18 frames, fade for 17, then clean up.");
                    if (crystalFlash)
                    {
                        Require(!replacement.IsActive && !replacement.IsArmed,
                            edit + ": Crystal Flash retained a lock after cleanup");
                        Require(Colors(replacement) == new ExplosionColors(0, 0, 0), edit + ": inactive Crystal Flash retained display colors");
                    }
                    baseline.Reset(); replacement.Reset();
                    RequireSameMechanics(baseline, replacement, edit + ": reset retains simulation state");
                    Require(Colors(replacement) == new ExplosionColors(0, 0, 0), edit + ": reset retains display colors");
                    Console.WriteLine($"{edit}: {(crystalFlash ? "Crystal Flash" : "Power Bomb")} cleanup at frame {frames}.");
                }
            }
            Require(changedColors > 0, "The edits never reached live fixed colors.");
            Require(File.ReadAllBytes(path).SequenceEqual(original), "Stock colors were modified.");
            File.Delete(Path.Combine(overrides, PowerBombFixedColorFormat.FileName));
            Require(MapPresentationFiles.Load(stockDirectory, overrides).ContentIdentity == stock.ContentIdentity,
                "Removing the override did not restore stock content identity.");
            foreach (D3D11DeviceKind kind in Enum.GetValues<D3D11DeviceKind>())
            {
                using var device = new D3D11RenderDevice(kind);
                using var renderer = new D3D11FrameRenderer(device);
                foreach (var sample in framesToRender)
                {
                    var restored = sample.Frame;
                    PixelComparison.Verify(restored, DisplayRendering.Render(restored),
                        renderer.RenderForReadback(restored), $"{kind}: {sample.Context}");
                }
                Console.WriteLine($"{kind}: {framesToRender.Count} edited explosion frames match the WARP reference.");
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

    /// <summary>Starts a Crystal Flash or arms and spawns a Power Bomb with the supplied presentation catalog.</summary>
    internal static SamusPowerBombExplosionState Create(PowerBombFixedColorCatalog colors, bool crystalFlash)
    {
        var state = new SamusPowerBombExplosionState { PresentationColors = colors };
        if (crystalFlash) state.BeginCrystalFlash(new(128, 112));
        else { state.Arm(); state.Spawn(new(128, 112)); }
        return state;
    }

    /// <summary>Compares all serialized explosion fields except the three deliberately editable COLDATA outputs.</summary>
    internal static void RequireSameMechanics(SamusPowerBombExplosionState first,
        SamusPowerBombExplosionState second, string context)
    {
        // Include private phase timers and every future serialized field. Only the
        // colour the explosion adds and nonserialized host binding may differ.
        foreach (FieldInfo field in MetadataShape.Fields(typeof(SamusPowerBombExplosionState), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (field.IsDefined(typeof(NonSerializedAttribute)) || field.Name is
                "<FixedColor>k__BackingField") continue;
            Require(Equals(field.GetValue(first), field.GetValue(second)),
                $"{context}: {field.Name} differs: stock={field.GetValue(first)}, edited={field.GetValue(second)}");
        }
    }

    /// <summary>Checks color-add window lifetime and every scanline's horizontal bounds independently of its color.</summary>
    private static void RequireSameGeometry(ScanlineColorAddRenderLayer? first,
        ScanlineColorAddRenderLayer? second, string context)
    {
        Require((first is null) == (second is null), context + ": window lifetime differs");
        if (first is null || second is null) return;
        for (int row = 0; row < first.Windows.Length; row++)
            Require(first.Windows[row].Left == second.Windows[row].Left &&
                first.Windows[row].Right == second.Windows[row].Right, context + $": window row {row} differs");
    }

    /// <summary>Captures the three live fixed-color outputs without including simulation state.</summary>
    internal static ExplosionColors Colors(SamusPowerBombExplosionState state) =>
        new ExplosionColors(state.FixedColor.Red, state.FixedColor.Green, state.FixedColor.Blue);

    /// <summary>Fails the focused isolation check with its current edit/frame context.</summary>
    internal static void Require(bool condition, string context)
    {
        if (!condition) throw new InvalidOperationException(context);
    }

    /// <summary>Checks every reachable Crystal Flash radius derives its afterglow step count from the stock row's largest channel.</summary>
    private static void CheckCompiledLifetimeOperands(PowerBombFixedColorCatalog stock)
    {
        // The pinned $88:8D85 rows are (14,14,10), (15,15,9), (16,16,8),
        // (17,17,7). Check every reachable radius, not just the final row.
        for (ushort radius = 0; radius < PowerBombExplosionDefinitions.CrystalFlashRadiusLimit; radius++)
        {
            var native = stock.Resolve(PowerBombFixedColorSequence.Explosion, radius >> 11);
            Require(PowerBombExplosionDefinitions.CrystalFlashAfterglowSteps(radius) ==
                Math.Max(native.Red, Math.Max(native.Green, native.Blue)), $"Native fade operand differs at radius {radius}.");
        }
        try
        {
            _ = PowerBombExplosionDefinitions.CrystalFlashAfterglowSteps(
                PowerBombExplosionDefinitions.CrystalFlashRadiusLimit);
        }
        catch (ArgumentOutOfRangeException) { return; }
        throw new InvalidOperationException("The compiled Crystal Flash radius domain accepted its exclusive limit.");
    }

    /// <summary>Round-trips the debugger graph at every active frame and compares its remaining stock lifetime. Old layouts are
    /// rejected, not migrated (#1270).</summary>
    private static void CheckStateRestoration(PowerBombFixedColorCatalog colors)
    {
        int restores = 0;
        foreach (bool crystalFlash in new[] { false, true })
        {
            var state = Create(colors, crystalFlash);
            do
            {
                var current = Restore(state, out string currentWarning);
                Require(currentWarning.Length == 0, "A current graph unexpectedly needed a migration.");
                Require(current.PresentationColors is null, "A debugger graph retained a host catalog.");
                current.PresentationColors = colors;
                RequireSameMechanics(state, current, "Current graph restore");
                Require(Colors(state) == Colors(current), "Current graph restore changed display colors.");
                CompareRemainder(state, current, colors, "Current graph resume");
                restores++;
                _ = state.StepFrame();
            } while (state.IsActive);
        }
        Console.WriteLine($"Explosion state compatibility: {restores} checkpoints resume with identical stock state, colors and cleanup.");
    }

    /// <summary>Checks edited checkpoints rebind to the colours supplied after loading.</summary>
    private static void CheckEditedRestore(SamusPowerBombExplosionState edited,
        PowerBombFixedColorCatalog reboundColors, string context)
    {
        var restored = Restore(edited, out string warning);
        Require(warning.Length == 0, context + ": current edited capture warned");
        restored.PresentationColors = reboundColors;
        RequireSameMechanics(edited, restored, context + ": edited checkpoint restore");
        CompareRemainder(edited, restored, reboundColors, context + ": edited checkpoint rebind");

    }

    /// <summary>Steps a restored pair through cleanup, comparing mechanics and display colors on each remaining frame.</summary>
    private static void CompareRemainder(SamusPowerBombExplosionState state,
        SamusPowerBombExplosionState restored, PowerBombFixedColorCatalog colors, string context)
    {
        var baseline = Restore(state, out _);
        baseline.PresentationColors = colors;
        int frames = 0;
        do
        {
            Require(++frames < 300, context + ": cleanup did not occur");
            Require(baseline.StepFrame() == restored.StepFrame(), context + ": cleanup frame differs");
            RequireSameMechanics(baseline, restored, context);
            Require(Colors(baseline) == Colors(restored), context + ": restored display fade differs");
        } while (baseline.IsActive);
    }

    /// <summary>Serializes the graph, restores it through the production codec, and captures any warning.</summary>
    private static SamusPowerBombExplosionState Restore(SamusPowerBombExplosionState state, out string warning)
    {
        using var data = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(data, state);
        data.Position = 0;
        using var errors = new StringWriter();
        TextWriter previous = Console.Error;
        try
        {
            Console.SetError(errors);
            return DebuggerObjectGraphSerializer.Deserialize<SamusPowerBombExplosionState>(data);
        }
        finally { Console.SetError(previous); warning = errors.ToString(); }
    }

}
