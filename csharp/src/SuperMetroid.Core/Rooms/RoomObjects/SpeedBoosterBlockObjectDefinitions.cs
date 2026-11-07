
namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Typed catalog for all five retail Speed Booster collision-block variants.</summary>
internal static class SpeedBoosterBlockObjectDefinitions
{
    /// <summary>The behaviour of a Speed Booster block that grows back.</summary>
    internal const int RespawningBts = 14;
    /// <summary>The behaviour of a Speed Booster block that stays broken.</summary>
    internal const int PermanentBts = 15;

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
}
