using System.Reflection;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using System.Text;
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

/// <summary>
/// Finite production-owner tests of the source-identified color/lifetime coupling in
/// #542. The input is an existing extracted installation, never a cartridge or replay.
/// </summary>
internal static class InstalledPowerBombIsolationTests
{
    /// <summary>JSON options: Camel-case property names.</summary>
    private static readonly JsonSerializerOptions CamelCaseJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>Runs five installed palette edits through Power Bomb and Crystal Flash, comparing mechanics, color windows, cleanup, and software/D3D pixels.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native OAM tables, native PPU geometry, native VRAM words")]
    internal static void Run(string installationRoot)
    {
        string stockDirectory = Path.Combine(Path.GetFullPath(installationRoot), "game", "maps");
        string path = Path.Combine(stockDirectory, PowerBombFixedColorFormat.FileName);
        byte[] original = File.ReadAllBytes(path);
        var options = CamelCaseJson;
        var document = JsonSerializer.Deserialize<PowerBombFixedColorDocument>(original, options)
            ?? throw new InvalidDataException($"Null color document: {path}");
        AreaMapPresentationCatalog stock = AreaMapPresentationCatalog.Load(stockDirectory, null);
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
                        bool expected = baseline.StepFrame(), actual = replacement.StepFrame();
                        string context = $"{edit}, {(crystalFlash ? "Crystal Flash" : "Power Bomb")}, frame {frames}";
                        RequireSameMechanics(baseline, replacement, context);
                        Require(expected == actual, context + ": cleanup result differs");
                        if (replacement.Phase == PowerBombExplosionPhase.CrystalFlashAfterglow)
                            CheckEditedRestore(replacement, stock.PowerBombFixedColors, context);
                        var first = SnesGameplayFrameRenderer.CapturePowerBombColorMath(PowerBombRenderSnapshot.Capture(baseline), new Position16<RoomPixels>(0, 0));
                        var second = SnesGameplayFrameRenderer.CapturePowerBombColorMath(PowerBombRenderSnapshot.Capture(replacement), new Position16<RoomPixels>(0, 0));
                        RequireSameGeometry(first, second, context);
                        if (Colors(baseline) != Colors(replacement)) changedColors++;
                        if (second is not null)
                        {
                            var memoryImage = new FramePaint(new Bgr555[SnesPpuLayout.CgramColorCount], []);
                            var packet = new RenderFrameSnapshot(new(framesToRender.Count + 1, 1, 0),
                                new LayeredRenderSnapshot(memoryImage, new RenderLayer[] { second }, 15));
                            framesToRender.Add(new ContextFrame(packet, context));
                            // With a black backdrop this center pixel must be exactly the
                            // selected expanded RGB, not merely different from stock.
                            ColorAddWindow center = second.Windows[112];
                            if (center.Left <= 128 && center.Right >= 128)
                                Require(SoftwareFrameSnapshotRenderer.Render(packet)[112 * packet.Size.X + 128] ==
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
            Require(AreaMapPresentationCatalog.Load(stockDirectory, overrides).ContentIdentity == stock.ContentIdentity,
                "Removing the override did not restore stock content identity.");
            foreach (D3D11DeviceKind kind in Enum.GetValues<D3D11DeviceKind>())
            {
                using var device = new D3D11RenderDevice(kind);
                using var renderer = new D3D11FrameRenderer(device);
                foreach (var sample in framesToRender)
                {
                    var restored = sample.Frame;
                    PixelComparison.Verify(restored, SoftwareFrameSnapshotRenderer.Render(restored),
                        renderer.RenderForReadback(restored), $"{kind}: {sample.Context}");
                }
                Console.WriteLine($"{kind}: {framesToRender.Count} edited explosion frames match software.");
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
        // three COLDATA presentation outputs and nonserialized host binding may differ.
        foreach (FieldInfo field in MetadataShape.Fields(typeof(SamusPowerBombExplosionState), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (field.IsDefined(typeof(NonSerializedAttribute)) || field.Name is
                "<FixedColorRed>k__BackingField" or "<FixedColorGreen>k__BackingField" or
                "<FixedColorBlue>k__BackingField") continue;
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
        new ExplosionColors(state.FixedColorRed, state.FixedColorGreen, state.FixedColorBlue);

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
        for (ushort radius = 0; radius < SamusSpecialSequenceRomData.PowerBomb.CrystalFlashRadiusLimit; radius++)
        {
            var native = stock.Resolve(PowerBombFixedColorSequence.Explosion, radius >> 11);
            Require(SamusSpecialSequenceRomData.PowerBomb.CrystalFlashAfterglowSteps(radius) ==
                Math.Max(native.Red, Math.Max(native.Green, native.Blue)), $"Native fade operand differs at radius {radius}.");
        }
        try
        {
            _ = SamusSpecialSequenceRomData.PowerBomb.CrystalFlashAfterglowSteps(
                SamusSpecialSequenceRomData.PowerBomb.CrystalFlashRadiusLimit);
        }
        catch (ArgumentOutOfRangeException) { return; }
        throw new InvalidOperationException("The compiled Crystal Flash radius domain accepted its exclusive limit.");
    }

    /// <summary>Round-trips current and prior debugger graphs at every active frame and compares their remaining stock lifetime.</summary>
    private static void CheckStateRestoration(PowerBombFixedColorCatalog colors)
    {
        int currentRestores = 0, legacyRestores = 0;
        foreach (bool crystalFlash in new[] { false, true })
        {
            var state = Create(colors, crystalFlash);
            do
            {
                var current = Restore(state, legacy: false, out string currentWarning);
                Require(currentWarning.Length == 0, "A current graph unexpectedly needed a migration.");
                Require(current.PresentationColors is null, "A debugger graph retained a host catalog.");
                current.PresentationColors = colors;
                RequireSameMechanics(state, current, "Current graph restore");
                Require(Colors(state) == Colors(current), "Current graph restore changed display colors.");
                CompareRemainder(state, current, colors, "Current graph resume");
                currentRestores++;

                var legacy = Restore(state, legacy: true, out string legacyWarning);
                Require(legacyWarning.Contains("Legacy Crystal Flash state", StringComparison.Ordinal),
                    "A legacy graph migration did not warn.");
                legacy.PresentationColors = colors;
                RequireSameMechanics(state, legacy, "Legacy stock graph restore");
                CompareRemainder(state, legacy, colors, "Legacy stock graph resume");
                legacyRestores++;
                _ = state.StepFrame();
            } while (state.IsActive);
        }
        Console.WriteLine($"Explosion state compatibility: {currentRestores} current and {legacyRestores} old-layout " +
            "checkpoints resume with identical stock state, colors and cleanup; old layouts warn explicitly.");
    }

    /// <summary>Checks edited current checkpoints rebind to stock while legacy modded captures retain their historical fade remainder.</summary>
    private static void CheckEditedRestore(SamusPowerBombExplosionState edited,
        PowerBombFixedColorCatalog reboundColors, string context)
    {
        var restored = Restore(edited, legacy: false, out string warning);
        Require(warning.Length == 0, context + ": current edited capture warned");
        restored.PresentationColors = reboundColors;
        RequireSameMechanics(edited, restored, context + ": edited checkpoint restore");
        CompareRemainder(edited, restored, reboundColors, context + ": edited checkpoint rebind");

        // A pre-separation modded capture genuinely lacks the stock control timeline.
        // Migration retains its historical fade remainder once, warns, and must not
        // recalculate it from whatever replacement catalog is rebound after loading.
        var historical = Restore(edited, legacy: true, out string historicalWarning);
        Require(historicalWarning.Contains("historical timing", StringComparison.Ordinal),
            context + ": modded legacy timing loss was not explained");
        historical.PresentationColors = reboundColors;
        var color = Colors(edited);
        int remaining = Math.Max(color.Red, Math.Max(color.Green, color.Blue)) + 1;
        for (int frame = 1; frame <= remaining; frame++)
            Require(historical.StepFrame() == (frame == remaining),
                context + ": modded legacy capture did not preserve its historical cleanup frame");
        Require(!historical.IsActive, context + ": modded legacy capture did not release HDMA");
    }

    /// <summary>Steps a restored pair through cleanup, comparing mechanics and display colors on each remaining frame.</summary>
    private static void CompareRemainder(SamusPowerBombExplosionState state,
        SamusPowerBombExplosionState restored, PowerBombFixedColorCatalog colors, string context)
    {
        var baseline = Restore(state, legacy: false, out _);
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

    /// <summary>Serializes a current or legacy graph, restores it through the production codec, and captures migration warnings.</summary>
    private static SamusPowerBombExplosionState Restore(SamusPowerBombExplosionState state,
        bool legacy, out string warning)
    {
        using var data = new MemoryStream();
        if (legacy) WriteLegacyGraph(data, state);
        else DebuggerObjectGraphSerializer.Serialize(data, state);
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

    /// <summary>Writes the prior field envelope without the new afterglow-steps field using production primitive serialization.</summary>
    private static void WriteLegacyGraph(Stream destination, SamusPowerBombExplosionState state)
    {
        // Construct the exact prior field envelope while using the production codec
        // for primitive values. No fixture changes the production serializer's API.
        const byte newObjectMarker = 2, fieldPayloadKind = 5;
        var fields = MetadataShape.Fields(typeof(SamusPowerBombExplosionState), BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic).Where(field =>
            !field.IsDefined(typeof(NonSerializedAttribute)) &&
            field.Name != "_crystalFlashAfterglowStepsRemaining").ToArray();
        using var writer = new BinaryWriter(destination, Encoding.UTF8, leaveOpen: true);
        writer.Write(newObjectMarker);
        writer.Write(1); // Root reference identity; remaining fields are primitive values.
        writer.Write(DebuggerStateTypeIdentity.GetSerializedName(typeof(SamusPowerBombExplosionState)));
        writer.Write(fieldPayloadKind);
        writer.Write(fields.Length);
        foreach (FieldInfo field in fields)
        {
            writer.Write(DebuggerStateTypeIdentity.GetSerializedName((field.DeclaringType ?? throw new InvalidOperationException("Verification requires field.DeclaringType."))));
            writer.Write(field.Name);
            writer.Flush();
            DebuggerObjectGraphSerializer.Serialize(destination, (field.GetValue(state) ?? throw new InvalidOperationException("Verification requires field.GetValue(state).")));
        }
    }
}
