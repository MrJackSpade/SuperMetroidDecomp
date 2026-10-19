using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Areas.Escape;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Room effects enemy AI requests during a pass: the hardcoded PLM spawns of Botwoon, Spore Spawn, Crocomire,
/// Kraid, Mother Brain, the Shitroid and the Chozo statues, and the downward gates' projectile requests.
/// </summary>
internal sealed class EnemyRoomEffects
{
    /// <summary>Destination for Mother Brain's room palette and FX transitions.</summary>
    private readonly SnesCgram _cgram;
    /// <summary>Receives layer-blending defaults selected by a Mother Brain FX entry.</summary>
    private readonly GameplayDisplayState _display;
    /// <summary>Publishes boss PLM requests and receives downward-gate projectile work.</summary>
    private readonly RoomEnemySystem _enemies;
    /// <summary>Starts the escape countdown on Mother Brain's requested frame.</summary>
    private readonly EscapeTimer _escapeTimer;
    /// <summary>Supplies Samus equipment and relinquishes the drained-phase timer handler.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Owns the shared forty-slot PLM pool that applies boss terrain changes.</summary>
    private readonly RoomPlmSystem _plms;
    /// <summary>Provides active level, area, FX pointer, and room scroll state.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Loads Mother Brain's requested layer-three FX record.</summary>
    private readonly RoomLayer3FxState _roomLayer3Fx;
    /// <summary>Spawns palette FX definitions published by Mother Brain's death sequence.</summary>
    private readonly RoomPaletteFxSystem _roomPaletteFx;
    /// <summary>Records boss bits and the Zebes timebomb event requested by the encounter.</summary>
    private readonly Bank80SystemState _system;

    /// <summary>Binds the room systems needed to consume enemy publications immediately after their AI pass.</summary>
    internal EnemyRoomEffects(EnemyRoomEffectsDependencies dependencies)
    {
        _cgram = dependencies.Cgram;
        _display = dependencies.Display;
        _enemies = dependencies.Enemies;
        _escapeTimer = dependencies.EscapeTimer;
        _player = dependencies.Player;
        _plms = dependencies.Plms;
        _room = dependencies.Room;
        _roomLayer3Fx = dependencies.RoomLayer3Fx;
        _roomPaletteFx = dependencies.RoomPaletteFx;
        _system = dependencies.System;
    }

    /// <summary>
    /// Applies the hardcoded PLMs and room effects that enemy AI requested during its pass.
    /// </summary>
    /// <remarks>
    /// Native actors call <c>Spawn_Hardcoded_PLM</c> ($84:83D7) and their other room writes
    /// inside EnemyMain, and each PLM's setup edits the level at once. Applying them right
    /// after the enemy pass lets Samus's movement later in the frame meet the edited
    /// terrain; Baby Metroid clears its invisible wall this way just before Samus runs
    /// through it.
    /// </remarks>
    internal void ApplyEnemyRequestedRoomEffects(EnemyRoomEffectRequests requests)
    {
        ApplyPendingBotwoonWallPlm(requests);
        ApplyPendingSporeSpawnCeilingPlm(requests);
        ApplyPendingCrocomireArenaPlms(requests);
        ApplyPendingKraidPlms(requests);
        ApplyPendingMotherBrainPlms(requests);
        ApplyPendingShitroidWallPlms(requests);
        ApplyPendingChozoStatuePlms();
    }

