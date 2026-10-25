using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy definition: its gameplay fields and how its room graphics set shows its tiles, as the import decodes its header.</summary>
/// <param name="TileByteCount">The bytes of the enemy's tile set, which its room graphics set shows in the enemy tile area.</param>
/// <param name="TilePlacement">Where the graphics set shows the tiles in the enemy tile area.</param>
/// <param name="Health">Header +$04 initial health in enemy damage units.</param>
/// <param name="Damage">Header +$06 contact-damage value before Samus damage-reduction rules.</param>
/// <param name="Radius">Header +$08/+$0A horizontal and vertical collision half-extents in room pixels.</param>
/// <param name="HurtAiTime">Header +$0D native hurt-AI duration byte, retained separately from mutable flash timers.</param>
/// <param name="HurtSoundEffect">Header +$0E: the library-two cry the enemy makes when a shot hurts it, or none.</param>
/// <param name="BossId">Header +$10 boss identity published when a room population contains this enemy; zero means no boss identity.</param>
/// <param name="InitializationAi">Header +$12: whether initialization runs the species' routine or the shared no-op.</param>
/// <param name="PartCount">Header +$14 native multipart-enemy count.</param>
/// <param name="MainAi">Header +$18: whether the ordinary AI is the species' routine or the shared no-op.</param>
/// <param name="GrappleAi">Header +$1A Grapple reaction.</param>
/// <param name="HurtAi">Header +$1C: whether the hurt AI is the species' routine or the shared no-op.</param>
/// <param name="DeathAnimation">Header +$22 native death-animation selector, rather than an AI address.</param>
/// <param name="PowerBombReaction">Header +$28 Power Bomb reaction the port names, or null when it names none.</param>
/// <param name="PowerBombReactionBody">What the header's Power Bomb reaction does when called.</param>
/// <param name="VariantIndex">Header +$2A hopper variant selector, 0 through 3; zero for an enemy whose behaviour reads none.</param>
/// <param name="Touch">Header +$30 Samus-contact reaction the port names, or null when it names none.</param>
/// <param name="TouchBody">What the header's Samus-contact reaction does when called.</param>
/// <param name="Shot">Header +$32 projectile-hit reaction the port names, or null when it names none.</param>
/// <param name="ShotBody">What the header's projectile-hit reaction does when called.</param>
/// <param name="NoOpCallbackBody">
/// What the enemy's code runs for the shared no-op callback, which header and hitbox callbacks name in the enemy's own
/// code: in most enemy code it returns at once, in some it is a different routine.
/// </param>
/// <param name="Layer">Header +$39 native drawing-layer byte, copied into the slot's word-sized layer field.</param>
/// <param name="DropChances">Header +$3A item-drop probability table, or null when the enemy drops nothing.</param>
/// <param name="Vulnerability">Header +$3C weapon-vulnerability table; a zero header word selects the default table.</param>
public readonly record struct RoomEnemyDefinition(
    int TileByteCount,
    EnemyTilePlacement TilePlacement,
    int Health,
    int Damage,
    Extent16<RoomPixels> Radius,
    int HurtAiTime,
    SoundEffectId? HurtSoundEffect,
    int BossId,
    EnemyAiRoutine InitializationAi,
    int PartCount,
    EnemyAiRoutine MainAi,
    GrappleAiRoutine GrappleAi,
    EnemyAiRoutine HurtAi,
    int DeathAnimation,
    EnemyInteractionCallback? PowerBombReaction,
    EnemyCallbackBody PowerBombReactionBody,
    int VariantIndex,
    EnemyInteractionCallback? Touch,
    EnemyCallbackBody TouchBody,
    EnemyInteractionCallback? Shot,
    EnemyCallbackBody ShotBody,
    EnemyCallbackBody NoOpCallbackBody,
    int Layer,
    EnemyDropTable? DropChances,
    EnemyVulnerabilityTable Vulnerability);
