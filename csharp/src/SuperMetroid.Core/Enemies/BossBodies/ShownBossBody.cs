using System.Collections.Immutable;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// A boss body as a gameplay frame shows it (#1466): pre-coloured pictures placed on the body layer, whose pages the
/// layer's arrangement lays out and its view position places on screen. Every cell of its rows shows exactly one part.
/// </summary>
/// <param name="Rows">The layer's rows, in pages of thirty-two.</param>
/// <param name="Parts">The parts showing and the cells each shows.</param>
public sealed record ShownBossBody(int Rows, ImmutableArray<ShownBossBodyPart> Parts);
