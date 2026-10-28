using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Reads the rectangle of Kraid's mouth shape and the edges its collision tests.</summary>
internal static class KraidMouthHitboxes
{
    /// <summary>The rectangle of <paramref name="shape"/>, relative to Kraid's body position.</summary>
    public static EnemyHitboxRectangle Resolve(KraidMouthHitbox shape) =>
        KraidMouthHitboxDefinitions.Rectangles.TryGetValue(shape, out EnemyHitboxRectangle rectangle)
            ? rectangle
            : throw new InvalidOperationException($"Undefined Kraid mouth hitbox {shape}.");

    /// <summary>The left, top and bottom edges collision tests against for <paramref name="shape"/>; the right edge is unused.</summary>
    public static KraidMouthCollisionEdges ResolveCollision(KraidMouthHitbox shape)
    {
        EnemyHitboxRectangle rectangle = Resolve(shape);
        return new(rectangle.Left, rectangle.Top, rectangle.Bottom);
    }
}
