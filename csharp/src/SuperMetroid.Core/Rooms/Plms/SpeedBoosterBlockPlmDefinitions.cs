
namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Typed catalog for all five retail Speed Booster collision-block variants.</summary>
internal static class SpeedBoosterBlockPlmDefinitions
{
    /// <summary>The behaviour of a Speed Booster block that grows back.</summary>
    internal const int RespawningBts = 0x0e;
    /// <summary>The behaviour of a Speed Booster block that stays broken.</summary>
    internal const int PermanentBts = 0x0f;

    /// <summary>Brinstar BTS <c>$82</c>: respawning, slower crumble.</summary>
    public static readonly SpeedBoosterBlockPlmDefinition BrinstarSlowRespawning = new(
        PlmHeaderId.SpeedBlockBrinstarSlowRespawning,
        SpeedBoosterBlockProgram.BrinstarSlowRespawning);

    /// <summary>Brinstar BTS <c>$83</c>: permanent, slower crumble.</summary>
    public static readonly SpeedBoosterBlockPlmDefinition BrinstarSlowPermanent = new(
        PlmHeaderId.SpeedBlockBrinstarSlowPermanent,
        SpeedBoosterBlockProgram.BrinstarSlowPermanent);

    /// <summary>Area-independent BTS <c>$0E</c>: standard respawning block.</summary>
    public static readonly SpeedBoosterBlockPlmDefinition Respawning = new(
        PlmHeaderId.SpeedBlockRespawning,
        SpeedBoosterBlockProgram.Respawning);

    /// <summary>Brinstar BTS <c>$84</c>: Dachora-room standard respawning block.</summary>
    public static readonly SpeedBoosterBlockPlmDefinition DachoraRespawning = new(
        PlmHeaderId.SpeedBlockDachoraRespawning,
        SpeedBoosterBlockProgram.DachoraRespawning);

    /// <summary>Area-independent BTS <c>$0F</c> and Brinstar BTS <c>$85</c>: permanent block.</summary>
    public static readonly SpeedBoosterBlockPlmDefinition Permanent = new(
        PlmHeaderId.SpeedBlockPermanent,
        SpeedBoosterBlockProgram.Permanent);
}
