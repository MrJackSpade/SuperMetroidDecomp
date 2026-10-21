using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.Core.Enemies;

/// <summary>The complete parsed 64-byte bank-$A0 enemy definition.</summary>
/// <param name="TileDataSize">Header +$00 byte-count word; bit 15 selects explicit tile placement, while the low fifteen bits give the upload size.</param>
/// <param name="PalettePointer">Header +$02 bank-relative pointer to the sixteen-color palette in the enemy's code/data bank.</param>
/// <param name="Health">Header +$04 initial health in enemy damage units.</param>
/// <param name="Damage">Header +$06 contact-damage value before Samus damage-reduction rules.</param>
/// <param name="Radius">Header +$08/+$0A horizontal and vertical collision half-extents in room pixels.</param>
/// <param name="Bank">Header +$0C bank used to resolve the enemy's AI, palette, and animation pointers.</param>
/// <param name="HurtAiTime">Header +$0D native hurt-AI duration byte, retained separately from mutable flash timers.</param>
/// <param name="HurtSoundEffect">Header +$0E native enemy cry sound selector.</param>
/// <param name="BossId">Header +$10 boss identity published when a room population contains this enemy; zero means no boss identity.</param>
/// <param name="InitializationAiPointer">Header +$12 bank-relative initialization AI entry point, resolved through <paramref name="Bank"/>.</param>
/// <param name="PartCount">Header +$14 native multipart-enemy count.</param>
/// <param name="Unused16">Header +$16 unused native word, retained verbatim rather than interpreted as runtime state.</param>
/// <param name="MainAiPointer">Header +$18 bank-relative ordinary AI entry point.</param>
/// <param name="GrappleAi">Header +$1A bank-relative Grapple reaction entry point.</param>
/// <param name="HurtAiPointer">Header +$1C bank-relative hurt AI entry point.</param>
/// <param name="FrozenAiPointer">Header +$1E bank-relative frozen-enemy AI entry point, separate from globally frozen game time.</param>
/// <param name="TimeFrozenAiPointer">Header +$20 bank-relative AI entry point used when game time is frozen.</param>
/// <param name="DeathAnimation">Header +$22 native death-animation selector, rather than an AI address.</param>
/// <param name="Unused24">Header +$24 first word of an unused four-byte native field, retained verbatim.</param>
/// <param name="Unused26">Header +$26 second word of the unused field beginning at +$24.</param>
/// <param name="PowerBombReactionPointer">Header +$28 bank-relative Power Bomb reaction entry point.</param>
/// <param name="VariantIndex">Header +$2A Sidehopper variant selector; other headers retain unused prototype instruction-list data in this word.</param>
/// <param name="Unused2C">Header +$2C first word of an unused four-byte native field, retained verbatim.</param>
/// <param name="Unused2E">Header +$2E second word of the unused field beginning at +$2C.</param>
/// <param name="TouchAiPointer">Header +$30 bank-relative Samus-contact reaction entry point.</param>
/// <param name="ShotAiPointer">Header +$32 bank-relative projectile-hit reaction entry point.</param>
/// <param name="InitialSpritemapPointer">Header +$34 legacy-named unused spritemap-pointer-table field; it is not a universal initial drawing frame.</param>
/// <param name="TileDataAddress">Header +$36 full 24-bit source address of the enemy's planar character data.</param>
/// <param name="Layer">Header +$39 native drawing-layer byte, copied into the slot's word-sized layer field.</param>
/// <param name="ItemDropChancesPointer">Header +$3A bank-$B4 pointer to enemy pickup-drop probabilities.</param>
/// <param name="VulnerabilityPointer">Header +$3C bank-$B4 pointer to weapon-vulnerability data.</param>
/// <param name="NamePointer">Header +$3E bank-$B4 pointer to the debug enemy-name record.</param>
public readonly record struct RoomEnemyDefinition(
    ushort TileDataSize,
    ushort PalettePointer,
    int Health,
    ushort Damage,
    Extent16<RoomPixels> Radius,
    byte Bank,
    byte HurtAiTime,
    ushort HurtSoundEffect,
    ushort BossId,
    ushort InitializationAiPointer,
    ushort PartCount,
    ushort Unused16,
    ushort MainAiPointer,
    GrappleAiRoutine GrappleAi,
    ushort HurtAiPointer,
    ushort FrozenAiPointer,
    ushort TimeFrozenAiPointer,
    ushort DeathAnimation,
    ushort Unused24,
    ushort Unused26,
    ushort PowerBombReactionPointer,
    ushort VariantIndex,
    ushort Unused2C,
    ushort Unused2E,
    ushort TouchAiPointer,
    ushort ShotAiPointer,
    ushort InitialSpritemapPointer,
    int TileDataAddress,
    byte Layer,
    ushort ItemDropChancesPointer,
    ushort VulnerabilityPointer,
    ushort NamePointer);
