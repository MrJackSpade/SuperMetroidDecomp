namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How a long Puyo fall starts over (#2282): a falling Puyo's place along the speed curve grows by its hop's step each
/// update, and a fall that carries it to 128 marks starts again from the curve's start, so a Puyo that falls far enough
/// slows to its slowest fall and speeds up again.
/// </summary>
/// <remarks>
/// Consulted on every airborne update of a falling Puyo's hop or bounce, after its moves, as its place steps along the
/// curve. Evidence: the Puyo's hop keeps its place as a sixteen-bit count of 256ths of a mark and tests the stepped count
/// by its sign, which turns at 128 marks. Its fall speed is read no further along than mark 64, eight pixels an update, so
/// from its apex a fall reaches 128 marks after 43 updates and 231 pixels at the long hop's three marks an update, 64
/// updates and 342 pixels at two, and 74 updates and 397 pixels at a dropped bounce's 1.75. Retail rooms place Puyos on
/// ledges hundreds of pixels above their floors (one Maridia room lists Puyos from 412 down to 1008 pixels), so a Puyo that
/// hops off its ledge falls that far.
/// </remarks>
internal static partial class PuyoFallRestart
{
    /// <summary>The whole marks along the curve at which a fall starts over.</summary>
    private const int RestartMarks = 128;

    /// <summary>
    /// Whether a fall stepped on to a place <paramref name="steppedWholeMarks"/> whole marks along the speed curve starts
    /// again from the curve's start.
    /// </summary>
    /// <param name="steppedWholeMarks">The whole marks of the place the fall's step reaches.</param>
    internal static bool StartsOver(int steppedWholeMarks) => steppedWholeMarks >= RestartMarks;
}
