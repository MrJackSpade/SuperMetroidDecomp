using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Bosses;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.SporeSpawn;

namespace SuperMetroid.Core.World;

/// <summary>
/// Room effects enemy AI requests during a pass: the fixed room-object spawns of Botwoon, Spore Spawn, Crocomire,
/// Kraid, Mother Brain, the Shitroid, Phantoon and the Chozo statues, and the downward gates' projectile requests. Each
/// lands in the room it is handed (<see cref="EnemyRoomEffectSite"/>, #2382).
/// </summary>
internal sealed class EnemyRoomEffects
{
    /// <summary>Starts the escape countdown on Mother Brain's requested frame.</summary>
    private readonly EscapeTimer _escapeTimer;
    /// <summary>Records defeated bosses and the Zebes timebomb event requested by the encounter.</summary>
    private readonly SharedGameState _system;

    /// <summary>Binds the room systems needed to consume enemy publications immediately after their AI pass.</summary>
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
    /// Applies the fixed room objects and room effects that enemy AI requested during its pass.
    /// </summary>
    /// <remarks>
    /// Actors spawn their fixed room objects and make their other room writes
    /// inside the enemy pass, and each room object's setup edits the level at once. Applying them right
    /// after the enemy pass lets Samus's movement later in the frame meet the edited
    /// terrain; Baby Metroid clears its invisible wall this way just before Samus runs
    /// through it.
    /// </remarks>
    internal void ApplyEnemyRequestedRoomEffects(EnemyRoomEffectRequests requests, EnemyRoomEffectSite site)
    {
        ApplyPendingBotwoonWallObject(requests, site);
        ApplyPendingSporeSpawnCeilingObject(requests, site);
        ApplyPendingCrocomireArenaObjects(requests, site);
        ApplyPendingKraidObjects(requests, site);
        ApplyPendingMotherBrainObjects(requests, site);
        ApplyPendingShitroidWallObjects(requests, site);
        ApplyPendingPhantoonDoorObject(requests, site);
        ApplyPendingChozoStatueObjects(site);
        ApplyPendingTorizoBellyGlow(requests, site);
    }

    /// <summary>
    /// Starts the glow an enemy's placement started, if any, among the room's colour effects (#1733): the gunship's glow,
    /// which its top hull's placement starts. The glow keeps the gunship it shows on and runs until the room's colour
    /// effects are cleared (#1783), as the original's glow object runs until the room load clears its colour effects.
    /// </summary>
    internal static void StartEnemyGlow(EnemyGlowStart? glow, EnemyRoomEffectSite site)
    {
        if (glow is not null)
            site.ColourEffects.StartEnemyGlow(glow, site.Samus.EquippedItems);
    }

    /// <summary>Starts the belly glow a Torizo's fight start asked for among the room's colour effects, watching that Torizo.</summary>
    private static void ApplyPendingTorizoBellyGlow(EnemyRoomEffectRequests requests, EnemyRoomEffectSite site)
    {
        if (requests.TorizoBellyGlow is { } glow)
            site.ColourEffects.StartTorizoBellyGlow(glow.Glow, glow.Torizo, site.Samus.EquippedItems);
    }

    /// <summary>
    /// Applies the effect published by Botwoon's AI through the shared
    /// room-object owner. This is called at both producer sites: room initialization
    /// for an already-defeated boss and the enemy pass when the final body segment lands.
    /// </summary>
    private static void ApplyPendingBotwoonWallObject(EnemyRoomEffectRequests requests, EnemyRoomEffectSite site)
    {
        if (requests.BotwoonWall is not BotwoonWallMutation mutation)
            return;

        // The wall's room object does not spawn while every room-object place is held; a room loads with every place
        // free.
        site.RoomObjects.BossRoomMutations.BotwoonWall.TrySpawn(site.Level, mutation);

        if (mutation == BotwoonWallMutation.Clear)
        {
            // With Botwoon already defeated the wall is cleared at once during setup; unlike the live crumble room
            // object, it does not wait for its
            // clearing step during the first room-object step.
            foreach (Position16<RoomScreens> cell in BotwoonWallMutationDefinitions.OpenedScrollCells)
                site.Scrolls.SetLogicalState(cell, RoomScrollState.OpenAlignedToTop);
        }
    }

    /// <summary>
    /// Applies Spore Spawn's ceiling request through the shared room-object
    /// owner. Initialization publishes the clear entry for a defeated save; the live death
    /// reaction publishes the animated crumble entry on the exact collision frame.
    /// </summary>
    private static void ApplyPendingSporeSpawnCeilingObject(EnemyRoomEffectRequests requests, EnemyRoomEffectSite site)
    {
        if (requests.SporeSpawnCeiling is not SporeSpawnCeilingMutation mutation)
            return;

        // A fixed room-object spawn silently drops the request if all forty places are held.
        site.RoomObjects.BossRoomMutations.SporeSpawnCeiling.TrySpawn(site.Level, mutation);
    }

