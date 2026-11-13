namespace SuperMetroid.Core.Fx;

/// <summary>A room's lava or acid (#1695): its tide and the heat haze it ripples the room's background with.</summary>
/// <param name="Tide">How the surface rises and falls about its base line.</param>
/// <param name="Haze">Which way the heat haze ripples the background, if at all.</param>
public sealed record RoomLavaOrAcid(LiquidTide Tide, HeatHazeDirection Haze) : RoomLiquid(Tide);
