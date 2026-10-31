using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Botwoon;
/// <summary>Compiled fixed geometry and path metadata for Botwoon's room navigation.</summary>
internal static class BotwoonNavigationDefinitions
{

    /// <summary>
    /// <c>$B3:989D</c>: the random selection of one of eight actions when Botwoon leaves a hole outside its health
    /// phases; the first <see cref="AuthoredPathActions"/> follow an authored path and the rest spit from the hole.
    /// </summary>
    internal static readonly RandomSampleField ActionChoice = new(1, 3);

    /// <summary>How many of the <see cref="ActionChoice"/> actions, counted from the first, follow an authored path.</summary>
    internal const int AuthoredPathActions = 3;

    /// <summary>The random selection of one of the target hole's four path descriptors.</summary>
    internal static readonly RandomSampleField PathChoice = new(3, 2);

    /// <summary>The head-position history ring's X/Y samples.</summary>
    internal const int HistoryEntries = 256;

    /// <summary>Wraps a history-ring index around the ring's <see cref="HistoryEntries"/> entries.</summary>
    internal static int WrapHistoryIndex(int index) => index & (HistoryEntries - 1);

    /// <summary>Path descriptors authored for each source hole and visibility.</summary>
    internal const int PathVariantsPerHole = 4;

    /// <summary>The first of the sixteen path choices that begin inside a hole; the visible choices precede them.</summary>
    internal const int HiddenPathChoiceBase = 16;

    /// <summary>The number of authored path choices: four variants from each of four holes, visible or hidden.</summary>
    internal const int PathChoiceCount = 32;

    /// <summary>
    /// The four room holes in their authored order: left (centre 64,112), bottom (128,176), top (160,96) and right
    /// (224,144). A hole's index here is its position in the path table's per-hole groups.
    /// </summary>
    internal static readonly BotwoonHoleDefinition[] Holes =
    [
        new(new(64 - 4, 112 - 4)),
        new(new(128 - 4, 176 - 4)),
        new(new(160 - 4, 96 - 4)),
        new(new(224 - 4, 144 - 4)),
    ];

    /// <summary>The number of room holes.</summary>
    internal static int HoleCount => Holes.Length;

    /// <summary>
    /// How many steps before an authored path's end a backward walk starts. The walk begins on the path's
    /// second-to-last step, so it never takes the last one, as the original game's backward walks do.
    /// </summary>
    internal const int BackwardStartStepsBeforeEnd = 2;
}
