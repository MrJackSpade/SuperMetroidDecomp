using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.RenderVerification.Samus;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>
/// Finite production-owner tests of the source-identified color/lifetime coupling in
/// #542. The input is an existing extracted installation, never a cartridge or replay.
/// </summary>
internal static class InstalledPowerBombIsolationTests
{
    /// <summary>Runs the installed Power Bomb and Crystal Flash colours through their lifetimes, checking cleanup, state restoration, and WARP/D3D pixels.</summary>
    internal static void Run(string installationRoot)
    {
        string stockDirectory = Path.Combine(Path.GetFullPath(installationRoot), "game", "maps");
        AreaMapPresentation stock = MapPresentationFiles.Load(stockDirectory).Game;
        CheckCompiledLifetimeOperands(stock.PowerBombGlowColors);
        CheckStateRestoration(stock.PowerBombGlowColors);
        var framesToRender = new List<ContextFrame>();
        foreach (bool crystalFlash in new[] { false, true })
        {
            var explosion = Create(stock.PowerBombGlowColors, crystalFlash);
            int frames = 0;
            do
            {
                frames++;
                Require(frames < 300, "The bounded stock explosion failed to clean up.");
                _ = explosion.StepFrame();
                string context = $"{(crystalFlash ? "Crystal Flash" : "Power Bomb")}, frame {frames}";
                var glow = GameplayEffectCapture.CapturePowerBombGlow(PowerBombRenderSnapshot.Capture(explosion), new Position16<RoomPixels>(0, 0));
                if (glow is not null)
                {
                    var packet = new DisplayPacket(0,
                        new LayeredScene(FramePaint.FromNative(new RgbColor[SnesCgram.ColorCount], []), new DisplayLayer[] { glow }, 15));
                    framesToRender.Add(new ContextFrame(packet, context));
                }
            } while (explosion.IsActive);
            if (crystalFlash)
            {
                Require(frames == 36, context: "Stock Crystal Flash must expand for 18 frames, fade for 17, then clean up.");
                Require(!explosion.IsActive && !explosion.IsArmed, "Crystal Flash retained a lock after cleanup");
                Require(Colors(explosion) == new ExplosionColors(0, 0, 0), "inactive Crystal Flash retained display colors");
            }
            explosion.Reset();
            Require(Colors(explosion) == new ExplosionColors(0, 0, 0), "reset retains display colors");
            Console.WriteLine($"{(crystalFlash ? "Crystal Flash" : "Power Bomb")} cleanup at frame {frames}.");
        }
        foreach (D3D11DeviceKind kind in Enum.GetValues<D3D11DeviceKind>())
        {
            using var device = new D3D11RenderDevice(kind);
            using var renderer = new D3D11FrameRenderer(device);
            foreach (var sample in framesToRender)
            {
                var frame = sample.Frame;
                PixelComparison.Verify(frame, DisplayRendering.Render(frame),
                    renderer.RenderForReadback(frame), $"{kind}: {sample.Context}");
            }
            Console.WriteLine($"{kind}: {framesToRender.Count} installed explosion frames match the WARP reference.");
        }
        Console.WriteLine("Power Bomb/Crystal Flash: installed colours run to cleanup and restore exactly. No ROM or player data used.");
    }

    /// <summary>Starts a Crystal Flash or arms and spawns a Power Bomb with the supplied presentation catalog.</summary>
    internal static SamusPowerBombExplosionState Create(InstalledPowerBombGlowColors colors, bool crystalFlash)
    {
        var state = new SamusPowerBombExplosionState { PresentationColors = colors };
        if (crystalFlash) state.BeginCrystalFlash(new(128, 112));
        else { state.Arm(); state.Spawn(new(128, 112)); }
        return state;
    }

