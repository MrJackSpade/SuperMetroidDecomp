namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The statue sets each boss statue alone makes (#2482), from which the sets the statues' shared state holds are built.</summary>
internal static class TourianGreyStatueSets
{
    /// <summary>The set holding each statue alone.</summary>
    internal static IReadOnlyDictionary<TourianStatue, TourianGreyStatues> Alone { get; } = new Dictionary<TourianStatue, TourianGreyStatues>
    {
        [TourianStatue.Phantoon] = TourianGreyStatues.Phantoon,
        [TourianStatue.Ridley] = TourianGreyStatues.Ridley,
        [TourianStatue.Kraid] = TourianGreyStatues.Kraid,
        [TourianStatue.Draygon] = TourianGreyStatues.Draygon,
    };
}
