namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>Places a gunship liftoff dust cloud by its spawn parameter.</summary>
internal static class GunshipDust
{
    /// <summary>The placement and program even parameter <paramref name="parameter"/> selects.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="parameter"/> is not 0, 2, 4, 6, 8 or A.</exception>
    internal static GunshipDustPlacement ForParameter(int parameter) =>
        GunshipDustDefinitions.Placements.TryGetValue(parameter, out GunshipDustPlacement placement)
            ? placement
            : throw new ArgumentOutOfRangeException(nameof(parameter), parameter, "Gunship dust parameter must be 0,2,4,6,8,A.");
}
