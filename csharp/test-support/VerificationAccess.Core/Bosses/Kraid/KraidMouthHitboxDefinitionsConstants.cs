using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Cartridge values and helpers of <see cref="KraidMouthHitboxDefinitions"/> that only verification reads.</summary>
internal static class KraidMouthHitboxDefinitionsConstants
{
    /// <summary>Aligned fixed records at $A7:9788..97C7.</summary>
    public static bool IsDefined(ushort pointer) => KraidNativeWords.MouthHitboxes.TryDecode(pointer, out _);
    /// <summary>
    /// $A7:9788..97C7, Hitbox_KraidMouth_0..7. The native projectile test uses
    /// left/top/bottom only; the authored right edge is retained as definition data.
    /// Entry four is unused but remains a defined all-zero rectangle.
    /// </summary>
    /// <remarks>
    /// Independently reviewed for #1165 against all 32 original words and pinned
    /// bank_A7.asm head-program selectors. These are named collision shapes selected
    /// by mouth stage and vulnerability, not a chronological numerical curve.
    /// The existing semantic cases preserve the otherwise unused right edge and
    /// zero shape. Live low-half geometry remains a separate caller-owned path.
    /// </remarks>
    public static EnemyHitboxRectangle Resolve(ushort pointer) =>
        KraidMouthHitboxes.Resolve(KraidNativeWords.MouthHitboxes.Decode(pointer, "Kraid mouth hitbox"));
    /// <summary>
    /// Resolves compiled cartridge geometry or a genuine bank-$A7 low-half live-memory,
    /// hardware, or open-bus alias. The seven upper-window bytes reachable when a low-half
    /// record crosses $7FFF are retained explicitly; unrelated cartridge addresses are not
    /// accepted as hitboxes.
    /// </summary>
    public static KraidMouthCollisionEdges ResolveCollision(ushort pointer)
    {
        if (KraidNativeWords.MouthHitboxes.TryDecode(pointer, out KraidMouthHitbox shape))
            return KraidMouthHitboxes.ResolveCollision(shape);
        throw new InvalidDataException(
            $"Kraid mouth hitbox $A7:{pointer:X4} is outside the compiled geometry catalog.");
    }
}
