using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.TestSupport.ScenarioFixtures;

/// <summary>Converts Samus's acceleration mode to the native WRAM <c>$0B4A</c> word for native traces and comparisons.</summary>
internal static class SamusAccelerationModeCodec
{
    /// <summary>The native word a mode is stored as.</summary>
    internal static int ToNative(SamusAccelerationMode mode) => mode switch
    {
        SamusAccelerationMode.Accelerating => 0,
        SamusAccelerationMode.Turning => 1,
        SamusAccelerationMode.Decelerating => 2,
        _ => throw new InvalidOperationException($"Undefined {nameof(SamusAccelerationMode)} {mode}."),
    };
}
