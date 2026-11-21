using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Cartridge values and helpers of <see cref="CeresDoorAnimations"/> that only verification reads.</summary>
internal static class CeresDoorAnimationsConstants
{
    /// <summary>The animation that starts at each segment a door or wall facing <paramref name="facing"/> starts.</summary>
    /// <param name="facing">The way the door faces.</param>
    internal static IReadOnlyDictionary<CeresDoorSegment, Animation<CeresDoorSegment>> StartingAt(HorizontalDirection facing) => CeresDoorAnimations.StartingAtBySide[facing];
}
