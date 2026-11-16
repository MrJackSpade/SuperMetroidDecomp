using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Compatibility;

/// <summary>Cartridge values and helpers of <see cref="DraygonRegrabSpiralLift"/> that only verification reads.</summary>
internal static class DraygonRegrabSpiralLiftConstants
{
    /// <summary>A center at <paramref name="center"/>, as a check states one.</summary>
    public static DraygonRegrabSpiralLift At(FixedPosition<RoomPixels> center) => new(center);
}
