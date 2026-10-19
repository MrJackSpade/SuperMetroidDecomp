using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Compiled collision geometry, independent of Kraid's editable head artwork.</summary>
internal static class KraidMouthHitboxDefinitions
{

    /// <summary>The rectangle of <paramref name="shape"/>, relative to Kraid's body position.</summary>
    public static (short Left, short Top, short Right, short Bottom) Resolve(KraidMouthHitbox shape) =>
        shape switch
        {
            KraidMouthHitbox.ClosedVulnerable => (16, -120, 40, -88),
            KraidMouthHitbox.OpeningVulnerable => (16, -120, 40, -104),
            KraidMouthHitbox.WiderVulnerable => (16, -128, 40, -112),
            KraidMouthHitbox.OpenVulnerable => (16, -128, 40, -120),
            KraidMouthHitbox.EmptyShape => (0, 0, 0, 0),
            KraidMouthHitbox.OpeningInvulnerable => (6, -96, 32, -80),
            KraidMouthHitbox.WiderInvulnerable => (0, -104, 32, -80),
            KraidMouthHitbox.OpenInvulnerable => (0, -112, 32, -80),
            _ => throw new InvalidOperationException($"Undefined Kraid mouth hitbox {shape}."),
        };

    /// <summary>The left, top and bottom edges collision tests against for <paramref name="shape"/>; the right edge is unused.</summary>
    public static (short Left, short Top, short Bottom) ResolveCollision(KraidMouthHitbox shape)
    {
        (short left, short top, _, short bottom) = Resolve(shape);
        return (left, top, bottom);
    }
}
