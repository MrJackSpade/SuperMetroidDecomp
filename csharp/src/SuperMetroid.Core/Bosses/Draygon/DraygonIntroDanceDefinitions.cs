using static SuperMetroid.Core.DefinitionRun;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Timing and the traced path of Draygon's opening Evir dance.</summary>
internal static class DraygonIntroDanceDefinitions
{
    /// <summary>The Evirs the opening dance spawns and moves.</summary>
    internal const int EvirCount = 4;

    /// <summary>How many updates the dance lasts before Draygon starts its first swoop.</summary>
    internal const int DurationFrames = 1232;

    /// <summary>
    /// How many dance updates each Evir waits before it takes the path's first step, in the order the Evirs spawn. Even
    /// the first Evir waits, so no Evir starts at once.
    /// </summary>
    internal static readonly int[] StartDelays = [128, 160, 192, 224];

    /// <summary>
    /// The handwritten Keiko Love path every dancing Evir follows, one move per update, right and down positive. The update
    /// after its last move ends the Evir's dance, which every Evir reaches before the dance ends.
    /// </summary>
    /// <remarks>Adding up the path's steps traces hidden lettering, so the path is kept as drawn: fitted splines,
    /// per-stroke corrections or a numeric switch would encode the same particular letter shapes and pen timing. This is
    /// not a sampled physical trajectory.</remarks>
    internal static readonly Offset16<RoomPixels>[] TracedPath =
    [
        .. Repeated<Offset16<RoomPixels>>(new(3, 0), 4), new(3, -1), new(2, -1), new(2, -2), new(2, -2), new(2, -3),
        new(1, -3), new(1, -2), new(1, -2), new(1, 0), new(1, 0), new(0, 1),
        .. Repeated<Offset16<RoomPixels>>(new(0, 2), 3), new(-1, 3), new(0, 3), new(0, 3), new(0, 4), new(0, 4),
        new(-1, 4), new(-1, 4), new(0, 5), new(-1, 4), new(-1, 4), new(-1, 5), new(-1, 4), new(-1, 4),
        .. Repeated<Offset16<RoomPixels>>(new(-1, 3), 4), new(-1, 2), new(0, 1), new(0, 1), new(-1, 0),
        .. Repeated<Offset16<RoomPixels>>(new(0, -1), 3), new(0, -2), new(0, -3), new(1, -3), new(0, -3), new(0, -4),
        new(1, -4), new(1, -4), new(0, -4), .. Repeated<Offset16<RoomPixels>>(new(1, -3), 3), new(2, -3), new(1, -3),
        new(1, -3), new(2, -2), new(1, -3), new(2, -2), new(1, -2), new(2, -2), new(2, -1),
        .. Repeated<Offset16<RoomPixels>>(new(3, -2), 6), new(3, -1), new(2, -1), new(2, -1),
        .. Repeated<Offset16<RoomPixels>>(new(2, 0), 3), new(1, 1), new(0, 2), new(0, 2), new(0, 3), new(-1, 3),
        new(-1, 3), .. Repeated<Offset16<RoomPixels>>(new(-2, 3), 5), new(-2, 2), new(-3, 2),
        .. Repeated<Offset16<RoomPixels>>(new(-2, 1), 3), new(-3, 1), new(-3, 1), new(-2, 0),
        .. Repeated<Offset16<RoomPixels>>(new(-3, -1), 4), new(-2, -2), new(-1, -2), new(1, -2), new(2, -2), new(3, -2),
        new(4, -1), .. Repeated<Offset16<RoomPixels>>(new(3, 0), 3), .. Repeated<Offset16<RoomPixels>>(new(2, 2), 4),
        .. Repeated<Offset16<RoomPixels>>(new(1, 3), 3), new(2, 3), new(1, 2),
        .. Repeated<Offset16<RoomPixels>>(new(1, 3), 3), new(1, 2), .. Repeated<Offset16<RoomPixels>>(new(2, 2), 4),
        new(2, 1), new(2, 1), new(3, 1), new(3, 1), new(3, 0), new(3, 1),
        .. Repeated<Offset16<RoomPixels>>(new(2, 0), 3), new(1, 0), .. Repeated<Offset16<RoomPixels>>(new(1, -1), 3),
        new(2, 0), new(1, -1), new(1, -1), new(2, -1), new(1, -1), new(2, -1),
        .. Repeated<Offset16<RoomPixels>>(new(1, -1), 6), .. Repeated<Offset16<RoomPixels>>(new(1, -2), 4), new(0, -2),
        new(0, -2), new(1, -2), new(0, -1), new(0, -1), new(0, -2), new(0, -2), new(-1, -2), new(0, -1),
        .. Repeated<Offset16<RoomPixels>>(new(-1, -2), 3), new(-1, -1), new(-1, -1), new(-2, -1), new(-1, 0),
        new(-1, 0), .. Repeated<Offset16<RoomPixels>>(new(-2, 1), 3), .. Repeated<Offset16<RoomPixels>>(new(-1, 1), 3),
        new(-1, 2), new(-1, 1), new(-1, 1), new(0, 1), new(0, 2), new(-1, 2),
        .. Repeated<Offset16<RoomPixels>>(new(0, 2), 3), new(0, 1), new(0, 1), new(0, 2), new(0, 2), new(0, 1),
        new(0, 2), new(0, 2), new(1, 1), new(0, 1), new(0, 1), new(0, 2), new(1, 1), new(1, 1), new(2, 0), new(1, 1),
        .. Repeated<Offset16<RoomPixels>>(new(2, 1), 4), new(2, 0), new(1, 0), new(1, 0),
        .. Repeated<Offset16<RoomPixels>>(new(2, 0), 5), new(1, 0), new(1, 0), new(1, -1), new(2, -1), new(1, 0),
        new(1, 0), new(1, -1), new(2, -1), .. Repeated<Offset16<RoomPixels>>(new(1, -1), 5), new(1, -2), new(1, -1),
        new(1, -2), .. Repeated<Offset16<RoomPixels>>(new(0, -2), 3), new(0, -1), new(0, -2),
        .. Repeated<Offset16<RoomPixels>>(new(1, -2), 7), new(0, -2), new(0, -2), new(0, -1), new(0, -1), new(1, -1),
        new(0, -2), new(0, -1), new(1, -1), new(0, -1), .. Repeated<Offset16<RoomPixels>>(new(0, 1), 4),
        .. Repeated<Offset16<RoomPixels>>(new(0, 2), 3), new(0, 1), new(0, 2), new(0, 2), new(0, 1),
        .. Repeated<Offset16<RoomPixels>>(new(0, 2), 4), new(0, 1), new(0, 1), new(0, 2), new(0, 1), new(0, 1),
        new(0, 2), new(0, 1), new(0, 2), new(0, 1), new(0, 1), new(1, 1), new(0, 1), new(0, 2), new(0, 1), new(1, 2),
        new(0, 1), .. Repeated<Offset16<RoomPixels>>(new(1, 1), 3), new(1, 0), new(1, 1), new(1, 1), new(1, 0),
        new(2, 0), new(2, 0), new(1, 0), .. Repeated<Offset16<RoomPixels>>(new(1, -1), 19), new(0, -1), new(1, -1),
        new(1, -1), new(0, -1), new(0, -1), .. Repeated<Offset16<RoomPixels>>(new(1, -1), 3), new(1, -2), new(0, -2),
        new(0, -1), new(0, -1), new(1, -2), new(1, -2), new(0, -2), new(1, -2),
        .. Repeated<Offset16<RoomPixels>>(new(0, -2), 11), new(0, -1), new(0, -1), new(-1, -1),
        .. Repeated<Offset16<RoomPixels>>(new(0, -1), 3), new(-1, 0), new(0, 1), new(0, 1), new(-1, 1), new(-1, 1),
        new(0, 2), new(-1, 2), new(-1, 2), new(-1, 1), new(0, 2), new(-1, 1), new(-1, 2), new(-1, 1),
        .. Repeated<Offset16<RoomPixels>>(new(0, 2), 3), .. Repeated<Offset16<RoomPixels>>(new(0, 1), 3), new(-1, 1),
        new(0, 1), new(0, 2), new(0, 2), new(0, 1), new(0, 1), new(0, 2), new(0, 1), new(0, 1), new(1, 1), new(0, 1),
        new(0, 2), new(0, 1), new(0, 1), new(0, 2), new(0, 1), new(0, 1), new(0, 2),
        .. Repeated<Offset16<RoomPixels>>(new(0, 1), 3), .. Repeated<Offset16<RoomPixels>>(new(0, 2), 5), new(0, 1),
        new(0, 2), new(-1, 2), new(0, 1), new(-1, 1), new(0, 1), new(-1, 0), new(0, -1), new(0, -1),
        .. Repeated<Offset16<RoomPixels>>(new(0, -2), 5), new(1, -2), new(0, -3), new(0, -2), new(0, -2), new(0, -3),
        new(1, -2), new(1, -2), new(0, -1), new(1, -2), .. Repeated<Offset16<RoomPixels>>(new(1, -1), 5), new(1, 0),
        new(1, 0), new(2, 1), .. Repeated<Offset16<RoomPixels>>(new(1, 1), 3), new(2, 1), new(1, 1), new(1, 1),
        new(1, 2), new(1, 2), .. Repeated<Offset16<RoomPixels>>(new(0, 2), 5), new(-1, 2), new(-1, 1), new(-1, 1),
        new(-1, 2), new(-1, 1), new(-1, 1), new(-1, 0), new(-2, 0), new(-2, -1), new(-2, 0), new(-1, -1), new(-1, 0),
        new(0, 1), new(1, 1), .. Repeated<Offset16<RoomPixels>>(new(1, 2), 3), new(1, 1), new(2, 1), new(1, 1),
        new(1, 0), new(1, 0), new(2, 0), new(2, -1), new(2, 0), new(1, 0), new(1, 0), new(1, -1), new(2, 0), new(2, 0),
        new(1, -2), new(2, -1), new(1, -2), new(2, -2), new(2, -1), new(1, -2),
        .. Repeated<Offset16<RoomPixels>>(new(2, -2), 3), new(2, -3), new(2, -3),
        .. Repeated<Offset16<RoomPixels>>(new(1, -3), 5), new(0, -3), new(1, -3), new(1, -3),
        .. Repeated<Offset16<RoomPixels>>(new(0, -2), 3), new(0, -1), new(-1, 0), new(0, 1), new(-1, 1), new(-1, 1),
        new(-1, 2), new(-2, 2), new(-1, 2), new(-1, 3), new(-1, 3), new(0, 3), new(-1, 3), new(-1, 3),
        .. Repeated<Offset16<RoomPixels>>(new(0, 3), 5), new(1, 3), .. Repeated<Offset16<RoomPixels>>(new(1, 2), 3),
        new(2, 1), new(1, 1), new(1, 1), new(2, 0), new(2, 0), new(2, -1), new(1, -1),
        .. Repeated<Offset16<RoomPixels>>(new(1, -2), 3), new(1, -3), new(2, -3),
        .. Repeated<Offset16<RoomPixels>>(new(1, -3), 6), new(0, -3), new(1, -2), new(0, -3),
        .. Repeated<Offset16<RoomPixels>>(new(-1, -2), 4), new(-1, -1), new(-2, -1), new(-2, -1), new(-3, 0),
        new(-3, 1), new(-2, 1), new(-2, 1), new(-1, 2), new(1, 2), .. Repeated<Offset16<RoomPixels>>(new(2, 1), 3),
        new(3, 0), new(3, 0), .. Repeated<Offset16<RoomPixels>>(new(2, 0), 4),
        .. Repeated<Offset16<RoomPixels>>(new(1, 0), 4), new(2, 0), .. Repeated<Offset16<RoomPixels>>(new(1, 0), 10),
        new(2, 0), .. Repeated<Offset16<RoomPixels>>(new(1, 0), 3), new(1, -1),
        .. Repeated<Offset16<RoomPixels>>(new(1, 0), 4), new(1, -1), .. Repeated<Offset16<RoomPixels>>(new(1, 0), 3),
        new(1, -1), new(1, 0), new(1, 0), new(1, -1), new(1, -1), .. Repeated<Offset16<RoomPixels>>(new(1, 0), 4),
        .. Repeated<Offset16<RoomPixels>>(new(1, -1), 7), new(1, 0), new(0, -1), new(1, 0), new(1, -1), new(1, 0),
        new(1, -1), new(1, -1), new(1, 0), .. Repeated<Offset16<RoomPixels>>(new(1, -1), 3), new(0, -1), new(1, -1),
        new(1, -1), .. Repeated<Offset16<RoomPixels>>(new(0, -1), 6), new(-1, -1), new(-1, 0), new(-1, -1), new(-1, -1),
        new(-1, 0), new(-1, 1), new(0, 1), new(-1, 1), new(-1, 1), new(0, 1), new(0, 1), new(0, 2), new(-1, 1),
        new(0, 1), new(-1, 2), .. Repeated<Offset16<RoomPixels>>(new(0, 1), 5), new(1, 1),
        .. Repeated<Offset16<RoomPixels>>(new(0, 1), 4), new(1, 1), .. Repeated<Offset16<RoomPixels>>(new(0, 1), 6),
        new(0, 2), .. Repeated<Offset16<RoomPixels>>(new(0, 1), 8), new(-1, 1),
        .. Repeated<Offset16<RoomPixels>>(new(0, 1), 3), new(0, 2), .. Repeated<Offset16<RoomPixels>>(new(0, 1), 5),
        new(-1, 1), .. Repeated<Offset16<RoomPixels>>(new(0, 1), 20), new(0, 2), new(0, 1), new(0, 1), new(0, 2),
        new(0, 1), new(0, 1), new(-1, 1), new(-1, 0), new(-1, 1), new(-1, 1), new(-2, 1), new(-1, 0), new(-1, -1),
        new(-1, 0), new(-2, 0), new(-1, 0), new(-1, -1), .. Repeated<Offset16<RoomPixels>>(new(0, -1), 6), new(1, -1),
        new(0, -1), new(1, -1), new(1, 0), new(1, -1), .. Repeated<Offset16<RoomPixels>>(new(1, 0), 3), new(2, 0),
        new(1, 0), new(1, 0), new(1, 1), new(1, 0), new(1, 1), new(1, 0), new(1, 0),
        .. Repeated<Offset16<RoomPixels>>(new(1, 1), 3), new(1, 0), .. Repeated<Offset16<RoomPixels>>(new(1, 1), 5),
        new(1, 0), new(0, 1), .. Repeated<Offset16<RoomPixels>>(new(1, 1), 4), new(0, 1),
        .. Repeated<Offset16<RoomPixels>>(new(1, 0), 6), new(1, 1), new(1, 0), new(1, 0), new(1, -1), new(2, 0),
        new(1, 0), new(1, 0), new(0, -1), new(0, -1), .. Repeated<Offset16<RoomPixels>>(new(1, -1), 4), new(0, -1),
        new(0, -1), new(0, -2), new(0, -2), .. Repeated<Offset16<RoomPixels>>(new(0, -1), 3), new(1, -2), new(1, -1),
        new(0, -1), new(0, -1), new(1, -1), new(0, -1), new(1, -1), new(0, -1), new(0, -1), new(-1, 0), new(-1, 1),
        new(0, 1), new(0, 1), new(-1, 1), new(-1, 1), new(-1, 0), .. Repeated<Offset16<RoomPixels>>(new(0, 1), 6),
        new(-1, 1), new(0, 1), new(0, 1), new(0, 2), new(0, 1), new(0, 2), new(0, 1), new(0, 2), new(0, 1), new(1, 1),
        new(0, 2), .. Repeated<Offset16<RoomPixels>>(new(0, 1), 3), new(1, 1), new(1, 1), new(1, 0), new(1, 0),
        new(0, -1), new(1, -1), new(0, -1), new(1, 0), .. Repeated<Offset16<RoomPixels>>(new(1, -1), 3),
        .. Repeated<Offset16<RoomPixels>>(new(0, -1), 4), new(1, -2), new(1, -1), new(0, -1), new(0, -1), new(0, -2),
        new(1, -2), new(0, -2), new(0, -2), new(0, -1), new(0, -1), new(0, -2), new(0, -1), new(0, -1), new(-1, -1),
        new(-1, -1), .. Repeated<Offset16<RoomPixels>>(new(-1, 0), 4), new(-1, 1), new(-1, 0),
        .. Repeated<Offset16<RoomPixels>>(new(-1, 1), 3), new(0, 1), new(0, 1), new(1, 1),
        .. Repeated<Offset16<RoomPixels>>(new(1, 0), 8), new(1, -1), new(1, 0),
        .. Repeated<Offset16<RoomPixels>>(new(1, -1), 5), new(0, -1), new(1, -1), new(1, 0),
        .. Repeated<Offset16<RoomPixels>>(new(0, 1), 3), new(0, 2), .. Repeated<Offset16<RoomPixels>>(new(0, 1), 3),
        new(0, 2), new(0, 2), new(-1, 2), new(0, 2), new(0, 2), .. Repeated<Offset16<RoomPixels>>(new(0, 1), 4),
        new(-1, 1), .. Repeated<Offset16<RoomPixels>>(new(0, 1), 4), new(1, 1), new(0, 1),
        .. Repeated<Offset16<RoomPixels>>(new(1, 0), 6), .. Repeated<Offset16<RoomPixels>>(new(1, -1), 3), new(0, -1),
        new(1, -1), new(1, -2), new(1, -1), new(0, -2), new(1, -1), new(1, -1), new(1, -2),
        .. Repeated<Offset16<RoomPixels>>(new(0, -1), 3), new(1, -2), new(0, -1), new(0, -2),
        .. Repeated<Offset16<RoomPixels>>(new(0, -1), 3), new(1, -1), new(0, -1),
        .. Repeated<Offset16<RoomPixels>>(new(0, 1), 7), new(1, 1), new(0, 1), new(1, 1), new(0, 1), new(0, 1),
        new(1, 1), new(1, 1), new(0, 1), new(1, 1), new(0, 1), new(1, 1), new(1, 0), new(1, 1), new(1, 1), new(1, 0),
        .. Repeated<Offset16<RoomPixels>>(new(1, 1), 5), new(1, 0), new(1, 1), new(1, 0), new(1, 1),
        .. Repeated<Offset16<RoomPixels>>(new(1, 0), 4), .. Repeated<Offset16<RoomPixels>>(new(1, -1), 6),
        .. Repeated<Offset16<RoomPixels>>(new(0, -1), 3), new(1, -1), new(0, -1), new(1, -1),
        .. Repeated<Offset16<RoomPixels>>(new(0, -1), 6), new(-1, -1), new(0, -2), new(0, -1), new(0, -2), new(0, -1),
        new(0, -1), new(-1, -1), new(-1, -1), new(0, -1), new(0, -1), new(-1, -1),
        .. Repeated<Offset16<RoomPixels>>(new(-1, 0), 4), new(0, 1), new(-1, 1),
        .. Repeated<Offset16<RoomPixels>>(new(0, 1), 3), .. Repeated<Offset16<RoomPixels>>(new(-1, 1), 3), new(0, 1),
        new(0, 1), new(-1, 1), new(0, 1), new(0, 1), new(0, 2), new(0, 1), new(0, 2),
        .. Repeated<Offset16<RoomPixels>>(new(0, 1), 3), new(0, 2), .. Repeated<Offset16<RoomPixels>>(new(0, 1), 3),
        .. Repeated<Offset16<RoomPixels>>(new(1, 1), 3), new(0, 1),
    ];
}
