using System.Collections.Immutable;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// A boss body as a gameplay frame shows it (#1466, #1514, #1540, #1609): its parts' pre-coloured pictures, each placed on
/// the body's layer, which the view's scroll places on screen; nothing repeats beyond the parts. Each part is drawn over
/// the parts before it, and a pixel its picture leaves clear shows them; a pixel no part inks draws nothing (#1513). The
/// frontmost part inking a pixel shows it at that part's depth (#1634): behind the sprites, or in front of them.
/// </summary>
/// <param name="Parts">The parts showing, the first drawn beneath the others.</param>
public sealed record ShownBossBody(ImmutableArray<ShownBossBodyPart> Parts);
