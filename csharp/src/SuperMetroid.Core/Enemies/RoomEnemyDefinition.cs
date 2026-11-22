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
/// <param name="GrappleResponse">
/// How the enemy reacts to the grapple beam reaching it; the elevator, whose own behaviour carries on, has none
/// (<see cref="EnemyGrappleResponse.Unreactive"/>, #2283).
/// </param>
/// <param name="HurtResponse">What the enemy does while it is hurt and flashing.</param>
/// <param name="DeathExplosion">The explosion the enemy leaves when an ordinary shot kills it.</param>
/// <param name="PowerBombReaction">
/// The Power Bomb reaction: the ignoring one, or the common Power Bomb damage (<see cref="EnemyReaction.CommonPowerBomb"/>).
/// </param>
/// <param name="Touch">The Samus-contact reaction that runs when Samus meets the enemy by its radius: a shared one or its species' own.</param>
/// <param name="Shot">The projectile-hit reaction that runs when a shot or bomb meets the enemy by its radius: a shared one or its species' own.</param>

/// <param name="TouchHitboxScan">Whether Samus's touch is tested against the enemy's extended hitboxes or withheld.</param>
/// <param name="ShotHitboxScan">Whether shots and bombs are tested against the enemy's extended hitboxes or withheld.</param>
/// <param name="Drops">What the enemy's death drops: nothing, or a pickup its chances roll for.</param>
/// <param name="Vulnerability">The enemy's vulnerabilities, the standard ones (<see cref="EnemyVulnerabilityDefinitions.Standard"/>) or its own.</param>
/// <param name="Stacking">Where the enemy's picture stands among the room's actors as it spawns (#2618); its behaviour may move it.</param>
public readonly partial record struct RoomEnemyDefinition(
    int Health,
    int Damage,
    Extent16<RoomPixels> Radius,
    int HurtFlashFrames,
    SoundEffectId? HurtSoundEffect,
    AreaBoss? Boss,
    EnemyGrappleResponse GrappleResponse,
    EnemyHurtResponse HurtResponse,
    EnemyDeathExplosion DeathExplosion,
    EnemyReaction PowerBombReaction,
    EnemyReaction Touch,
    EnemyReaction Shot,
    EnemyHitboxScan TouchHitboxScan,
    EnemyHitboxScan ShotHitboxScan,
    EnemyDrops Drops,
    EnemyVulnerabilities Vulnerability,
    EnemyStacking Stacking);
