using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy pass's results as its producers write them. Reset at the start of each pass and at room load, so a paused
/// or frozen actor never replays a request, and read back until the next pass.
/// </summary>
internal sealed class EnemyFrameResultBuilder
{
    /// <summary>The elevator lifecycle event, if any.</summary>
    internal ElevatorFrameEvent Elevator { get; set; }

    /// <summary>The elevator called <c>ResetProjectileData</c>.</summary>
    internal bool ElevatorClearedProjectileData { get; set; }

    /// <summary>Draygon grabbed or released Samus.</summary>
    internal bool DraygonDiscardedSamusPoseRequests { get; set; }

    /// <summary>Ceres Ridley started the escape.</summary>
    internal bool CeresEscapeStarted { get; set; }

    /// <summary>The Shitroid's direct layer-one X write in room pixels, if any.</summary>
    internal int? LayerOneXOverride { get; set; }

    /// <summary>Mother Brain's tube landed, deleting HDMA objects 0-3.</summary>
    internal bool DeleteLiquidHdmaObjects { get; set; }

    /// <summary>The gunship lifecycle event, if any.</summary>
    internal GunshipFrameEvent Gunship { get; set; }

    /// <summary>Botwoon's wall PLM.</summary>
    internal PlmHeaderId? BotwoonWall { get; set; }

    /// <summary>Spore Spawn's ceiling PLM.</summary>
    internal SporeSpawnPlmRequest? SporeSpawnCeiling { get; set; }

    /// <summary>Crocomire's arena mutations, in publication order.</summary>
    internal List<CrocomirePlmRequest> CrocomirePlms { get; } = [];

    /// <summary>Kraid's ceiling and platform mutations, in publication order.</summary>
    internal List<KraidPlmRequest> KraidPlms { get; } = [];

    /// <summary>Mother Brain's death-sequence step.</summary>
    internal MotherBrainRainbowBeamAttackStepResult? MotherBrainDeathStep { get; set; }

    /// <summary>
    /// Mother Brain's pending room FX entry index, or zero for none: its initialization requests entry one and its
    /// tube collapse entry two, and the pass reset clears it.
    /// </summary>
    internal int MotherBrainFxEntry { get; set; }

    /// <summary>Mother Brain's room mutations, in publication order.</summary>
    internal List<MotherBrainPlmRequest> MotherBrainPlms { get; } = [];

    /// <summary>The Shitroid's wall mutations, in publication order.</summary>
    internal List<ShitroidPlmRequest> ShitroidPlms { get; } = [];

    /// <summary>Clears every result before a pass or a room load.</summary>
    internal void Begin()
    {
        Elevator = ElevatorFrameEvent.None;
        ElevatorClearedProjectileData = false;
        DraygonDiscardedSamusPoseRequests = false;
        CeresEscapeStarted = false;
        LayerOneXOverride = null;
        DeleteLiquidHdmaObjects = false;
        Gunship = GunshipFrameEvent.None;
        BotwoonWall = null;
        SporeSpawnCeiling = null;
        CrocomirePlms.Clear();
        KraidPlms.Clear();
        MotherBrainDeathStep = null;
        MotherBrainFxEntry = 0;
        MotherBrainPlms.Clear();
        ShitroidPlms.Clear();
    }

    /// <summary>The results written so far.</summary>
    internal EnemyFrameResult Build() => new(Elevator, ElevatorClearedProjectileData, DraygonDiscardedSamusPoseRequests,
        CeresEscapeStarted, LayerOneXOverride, DeleteLiquidHdmaObjects, Gunship,
        new EnemyRoomEffectRequests(BotwoonWall, SporeSpawnCeiling, [.. CrocomirePlms], [.. KraidPlms], MotherBrainDeathStep,
            MotherBrainFxEntry, [.. MotherBrainPlms], [.. ShitroidPlms]));
}
