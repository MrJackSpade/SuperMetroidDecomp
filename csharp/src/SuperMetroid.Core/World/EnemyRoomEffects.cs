using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Bosses;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.SporeSpawn;

namespace SuperMetroid.Core.World;

/// <summary>
/// Room effects enemy AI requests during a pass: the hardcoded PLM spawns of Botwoon, Spore Spawn, Crocomire,
/// Kraid, Mother Brain, the Shitroid and the Chozo statues, and the downward gates' projectile requests.
/// </summary>
internal sealed class EnemyRoomEffects
{
    /// <summary>Starts the escape countdown on Mother Brain's requested frame.</summary>
    private readonly EscapeTimer _escapeTimer;
    /// <summary>Supplies Samus equipment and relinquishes the drained-phase timer handler.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Provides active level, area, FX pointer, and room scroll state.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Records boss bits and the Zebes timebomb event requested by the encounter.</summary>
    private readonly SharedGameState _system;

    /// <summary>Binds the room systems needed to consume enemy publications immediately after their AI pass.</summary>
    internal EnemyRoomEffects(EnemyRoomEffectsDependencies dependencies)
    {
        _escapeTimer = dependencies.EscapeTimer;
        _player = dependencies.Player;
        _room = dependencies.Room;
        _system = dependencies.System;
    }

    /// <summary>
    /// Starts the Ceres escape timer: Ceres Ridley starts it on the same EnemyMain call that changes ceres_status
    /// from one to two.
    /// </summary>
    internal void StartCeresEscape() => _escapeTimer.RequestCeresStart();

    /// <summary>
    /// Applies the hardcoded PLMs and room effects that enemy AI requested during its pass.
    /// </summary>
    /// <remarks>
    /// Actors spawn their fixed room objects and make their other room writes
    /// inside EnemyMain, and each PLM's setup edits the level at once. Applying them right
    /// after the enemy pass lets Samus's movement later in the frame meet the edited
    /// terrain; Baby Metroid clears its invisible wall this way just before Samus runs
    /// through it.
    /// </remarks>
    internal void ApplyEnemyRequestedRoomEffects(EnemyRoomEffectRequests requests, RoomObjectSystem roomObjects, RoomFxOverlayState roomFxOverlay, RoomPaletteFxSystem roomPaletteFx, RoomEnemySystem enemies)
    {
        ApplyPendingBotwoonWallObject(requests, roomObjects);
        ApplyPendingSporeSpawnCeilingObject(requests, roomObjects);
        ApplyPendingCrocomireArenaObjects(requests, roomObjects);
        ApplyPendingKraidObjects(requests, roomObjects);
        ApplyPendingMotherBrainObjects(requests, roomObjects, roomFxOverlay, roomPaletteFx);
        ApplyPendingShitroidWallObjects(requests, roomObjects);
        ApplyPendingChozoStatueObjects(roomObjects, roomFxOverlay, enemies);
        ApplyPendingTorizoBellyGlow(requests, roomPaletteFx);
    }

    /// <summary>Spawns the belly glow a Torizo's fight start asked for in the palette-FX handler's pool.</summary>
    internal void ApplyPendingTorizoBellyGlow(EnemyRoomEffectRequests requests, RoomPaletteFxSystem roomPaletteFx)
    {
        if (requests.TorizoBellyGlow is { } glow)
            roomPaletteFx.SpawnDefinition(glow, _player.IntroducedSamus.EquippedItems);
    }

