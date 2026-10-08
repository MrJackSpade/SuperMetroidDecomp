namespace SuperMetroid.Core.Game;

/// <summary>Immutable Mother Brain head rectangle and native touch/shot callback identities.</summary>
internal readonly record struct MotherBrainHeadCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);

/// <summary>
/// Bank-$A9 physical collision for Mother Brain's head enemy. The head keeps
/// <c>InstList_MotherBrainHead_InitialDummy</c> for life, so its only extended frame is
/// $A9:A320: one zero-offset component whose hitbox list is
/// <c>Hitbox_MotherBrainBody_0</c>. The visible brain is a separate draw-hook list and
/// never changes this geometry.
/// </summary>
internal static class MotherBrainHeadCollisionDefinitions
{
    /// <summary>Native bank of the head's extended frame and hitbox list.</summary>
    internal const byte Bank = 0xa9;

    /// <summary><c>UNUSED_ExtendedSpritemap_MotherBrainBrain_A9A320</c>, the dummy list's frame.</summary>
    internal const ushort HitboxFrame = 0xa320;

    /// <summary>
    /// <c>Hitbox_MotherBrainBody_0</c> at $A9:A4AC: left -$14, top -$15, right +$10,
    /// bottom +$17 around the head, dispatching the head's touch and shot callbacks.
    /// </summary>
    internal static MotherBrainHeadCollisionHitbox Hitbox { get; } = new(
        -0x14, -0x15, 0x10, 0x17,
        EnemyAiCodePointers.BankA9.MotherBrainHeadTouch,
        EnemyAiCodePointers.BankA9.MotherBrainHeadShot);
}
