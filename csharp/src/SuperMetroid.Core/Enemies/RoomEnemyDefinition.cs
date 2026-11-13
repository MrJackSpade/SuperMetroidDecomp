using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy definition: its gameplay fields, as the import decodes them.</summary>
/// <param name="Health">Initial health in enemy damage units.</param>
/// <param name="Damage">Contact-damage value before Samus damage-reduction rules.</param>
/// <param name="Radius">Horizontal and vertical collision half-extents in room pixels.</param>
/// <param name="HurtFlashFrames">The frames the enemy flashes when hurt, before any weapon's longer flash.</param>
/// <param name="HurtSoundEffect">The enemy-library cry the enemy makes when a shot hurts it, or none.</param>
/// <param name="Boss">The boss a room containing this enemy holds, or null when the enemy is no boss.</param>
/// <param name="InitializationAi">Whether initialization runs the species' own behaviour or nothing.</param>
/// <param name="MainAi">Whether the ordinary AI is the species' own behaviour or nothing.</param>
/// <param name="GrappleAi">The Grapple reaction.</param>
/// <param name="HurtAi">Whether the hurt AI is the species' own behaviour or nothing.</param>
/// <param name="DeathExplosion">The explosion the enemy leaves when an ordinary shot kills it.</param>
/// <param name="PowerBombReaction">
/// The Power Bomb reaction, or null when the definition names none and the common Power Bomb damage applies.
/// </param>
/// <param name="VariantIndex">The hopper variant, 0 through 3; zero for an enemy whose behaviour reads none.</param>
/// <param name="Touch">The Samus-contact reaction, or null when the definition names none and only extended hitboxes name one.</param>
/// <param name="Shot">The projectile-hit reaction, or null when the definition names none and only extended hitboxes name one.</param>
/// <param name="TouchHitboxScan">Whether Samus's touch is tested against the enemy's extended hitboxes or withheld.</param>
/// <param name="ShotHitboxScan">Whether shots and bombs are tested against the enemy's extended hitboxes or withheld.</param>
/// <param name="DropChances">The item-drop chances, or null when the enemy drops nothing.</param>
/// <param name="Vulnerability">
/// The enemy's own vulnerabilities, or null when it takes the standard ones; <see cref="EnemyVulnerabilities.Of"/> resolves it.
/// </param>
public readonly record struct RoomEnemyDefinition(
    int Health,
    int Damage,
    Extent16<RoomPixels> Radius,
    int HurtFlashFrames,
    SoundEffectId? HurtSoundEffect,
    AreaBoss? Boss,
    EnemyAiRoutine InitializationAi,
    EnemyAiRoutine MainAi,
    GrappleAiRoutine GrappleAi,
    EnemyAiRoutine HurtAi,
    EnemyDeathExplosion DeathExplosion,
    EnemyReaction? PowerBombReaction,
    int VariantIndex,
    EnemyReaction? Touch,
    EnemyReaction? Shot,
    EnemyHitboxScan TouchHitboxScan,
    EnemyHitboxScan ShotHitboxScan,
    EnemyDropChances? DropChances,
    EnemyVulnerabilities? Vulnerability);
