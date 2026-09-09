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
        if (type == typeof(SuperMetroid.Core.Frontend.SuperMetroidGameOptions) && count == 9 && current.Length == 11)
        {
            // Both fields were added after the original nine-option host layout.
            // Omitted bool/nullable values restore as false/null: no countdown clamp
            // and no ending-time override, matching the capabilities of that build.
            Console.Error.WriteLine("WARNING: Older debugger options predate escape-timeout and ending-time overrides; leaving both disabled.");
            return current.Where(field => field.Name is not "<PreventEscapeTimeout>k__BackingField"
                and not "<EndingTimeOverrideMinutes>k__BackingField").ToArray();
        }
        if (type == typeof(SuperMetroid.Core.Runtime.SuperMetroidRuntime) && count == current.Length - 1)
        {
            Console.Error.WriteLine("WARNING: Older debugger state predates the statue sequence; it initializes on room entry.");
            return current.Where(field => field.Name != "_tourianStatues").ToArray();
        }
        if (type == typeof(RoomEnemySystem) && count == current.Length - 2)
        {
            Console.Error.WriteLine("WARNING: Older debugger state has no statue displacement/water surface; initializing to zero.");
            return current.Where(field => field.Name is not "<TourianEntranceStatueVerticalOffset>k__BackingField"
                and not "<TourianStatueWaterY>k__BackingField").ToArray();
        }
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
