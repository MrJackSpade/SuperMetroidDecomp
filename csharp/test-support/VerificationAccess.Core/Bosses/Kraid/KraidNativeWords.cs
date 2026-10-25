using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Verification's native view of the Kraid identities (#1413): the cartridge words naming each member. Core's values are
/// arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class KraidNativeWords
{
    /// <summary>The bank-$A7 hitbox record address naming each <see cref="KraidMouthHitbox"/>.</summary>
    internal static readonly NativeWordIdentities<KraidMouthHitbox> MouthHitboxes = new(new Dictionary<KraidMouthHitbox, ushort>
    {
        [KraidMouthHitbox.ClosedVulnerable] = 0x9788,
        [KraidMouthHitbox.OpeningVulnerable] = 0x9790,
        [KraidMouthHitbox.WiderVulnerable] = 0x9798,
        [KraidMouthHitbox.OpenVulnerable] = 0x97a0,
        [KraidMouthHitbox.EmptyShape] = 0x97a8,
        [KraidMouthHitbox.OpeningInvulnerable] = 0x97b0,
        [KraidMouthHitbox.WiderInvulnerable] = 0x97b8,
        [KraidMouthHitbox.OpenInvulnerable] = 0x97c0,
    });
}
