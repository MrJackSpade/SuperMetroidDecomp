namespace SuperMetroid.Core.Fx;

/// <summary>A room's water (#1695): its tide and what its surface does.</summary>
/// <param name="Tide">How the surface rises and falls about its base line.</param>
/// <param name="SurfaceDrift">Whether the effect overlay's surface texture drifts right by a quarter pixel every frame.</param>
/// <param name="BackgroundWave">
/// Whether the background below the surface ripples with the horizontal water wave. Only water shows it: the statue
/// effect's water moves the statue instead.
/// </param>
/// <param name="IgnoresSurfaceUntilEnabled">
/// Whether Samus's movement ignores the surface until a room object enables it (the n00b tube's glass), as though the
/// room were dry.
/// </param>
[IndependentFlag("independent options a room's effect record gives its water: surface drift, background wave and whether Samus ignores the surface; retail records combine them freely")]
public sealed record RoomWater(LiquidTide Tide, bool SurfaceDrift, bool BackgroundWave, bool IgnoresSurfaceUntilEnabled)
    : RoomLiquid(Tide)
{
    /// <summary>This water with its surface affecting Samus, as the room object that enables water physics leaves it.</summary>
    internal RoomWater WithSurfaceEnabled() => this with { IgnoresSurfaceUntilEnabled = false };
}