    /// <summary>
    /// Applies the cross-bank effect published by Botwoon's bank-$B3 AI through the shared
    /// bank-$84 PLM owner. This is called at both native producer sites: room initialization
    /// for an already-defeated boss and EnemyMain when the final body segment lands.
    /// </summary>
    internal void ApplyPendingBotwoonWallPlm(EnemyRoomEffectRequests requests)
    {
        if (requests.BotwoonWall is not PlmHeaderId header)
            return;
        if (_room.LevelData is null || _room.Camera is null)
        {
            throw new InvalidOperationException(
                "Botwoon published a wall PLM without an active room level and scroll grid.");
        }

        // SpawnHardcodedPLM silently returns when all native slots are occupied. Preserve
        // that allocator behavior; normal load begins from Reset's empty pool, while an
        // artificial full-pool audit must not gain a host-only exception or terrain edit.
        _plms.BossRoomMutations.TrySpawnBotwoonWall(_room.LevelData, header);

        if (header == PlmHeaderId.ClearBotwoonWall)
        {
            // The already-defeated branch performs this 16-bit `$0101` store directly in
            // `$B3:959E`; unlike the live crumble PLM, it does not wait for instruction
            // `$84:AB51` during the first handler pass.
            _room.Camera.Scrolls.SetLogicalState(new(0, 0), RoomScrollState.Blue);
            _room.Camera.Scrolls.SetLogicalState(new(1, 0), RoomScrollState.Blue);
        }
    }

    /// <summary>
    /// Applies Spore Spawn's bank-$A5 ceiling request through the shared bank-$84 PLM
    /// owner. Initialization publishes the clear entry for a defeated save; the live death
    /// reaction publishes the animated crumble entry on the exact collision frame.
    /// </summary>
    internal void ApplyPendingSporeSpawnCeilingPlm(EnemyRoomEffectRequests requests)
    {
        if (requests.SporeSpawnCeiling is not SporeSpawnPlmRequest request)
            return;
        if (_room.LevelData is null)
        {
            throw new InvalidOperationException(
                "Spore Spawn published a ceiling PLM without active room level data.");
        }
        if (request.Block != new Position16<RoomBlocks>(7, 30))
        {
            throw new InvalidDataException(
                $"Spore Spawn published non-cartridge ceiling coordinates " +
                $"({request.Block.X},{request.Block.Y}).");
        }

        // SpawnHardcodedPLM silently drops the request if all forty native slots are full.
        _plms.BossRoomMutations.TrySpawnSporeSpawnCeiling(_room.LevelData, request.Header);
    }

    /// <summary>
    /// Applies Crocomire's bank-$A4 hardcoded arena mutations through the shared bank-$84
    /// PLM owner. Consuming the full frame-local list preserves native publication order
    /// and fixed-pool exhaustion behavior.
    /// </summary>
    internal void ApplyPendingCrocomireArenaPlms(EnemyRoomEffectRequests requests)
    {
        if (requests.CrocomirePlms.Count == 0)
            return;
        if (_room.LevelData is null)
        {
            throw new InvalidOperationException(
                "Crocomire published an arena PLM without an active room level.");
        }

        foreach (CrocomirePlmRequest request in requests.CrocomirePlms)
        {
            // SpawnHardcodedPLM silently loses a request when all forty native slots are
            // occupied. TrySpawnCrocomireArenaMutation deliberately mirrors that result.
            _plms.BossRoomMutations.TrySpawnCrocomireArenaMutation(
                _room.LevelData,
                request.Block,
                request.Header);
        }
    }

    /// <summary>
    /// Transfers Kraid's bank-$A7 hardcoded ceiling/platform calls into the shared bank-$84
    /// PLM pool before the current frame's handler pass.
    /// </summary>
    internal void ApplyPendingKraidPlms(EnemyRoomEffectRequests requests)
    {
        if (requests.KraidPlms.Count == 0)
            return;
        if (_room.LevelData is null)
            throw new InvalidOperationException("Kraid published room PLMs without active level data.");

        foreach (KraidPlmRequest request in requests.KraidPlms)
        {
            _plms.BossRoomMutations.TrySpawnKraidRoomMutation(
                _room.LevelData,
                request.Block,
                request.Header);
        }
    }

