namespace SuperMetroid.Core.Progression;

/// <summary>What one area of the game is (<see cref="AreaDefinitions.Areas"/>).</summary>
/// <param name="AllowsPausing">Whether the player may pause in the area's rooms.</param>
/// <param name="PoweredFromBossDefeat">
/// Whether the area's machinery has power only once its boss is defeated: until then its on-foot conveyors stand still and
/// its heavy spike blocks are harmless.
/// </param>
internal sealed record AreaDefinition(bool AllowsPausing, bool PoweredFromBossDefeat);