    /// <summary>
    /// Applies Crocomire's arena changes through the shared
    /// room-object owner. Consuming the full frame-local list preserves publication order
    /// and fixed-pool exhaustion behavior.
    /// </summary>
    private static void ApplyPendingCrocomireArenaObjects(EnemyRoomEffectRequests requests, EnemyRoomEffectSite site)
    {
        foreach (RoomObjectSpawnRequest<CrocomireArenaMutation> request in requests.CrocomireObjects)
        {
            // A fixed room-object spawn silently loses a request when all forty places are
            // held. The arena spawner deliberately mirrors that result.
            site.RoomObjects.BossRoomMutations.CrocomireArena.TrySpawn(site.Level, request);
        }
    }

    /// <summary>
    /// Transfers Kraid's ceiling and platform requests into the shared
    /// room-object pool before the current frame's room-object step.
    /// </summary>
    private static void ApplyPendingKraidObjects(EnemyRoomEffectRequests requests, EnemyRoomEffectSite site)
    {
        foreach (RoomObjectSpawnRequest<KraidRoomMutation> request in requests.KraidObjects)
            site.RoomObjects.BossRoomMutations.KraidRoom.TrySpawn(site.Level, request);
    }

    /// <summary>
    /// Transfers Mother Brain's fixed room-object requests to the shared
    /// owner. The enemy state publishes requests in the order it makes them; consuming them
    /// here keeps the places they take in update order and the one-frame execution seam.
    /// </summary>
    internal void ApplyPendingMotherBrainObjects(EnemyRoomEffectRequests requests, EnemyRoomEffectSite site)
    {
        if (requests.MotherBrainDeathStep is { } death)
        {
            foreach (ColourEffectId definition in death.EscapeColourEffectRequests)
                site.ColourEffects.Start(definition, site.Samus.EquippedItems);
            if (death.GetUpReleaseRequested)
                site.Samus.Drained.RelinquishGetUp();
            if (death.MotherBrainEscapeTimerStartRequested)
                _escapeTimer.RequestMotherBrainStart(); // The runtime already owns per-frame timer processing/drawing.
            if (death.MotherBrainDefeatRecordRequested)
                _system.RecordBossesDefeated(site.Area, DefeatedBosses.AreaMiniBoss);
            if (death.ZebesTimebombEventRequested)
                _system.SetEvent(StoryEvent.ZebesTimebombSet);
        }
        if (requests.MotherBrainAcidChange is { } acidChange)
        {
            // The acid change is applied once, on the pass that requested it. Reapplying it each frame would reset
            // its delay and prevent the acid from moving.
            site.FxOverlay.ApplyEntry(acidChange);
        }

        foreach (MotherBrainRoomChangeRequest request in requests.MotherBrainObjects)
            site.RoomObjects.BossRoomMutations.MotherBrainRoom.TrySpawn(site.Level, request);
    }

    /// <summary>
    /// Transfers the Shitroid's wall requests to the shared room-object
    /// pool at both producer seams: enemy initialization and the end of each enemy pass.
    /// </summary>
    private static void ApplyPendingShitroidWallObjects(EnemyRoomEffectRequests requests, EnemyRoomEffectSite site)
    {
        foreach (RoomObjectSpawnRequest<ShitroidWallMutation> request in requests.ShitroidObjects)
            site.RoomObjects.BossRoomMutations.ShitroidWall.TrySpawn(site.Level, request);
    }

    /// <summary>
    /// Spawns the door change Phantoon's fight asked for in the pass (#1678) among the room's objects: the fight's door
    /// as the starting flames close in, the restored door once the Wrecked Ship's power is back. Its setup holds the
    /// door's top block solid until its first update draws the door.
    /// </summary>
    private static void ApplyPendingPhantoonDoorObject(EnemyRoomEffectRequests requests, EnemyRoomEffectSite site)
    {
        if (requests.PhantoonDoor is not { } change)
            return;

        // A spawn into a full room is lost, as every boss terrain change's is (RoomObjectPlaces).
        site.RoomObjects.BossRoomMutations.PhantoonDoor.TrySpawn(site.Level, change);
    }

    /// <summary>
    /// Consumes pending Chozo terrain publications. Also runs at the frame's entry, before movement, so Samus cannot
    /// collide before a pending initialization setup has installed the hand block.
    /// </summary>
    internal static void ApplyPendingChozoStatueObjects(EnemyRoomEffectSite site) =>
        site.Enemies.RoomObjectRequests.ApplyPendingChozoStatueObjects(site.Level, site.RoomObjects.ChozoStatue);

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
