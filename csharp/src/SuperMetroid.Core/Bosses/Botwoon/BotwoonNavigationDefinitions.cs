namespace SuperMetroid.Core.Bosses.Botwoon;
/// <summary>Compiled fixed geometry and path metadata for Botwoon's room navigation.</summary>
internal static class BotwoonNavigationDefinitions
{

    /// <summary>
    /// <c>$B3:989D</c>: random bits 1-3 pick an even action offset 0-14 when Botwoon leaves a hole outside
    /// its health phases; offsets 0, 2 and 4 follow an authored path and the rest spit from the hole.
    /// </summary>
    internal const ushort ActionChoiceMask = 0x000e;

    /// <summary>Random bits 3-4 pick one of the target hole's four path descriptors.</summary>
    internal const ushort PathChoiceMask = 0x0018;

    /// <summary>The head-position history ring's X/Y samples.</summary>
    internal const int HistoryEntries = 256;

    /// <summary>Bytes per history sample in native WRAM; native cursors and spacings are entry indices times this.</summary>
    internal const int HistoryEntryBytes = 4;

    /// <summary>Wraps a history-ring index, as the native <c>AND #$03FF</c> does to its byte offset.</summary>
    internal static int WrapHistoryIndex(int index) => index & (HistoryEntries - 1);

    /// <summary>Random bits below the path variant: bits 3-4 select one of a hole's four path descriptors.</summary>
    internal const int PathChoiceVariantShift = 3;

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

    /// <summary>The packed movement sample's X nibble sits above its Y nibble.</summary>
    internal const int MovementXShift = 4;

    /// <summary>Keeps one packed movement nibble.</summary>
    internal const int MovementComponentMask = 0x0f;

    /// <summary>
    /// The authored step each packed movement nibble code stands for, in code order: the stream separator/terminator
    /// <see cref="sbyte.MinValue"/>, then -16, -1, 0 and 1.
    /// </summary>
    internal static readonly sbyte[] MovementSteps = [sbyte.MinValue, -16, -1, 0, 1];
}