    /// <summary>
    /// Applies the effect published by Botwoon's AI through the shared
    ///  PLM owner. This is called at both producer sites: room initialization
    /// for an already-defeated boss and EnemyMain when the final body segment lands.
    /// </summary>
    internal void ApplyPendingBotwoonWallObject(EnemyRoomEffectRequests requests, RoomObjectSystem roomObjects)
    {
        if (requests.BotwoonWall is not BotwoonWallMutation mutation)
            return;
        if (_room.LevelData is null || _room.Camera is null)
        {
            throw new InvalidOperationException(
                "Botwoon published a wall PLM without an active room level and scroll grid.");
        }

        // SpawnHardcodedPLM silently returns when all slots are occupied. Preserve
        // that allocator behavior; normal load begins from Reset's empty pool, while an
        // artificial full-pool audit must not gain a host-only exception or terrain edit.
        roomObjects.BossRoomMutations.BotwoonWall.TrySpawn(_room.LevelData, mutation);

        if (mutation == BotwoonWallMutation.Clear)
        {
            // The already-defeated branch clears the wall directly with one store
            // during setup; unlike the live crumble PLM, it does not wait for its
            // clearing step during the first handler pass.
            _room.Camera.Scrolls.SetLogicalState(new(0, 0), RoomScrollState.Blue);
            _room.Camera.Scrolls.SetLogicalState(new(1, 0), RoomScrollState.Blue);
        }
    }

    /// <summary>
    /// Applies Spore Spawn's ceiling request through the shared PLM
    /// owner. Initialization publishes the clear entry for a defeated save; the live death
    /// reaction publishes the animated crumble entry on the exact collision frame.
    /// </summary>
    internal void ApplyPendingSporeSpawnCeilingObject(EnemyRoomEffectRequests requests, RoomObjectSystem roomObjects)
    {
        if (requests.SporeSpawnCeiling is not SporeSpawnCeilingMutation mutation)
            return;
        if (_room.LevelData is null)
        {
            throw new InvalidOperationException(
                "Spore Spawn published a ceiling PLM without active room level data.");
        }

        // SpawnHardcodedPLM silently drops the request if all forty slots are full.
        roomObjects.BossRoomMutations.SporeSpawnCeiling.TrySpawn(_room.LevelData, mutation);
    }

    /// <summary>
    /// Applies Crocomire's hardcoded arena mutations through the shared 
    /// PLM owner. Consuming the full frame-local list preserves publication order
    /// and fixed-pool exhaustion behavior.
    /// </summary>
    internal void ApplyPendingCrocomireArenaObjects(EnemyRoomEffectRequests requests, RoomObjectSystem roomObjects)
    {
        if (requests.CrocomireObjects.Count == 0)
            return;
        if (_room.LevelData is null)
        {
            throw new InvalidOperationException(
                "Crocomire published an arena PLM without an active room level.");
        }

        foreach (RoomObjectSpawnRequest<CrocomireArenaMutation> request in requests.CrocomireObjects)
        {
            // SpawnHardcodedPLM silently loses a request when all forty slots are
            // occupied. The arena spawner deliberately mirrors that result.
            roomObjects.BossRoomMutations.CrocomireArena.TrySpawn(_room.LevelData, request);
        }
    }

    /// <summary>
    /// Transfers Kraid's hardcoded ceiling/platform calls into the shared 
    /// PLM pool before the current frame's handler pass.
    /// </summary>
    internal void ApplyPendingKraidObjects(EnemyRoomEffectRequests requests, RoomObjectSystem roomObjects)
    {
        if (requests.KraidObjects.Count == 0)
            return;
        if (_room.LevelData is null)
            throw new InvalidOperationException("Kraid published room PLMs without active level data.");

        foreach (RoomObjectSpawnRequest<KraidRoomMutation> request in requests.KraidObjects)
            roomObjects.BossRoomMutations.KraidRoom.TrySpawn(_room.LevelData, request);
    }

