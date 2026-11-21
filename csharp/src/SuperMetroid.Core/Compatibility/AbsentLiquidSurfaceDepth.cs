namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Where a room liquid's rise finds a surface or target the room's effect does not name (#1546): below every line of the
/// room, the deepest line a surface can hold. A liquid with no target and a positive speed therefore starts moving toward
/// it, waiting out its delay with the rising liquid's rumble and earthquake, and stops on its first step; a liquid with no
/// surface and a negative speed starts moving from that line toward its target. The absent surface or target is otherwise
/// none (<c>int?</c>); the Speed Booster escape's lava stages give a lava with no surface their highest line as a case of
/// their own (#2558).
/// </summary>
/// <remarks>
/// Consulted whenever the liquid's rise waits for its speed to point from its surface toward its target, when a moving
/// liquid with no surface takes its step, and during the room effect's per-line liquid motion before the update's random
/// draw. Evidence: the rise routines compare the room effect's base and target words without testing their sign, as
/// unsigned words, and move the base word by the speed, wrapping as a word; a room effect record that names no surface or
/// target stores the word 65535, the largest word.
/// </remarks>
[CompatibilityService("a room liquid's rise reads a missing surface or target as lying below every line of the room")]
public static partial class AbsentLiquidSurfaceDepth
{
    /// <summary>The line a missing surface or target is compared and moved at: the deepest line a surface can hold.</summary>
    private const int BelowEveryLine = ushort.MaxValue;

    /// <summary>The line the rise compares and moves: <paramref name="line"/>, or the line below every line of the room.</summary>
    /// <param name="line">The room's liquid surface or target line, or none.</param>
    public static int Of(int? line) => line ?? BelowEveryLine;
}
