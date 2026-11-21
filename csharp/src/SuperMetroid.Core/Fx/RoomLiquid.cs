namespace SuperMetroid.Core.Fx;

/// <summary>
/// The liquid a room effect fills its room with (#1695): <see cref="RoomWater"/> for water and the Tourian entrance
/// statue's water, <see cref="RoomLavaOrAcid"/> for lava and acid. Every liquid's surface follows its <see cref="Tide"/>.
/// </summary>
/// <param name="Tide">How the surface rises and falls about its base line.</param>
public abstract record RoomLiquid(LiquidTide Tide);
