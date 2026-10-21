using SuperMetroid.Core.Enemies.Evir;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Evir body, arms, and projectile compositions shown by the Evir's typed programs (<see cref="EvirAnimations"/>).
/// The three-slot movement, attack, and regeneration logic stays in <see cref="EvirBehavior"/>.
/// </summary>
internal static class EvirVisualDefinitions
{
    /// <summary>Evir's native instruction and spritemap bank, $A8.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>Names Evir's left/right body and arm poses plus flight and regeneration projectile OAM roots in bank $A8.</summary>
    internal static NativeEnemySpritemapDefinition[] Frames() =>
    [
        new(Bank, 0x8b59, "evir_body_left_0"),
        new(Bank, 0x8b88, "evir_body_left_1"),
        new(Bank, 0x8bb7, "evir_body_left_2"),
        new(Bank, 0x8be6, "evir_body_left_3"),
        new(Bank, 0x8c15, "evir_body_left_4"),
        new(Bank, 0x8c44, "evir_body_left_5"),
        new(Bank, 0x8ca2, "evir_arms_left_0"),
        new(Bank, 0x8cbd, "evir_arms_left_1"),
        new(Bank, 0x8cd8, "evir_arms_left_2"),
        new(Bank, 0x8cf3, "evir_arms_left_3"),
        new(Bank, 0x8d13, "evir_arms_left_4"),
        new(Bank, 0x8d81, "evir_body_right_0"),
        new(Bank, 0x8db0, "evir_body_right_1"),
        new(Bank, 0x8ddf, "evir_body_right_2"),
        new(Bank, 0x8e0e, "evir_body_right_3"),
        new(Bank, 0x8e3d, "evir_body_right_4"),
        new(Bank, 0x8e6c, "evir_body_right_5"),
        new(Bank, 0x8eca, "evir_arms_right_0"),
        new(Bank, 0x8ee5, "evir_arms_right_1"),
        new(Bank, 0x8f00, "evir_arms_right_2"),
        new(Bank, 0x8f1b, "evir_arms_right_3"),
        new(Bank, 0x8f3b, "evir_arms_right_4"),
        new(Bank, 0x8d7a, "evir_projectile_flight"),
        new(Bank, 0x8d64, "evir_projectile_regenerating"),
    ];
}
