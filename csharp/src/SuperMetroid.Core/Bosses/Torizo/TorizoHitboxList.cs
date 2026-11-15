using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// One ordered list of a Torizo body's touch and shot rectangles, shared by every frame that names it
/// (<see cref="TorizoCollisionDefinitions"/>).
/// </summary>
internal sealed class TorizoHitboxList
{
    /// <summary>Creates a list of rectangles.</summary>
    /// <param name="rectangles">Rectangles in test order; the first one a probe overlaps decides the reaction.</param>
    internal TorizoHitboxList(EnemyCollisionHitbox[] rectangles) => Rectangles = Ensure.NotNull(rectangles);

    /// <summary>Rectangles in test order, about Torizo's origin; empty for a frame without collision.</summary>
    internal IReadOnlyList<EnemyCollisionHitbox> Rectangles { get; }
}
