namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Reads Samus's acceleration mode from the native WRAM <c>$0B4A</c> word at the snapshot import boundary.</summary>
internal static class SamusAccelerationModeImport
{
    /// <summary>The mode a native word holds.</summary>
    /// <exception cref="InvalidDataException">The word is none of the three modes the cartridge writes.</exception>
    internal static SamusAccelerationMode FromNative(int word) => word switch
    {
        0 => SamusAccelerationMode.Accelerating,
        1 => SamusAccelerationMode.Turning,
        2 => SamusAccelerationMode.Decelerating,
        _ => throw new InvalidDataException($"Acceleration mode word {word:X4} is none of the three modes the cartridge writes."),
    };
}
