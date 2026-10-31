using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// The cartridge's one-based message numbers naming each <see cref="GameplayMessageId"/> (#1481): the number a message
/// box is opened with and, less one, the message's entry in the bank-$85 definition table, which the message extractors
/// translate once at import. The gunship's confirmation lies past the table's terminator. Core's values are arbitrary.
/// </summary>
internal static class GameplayMessageNumbers
{
    /// <summary>The message number of each gameplay message.</summary>
    internal static readonly NativeWordIdentities<GameplayMessageId> Numbers = new(new Dictionary<GameplayMessageId, ushort>
    {
        [GameplayMessageId.EnergyTank] = 1,
        [GameplayMessageId.MissileTank] = 2,
        [GameplayMessageId.SuperMissileTank] = 3,
        [GameplayMessageId.PowerBombTank] = 4,
        [GameplayMessageId.GrappleBeam] = 5,
        [GameplayMessageId.XrayScope] = 6,
        [GameplayMessageId.VariaSuit] = 7,
        [GameplayMessageId.SpringBall] = 8,
        [GameplayMessageId.MorphBall] = 9,
        [GameplayMessageId.ScrewAttack] = 10,
        [GameplayMessageId.HiJumpBoots] = 11,
        [GameplayMessageId.SpaceJump] = 12,
        [GameplayMessageId.SpeedBooster] = 13,
        [GameplayMessageId.ChargeBeam] = 14,
        [GameplayMessageId.IceBeam] = 15,
        [GameplayMessageId.WaveBeam] = 16,
        [GameplayMessageId.SpazerBeam] = 17,
        [GameplayMessageId.PlasmaBeam] = 18,
        [GameplayMessageId.Bombs] = 19,
        [GameplayMessageId.MapDataAccessCompleted] = 20,
        [GameplayMessageId.EnergyRechargeCompleted] = 21,
        [GameplayMessageId.MissileRechargeCompleted] = 22,
        [GameplayMessageId.SaveConfirmation] = 23,
        [GameplayMessageId.SaveCompleted] = 24,
        [GameplayMessageId.ReserveTank] = 25,
        [GameplayMessageId.GravitySuit] = 26,
        [GameplayMessageId.GunshipSaveConfirmation] = 28,
    });

    /// <summary>The bank-$85 definition-table entry of <paramref name="id"/>: its message number less one.</summary>
    internal static int DefinitionEntry(GameplayMessageId id) => Numbers.Word(id) - 1;
}