    /// <summary>Compares all serialized explosion fields except the editable colour output and the bound colours.</summary>
    internal static void RequireSameMechanics(SamusPowerBombExplosionState first,
        SamusPowerBombExplosionState second, string context)
    {
        // Include private phase timers and every future serialized field. Only the
        // colour the explosion adds and the installed colours it is bound to may differ.
        foreach (FieldInfo field in MetadataShape.Fields(typeof(SamusPowerBombExplosionState), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (field.IsDefined(typeof(NonSerializedAttribute)) || field.Name is
                "<GlowColor>k__BackingField" or "<PresentationColors>k__BackingField") continue;
            Require(Equals(field.GetValue(first), field.GetValue(second)),
                $"{context}: {field.Name} differs: first={field.GetValue(first)}, second={field.GetValue(second)}");
        }
    }

    /// <summary>Captures the three live glow colour outputs without including simulation state.</summary>
    internal static ExplosionColors Colors(SamusPowerBombExplosionState state) =>
        new ExplosionColors(state.GlowColor.FadeLevels.Red, state.GlowColor.FadeLevels.Green, state.GlowColor.FadeLevels.Blue);

    /// <summary>Fails the focused isolation check with its current edit/frame context.</summary>
    internal static void Require(bool condition, string context)
    {
        if (!condition) throw new InvalidOperationException(context);
    }

    /// <summary>Checks every reachable Crystal Flash radius derives its afterglow step count from the stock row's largest channel.</summary>
    private static void CheckCompiledLifetimeOperands(InstalledPowerBombGlowColors stock)
    {
        // The pinned $88:8D85 rows are (14,14,10), (15,15,9), (16,16,8),
        // (17,17,7). Check every reachable radius, not just the final row.
        for (int whole = 0; PowerBombRadius.FromWholePixels(whole) < PowerBombExplosionDefinitions.CrystalFlashRadiusLimit; whole++)
        for (int fraction = 0; fraction < PowerBombRadius.FractionsPerPixel; fraction++)
        {
            int radius = whole * PowerBombRadius.FractionsPerPixel + fraction;
            FadeLevelColour native = stock.Resolve(PowerBombGlowSequence.Explosion, radius >> 11).FadeLevels;
            Require(PowerBombExplosionAfterglow.CrystalFlashAfterglowSteps(PowerBombRadius.FromPixels(whole, fraction)) ==
                Math.Max(native.Red, Math.Max(native.Green, native.Blue)), $"Native fade operand differs at radius {radius}.");
        }
        try
        {
            _ = PowerBombExplosionAfterglow.CrystalFlashAfterglowSteps(
                PowerBombExplosionDefinitions.CrystalFlashRadiusLimit);
        }
        catch (ArgumentOutOfRangeException) { return; }
        throw new InvalidOperationException("The compiled Crystal Flash radius domain accepted its exclusive limit.");
    }

    /// <summary>Round-trips the debugger graph at every active frame and compares its remaining stock lifetime. Old layouts are
    /// rejected, not migrated (#1270).</summary>
    private static void CheckStateRestoration(InstalledPowerBombGlowColors colors)
    {
        int restores = 0;
        foreach (bool crystalFlash in new[] { false, true })
        {
            var state = Create(colors, crystalFlash);
            do
            {
                var current = Restore(state, colors, out string currentWarning);
                Require(currentWarning.Length == 0, "A current graph unexpectedly needed a migration.");
                Require(ReferenceEquals(current.PresentationColors, colors), "A debugger graph did not name its bound colours.");
                RequireSameMechanics(state, current, "Current graph restore");
                Require(Colors(state) == Colors(current), "Current graph restore changed display colors.");
                CompareRemainder(state, current, colors, "Current graph resume");
                restores++;
                _ = state.StepFrame();
            } while (state.IsActive);
        }
        Console.WriteLine($"Explosion state compatibility: {restores} checkpoints resume with identical stock state, colors and cleanup.");
    }

    /// <summary>Steps a restored pair through cleanup, comparing mechanics and display colors on each remaining frame.</summary>
    private static void CompareRemainder(SamusPowerBombExplosionState state,
        SamusPowerBombExplosionState restored, InstalledPowerBombGlowColors colors, string context)
    {
        var baseline = Restore(state, colors, out _);
        int frames = 0;
        do
        {
            Require(++frames < 300, context + ": cleanup did not occur");
            Require(baseline.StepFrame() == restored.StepFrame(), context + ": cleanup frame differs");
            RequireSameMechanics(baseline, restored, context);
            Require(Colors(baseline) == Colors(restored), context + ": restored display fade differs");
        } while (baseline.IsActive);
    }

    /// <summary>
    /// Serializes the graph with its colours bound, restores it through the production codec bound to
    /// <paramref name="colors"/>, and captures any warning.
    /// </summary>
    private static SamusPowerBombExplosionState Restore(SamusPowerBombExplosionState state,
        InstalledPowerBombGlowColors colors, out string warning)
    {
        using var data = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(data, state, new BoundObjectGraph([state.PresentationColors
            ?? throw new InvalidOperationException("A captured explosion is bound to its colours.")]));
        data.Position = 0;
        using var errors = new StringWriter();
        TextWriter previous = Console.Error;
        try
        {
            Console.SetError(errors);
            return DebuggerObjectGraphSerializer.Deserialize<SamusPowerBombExplosionState>(data, new BoundObjectGraph([colors]));
        }
        finally { Console.SetError(previous); warning = errors.ToString(); }
    }

}
