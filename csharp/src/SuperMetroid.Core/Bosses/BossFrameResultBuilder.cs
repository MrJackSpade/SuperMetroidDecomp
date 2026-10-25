using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses;

/// <summary>
/// The bosses' results from the enemy pass as they write them (#1395). Reset at the start of each pass and at room load,
/// so a paused or frozen boss never replays a request, and read back until the next pass.
/// </summary>
internal sealed class BossFrameResultBuilder
{
    /// <summary>Draygon grabbed or released Samus.</summary>
    internal bool DraygonDiscardedSamusPoseRequests { get; set; }

    /// <summary>Ceres Ridley started the escape.</summary>
    internal bool CeresEscapeStarted { get; set; }

    /// <summary>The Shitroid's direct layer-one X write in room pixels, if any.</summary>
    internal int? LayerOneXOverride { get; set; }

    /// <summary>Mother Brain's tube landed, removing the room's liquid effects.</summary>
    internal bool RemoveLiquidEffects { get; set; }

    /// <summary>Botwoon's wall PLM.</summary>
    internal PlmHeaderId? BotwoonWall { get; set; }

    /// <summary>Spore Spawn's ceiling PLM.</summary>
    internal PlmSpawnRequest? SporeSpawnCeiling { get; set; }

    /// <summary>Crocomire's arena mutations, in publication order.</summary>
    internal List<PlmSpawnRequest> CrocomirePlms { get; } = [];

    /// <summary>Kraid's ceiling and platform mutations, in publication order.</summary>
    internal List<PlmSpawnRequest> KraidPlms { get; } = [];

    /// <summary>Mother Brain's death-sequence step.</summary>
    internal MotherBrainRainbowBeamAttackStepResult? MotherBrainDeathStep { get; set; }

    /// <summary>
    /// Mother Brain's pending room FX entry index, or zero for none: its initialization requests entry one and its
    /// tube collapse entry two, and the pass reset clears it.
    /// </summary>
    internal int MotherBrainFxEntry { get; set; }

    /// <summary>Mother Brain's room mutations, in publication order.</summary>
    internal List<PlmSpawnRequest> MotherBrainPlms { get; } = [];

    /// <summary>The Shitroid's wall mutations, in publication order.</summary>
    internal List<PlmSpawnRequest> ShitroidPlms { get; } = [];

    /// <summary>Clears every result before a pass or a room load.</summary>
    internal void Begin()
    {
        DraygonDiscardedSamusPoseRequests = false;
        CeresEscapeStarted = false;
        LayerOneXOverride = null;
        RemoveLiquidEffects = false;
        BotwoonWall = null;
        SporeSpawnCeiling = null;
        CrocomirePlms.Clear();
        KraidPlms.Clear();
        MotherBrainDeathStep = null;
        MotherBrainFxEntry = 0;
        MotherBrainPlms.Clear();
        ShitroidPlms.Clear();
    }

    /// <summary>The room effects the bosses requested so far.</summary>
    internal EnemyRoomEffectRequests RoomEffects() =>
        new(BotwoonWall, SporeSpawnCeiling, [.. CrocomirePlms], [.. KraidPlms], MotherBrainDeathStep,
            MotherBrainFxEntry, [.. MotherBrainPlms], [.. ShitroidPlms]);
}
