using System.Collections.Immutable;
namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Fixed hidden-wall rumble schedule.</summary>
internal static class CrocomireRumbleDefinitions
{
    /// <summary>
    /// The rumble's oscillations in the order it plays them (#1879): the swing grows from one pixel to four over the
    /// first five, faster and more often in the middle, then dies away. The rumble ends when the last finishes.
    /// </summary>
    internal static ImmutableArray<CrocomireRumbleOscillation> Oscillations { get; } =
    [
        new(ReturnOffset: 0, ExtremeOffset: -1, Repeats: 10, Speed: 1),
        new(ReturnOffset: 1, ExtremeOffset: -1, Repeats: 8, Speed: 1),
        new(ReturnOffset: 1, ExtremeOffset: -2, Repeats: 12, Speed: 1),
        new(ReturnOffset: 2, ExtremeOffset: -2, Repeats: 16, Speed: 2),
        new(ReturnOffset: 2, ExtremeOffset: -4, Repeats: 16, Speed: 2),
        new(ReturnOffset: 1, ExtremeOffset: -2, Repeats: 8, Speed: 1),
        new(ReturnOffset: 1, ExtremeOffset: -1, Repeats: 3, Speed: 1),
    ];
}
