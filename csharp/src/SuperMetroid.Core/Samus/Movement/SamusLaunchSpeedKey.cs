namespace SuperMetroid.Core.Samus.Movement;

/// <summary>The conditions a launch speed is selected by, apart from jump equipment.</summary>
/// <param name="Medium">The medium Samus launches from.</param>
/// <param name="Kind">Whether Samus jumps or kicks away from a wall.</param>
internal readonly record struct SamusLaunchSpeedKey(SamusLiquidMedium Medium, SamusLaunchKind Kind);
