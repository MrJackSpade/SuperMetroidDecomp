using System.Reflection;
using SuperMetroid.Core.Game;

// Legacy namespace is persisted in debugger identities; ownership is now platform-neutral.
namespace SuperMetroid.Desktop;

/// <summary>Explicit, loss-aware compatibility rules for older debugger object layouts.</summary>
internal static class DebuggerStateFieldMigrations
{
    /// <summary>
    /// #342 adds the previously unmodeled WRAM $0E00 latch. Older builds never captured
    /// it, so their state cannot supply an exact value. Neutral zero avoids inventing a
    /// Fire press; the next normal draw/script update populates the native latch.
    /// </summary>
    private const string PreviousDrawNewInputField = "<PreviousDrawNewInput>k__BackingField";

    internal static FieldInfo[] SelectSerializedFields(Type type, FieldInfo[] current, int count)
    {
        if (count == current.Length) return current;
        if ((type == typeof(SuperMetroid.Core.Runtime.GameplayPpuRenderSnapshot) && count == 7 && current.Length == 9) ||
            (type == typeof(SuperMetroid.Core.Rendering.OrdinaryGameplayRegisters) && count == 11 && current.Length == 13))
        {
            Console.Error.WriteLine("WARNING: Older debugger state has no BG2 scanline window; the next accepted NMI reconstructs it.");
            return current.Where(field => field.Name is not "<Bg2FirstScanline>k__BackingField" and not "<Bg2EndScanline>k__BackingField").ToArray();
        }
        if (type == typeof(SamusState) && count == current.Length - 1 &&
            current.Any(field => field.Name == PreviousDrawNewInputField))
        {
            Console.Error.WriteLine("WARNING: Older debugger state has no Samus previous-draw input latch; initializing it to neutral input.");
            return current.Where(field => field.Name != PreviousDrawNewInputField).ToArray();
        }
        throw new InvalidDataException($"Serialized {type.FullName} contains {count} fields; this build expects {current.Length}.");
    }
}
