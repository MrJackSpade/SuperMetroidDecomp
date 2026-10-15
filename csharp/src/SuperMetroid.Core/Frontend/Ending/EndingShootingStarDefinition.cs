namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>One shooting star's motion and animation record.</summary>
/// <param name="Acceleration">Signed 8.8 velocity change per call on both axes.</param>
/// <param name="Period">Animation frame duration.</param>
/// <param name="Delay">Initial invisible delay; zero starts immediately.</param>
internal readonly record struct EndingShootingStarDefinition(
    EightEightVector<ScreenPixels> Acceleration, ushort Period, ushort Delay);
