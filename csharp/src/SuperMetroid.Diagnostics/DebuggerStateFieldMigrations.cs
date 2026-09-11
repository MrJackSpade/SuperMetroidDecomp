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
        if (type == typeof(SamusGrappleState) && count == current.Length - 1)
        {
            Console.Error.WriteLine("WARNING: Legacy grapple state has no pose-change auto-fire timer; unavailable firing age restores expired until the next shot.");
            return current.Where(field => field.Name != "<PoseChangeAutoFireTimer>k__BackingField").ToArray();
        }
        if (type.FullName == "SuperMetroid.Core.Frontend.IntroCinematicObjectSystem" && count == current.Length - 1)
        {
            Console.Error.WriteLine("WARNING: Legacy intro state has no text-glow history; existing glyph ages cannot be recovered. Newly drawn glyphs start native glow normally.");
            return current.Where(field => field.Name != "textGlow").ToArray();
        }
        if (type == typeof(PhantoonBlendingState) && count == current.Length - 1)
        {
            Console.Error.WriteLine("WARNING: Legacy Phantoon display state lacks MOSAIC history; restores ungrouped until the next accepted NMI.");
            return current.Where(field => field.Name != "<DisplayedMosaic>k__BackingField").ToArray();
        }
        if (type == typeof(SuperMetroid.Core.Rendering.OrdinaryGameplayRegisters) &&
            current.Any(field => field.Name == "<Bg2Mosaic>k__BackingField") && count is 11 or 13 or 15)
        {
            Console.Error.WriteLine("WARNING: Legacy gameplay display registers lack BG2 mosaic; restoring original ungrouped sampling.");
            return SelectSerializedFields(type, current.Where(field => field.Name != "<Bg2Mosaic>k__BackingField").ToArray(), count);
        }
        if (type == typeof(PhantoonEnemyState) &&
            (count == current.Length - 1 || count == current.Length - 2))
        {
            Console.Error.WriteLine("WARNING: Legacy Phantoon state lacks blend HDMA history; setup restarts. Pre-wave states also restore missing wave history inactive until its next native spawn.");
            return current.Where(field => field.Name != "_blending" &&
                (count != current.Length - 2 || field.Name != "_wave")).ToArray();
        }
        if (type == typeof(SamusState) && current.Any(field => field.Name == "_poseHistory") &&
            (count == current.Length - 1 || count == current.Length - 2 ||
             count == current.Length - 4 || count == current.Length - 5))
        {
            Console.Error.WriteLine("WARNING: Older Samus state has no transition pose history; the unavailable history restores as zero until subsequent transitions populate it.");
            // First remove this addition, then apply the explicitly supported older
            // auto-jump / draw-input layouts. Their remaining field identities and
            // ordering are still checked by the graph reader, not inferred silently.
            return SelectSerializedFields(type,
                current.Where(field => field.Name != "_poseHistory").ToArray(), count);
        }
        if (type == typeof(SamusState) && current.Any(field => field.Name == "_poseHistory"))
            throw new InvalidDataException($"Unsupported legacy Samus pose-history layout with {count} fields.");
        if (type == typeof(SamusState) &&
            (count == current.Length - 3 || count == current.Length - 4))
        {
            Console.Error.WriteLine("WARNING: Older Samus state lacks auto-jump history; restoring neutral history and ordinary input handling.");
            bool lacksNewInput = count == current.Length - 4;
            return current.Where(field => field.Name is not "<AutoJumpTimer>k__BackingField"
                and not "<PreviousDrawHeldInput>k__BackingField"
                and not "<AutoJumpInputPending>k__BackingField" &&
                (!lacksNewInput || field.Name != PreviousDrawNewInputField)).ToArray();
        }
        if (type == typeof(SuperMetroid.Core.Rendering.BgSubscreenAddRenderLayer) && count == 6 && current.Length == 7)
        {
            Console.Error.WriteLine("WARNING: Legacy subscreen layer has no vertical-scroll field; restoring its original unscrolled sampling.");
            return current.Where(field => field.Name != "<VerticalScroll>k__BackingField").ToArray();
        }
        if (type == typeof(SuperMetroid.Core.Rendering.Mode7RenderLayer) && count == 2 && current.Length == 3)
        {
            Console.Error.WriteLine("WARNING: Legacy Mode 7 layer has no BG1 subscreen addition; retaining its original composition.");
            return current.Where(field => field.Name != "<AddBg1Subscreen>k__BackingField").ToArray();
        }
        if (type == typeof(SuperMetroid.Core.Rendering.Mode7RenderRegisters) && count == 9 && current.Length == 10)
        {
            Console.Error.WriteLine("WARNING: Legacy Mode 7 snapshot has no wrap control; retaining its original overflow behavior.");
            return current.Where(field => field.Name != "<WrapOutsideMap>k__BackingField").ToArray();
        }
        if (type == typeof(ScrollBoundaryCamera) && count == 11 && current.Length == 12)
        {
            Console.Error.WriteLine("WARNING: Legacy camera has no previous-scroll Samus checkpoint; initializing on its first scrolling pass.");
            return current.Where(field => field.Name != "<PreviousSamusPoint>k__BackingField").ToArray();
        }
        if (type == typeof(SuperMetroid.Core.Runtime.SuperMetroidRuntime) && count == 106 && current.Length == 110)
        {
            // Additions verified against b944f1b5: statue owner (5ff0476a),
            // timeout option (fc59514a), escape quake (a74aa6d1), treadmill owner (e361a6b1).
            // ReadFields still validates every surviving declaring type/name/order.
            Console.Error.WriteLine("WARNING: Legacy runtime lacks statue, escape-quake, timeout and treadmill state; added features restore inactive.");
            return current.Where(field => field.Name is not "_tourianStatues"
                and not "_escapeDiagonalFrames"
                and not "<PreventEscapeTimeout>k__BackingField"
                and not "<RoomTreadmills>k__BackingField").ToArray();
        }
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
        if (type == typeof(SuperMetroid.Core.Rendering.OrdinaryGameplayRegisters) &&
            count is 11 or 13 && current.Length == 15)
        {
            // Hardware windows added two fields after the scanline-window schema.
            // Keep both historical layouts explicit; never infer missing fields from
            // a count alone or reorder the older fields to fit the newer record.
            Console.Error.WriteLine("WARNING: Older gameplay capture has no hardware window registers; restoring disabled windows.");
            if (count == 11)
                Console.Error.WriteLine("WARNING: Older gameplay capture also has no BG2 scanline window; the next accepted NMI reconstructs it.");
            return current.Where(field => field.Name is not "<Windows>k__BackingField"
                and not "<MainScreenWindowMask>k__BackingField" &&
                (count == 13 || field.Name is not "<Bg2FirstScanline>k__BackingField"
                    and not "<Bg2EndScanline>k__BackingField")).ToArray();
        }
        if (type == typeof(SuperMetroid.Core.Runtime.GameplayPpuRenderSnapshot) && count == 7 && current.Length == 9)
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

    /// <summary>Constructs an empty owner only for the known legacy layout that omitted it.</summary>
    internal static void InitializeMissingFields(object instance, int serializedCount)
    {
        if (instance is SuperMetroid.Core.Runtime.SuperMetroidRuntime && serializedCount == 106)
        {
            // Constructors are bypassed by graph restoration. No animation existed
            // in this layout; normal room loading will select the next room's objects.
            typeof(SuperMetroid.Core.Runtime.SuperMetroidRuntime)
                .GetField("<RoomTreadmills>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(instance, new RoomTreadmillAnimatedTilesState());
        }
    }
}
