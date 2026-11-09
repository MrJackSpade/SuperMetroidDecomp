
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Typed catalog for all five retail Speed Booster collision-block variants.</summary>
internal static class SpeedBoosterBlockObjectDefinitions
{

    /// <summary>Brinstar's respawning Speed Booster block with the slower crumble.</summary>
    public static readonly SpeedBoosterBlockObjectDefinition BrinstarSlowRespawning = new(
        RoomObjectHeaderId.SpeedBlockBrinstarSlowRespawning,
        SpeedBoosterBlockProgram.BrinstarSlowRespawning);

    /// <summary>Brinstar's permanent Speed Booster block with the slower crumble.</summary>
    public static readonly SpeedBoosterBlockObjectDefinition BrinstarSlowPermanent = new(
        RoomObjectHeaderId.SpeedBlockBrinstarSlowPermanent,
        SpeedBoosterBlockProgram.BrinstarSlowPermanent);

    /// <summary>The area-independent standard respawning block.</summary>
    public static readonly SpeedBoosterBlockObjectDefinition Respawning = new(
        RoomObjectHeaderId.SpeedBlockRespawning,
        SpeedBoosterBlockProgram.Respawning);

    /// <summary>Brinstar's Dachora-room standard respawning block.</summary>
    public static readonly SpeedBoosterBlockObjectDefinition DachoraRespawning = new(
        RoomObjectHeaderId.SpeedBlockDachoraRespawning,
        SpeedBoosterBlockProgram.DachoraRespawning);

    /// <summary>The permanent block, by the area-independent behaviour or Brinstar's own.</summary>
    public static readonly SpeedBoosterBlockObjectDefinition Permanent = new(
        RoomObjectHeaderId.SpeedBlockPermanent,
        SpeedBoosterBlockProgram.Permanent);

    /// <summary>The Speed Booster blocks the common special-block reactions select, in every area.</summary>
    public static readonly IReadOnlyDictionary<BlockReaction, SpeedBoosterBlockObjectDefinition> CommonBlocks =
        new Dictionary<BlockReaction, SpeedBoosterBlockObjectDefinition>
        {
            [BlockReaction.Common(14)] = Respawning,
            [BlockReaction.Common(15)] = Permanent,
        };

    /// <summary>The Speed Booster blocks Brinstar's own special-block reactions select.</summary>
    public static readonly IReadOnlyDictionary<BlockReaction, SpeedBoosterBlockObjectDefinition> BrinstarBlocks =
        new Dictionary<BlockReaction, SpeedBoosterBlockObjectDefinition>
        {
            [BlockReaction.AreaSpecific(2)] = BrinstarSlowRespawning,
            [BlockReaction.AreaSpecific(3)] = BrinstarSlowPermanent,
            [BlockReaction.AreaSpecific(4)] = DachoraRespawning,
            [BlockReaction.AreaSpecific(5)] = Permanent,
        };
}
