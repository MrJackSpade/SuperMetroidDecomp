namespace SuperMetroid.Core.Bosses.Botwoon;
/// <summary>Compiled fixed geometry and path metadata for Botwoon's room navigation.</summary>
internal static class BotwoonNavigationDefinitions
{

    /// <summary>
    /// The random selection of one of eight actions when Botwoon leaves a hole outside its health
    /// phases; the first <see cref="AuthoredPathActions"/> follow an authored path and the rest spit from the hole.
    /// </summary>
    internal static readonly RandomSampleField ActionChoice = new(1, 3);

    /// <summary>How many of the <see cref="ActionChoice"/> actions, counted from the first, follow an authored path.</summary>
    internal const int AuthoredPathActions = 3;

    /// <summary>Whether Botwoon starts its next path inside the hole: taken by an odd sample.</summary>
    internal static readonly RandomSampleField StartsInsideHole = new(0, 1);

    /// <summary>The random draw of one of the four destinations a hole's walks lead to.</summary>
    internal static readonly RandomSampleField PathDraw = new(3, 2);

    /// <summary>
    /// The hole each of the four draws leads to from a hole when Botwoon walks in the open: the other three holes in
    /// turn, then back into the hole he left.
    /// </summary>
    internal static readonly Dictionary<BotwoonHole, BotwoonHole[]> OpenDestinations = new()
    {
        [BotwoonHole.Left] = [BotwoonHole.Bottom, BotwoonHole.Top, BotwoonHole.Right, BotwoonHole.Left],
        [BotwoonHole.Bottom] = [BotwoonHole.Left, BotwoonHole.Top, BotwoonHole.Right, BotwoonHole.Bottom],
        [BotwoonHole.Top] = [BotwoonHole.Left, BotwoonHole.Bottom, BotwoonHole.Right, BotwoonHole.Top],
        [BotwoonHole.Right] = [BotwoonHole.Left, BotwoonHole.Bottom, BotwoonHole.Top, BotwoonHole.Right],
    };

    /// <summary>
    /// The hole each of the four draws leads to from a hole when Botwoon travels inside the wall: the other three holes
    /// in turn, the last of them drawn twice, since no wall path leads back into the hole he left.
    /// </summary>
    internal static readonly Dictionary<BotwoonHole, BotwoonHole[]> WallDestinations = new()
    {
        [BotwoonHole.Left] = [BotwoonHole.Bottom, BotwoonHole.Top, BotwoonHole.Right, BotwoonHole.Right],
        [BotwoonHole.Bottom] = [BotwoonHole.Left, BotwoonHole.Top, BotwoonHole.Right, BotwoonHole.Right],
        [BotwoonHole.Top] = [BotwoonHole.Left, BotwoonHole.Bottom, BotwoonHole.Right, BotwoonHole.Right],
        [BotwoonHole.Right] = [BotwoonHole.Left, BotwoonHole.Bottom, BotwoonHole.Top, BotwoonHole.Top],
    };

    /// <summary>
    /// The four room holes in the order the game tests the head against them, last first: left (centre 64,112), bottom
    /// (128,176), top (160,96) and right (224,144).
    /// </summary>
    internal static readonly BotwoonHoleDefinition[] Holes =
    [
        new(BotwoonHole.Left, new(64 - 4, 112 - 4)),
        new(BotwoonHole.Bottom, new(128 - 4, 176 - 4)),
        new(BotwoonHole.Top, new(160 - 4, 96 - 4)),
        new(BotwoonHole.Right, new(224 - 4, 144 - 4)),
    ];

    /// <summary>
    /// How many steps before an authored path's end a backward walk starts. The walk begins on the path's
    /// second-to-last step, so it never takes the last one, as the game's backward walks do.
    /// </summary>
    internal const int BackwardStartStepsBeforeEnd = 2;
}
