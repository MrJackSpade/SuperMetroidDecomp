using System.Collections.Immutable;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// One thing a boss draws on its body layer at once (#1466): a body pose, a part of one, a melt image or a head, as the
/// parts its installed sheet split it into. Drawing it places every part, each over the cells it covers.
/// </summary>
/// <param name="Name">The frame's name in its sheet.</param>
/// <param name="Parts">Its parts: its still cells, and a part for the cells of each animation they show.</param>
public sealed record BossBodyFrame(string Name, ImmutableArray<BossBodyPart> Parts);
