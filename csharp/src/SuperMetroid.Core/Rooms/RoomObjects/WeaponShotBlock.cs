using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A shot block only some weapons break (<see cref="WeaponBlockReactionDefinitions.WeaponShotBlocks"/>): the parent its
/// break restores, its break, the reveal a weapon that does not break it shows, and the weapons it yields to.
/// </summary>
/// <param name="Parent">The setup block whose picture the break keeps the struck block's collision under.</param>
/// <param name="Break">The change a weapon that breaks the block sets off.</param>
/// <param name="Reveal">The one-update reveal a weapon that shows it sets off, leaving the live block untouched.</param>
/// <param name="Weapons">What each weapon family the block yields to does; any other family leaves the block as it is.</param>
internal sealed record WeaponShotBlock(
    RoomObjectSetupBlock Parent,
    BlockReactionChange Break,
    LastingBlockReactionChange Reveal,
    IReadOnlyDictionary<SamusProjectileFamily, ShotBlockWeaponEffect> Weapons);
