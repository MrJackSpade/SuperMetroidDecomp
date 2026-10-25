using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Compiled collision geometry, independent of Kraid's editable head artwork.</summary>
internal static class KraidMouthHitboxDefinitions
{

    /// <summary>The rectangle of <paramref name="shape"/>, relative to Kraid's body position.</summary>
    public static EnemyHitboxRectangle Resolve(KraidMouthHitbox shape) =>
        shape switch
        {
            KraidMouthHitbox.ClosedVulnerable => new(16, -120, 40, -88),
            KraidMouthHitbox.OpeningVulnerable => new(16, -120, 40, -104),
            KraidMouthHitbox.WiderVulnerable => new(16, -128, 40, -112),
            KraidMouthHitbox.OpenVulnerable => new(16, -128, 40, -120),
            KraidMouthHitbox.EmptyShape => new(0, 0, 0, 0),
            KraidMouthHitbox.OpeningInvulnerable => new(6, -96, 32, -80),
            KraidMouthHitbox.WiderInvulnerable => new(0, -104, 32, -80),
            KraidMouthHitbox.OpenInvulnerable => new(0, -112, 32, -80),
            _ => throw new InvalidOperationException($"Undefined Kraid mouth hitbox {shape}."),
        };

    /// <summary>The left, top and bottom edges collision tests against for <paramref name="shape"/>; the right edge is unused.</summary>
    public static KraidMouthCollisionEdges ResolveCollision(KraidMouthHitbox shape)
    {
        EnemyHitboxRectangle rectangle = Resolve(shape);
        return new(rectangle.Left, rectangle.Top, rectangle.Bottom);
    }
}
