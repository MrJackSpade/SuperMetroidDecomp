using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Rooms.RoomObjects;

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

    /// <summary>The Shitroid's direct camera X write in room pixels, if any.</summary>
    internal int? CameraXOverride { get; set; }

    /// <summary>Mother Brain's tube landed, removing the room's liquid effects.</summary>
    internal bool RemoveLiquidEffects { get; set; }

    /// <summary>Botwoon's wall room object.</summary>
    internal BotwoonWallMutation? BotwoonWall { get; set; }

    /// <summary>Spore Spawn's ceiling room object.</summary>
    internal SporeSpawnCeilingMutation? SporeSpawnCeiling { get; set; }

    /// <summary>The belly glow a Torizo's fight start asks the colour effect handler for, or null.</summary>
    internal TorizoBellyGlowRequest? TorizoBellyGlow { get; set; }

    /// <summary>Phantoon's door change (#1678): the fight's door as the starting flames close in, or the restored door.</summary>
    internal PhantoonDoorChange? PhantoonDoor { get; set; }

    /// <summary>Crocomire's arena mutations, in publication order.</summary>
    internal List<RoomObjectSpawnRequest<CrocomireArenaMutation>> CrocomireObjects { get; } = [];

    /// <summary>Kraid's ceiling and platform mutations, in publication order.</summary>
    internal List<RoomObjectSpawnRequest<KraidRoomMutation>> KraidObjects { get; } = [];

    /// <summary>Mother Brain's death-sequence step.</summary>
    internal MotherBrainRainbowBeamAttackStepResult? MotherBrainDeathStep { get; set; }

    /// <summary>
    /// The change Mother Brain's fight makes to her room's acid, or null for none: her initialization requests the acid
    /// rising and her fake death its draining (<see cref="MotherBrainRoomFxDefinitions"/>), and the pass reset clears it.
    /// </summary>
    internal RoomFxRecordDefinition? MotherBrainAcidChange { get; set; }

    /// <summary>Mother Brain's room mutations, in publication order.</summary>
    internal List<RoomObjectSpawnRequest<MotherBrainRoomMutation>> MotherBrainObjects { get; } = [];

    /// <summary>The Shitroid's wall mutations, in publication order.</summary>
    internal List<RoomObjectSpawnRequest<ShitroidWallMutation>> ShitroidObjects { get; } = [];

    /// <summary>Clears every result before a pass or a room load.</summary>
    internal void Begin()
    {
        DraygonDiscardedSamusPoseRequests = false;
        CeresEscapeStarted = false;
        CameraXOverride = null;
        RemoveLiquidEffects = false;
        BotwoonWall = null;
        SporeSpawnCeiling = null;
        TorizoBellyGlow = null;
        PhantoonDoor = null;
        CrocomireObjects.Clear();
        KraidObjects.Clear();
        MotherBrainDeathStep = null;
        MotherBrainAcidChange = null;
        MotherBrainObjects.Clear();
        ShitroidObjects.Clear();
    }

    /// <summary>The room effects the bosses requested so far.</summary>
    internal EnemyRoomEffectRequests RoomEffects() =>
        new(BotwoonWall, SporeSpawnCeiling, [.. CrocomireObjects], [.. KraidObjects], MotherBrainDeathStep,
            MotherBrainAcidChange, [.. MotherBrainObjects], [.. ShitroidObjects], TorizoBellyGlow, PhantoonDoor);
}