    /// <summary>
    /// Transfers Mother Brain's bank-$A9 hardcoded-PLM requests to the shared bank-$84
    /// owner. The enemy state publishes requests in cartridge call order; consuming them
    /// here retains the native descending-slot allocation and one-frame execution seam.
    /// </summary>
    internal void ApplyPendingMotherBrainPlms(EnemyRoomEffectRequests requests)
    {
        if (requests.MotherBrainDeathStep is { } death)
        {
            foreach (PaletteFxObject definition in death.EscapePaletteFxRequests)
                _roomPaletteFx.SpawnDefinition(definition, _player.IntroducedSamus.EquippedItems);
            if (death.TimerHandlingEnableRequested)
                _player.IntroducedSamus.Drained.RelinquishTimerHackHandler();
            if (death.MotherBrainEscapeTimerStartRequested)
                _escapeTimer.RequestMotherBrainStart(); // The runtime already owns per-frame timer processing/drawing.
            if (death.MotherBrainBossBitRequested)
                _system.SetBossBits(_room.LoadedRoom.AreaIndex, BossBits.AreaMiniBoss);
            if (death.ZebesTimebombEventRequested)
                _system.SetEvent(EventNumber.ZebesTimebombSet);
        }
        if (requests.MotherBrainFxEntry > 0)
        {
            // The pending native LoadFxEntry call is applied once, on the pass that requested it. Reapplying it
            // each frame would reset its delay and prevent the acid from moving.
            RoomFxDefinition fx = _room.LoadedRoom.State.Fx
                ?? throw new InvalidOperationException("Mother Brain requested an FX entry in a room without FX.");
            _display.LayerBlendingDefaultConfig = _roomLayer3Fx.ApplyEntry(_cgram,
                fx.Records[requests.MotherBrainFxEntry]);
        }
        if (requests.MotherBrainPlms.Count == 0)
            return;
        if (_room.LevelData is null)
        {
            throw new InvalidOperationException(
                "Mother Brain published a room PLM without active room level data.");
        }

        foreach (MotherBrainPlmRequest request in requests.MotherBrainPlms)
        {
            _plms.BossRoomMutations.TrySpawnMotherBrainMutation(
                _room.LevelData,
                request.Block,
                request.Header);
        }
    }

    /// <summary>
    /// Transfers Shitroid's bank-$A9 hardcoded wall requests to the shared bank-$84 PLM
    /// pool at both producer seams: enemy initialization and the end of each enemy pass.
    /// </summary>
    internal void ApplyPendingShitroidWallPlms(EnemyRoomEffectRequests requests)
    {
        if (requests.ShitroidPlms.Count == 0)
            return;
        if (_room.LevelData is null)
        {
            throw new InvalidOperationException(
                "Shitroid published wall PLMs without active room level data.");
        }

        foreach (ShitroidPlmRequest request in requests.ShitroidPlms)
        {
            _plms.BossRoomMutations.TrySpawnShitroidWallMutation(
                _room.LevelData,
                request.Block,
                request.Header);
        }
    }

    /// <summary>
    /// Binds transient Chozo services and consumes pending bank-$AA terrain publications.
    /// Also runs before movement so restored states cannot collide before their pending
    /// initialization setup has installed the hand block.
    /// </summary>
    internal void ApplyPendingChozoStatuePlms()
    {
        _plms.ChozoStatue.BindChozoStatueContext(_enemies, _roomLayer3Fx);
        if (_room.LevelData is not null)
            _enemies.ChozoStatueBehavior.ApplyPendingChozoStatuePlms(_room.LevelData, _plms.ChozoStatue);
    }

    /// <summary>Transfers bank-$84 downward-gate spawn/wake work into bank $86.</summary>
    internal void ApplyPendingDownwardGateProjectileRequests()
    {
        if (_room.LevelData is null)
            throw new InvalidOperationException("Gate projectile work requires active room level data.");

        foreach (DownwardGateProjectileRequest request in _plms.Gates.TakeDownwardGateProjectileRequests())
        {
            if (_player.Samus?.Xray.AreEnemyProjectilesSuspended == true)
                continue;
            _enemies.DownwardGateBehavior.ApplyDownwardGateProjectileRequest(request, _room.LevelData.Size.X);
        }
    }
}
