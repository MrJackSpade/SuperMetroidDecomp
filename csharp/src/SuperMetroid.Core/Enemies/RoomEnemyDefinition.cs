using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy definition: its native header's gameplay fields, with its routine and callback words decoded to what they name.</summary>
/// <param name="TileDataSize">Header +$00 byte-count word; bit 15 selects explicit tile placement, while the low fifteen bits give the upload size.</param>
/// <param name="Health">Header +$04 initial health in enemy damage units.</param>
/// <param name="Damage">Header +$06 contact-damage value before Samus damage-reduction rules.</param>
/// <param name="Radius">Header +$08/+$0A horizontal and vertical collision half-extents in room pixels.</param>
/// <param name="HurtAiTime">Header +$0D native hurt-AI duration byte, retained separately from mutable flash timers.</param>
/// <param name="HurtSoundEffect">Header +$0E native enemy cry sound selector.</param>
/// <param name="BossId">Header +$10 boss identity published when a room population contains this enemy; zero means no boss identity.</param>
/// <param name="InitializationAi">Header +$12: whether initialization runs the species' routine or the shared no-op.</param>
/// <param name="PartCount">Header +$14 native multipart-enemy count.</param>
/// <param name="Unused16">Header +$16 unused native word, retained verbatim rather than interpreted as runtime state.</param>
/// <param name="MainAi">Header +$18: whether the ordinary AI is the species' routine or the shared no-op.</param>
/// <param name="GrappleAi">Header +$1A Grapple reaction.</param>
/// <param name="HurtAi">Header +$1C: whether the hurt AI is the species' routine or the shared no-op.</param>
/// <param name="DeathAnimation">Header +$22 native death-animation selector, rather than an AI address.</param>
/// <param name="Unused24">Header +$24 first word of an unused four-byte native field, retained verbatim.</param>
/// <param name="Unused26">Header +$26 second word of the unused field beginning at +$24.</param>
/// <param name="PowerBombReaction">Header +$28 Power Bomb reaction the port names, or null when it names none.</param>
/// <param name="PowerBombReactionBody">What the header's Power Bomb reaction does when called.</param>
/// <param name="VariantIndex">Header +$2A Sidehopper variant selector; other headers retain unused prototype instruction-list data in this word.</param>
/// <param name="Unused2C">Header +$2C first word of an unused four-byte native field, retained verbatim.</param>
/// <param name="Unused2E">Header +$2E second word of the unused field beginning at +$2C.</param>
/// <param name="Touch">Header +$30 Samus-contact reaction the port names, or null when it names none.</param>
/// <param name="TouchBody">What the header's Samus-contact reaction does when called.</param>
/// <param name="Shot">Header +$32 projectile-hit reaction the port names, or null when it names none.</param>
/// <param name="ShotBody">What the header's projectile-hit reaction does when called.</param>
/// <param name="NoOpCallbackBody">
/// What the enemy's code runs for the shared no-op callback, which header and hitbox callbacks name in the enemy's own
/// code: in most enemy code it returns at once, in some it is a different routine.
/// </param>
/// <param name="TileDataAddress">Header +$36 full 24-bit source address of the enemy's planar character data.</param>
/// <param name="Layer">Header +$39 native drawing-layer byte, copied into the slot's word-sized layer field.</param>
/// <param name="DropChances">Header +$3A item-drop probability table, or null when the enemy drops nothing.</param>
/// <param name="Vulnerability">Header +$3C weapon-vulnerability table; a zero header word selects the default table.</param>
public readonly record struct RoomEnemyDefinition(
    ushort TileDataSize,
    int Health,
    ushort Damage,
    Extent16<RoomPixels> Radius,
    byte HurtAiTime,
    ushort HurtSoundEffect,
    ushort BossId,
    EnemyAiRoutine InitializationAi,
    ushort PartCount,
    ushort Unused16,
    EnemyAiRoutine MainAi,
    GrappleAiRoutine GrappleAi,
    EnemyAiRoutine HurtAi,
    ushort DeathAnimation,
    ushort Unused24,
    ushort Unused26,
    EnemyInteractionCallback? PowerBombReaction,
    EnemyCallbackBody PowerBombReactionBody,
    ushort VariantIndex,
    ushort Unused2C,
    ushort Unused2E,
    EnemyInteractionCallback? Touch,
    EnemyCallbackBody TouchBody,
    EnemyInteractionCallback? Shot,
    EnemyCallbackBody ShotBody,
    EnemyCallbackBody NoOpCallbackBody,
    int TileDataAddress,
    byte Layer,
    EnemyDropTable? DropChances,
    EnemyVulnerabilityTable Vulnerability);
