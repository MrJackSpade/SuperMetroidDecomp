using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Compiled collision geometry, independent of Kraid's editable head artwork.</summary>
internal static class KraidMouthHitboxDefinitions
{
    /// <summary>The rectangle of each mouth shape, relative to Kraid's body position.</summary>
    internal static readonly Dictionary<KraidMouthHitbox, EnemyHitboxRectangle> Rectangles = new()
    {
        [KraidMouthHitbox.ClosedVulnerable] = new(16, -120, 40, -88),
        [KraidMouthHitbox.OpeningVulnerable] = new(16, -120, 40, -104),
        [KraidMouthHitbox.WiderVulnerable] = new(16, -128, 40, -112),
        [KraidMouthHitbox.OpenVulnerable] = new(16, -128, 40, -120),
        [KraidMouthHitbox.EmptyShape] = new(0, 0, 0, 0),
        [KraidMouthHitbox.OpeningInvulnerable] = new(6, -96, 32, -80),
        [KraidMouthHitbox.WiderInvulnerable] = new(0, -104, 32, -80),
        [KraidMouthHitbox.OpenInvulnerable] = new(0, -112, 32, -80),
    };
}
