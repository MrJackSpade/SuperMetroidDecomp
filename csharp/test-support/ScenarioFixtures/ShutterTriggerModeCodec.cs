using SuperMetroid.Core.Enemies.Shutters;

namespace SuperMetroid.TestSupport.ScenarioFixtures;

/// <summary>Converts a shutter trigger mode to the native even offset into its five-entry initial-function table (<c>$780E</c>).</summary>
internal static class ShutterTriggerModeCodec
{
    /// <summary>The native table offset the cartridge stores for a mode.</summary>
    internal static ushort ToNativeOffset(ShutterTriggerMode mode) => mode switch
    {
        ShutterTriggerMode.Timer => 0,
        ShutterTriggerMode.HorizontalProximity => 2,
        ShutterTriggerMode.Immediate => 4,
        ShutterTriggerMode.OneShotReaction => 6,
        ShutterTriggerMode.RepeatableReaction => 8,
        _ => throw new InvalidOperationException($"Undefined {nameof(ShutterTriggerMode)} {mode}."),
    };
}
