using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;

namespace SuperMetroid.Core.World;

/// <summary>
/// The room effects a room's enemies ask for in a pass or as their room loads: the fixed room-object spawns of Botwoon,
/// Spore Spawn, Crocomire, Kraid, Mother Brain, the Baby Metroid, Phantoon and the Chozo statues, the colour effects and acid
/// changes of the Torizos and Mother Brain, the effects of Mother Brain's death, and the downward gates' projectile
/// requests. Each lands in the room it is handed (<see cref="EnemyRoomEffectSite"/>, #2382).
/// </summary>
internal sealed partial class EnemyRoomEffects
{
    /// <summary>Starts the escape countdown on Mother Brain's requested frame.</summary>
    private readonly EscapeTimer _escapeTimer;
    /// <summary>Records defeated bosses and the Zebes timebomb event requested by the encounter.</summary>
    private readonly SharedGameState _system;

    /// <summary>Binds the room systems that act on the enemies' events immediately after their AI pass.</summary>
    internal EnemyRoomEffects(EnemyRoomEffectsDependencies dependencies)
    {
        _escapeTimer = dependencies.EscapeTimer;
        _system = dependencies.System;
    }

    /// <summary>
    /// Starts the Ceres escape timer: Ceres Ridley starts it on the same enemy pass that moves Ceres from the fight to
    /// the escape.
    /// </summary>
    internal void StartCeresEscape() => _escapeTimer.RequestCeresStart();

    /// <summary>
    /// Spawns the room objects and starts the room effects that <paramref name="pass"/>'s enemies asked for, in the order
    /// they asked for them; the pass's other events are the runtime's (<see cref="EnemyFramePhase"/>).
    /// </summary>
    /// <remarks>
    /// The enemies make these room changes during their pass, and each room object's setup changes the terrain at once,
    /// so Samus's movement later in the update meets the changed terrain; the Baby Metroid clears its invisible wall
    /// this way just before Samus runs through it. A room object spawned into a room whose places are all held is lost
    /// (RoomObjectPlaces); a room loads with every place free.
    /// </remarks>
    internal void ApplyEnemyRequestedRoomEffects(EnemyFrameResult pass, EnemyRoomEffectSite site)
    {
        foreach (EnemyPassEvent passEvent in pass.Events)
        {
            switch (passEvent)
            {
                case BotwoonWallChange wall:
                    ApplyBotwoonWallChange(wall.Mutation, site);
                    break;
                case SporeSpawnCeilingChange ceiling:
                    // Its setup in a defeated room asks for the clear ceiling; the live death the animated crumble, on
                    // the exact collision update.
                    site.RoomObjects.BossRoomMutations.SporeSpawnCeiling.Spawn(site.Level, ceiling.Mutation);
                    break;
                case CrocomireArenaChange arena:
                    site.RoomObjects.BossRoomMutations.CrocomireArena.Spawn(site.Level, arena.Request);
                    break;
                case KraidRoomChange kraid:
                    site.RoomObjects.BossRoomMutations.KraidRoom.Spawn(site.Level, kraid.Request);
                    break;
                case MotherBrainDeathStepTaken death:
                    ApplyMotherBrainDeathStep(death.Step, site);
                    break;
                case MotherBrainAcidChange acid:
                    site.FxOverlay.ApplyEntry(acid.Change);
                    break;
                case MotherBrainRoomChangeRequest change:
                    site.RoomObjects.BossRoomMutations.MotherBrainRoom.Spawn(site.Level, change);
                    break;
                case BabyMetroidWallChange babyMetroidWall:
                    site.RoomObjects.BossRoomMutations.BabyMetroidWall.Spawn(site.Level, babyMetroidWall.Request);
                    break;
                case PhantoonDoorChanged door:
                    // The door's setup holds its top block solid until its first update draws the door.
                    site.RoomObjects.BossRoomMutations.PhantoonDoor.Spawn(site.Level, door.Change);
                    break;
                case ChozoStatueObjectAsked chozo:
                    site.RoomObjects.ChozoStatue.SpawnChozoStatueObject(site.Level, chozo.Request);
                    break;
                case TorizoBellyGlowRequest glow:
                    site.ColourEffects.StartTorizoBellyGlow(glow.Glow, glow.Torizo, site.Samus.EquippedItems);
                    break;
                default:
                    // The pass's events for Samus, the camera, the escape and the game are the runtime's.
                    break;
            }
        }
    }

    /// <summary>
    /// Starts the glows the pass's enemies' placements started among the room's colour effects (#1733): the gunship's
    /// glow, which its top hull's placement starts. The glow keeps the gunship it shows on and runs until the room's colour
    /// effects are cleared, which a room load does (#1783).
    /// </summary>
    internal static void StartEnemyGlows(EnemyFrameResult pass, EnemyRoomEffectSite site)
    {
        foreach (EnemyGlowStarted glow in pass.All<EnemyGlowStarted>())
            site.ColourEffects.StartEnemyGlow(glow.Glow, site.Samus.EquippedItems);
    }

    /// <summary>
    /// Spawns Botwoon's wall change through the shared room-object owner, at both of its producers: his setup in a
    /// defeated room and the enemy pass on which his final body segment lands.
    /// </summary>
    private static void ApplyBotwoonWallChange(BotwoonWallMutation mutation, EnemyRoomEffectSite site)
    {
        site.RoomObjects.BossRoomMutations.BotwoonWall.Spawn(site.Level, mutation);

        if (mutation == BotwoonWallMutation.Clear)
        {
            // With Botwoon already defeated the wall is cleared at once during setup; unlike the live crumble room
            // object, it does not wait for its
            // clearing step during the first room-object step.
            foreach (Position16<RoomScreens> cell in BotwoonWallMutationDefinitions.OpenedScrollCells)
                site.Scrolls.SetRoomScreen(cell, RoomScrollState.OpenAlignedToTop);
        }
    }

    /// <summary>
    /// Carries out the room-level effects one step of Mother Brain's death asked for: the escape's colour effects, the end
    /// of Samus's drained get-up, the escape countdown, her defeat's record and the Zebes time bomb's story event.
    /// </summary>
    internal void ApplyMotherBrainDeathStep(MotherBrainRainbowBeamAttackStepResult death, EnemyRoomEffectSite site)
    {
        foreach (ColourEffectId definition in death.EscapeColourEffectRequests)
            site.ColourEffects.Start(definition, site.Samus.EquippedItems);
        if (death.Starts(MotherBrainStepEffect.GetUpRelease))
            site.Samus.Drained.RelinquishGetUp();
        if (death.Starts(MotherBrainStepEffect.EscapeTimerStart))
            _escapeTimer.RequestMotherBrainStart(); // The runtime already owns per-frame timer processing/drawing.
        if (death.Starts(MotherBrainStepEffect.DefeatRecord))
            _system.RecordBossesDefeated(site.Area, DefeatedBosses.AreaMiniBoss);
        if (death.Starts(MotherBrainStepEffect.ZebesTimebombEvent))
            _system.SetEvent(StoryEvent.ZebesTimebombSet);
    }

    /// <summary>Transfers downward-gate spawn/wake work into the enemy-projectile pool.</summary>
    internal static void ApplyPendingDownwardGateProjectileRequests(EnemyRoomEffectSite site)
    {
        foreach (DownwardGateProjectileRequest request in site.RoomObjects.Gates.TakeDownwardGateProjectileRequests())
        {
            if (site.Samus.Xray.AreEnemyProjectilesSuspended)
                continue;
            site.Enemies.RoomObjectRequests.ApplyDownwardGateProjectileRequest(request);
        }
    }
}
