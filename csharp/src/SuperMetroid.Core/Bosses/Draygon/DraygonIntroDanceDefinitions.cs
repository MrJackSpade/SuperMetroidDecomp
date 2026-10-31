using static SuperMetroid.Core.Bosses.Draygon.DraygonIntroMovement;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Timing and the traced path of Draygon's opening Evir dance.</summary>
internal static class DraygonIntroDanceDefinitions
{
    /// <summary>The Evirs the opening dance moves: the first four sprite objects in processing order.</summary>
    internal const int EvirCount = 4;

    /// <summary>How many updates the dance lasts before Draygon starts its first swoop.</summary>
    internal const int DurationFrames = 1232;

    /// <summary>The sprite-object slot of the first dancing Evir; the four dancers occupy it and the next three.</summary>
    internal const int FirstDancerSlot = 28;

    /// <summary>
    /// How many dance updates each Evir waits before it takes the path's first step, by slot from
    /// <see cref="FirstDancerSlot"/>. Even the last Evir waits 128 updates: the original game reads its four delays
    /// from the first half of an eight-entry table, so no Evir starts at once.
    /// </summary>
    internal static readonly int[] StartDelays = [224, 192, 160, 128];

    /// <summary>
    /// The handwritten Keiko Love path every dancing Evir follows, one step per update. Two delete steps end each
    /// Evir's dance part way through; the steps after them stay part of the path.
    /// </summary>
    /// <remarks>Independently reviewed for #1165: integrating the original deltas visibly traces the lettering named
    /// by the pinned disassembly. Retain this drawing content under the nonsense exception: fitted splines, per-stroke
    /// corrections or a numeric switch would encode the same particular letter shapes and pen timing. This is not a
    /// sampled physical trajectory. See draygonIntroTraceReview in the #1165 inventory.</remarks>
    internal static readonly DraygonIntroMovement[] TracedPath =
    [
        Move(new(3, 0)), Move(new(3, 0)), Move(new(3, 0)), Move(new(3, 0)), Move(new(3, -1)), Move(new(2, -1)), Move(new(2, -2)), Move(new(2, -2)), Move(new(2, -3)),
        Move(new(1, -3)), Move(new(1, -2)), Move(new(1, -2)), Move(new(1, 0)), Move(new(1, 0)), Move(new(0, 1)), Move(new(0, 2)), Move(new(0, 2)), Move(new(0, 2)),
        Move(new(-1, 3)), Move(new(0, 3)), Move(new(0, 3)), Move(new(0, 4)), Move(new(0, 4)), Move(new(-1, 4)), Move(new(-1, 4)), Move(new(0, 5)), Move(new(-1, 4)),
        Move(new(-1, 4)), Move(new(-1, 5)), Move(new(-1, 4)), Move(new(-1, 4)), Move(new(-1, 3)), Move(new(-1, 3)), Move(new(-1, 3)), Move(new(-1, 3)),
        Move(new(-1, 2)), Move(new(0, 1)), Move(new(0, 1)), Move(new(-1, 0)), Move(new(0, -1)), Move(new(0, -1)), Move(new(0, -1)), Move(new(0, -2)),
        Move(new(0, -3)), Move(new(1, -3)), Move(new(0, -3)), Move(new(0, -4)), Move(new(1, -4)), Move(new(1, -4)), Move(new(0, -4)), Move(new(1, -3)),
        Move(new(1, -3)), Move(new(1, -3)), Move(new(2, -3)), Move(new(1, -3)), Move(new(1, -3)), Move(new(2, -2)), Move(new(1, -3)), Move(new(2, -2)),
        Move(new(1, -2)), Move(new(2, -2)), Move(new(2, -1)), Move(new(3, -2)), Move(new(3, -2)), Move(new(3, -2)), Move(new(3, -2)), Move(new(3, -2)),
        Move(new(3, -2)), Move(new(3, -1)), Move(new(2, -1)), Move(new(2, -1)), Move(new(2, 0)), Move(new(2, 0)), Move(new(2, 0)), Move(new(1, 1)), Move(new(0, 2)),
        Move(new(0, 2)), Move(new(0, 3)), Move(new(-1, 3)), Move(new(-1, 3)), Move(new(-2, 3)), Move(new(-2, 3)), Move(new(-2, 3)), Move(new(-2, 3)),
        Move(new(-2, 3)), Move(new(-2, 2)), Move(new(-3, 2)), Move(new(-2, 1)), Move(new(-2, 1)), Move(new(-2, 1)), Move(new(-3, 1)), Move(new(-3, 1)),
        Move(new(-2, 0)), Move(new(-3, -1)), Move(new(-3, -1)), Move(new(-3, -1)), Move(new(-3, -1)), Move(new(-2, -2)), Move(new(-1, -2)), Move(new(1, -2)),
        Move(new(2, -2)), Move(new(3, -2)), Move(new(4, -1)), Move(new(3, 0)), Move(new(3, 0)), Move(new(3, 0)), Move(new(2, 2)), Move(new(2, 2)), Move(new(2, 2)),
        Move(new(2, 2)), Move(new(1, 3)), Move(new(1, 3)), Move(new(1, 3)), Move(new(2, 3)), Move(new(1, 2)), Move(new(1, 3)), Move(new(1, 3)), Move(new(1, 3)),
        Move(new(1, 2)), Move(new(2, 2)), Move(new(2, 2)), Move(new(2, 2)), Move(new(2, 2)), Move(new(2, 1)), Move(new(2, 1)), Move(new(3, 1)), Move(new(3, 1)),
        Move(new(3, 0)), Move(new(3, 1)), Move(new(2, 0)), Move(new(2, 0)), Move(new(2, 0)), Move(new(1, 0)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)),
        Move(new(2, 0)), Move(new(1, -1)), Move(new(1, -1)), Move(new(2, -1)), Move(new(1, -1)), Move(new(2, -1)), Move(new(1, -1)), Move(new(1, -1)),
        Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -2)), Move(new(1, -2)), Move(new(1, -2)), Move(new(1, -2)),
        Move(new(0, -2)), Move(new(0, -2)), Move(new(1, -2)), Move(new(0, -1)), Move(new(0, -1)), Move(new(0, -2)), Move(new(0, -2)), Move(new(-1, -2)),
        Move(new(0, -1)), Move(new(-1, -2)), Move(new(-1, -2)), Move(new(-1, -2)), Move(new(-1, -1)), Move(new(-1, -1)), Move(new(-2, -1)), Move(new(-1, 0)),
        Move(new(-1, 0)), Move(new(-2, 1)), Move(new(-2, 1)), Move(new(-2, 1)), Move(new(-1, 1)), Move(new(-1, 1)), Move(new(-1, 1)), Move(new(-1, 2)),
        Move(new(-1, 1)), Move(new(-1, 1)), Move(new(0, 1)), Move(new(0, 2)), Move(new(-1, 2)), Move(new(0, 2)), Move(new(0, 2)), Move(new(0, 2)), Move(new(0, 1)),
        Move(new(0, 1)), Move(new(0, 2)), Move(new(0, 2)), Move(new(0, 1)), Move(new(0, 2)), Move(new(0, 2)), Move(new(1, 1)), Move(new(0, 1)), Move(new(0, 1)),
        Move(new(0, 2)), Move(new(1, 1)), Move(new(1, 1)), Move(new(2, 0)), Move(new(1, 1)), Move(new(2, 1)), Move(new(2, 1)), Move(new(2, 1)), Move(new(2, 1)),
        Move(new(2, 0)), Move(new(1, 0)), Move(new(1, 0)), Move(new(2, 0)), Move(new(2, 0)), Move(new(2, 0)), Move(new(2, 0)), Move(new(2, 0)), Move(new(1, 0)),
        Move(new(1, 0)), Move(new(1, -1)), Move(new(2, -1)), Move(new(1, 0)), Move(new(1, 0)), Move(new(1, -1)), Move(new(2, -1)), Move(new(1, -1)),
        Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -2)), Move(new(1, -1)), Move(new(1, -2)), Move(new(0, -2)),
        Move(new(0, -2)), Move(new(0, -2)), Move(new(0, -1)), Move(new(0, -2)), Move(new(1, -2)), Move(new(1, -2)), Move(new(1, -2)), Move(new(1, -2)),
        Move(new(1, -2)), Move(new(1, -2)), Move(new(1, -2)), Move(new(0, -2)), Move(new(0, -2)), Move(new(0, -1)), Move(new(0, -1)), Move(new(1, -1)),
        Move(new(0, -2)), Move(new(0, -1)), Move(new(1, -1)), Move(new(0, -1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 2)),
        Move(new(0, 2)), Move(new(0, 2)), Move(new(0, 1)), Move(new(0, 2)), Move(new(0, 2)), Move(new(0, 1)), Move(new(0, 2)), Move(new(0, 2)), Move(new(0, 2)),
        Move(new(0, 2)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 2)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 2)), Move(new(0, 1)), Move(new(0, 2)),
        Move(new(0, 1)), Move(new(0, 1)), Move(new(1, 1)), Move(new(0, 1)), Move(new(0, 2)), Move(new(0, 1)), Move(new(1, 2)), Move(new(0, 1)), Move(new(1, 1)),
        Move(new(1, 1)), Move(new(1, 1)), Move(new(1, 0)), Move(new(1, 1)), Move(new(1, 1)), Move(new(1, 0)), Move(new(2, 0)), Move(new(2, 0)), Move(new(1, 0)),
        Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)),
        Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)),
        Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(0, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(0, -1)), Move(new(0, -1)),
        Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -2)), Move(new(0, -2)), Move(new(0, -1)), Move(new(0, -1)), Move(new(1, -2)),
        Move(new(1, -2)), Move(new(0, -2)), Move(new(1, -2)), Move(new(0, -2)), Move(new(0, -2)), Move(new(0, -2)), Move(new(0, -2)), Move(new(0, -2)),
        Move(new(0, -2)), Move(new(0, -2)), Move(new(0, -2)), Move(new(0, -2)), Move(new(0, -2)), Move(new(0, -2)), Move(new(0, -1)), Move(new(0, -1)),
        Move(new(-1, -1)), Move(new(0, -1)), Move(new(0, -1)), Move(new(0, -1)), Move(new(-1, 0)), Move(new(0, 1)), Move(new(0, 1)), Move(new(-1, 1)),
        Move(new(-1, 1)), Move(new(0, 2)), Move(new(-1, 2)), Move(new(-1, 2)), Move(new(-1, 1)), Move(new(0, 2)), Move(new(-1, 1)), Move(new(-1, 2)),
        Move(new(-1, 1)), Move(new(0, 2)), Move(new(0, 2)), Move(new(0, 2)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(-1, 1)), Move(new(0, 1)),
        Move(new(0, 2)), Move(new(0, 2)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 2)), Move(new(0, 1)), Move(new(0, 1)), Move(new(1, 1)), Move(new(0, 1)),
        Move(new(0, 2)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 2)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 2)), Move(new(0, 1)), Move(new(0, 1)),
        Move(new(0, 1)), Move(new(0, 2)), Move(new(0, 2)), Move(new(0, 2)), Move(new(0, 2)), Move(new(0, 2)), Move(new(0, 1)), Move(new(0, 2)), Move(new(-1, 2)),
        Move(new(0, 1)), Move(new(-1, 1)), Move(new(0, 1)), Move(new(-1, 0)), Move(new(0, -1)), Move(new(0, -1)), Move(new(0, -2)), Move(new(0, -2)),
        Move(new(0, -2)), Move(new(0, -2)), Move(new(0, -2)), Move(new(1, -2)), Move(new(0, -3)), Move(new(0, -2)), Move(new(0, -2)), Move(new(0, -3)),
        Move(new(1, -2)), Move(new(1, -2)), Move(new(0, -1)), Move(new(1, -2)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)),
        Move(new(1, -1)), Move(new(1, 0)), Move(new(1, 0)), Move(new(2, 1)), Move(new(1, 1)), Move(new(1, 1)), Move(new(1, 1)), Move(new(2, 1)), Move(new(1, 1)),
        Move(new(1, 1)), Move(new(1, 2)), Move(new(1, 2)), Move(new(0, 2)), Move(new(0, 2)), Move(new(0, 2)), Move(new(0, 2)), Move(new(0, 2)), Move(new(-1, 2)),
        Move(new(-1, 1)), Move(new(-1, 1)), Move(new(-1, 2)), Move(new(-1, 1)), Move(new(-1, 1)), Move(new(-1, 0)), Move(new(-2, 0)), Move(new(-2, -1)),
        Move(new(-2, 0)), Move(new(-1, -1)), Move(new(-1, 0)), Move(new(0, 1)), Move(new(1, 1)), Move(new(1, 2)), Move(new(1, 2)), Move(new(1, 2)), Move(new(1, 1)),
        Move(new(2, 1)), Move(new(1, 1)), Move(new(1, 0)), Move(new(1, 0)), Move(new(2, 0)), Move(new(2, -1)), Move(new(2, 0)), Move(new(1, 0)), Move(new(1, 0)),
        Move(new(1, -1)), Move(new(2, 0)), Move(new(2, 0)), Move(new(1, -2)), Move(new(2, -1)), Move(new(1, -2)), Move(new(2, -2)), Move(new(2, -1)),
        Move(new(1, -2)), Move(new(2, -2)), Move(new(2, -2)), Move(new(2, -2)), Move(new(2, -3)), Move(new(2, -3)), Move(new(1, -3)), Move(new(1, -3)),
        Move(new(1, -3)), Move(new(1, -3)), Move(new(1, -3)), Move(new(0, -3)), Move(new(1, -3)), Move(new(1, -3)), Move(new(0, -2)), Move(new(0, -2)),
        Move(new(0, -2)), Move(new(0, -1)), Move(new(-1, 0)), Move(new(0, 1)), Move(new(-1, 1)), Move(new(-1, 1)), Move(new(-1, 2)), Move(new(-2, 2)),
        Move(new(-1, 2)), Move(new(-1, 3)), Move(new(-1, 3)), Move(new(0, 3)), Move(new(-1, 3)), Move(new(-1, 3)), Move(new(0, 3)), Move(new(0, 3)), Move(new(0, 3)),
        Move(new(0, 3)), Move(new(0, 3)), Move(new(1, 3)), Move(new(1, 2)), Move(new(1, 2)), Move(new(1, 2)), Move(new(2, 1)), Move(new(1, 1)), Move(new(1, 1)),
        Move(new(2, 0)), Move(new(2, 0)), Move(new(2, -1)), Move(new(1, -1)), Move(new(1, -2)), Move(new(1, -2)), Move(new(1, -2)), Move(new(1, -3)),
        Move(new(2, -3)), Move(new(1, -3)), Move(new(1, -3)), Move(new(1, -3)), Move(new(1, -3)), Move(new(1, -3)), Move(new(1, -3)), Move(new(0, -3)),
        Move(new(1, -2)), Move(new(0, -3)), Move(new(-1, -2)), Move(new(-1, -2)), Move(new(-1, -2)), Move(new(-1, -2)), Move(new(-1, -1)), Move(new(-2, -1)),
        Move(new(-2, -1)), Move(new(-3, 0)), Move(new(-3, 1)), Move(new(-2, 1)), Move(new(-2, 1)), Move(new(-1, 2)), Move(new(1, 2)), Move(new(2, 1)),
        Move(new(2, 1)), Move(new(2, 1)), Move(new(3, 0)), Move(new(3, 0)), Move(new(2, 0)), Move(new(2, 0)), Move(new(2, 0)), Move(new(2, 0)), Move(new(1, 0)),
        Move(new(1, 0)), Move(new(1, 0)), Move(new(1, 0)), Move(new(2, 0)), Move(new(1, 0)), Move(new(1, 0)), Move(new(1, 0)), Move(new(1, 0)), Move(new(1, 0)),
        Move(new(1, 0)), Move(new(1, 0)), Move(new(1, 0)), Move(new(1, 0)), Move(new(1, 0)), Move(new(2, 0)), Move(new(1, 0)), Move(new(1, 0)), Move(new(1, 0)),
        Move(new(1, -1)), Move(new(1, 0)), Move(new(1, 0)), Move(new(1, 0)), Move(new(1, 0)), Move(new(1, -1)), Move(new(1, 0)), Move(new(1, 0)), Move(new(1, 0)),
        Move(new(1, -1)), Move(new(1, 0)), Move(new(1, 0)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, 0)), Move(new(1, 0)), Move(new(1, 0)), Move(new(1, 0)),
        Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, 0)),
        Move(new(0, -1)), Move(new(1, 0)), Move(new(1, -1)), Move(new(1, 0)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, 0)), Move(new(1, -1)),
        Move(new(1, -1)), Move(new(1, -1)), Move(new(0, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(0, -1)), Move(new(0, -1)), Move(new(0, -1)),
        Move(new(0, -1)), Move(new(0, -1)), Move(new(0, -1)), Move(new(-1, -1)), Move(new(-1, 0)), Move(new(-1, -1)), Move(new(-1, -1)), Move(new(-1, 0)),
        Move(new(-1, 1)), Move(new(0, 1)), Move(new(-1, 1)), Move(new(-1, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 2)), Move(new(-1, 1)), Move(new(0, 1)),
        Move(new(-1, 2)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(1, 1)), Move(new(0, 1)), Move(new(0, 1)),
        Move(new(0, 1)), Move(new(0, 1)), Move(new(1, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)),
        Move(new(0, 2)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)),
        Move(new(-1, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 2)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)),
        Move(new(0, 1)), Move(new(-1, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)),
        Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)),
        Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 2)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 2)), Move(new(0, 1)),
        Move(new(0, 1)), Move(new(-1, 1)), Move(new(-1, 0)), Move(new(-1, 1)), Move(new(-1, 1)), Move(new(-2, 1)), Move(new(-1, 0)), Move(new(-1, -1)),
        Move(new(-1, 0)), Move(new(-2, 0)), Move(new(-1, 0)), Move(new(-1, -1)), Move(new(0, -1)), Move(new(0, -1)), Move(new(0, -1)), Move(new(0, -1)),
        Move(new(0, -1)), Move(new(0, -1)), Move(new(1, -1)), Move(new(0, -1)), Move(new(1, -1)), Move(new(1, 0)), Move(new(1, -1)), Move(new(1, 0)),
        Move(new(1, 0)), Move(new(1, 0)), Move(new(2, 0)), Move(new(1, 0)), Move(new(1, 0)), Move(new(1, 1)), Move(new(1, 0)), Move(new(1, 1)), Move(new(1, 0)),
        Move(new(1, 0)), Move(new(1, 1)), Move(new(1, 1)), Move(new(1, 1)), Move(new(1, 0)), Move(new(1, 1)), Move(new(1, 1)), Move(new(1, 1)), Move(new(1, 1)),
        Move(new(1, 1)), Move(new(1, 0)), Move(new(0, 1)), Move(new(1, 1)), Move(new(1, 1)), Move(new(1, 1)), Move(new(1, 1)), Move(new(0, 1)), Move(new(1, 0)),
        Move(new(1, 0)), Move(new(1, 0)), Move(new(1, 0)), Move(new(1, 0)), Move(new(1, 0)), Move(new(1, 1)), Move(new(1, 0)), Move(new(1, 0)), Move(new(1, -1)),
        Move(new(2, 0)), Move(new(1, 0)), Move(new(1, 0)), Move(new(0, -1)), Move(new(0, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)),
        Move(new(1, -1)), Move(new(0, -1)), Move(new(0, -1)), Move(new(0, -2)), Move(new(0, -2)), Move(new(0, -1)), Move(new(0, -1)), Move(new(0, -1)),
        Move(new(1, -2)), Move(new(1, -1)), Move(new(0, -1)), Move(new(0, -1)), Move(new(1, -1)), Move(new(0, -1)), Move(new(1, -1)), Move(new(0, -1)),
        Move(new(0, -1)), Move(new(-1, 0)), Move(new(-1, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(-1, 1)), Move(new(-1, 1)), Move(new(-1, 0)),
        Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(-1, 1)), Move(new(0, 1)), Move(new(0, 1)),
        Move(new(0, 2)), Move(new(0, 1)), Move(new(0, 2)), Move(new(0, 1)), Move(new(0, 2)), Move(new(0, 1)), Move(new(1, 1)), Move(new(0, 2)), Move(new(0, 1)),
        Move(new(0, 1)), Move(new(0, 1)), Move(new(1, 1)), Move(new(1, 1)), Move(new(1, 0)), Move(new(1, 0)), Move(new(0, -1)), Move(new(1, -1)), Move(new(0, -1)),
        Move(new(1, 0)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(0, -1)), Move(new(0, -1)), Move(new(0, -1)), Move(new(0, -1)),
        Move(new(1, -2)), Move(new(1, -1)), Move(new(0, -1)), Move(new(0, -1)), Move(new(0, -2)), Move(new(1, -2)), Move(new(0, -2)), Move(new(0, -2)),
        Move(new(0, -1)), Move(new(0, -1)), Move(new(0, -2)), Move(new(0, -1)), Move(new(0, -1)), Move(new(-1, -1)), Move(new(-1, -1)), Move(new(-1, 0)),
        Move(new(-1, 0)), Move(new(-1, 0)), Move(new(-1, 0)), Move(new(-1, 1)), Move(new(-1, 0)), Move(new(-1, 1)), Move(new(-1, 1)), Move(new(-1, 1)),
        Move(new(0, 1)), Move(new(0, 1)), Move(new(1, 1)), Move(new(1, 0)), Move(new(1, 0)), Move(new(1, 0)), Move(new(1, 0)), Move(new(1, 0)), Move(new(1, 0)),
        Move(new(1, 0)), Move(new(1, 0)), Move(new(1, -1)), Move(new(1, 0)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)),
        Move(new(1, -1)), Move(new(0, -1)), Move(new(1, -1)), Move(new(1, 0)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 2)), Move(new(0, 1)),
        Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 2)), Move(new(0, 2)), Move(new(-1, 2)), Move(new(0, 2)), Move(new(0, 2)), Move(new(0, 1)), Move(new(0, 1)),
        Move(new(0, 1)), Move(new(0, 1)), Move(new(-1, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(1, 1)), Move(new(0, 1)),
        Move(new(1, 0)), Move(new(1, 0)), Move(new(1, 0)), Move(new(1, 0)), Move(new(1, 0)), Move(new(1, 0)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)),
        Move(new(0, -1)), Move(new(1, -1)), Move(new(1, -2)), Move(new(1, -1)), Move(new(0, -2)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -2)),
        Move(new(0, -1)), Move(new(0, -1)), Move(new(0, -1)), Move(new(1, -2)), Move(new(0, -1)), Move(new(0, -2)), Move(new(0, -1)), Move(new(0, -1)),
        Move(new(0, -1)), Move(new(1, -1)), Move(new(0, -1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)),
        Move(new(0, 1)), Move(new(1, 1)), Move(new(0, 1)), Move(new(1, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(1, 1)), Move(new(1, 1)), Move(new(0, 1)),
        Move(new(1, 1)), Move(new(0, 1)), Move(new(1, 1)), Move(new(1, 0)), Move(new(1, 1)), Move(new(1, 1)), Move(new(1, 0)), Move(new(1, 1)), Move(new(1, 1)),
        Move(new(1, 1)), Move(new(1, 1)), Move(new(1, 1)), Move(new(1, 0)), Move(new(1, 1)), Move(new(1, 0)), Move(new(1, 1)), Move(new(1, 0)), Move(new(1, 0)),
        Move(new(1, 0)), Move(new(1, 0)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)),
        Move(new(0, -1)), Move(new(0, -1)), Move(new(0, -1)), Move(new(1, -1)), Move(new(0, -1)), Move(new(1, -1)), Move(new(0, -1)), Move(new(0, -1)),
        Move(new(0, -1)), Move(new(0, -1)), Move(new(0, -1)), Move(new(0, -1)), Move(new(-1, -1)), Move(new(0, -2)), Move(new(0, -1)), Move(new(0, -2)),
        Move(new(0, -1)), Move(new(0, -1)), Move(new(-1, -1)), Move(new(-1, -1)), Move(new(0, -1)), Move(new(0, -1)), Move(new(-1, -1)), Move(new(-1, 0)),
        Move(new(-1, 0)), Move(new(-1, 0)), Move(new(-1, 0)), Move(new(0, 1)), Move(new(-1, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(-1, 1)),
        Move(new(-1, 1)), Move(new(-1, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(-1, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 2)), Move(new(0, 1)),
        Move(new(0, 2)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 2)), Move(new(0, 1)), Move(new(0, 1)), Move(new(0, 1)), Move(new(1, 1)),
        Move(new(1, 1)), Move(new(1, 1)), Move(new(0, 1)), Delete, Delete, Move(new(1, -1)), Move(new(1, -1)), Move(new(1, -1)), Move(new(2, -1)),
        Move(new(2, -1)), Move(new(1, -1)), Move(new(1, -2)), Move(new(2, -2)), Move(new(1, -1)), Move(new(1, -2)), Move(new(1, -2)), Move(new(1, -2)),
        Move(new(1, -3)), Move(new(1, -2)), Move(new(1, -2)), Move(new(1, -2)), Move(new(1, -3)), Move(new(1, -2)), Move(new(1, -3)), Move(new(1, -3)),
        Move(new(1, -3)), Move(new(1, -3)), Move(new(0, -3)), Move(new(1, -2)), Move(new(0, -1)), Move(new(-1, -1)), Move(new(-1, -1)), Move(new(-1, 0)),
        Move(new(-1, 1)), Move(new(-1, 1)), Move(new(-1, 2)), Move(new(-2, 2)), Move(new(-2, 3)), Move(new(-2, 2)), Move(new(-2, 3)), Move(new(-2, 2)),
        Move(new(-1, 3)), Move(new(-1, 4)), Move(new(-1, 3)), Move(new(-1, 3)), Move(new(0, 3)), Move(new(0, 3)), Move(new(0, 3)), Move(new(0, 3)), Move(new(1, 3)),
        Move(new(0, 2)), Move(new(1, 3)), Move(new(1, 2)), Move(new(1, 2)), Move(new(1, 2)), Move(new(2, 1)), Move(new(2, 1)), Move(new(2, 1)), Move(new(2, 0)),
        Move(new(1, 0)), Move(new(2, -1)), Move(new(1, -1)), Move(new(2, -1)), Move(new(1, -1)), Move(new(2, -1)), Move(new(2, -2)), Move(new(2, -2)),
        Move(new(2, -2)), Move(new(1, -3)), Move(new(1, -2)), Move(new(1, -3)), Move(new(1, -3)), Move(new(1, -3)), Move(new(0, -4)), Move(new(0, -3)),
        Move(new(0, -3)), Move(new(0, -3)), Move(new(0, -3)), Move(new(-1, -3)), Move(new(0, -2)), Move(new(-1, -2)), Move(new(-2, -2)), Move(new(-1, -2)),
        Move(new(-2, -2)), Move(new(-1, -1)), Move(new(-1, -1)), Move(new(-1, 0)), Move(new(-1, 1)), Move(new(-2, 1)), Move(new(-2, 2)), Move(new(-1, 2)),
        Move(new(-1, 3)), Move(new(0, 3)), Move(new(-1, 2)), Move(new(0, 2)), Move(new(0, 3)), Move(new(1, 2)), Move(new(1, 2)), Move(new(2, 2)), Move(new(1, 2)),
        Move(new(2, 2)),
    ];
}
