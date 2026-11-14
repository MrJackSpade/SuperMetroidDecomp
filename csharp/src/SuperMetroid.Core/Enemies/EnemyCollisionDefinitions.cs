namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The fixed values the common enemy collision applies (#1847): the damage Samus's attacking contacts deal, how long a
/// freeze and the protection after a hit last, how far a weapon's hit extends the hurt flash, and what an enemy's touch
/// does to Samus.
/// </summary>
internal static class EnemyCollisionDefinitions
{
    /// <summary>The damage a Speed Booster contact deals an enemy, before the enemy's Speed Booster vulnerability.</summary>
    internal const int SpeedBoosterContactDamage = 500;

    /// <summary>The damage a shinespark contact deals an enemy, before the enemy's shinespark vulnerability.</summary>
    internal const int ShinesparkContactDamage = 300;

    /// <summary>The damage a Screw Attack contact deals an enemy, before the enemy's Screw Attack vulnerability.</summary>
    internal const int ScrewAttackContactDamage = 2000;

    /// <summary>The damage a Pseudo Screw contact deals an enemy, before the enemy's Pseudo Screw vulnerability.</summary>
    internal const int PseudoScrewContactDamage = 200;

    /// <summary>The updates an enemy a shot freezes in a Norfair room stays frozen.</summary>
    internal const int NorfairFreezeUpdates = 300;

    /// <summary>The updates an enemy a shot freezes in a room of any other area stays frozen.</summary>
    internal const int FreezeUpdates = 400;

    /// <summary>The updates a newly frozen enemy is protected from weapons and contact.</summary>
    internal const int FreezeInvincibilityUpdates = 10;

    /// <summary>The updates an enemy the common Power Bomb reaction hurts is protected from weapons and contact.</summary>
    internal const int PowerBombInvincibilityUpdates = 48;

    /// <summary>
    /// The updates a shot's, a bomb's or a Power Bomb's hit adds to the enemy's hurt flash; a hurting contact shows the hurt
    /// flash alone.
    /// </summary>
    internal const int WeaponHurtFlashExtension = 8;

    /// <summary>The updates Samus is invincible after an enemy's touch hurts her.</summary>
    internal const int SamusTouchInvincibilityUpdates = 96;

    /// <summary>The updates of knockback an enemy's touch requests for Samus.</summary>
    internal const int SamusTouchKnockbackUpdates = 5;
}
