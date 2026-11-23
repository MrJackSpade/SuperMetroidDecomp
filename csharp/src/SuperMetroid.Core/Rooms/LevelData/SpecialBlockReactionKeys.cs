namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The installed key of each <see cref="SpecialBlockReaction"/> (#2520), written out so renaming a member never changes
/// a name the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class SpecialBlockReactionKeys
{
    /// <summary>Each SpecialBlockReaction member's installed key.</summary>
    internal static InstalledKeys<SpecialBlockReaction> Installed { get; } = new(new Dictionary<SpecialBlockReaction, string>
    {
        [SpecialBlockReaction.RespawningCrumble1x1] = "RespawningCrumble1x1",
        [SpecialBlockReaction.RespawningCrumble2x1] = "RespawningCrumble2x1",
        [SpecialBlockReaction.RespawningCrumble1x2] = "RespawningCrumble1x2",
        [SpecialBlockReaction.RespawningCrumble2x2] = "RespawningCrumble2x2",
        [SpecialBlockReaction.PermanentCrumble1x1] = "PermanentCrumble1x1",
        [SpecialBlockReaction.PermanentCrumble2x1] = "PermanentCrumble2x1",
        [SpecialBlockReaction.PermanentCrumble1x2] = "PermanentCrumble1x2",
        [SpecialBlockReaction.PermanentCrumble2x2] = "PermanentCrumble2x2",
        [SpecialBlockReaction.XrayRevealedSolid] = "XrayRevealedSolid",
        [SpecialBlockReaction.RespawningSpeedBlock] = "RespawningSpeedBlock",
        [SpecialBlockReaction.PermanentSpeedBlock] = "PermanentSpeedBlock",
        [SpecialBlockReaction.BombRevealsSpeedBlock] = "BombRevealsSpeedBlock",
        [SpecialBlockReaction.CollectibleTrigger] = "CollectibleTrigger",
        [SpecialBlockReaction.MapStationRight] = "MapStationRight",
        [SpecialBlockReaction.MapStationLeft] = "MapStationLeft",
        [SpecialBlockReaction.EnergyStationRight] = "EnergyStationRight",
        [SpecialBlockReaction.EnergyStationLeft] = "EnergyStationLeft",
        [SpecialBlockReaction.MissileStationRight] = "MissileStationRight",
        [SpecialBlockReaction.MissileStationLeft] = "MissileStationLeft",
        [SpecialBlockReaction.SaveStationFloor] = "SaveStationFloor",
        [SpecialBlockReaction.Solid] = "Solid",
        [SpecialBlockReaction.WreckedShipChozoHand] = "WreckedShipChozoHand",
        [SpecialBlockReaction.LowerNorfairChozoHand] = "LowerNorfairChozoHand",
        [SpecialBlockReaction.SlowRespawningSpeedBlock] = "SlowRespawningSpeedBlock",
        [SpecialBlockReaction.SlowPermanentSpeedBlock] = "SlowPermanentSpeedBlock",
        [SpecialBlockReaction.BrinstarRespawningSpeedBlock] = "BrinstarRespawningSpeedBlock",
        [SpecialBlockReaction.BrinstarPermanentSpeedBlock] = "BrinstarPermanentSpeedBlock",
    });

    /// <summary>The SpecialBlockReaction members in the order the installation and a kind's reaction list name them (#2567), written out beside the keys so reordering the members never reorders them.</summary>
    internal static IReadOnlyList<SpecialBlockReaction> ListedOrder { get; } =
    [
        SpecialBlockReaction.RespawningCrumble1x1,
        SpecialBlockReaction.RespawningCrumble2x1,
        SpecialBlockReaction.RespawningCrumble1x2,
        SpecialBlockReaction.RespawningCrumble2x2,
        SpecialBlockReaction.PermanentCrumble1x1,
        SpecialBlockReaction.PermanentCrumble2x1,
        SpecialBlockReaction.PermanentCrumble1x2,
        SpecialBlockReaction.PermanentCrumble2x2,
        SpecialBlockReaction.XrayRevealedSolid,
        SpecialBlockReaction.RespawningSpeedBlock,
        SpecialBlockReaction.PermanentSpeedBlock,
        SpecialBlockReaction.BombRevealsSpeedBlock,
        SpecialBlockReaction.CollectibleTrigger,
        SpecialBlockReaction.MapStationRight,
        SpecialBlockReaction.MapStationLeft,
        SpecialBlockReaction.EnergyStationRight,
        SpecialBlockReaction.EnergyStationLeft,
        SpecialBlockReaction.MissileStationRight,
        SpecialBlockReaction.MissileStationLeft,
        SpecialBlockReaction.SaveStationFloor,
        SpecialBlockReaction.Solid,
        SpecialBlockReaction.WreckedShipChozoHand,
        SpecialBlockReaction.LowerNorfairChozoHand,
        SpecialBlockReaction.SlowRespawningSpeedBlock,
        SpecialBlockReaction.SlowPermanentSpeedBlock,
        SpecialBlockReaction.BrinstarRespawningSpeedBlock,
        SpecialBlockReaction.BrinstarPermanentSpeedBlock,
    ];
}