    /// <summary>
    /// Transfers Mother Brain's hardcoded-PLM requests to the shared 
    /// owner. The enemy state publishes requests in call order; consuming them
    /// here retains the descending-slot allocation and one-frame execution seam.
    /// </summary>
    internal void ApplyPendingMotherBrainObjects(EnemyRoomEffectRequests requests, RoomObjectSystem roomObjects, RoomFxOverlayState roomFxOverlay, RoomPaletteFxSystem roomPaletteFx)
    {
        if (requests.MotherBrainDeathStep is { } death)
        {
            foreach (PaletteFxObject definition in death.EscapePaletteFxRequests)
                roomPaletteFx.SpawnDefinition(definition, _player.IntroducedSamus.EquippedItems);
            if (death.TimerHandlingEnableRequested)
                _player.IntroducedSamus.Drained.RelinquishTimerHackHandler();
            if (death.MotherBrainEscapeTimerStartRequested)
                _escapeTimer.RequestMotherBrainStart(); // The runtime already owns per-frame timer processing/drawing.
            if (death.MotherBrainBossBitRequested)
                _system.SetBossBits(_room.LoadedRoom.AreaIndex, BossBits.AreaMiniBoss);
            if (death.ZebesTimebombEventRequested)
                _system.SetEvent(EventNumber.ZebesTimebombSet);
        }
        if (requests.MotherBrainAcidChange is { } acidChange)
        {
            // The acid change is applied once, on the pass that requested it. Reapplying it each frame would reset
            // its delay and prevent the acid from moving.
            roomFxOverlay.ApplyEntry(acidChange);
        }
        if (requests.MotherBrainObjects.Count == 0)
            return;
        if (_room.LevelData is null)
        {
            throw new InvalidOperationException(
                "Mother Brain published a room PLM without active room level data.");
        }

        foreach (RoomObjectSpawnRequest<MotherBrainRoomMutation> request in requests.MotherBrainObjects)
            roomObjects.BossRoomMutations.MotherBrainRoom.TrySpawn(_room.LevelData, request);
    }

    /// <summary>
    /// Transfers Shitroid's hardcoded wall requests to the shared PLM
    /// pool at both producer seams: enemy initialization and the end of each enemy pass.
    /// </summary>
    internal void ApplyPendingShitroidWallObjects(EnemyRoomEffectRequests requests, RoomObjectSystem roomObjects)
    {
        if (requests.ShitroidObjects.Count == 0)
            return;
        if (_room.LevelData is null)
        {
            throw new InvalidOperationException(
                "Shitroid published wall PLMs without active room level data.");
        }

        foreach (RoomObjectSpawnRequest<ShitroidWallMutation> request in requests.ShitroidObjects)
            roomObjects.BossRoomMutations.ShitroidWall.TrySpawn(_room.LevelData, request);
    }

    /// <summary>
    /// Binds transient Chozo services and consumes pending terrain publications.
    /// Also runs at the frame's entry, before movement, so Samus cannot collide before a pending
    /// initialization setup has installed the hand block.
    /// </summary>
    internal void ApplyPendingChozoStatueObjects(RoomObjectSystem roomObjects, RoomFxOverlayState roomFxOverlay, RoomEnemySystem enemies)
    {
        roomObjects.ChozoStatue.BindChozoStatueContext(enemies.RoomObjectRequests, enemies.RoomFx, roomFxOverlay);
        if (_room.LevelData is not null)
            enemies.RoomObjectRequests.ApplyPendingChozoStatueObjects(_room.LevelData, roomObjects.ChozoStatue);
    }

    /// <summary>Transfers downward-gate spawn/wake work into the enemy-projectile pool.</summary>
    internal void ApplyPendingDownwardGateProjectileRequests(RoomObjectSystem roomObjects, RoomEnemySystem enemies)
    {
        if (_room.LevelData is null)
            throw new InvalidOperationException("Gate projectile work requires active room level data.");

        foreach (DownwardGateProjectileRequest request in roomObjects.Gates.TakeDownwardGateProjectileRequests())
        {
            if (_player.Samus?.Xray.AreEnemyProjectilesSuspended == true)
                continue;
            enemies.RoomObjectRequests.ApplyDownwardGateProjectileRequest(request, _room.LevelData.Size.X);
        }
    }
}
