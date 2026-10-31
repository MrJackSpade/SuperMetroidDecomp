using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy definition: its gameplay fields and how its room graphics set shows its tiles, as the import decodes its header.</summary>
/// <param name="TileByteCount">The bytes of the enemy's tile set, which its room graphics set shows in the enemy tile area.</param>
/// <param name="TilePlacement">Where the graphics set shows the tiles in the enemy tile area.</param>
/// <param name="Health">Initial health in enemy damage units.</param>
/// <param name="Damage">Contact-damage value before Samus damage-reduction rules.</param>
/// <param name="Radius">Horizontal and vertical collision half-extents in room pixels.</param>
/// <param name="HurtFlashFrames">The frames the enemy flashes when hurt, before any weapon's longer flash.</param>
/// <param name="HurtSoundEffect">The library-two cry the enemy makes when a shot hurts it, or none.</param>
/// <param name="Boss">The boss a room containing this enemy holds, or null when the enemy is no boss.</param>
/// <param name="InitializationAi">Whether initialization runs the species' routine or the shared no-op.</param>
/// <param name="MainAi">Whether the ordinary AI is the species' routine or the shared no-op.</param>
/// <param name="GrappleAi">The Grapple reaction.</param>
/// <param name="HurtAi">Whether the hurt AI is the species' routine or the shared no-op.</param>
/// <param name="DeathExplosion">The explosion the enemy leaves when an ordinary shot kills it.</param>
/// <param name="PowerBombReaction">The Power Bomb reaction the port names, or null when it names none.</param>
/// <param name="PowerBombReactionBody">What the Power Bomb reaction does when called.</param>
/// <param name="VariantIndex">The hopper variant, 0 through 3; zero for an enemy whose behaviour reads none.</param>
/// <param name="Touch">The Samus-contact reaction the port names, or null when it names none.</param>
/// <param name="TouchBody">What the Samus-contact reaction does when called.</param>
/// <param name="Shot">The projectile-hit reaction the port names, or null when it names none.</param>
/// <param name="ShotBody">What the projectile-hit reaction does when called.</param>
/// <param name="NoOpCallbackBody">
/// What the enemy's code runs for the shared no-op callback, which header and hitbox callbacks name in the enemy's own
/// code: in most enemy code it returns at once, in some it is a different routine.
/// </param>
/// <param name="Layer">The drawing layer the enemy starts on, 0 through 7, lower layers drawn first.</param>
/// <param name="DropChances">The item-drop chances, or null when the enemy drops nothing.</param>
/// <param name="Vulnerability">
/// The enemy's own vulnerabilities, or null when it takes the standard ones; <see cref="EnemyVulnerabilities.Of"/> resolves it.
/// </param>
public readonly record struct RoomEnemyDefinition(
    int TileByteCount,
    EnemyTilePlacement TilePlacement,
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
    EnemyInteractionCallback? PowerBombReaction,
    EnemyCallbackBody PowerBombReactionBody,
    int VariantIndex,
    EnemyInteractionCallback? Touch,
    EnemyCallbackBody TouchBody,
    EnemyInteractionCallback? Shot,
    EnemyCallbackBody ShotBody,
    EnemyCallbackBody NoOpCallbackBody,
    int Layer,
    EnemyDropChances? DropChances,
    EnemyVulnerabilities? Vulnerability);
