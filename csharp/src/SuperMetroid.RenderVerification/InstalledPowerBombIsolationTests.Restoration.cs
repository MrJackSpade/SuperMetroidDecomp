using System.Reflection;
using System.Text;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Desktop;

internal static partial class InstalledPowerBombIsolationTests
{
    /// <summary>Checks every reachable Crystal Flash radius against its compiled fade operand and enforces the exclusive limit.</summary>
    /// <param name="stock">Stock fixed-color catalog supplying the native lifetime colors.</param>
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

    /// <summary>Verifies current and legacy state graphs restore and resume both normal and Crystal Flash explosions.</summary>
    /// <param name="colors">Stock color catalog rebound after each debugger-state restore.</param>
    private static void CheckStateRestoration(PowerBombFixedColorCatalog colors)
    {
        int currentRestores = 0, legacyRestores = 0;
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
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
                _ = state.StepFrame(memory);
            } while (state.IsActive);
        }
        Console.WriteLine($"Explosion state compatibility: {currentRestores} current and {legacyRestores} old-layout " +
            "checkpoints resume with identical stock state, colors and cleanup; old layouts warn explicitly.");
    }

    /// <summary>Checks current edited captures and legacy modded captures retain their timing after catalog rebinding.</summary>
    /// <param name="edited">Explosion state captured with customized presentation colors.</param>
    /// <param name="reboundColors">Replacement catalog assigned after restoring the checkpoint.</param>
    /// <param name="context">Scenario label used to identify a failed assertion.</param>
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
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        for (int frame = 1; frame <= remaining; frame++)
            Require(historical.StepFrame(memory) == (frame == remaining),
                context + ": modded legacy capture did not preserve its historical cleanup frame");
        Require(!historical.IsActive, context + ": modded legacy capture did not release HDMA");
    }

    /// <summary>Compares cleanup timing, mechanics, and displayed fade colors from a restored checkpoint onward.</summary>
    /// <param name="state">Captured state used to create the comparison baseline.</param>
    /// <param name="restored">Restored state whose remaining execution is compared with the baseline.</param>
    /// <param name="colors">Catalog rebound to both states before they resume.</param>
    /// <param name="context">Scenario label included in assertion failures.</param>
    private static void CompareRemainder(SamusPowerBombExplosionState state,
        SamusPowerBombExplosionState restored, PowerBombFixedColorCatalog colors, string context)
    {
        var baseline = Restore(state, legacy: false, out _);
        baseline.PresentationColors = colors;
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        int frames = 0;
        do
        {
            Require(++frames < 300, context + ": cleanup did not occur");
            Require(baseline.StepFrame(memory) == restored.StepFrame(memory), context + ": cleanup frame differs");
            RequireSameMechanics(baseline, restored, context);
            Require(Colors(baseline) == Colors(restored), context + ": restored display fade differs");
        } while (baseline.IsActive);
    }

    /// <summary>Serializes and deserializes an explosion state using either the current or legacy field layout.</summary>
    /// <param name="state">Checkpoint to round-trip.</param>
    /// <param name="legacy">Whether to write the prior field envelope before deserialization.</param>
    /// <param name="warning">Receives any migration warning written during deserialization.</param>
    /// <returns>The reconstructed explosion state.</returns>
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

    /// <summary>Writes the pre-separation explosion-state envelope, omitting the newer afterglow countdown field.</summary>
    /// <param name="destination">Stream receiving the legacy object graph.</param>
    /// <param name="state">State whose legacy-compatible fields are serialized.</param>
    private static void WriteLegacyGraph(Stream destination, SamusPowerBombExplosionState state)
    {
        // Construct the exact prior field envelope while using the production codec
        // for primitive values. No fixture changes the production serializer's API.
        const byte newObjectMarker = 2, fieldPayloadKind = 5;
        var fields = typeof(SamusPowerBombExplosionState).GetFields(BindingFlags.Instance |
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
            writer.Write(DebuggerStateTypeIdentity.GetSerializedName(field.DeclaringType!));
            writer.Write(field.Name);
            writer.Flush();
            DebuggerObjectGraphSerializer.Serialize(destination, field.GetValue(state)!);
        }
    }
}
