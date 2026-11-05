using System.Collections.Immutable;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// A boss body as a gameplay frame shows it (#1466): pre-coloured pictures placed on the body layer, whose pages of
/// 256 pixels lie left to right and then downward across <see cref="Area"/>, which repeats beyond its edges; the view
/// position places the area on screen. Every cell of its rows shows at most one part, and a cell no part shows draws
/// nothing (#1513).
/// </summary>
/// <param name="Rows">The layer's rows, in pages of thirty-two.</param>
/// <param name="Parts">The parts showing and the cells each shows.</param>
/// <param name="Area">The area the body's pages cover before it repeats (#1514), in whole pages.</param>
public sealed record ShownBossBody(int Rows, ImmutableArray<ShownBossBodyPart> Parts, Extent16<BackgroundPixels> Area);
